using System;
using UnityEngine;

[System.Serializable]
public class Boundary
{
    public float xMin, xMax, zMin, zMax;
}

public enum HorseStates
{
    Horse_Walk,
    Horse_Idle,
    Horse_Run
}

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float tilt;

    internal void Stop()
    {
        speed = 0;
    }

    public Boundary boundary;
    private Rigidbody _body;
    private Animator _animator;
    private Terrain _terrain;
    private Vector3 _spawnPosition;
    private float _startingLinePosition;
    public float startingPositionX;
    [SerializeField] private float jumpCooldown = 0.05f;
    [SerializeField] private float jumpBufferDuration = 0.2f;
    [SerializeField] private float groundCheckDistance = 1.25f;
    [SerializeField] private float trackHalfWidth = 4f;
    [SerializeField] private float respawnDepth = 4f;
    private float _nextJumpTime;
    private float _jumpRequestExpiresAt;

    // Use this for initialization
    private void Start()
    {
        _body = GetComponentInChildren<Rigidbody>();
        _animator = GetComponent<Animator>();
        _terrain = Terrain.activeTerrain;
        _spawnPosition = _body.position;
        _body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        int walkhash = Animator.StringToHash("a");

        _startingLinePosition = transform.position.z;

    }

    private void FixedUpdate()
    {
        RecoverFromFall();

        if (startingPositionX != 0)
        {
            _body.position = new Vector3(transform.position.x + startingPositionX, transform.position.y, transform.position.z);
            startingPositionX = 0;
        }

        // udržení jízdního pruhu
        float positionZ = 0;
        if (Mathf.Abs(_startingLinePosition - transform.position.z) > 0.2)
        {
            positionZ = _startingLinePosition - transform.position.z;
        }

        // add force in the move direction.
        float powerHandicap = (360 - Mathf.Abs(360 - _body.rotation.eulerAngles.z)) * 0.02f;

        Vector3 power = new Vector3(
             speed * 4 * (_body.linearVelocity.x >= 5 ? 0 : 1) - powerHandicap,
             0,
             positionZ * 5);

        //kdyz je ve vzduchu

        if (!IsOnGround)
        {
            power = power / 3;
        }

        _body.AddForce(power);

        if (Time.time <= _jumpRequestExpiresAt && IsOnGround && Time.time >= _nextJumpTime)
        {
            _body.AddForce(Vector3.up * 6, ForceMode.Impulse);
            _nextJumpTime = Time.time + jumpCooldown;
            _jumpRequestExpiresAt = 0;
        }

        KeepOnTrack();

        _body.rotation = Quaternion.Slerp(_body.rotation, Quaternion.Euler(0,
            0,
            0), Time.fixedDeltaTime * 2.3f);
    }

    public void Jump()
    {
        _jumpRequestExpiresAt = Time.time + jumpBufferDuration;
    }

    private bool IsOnGround
    {
        get
        {
            Vector3 startcast = _body.position + Vector3.up * 0.2f;
            Debug.DrawLine(startcast, startcast - Vector3.up * groundCheckDistance);

            return Physics.Raycast(startcast, Vector3.down, groundCheckDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }
    }

    private void KeepOnTrack()
    {
        Vector3 position = _body.position;
        float minZ = Mathf.Max(boundary.zMin, _startingLinePosition - trackHalfWidth);
        float maxZ = Mathf.Min(boundary.zMax, _startingLinePosition + trackHalfWidth);
        float clampedX = Mathf.Clamp(position.x, boundary.xMin, boundary.xMax);
        float clampedZ = Mathf.Clamp(position.z, minZ, maxZ);

        if (!Mathf.Approximately(position.x, clampedX) || !Mathf.Approximately(position.z, clampedZ))
        {
            _body.position = new Vector3(clampedX, position.y, clampedZ);
            Vector3 velocity = _body.linearVelocity;
            velocity.z = 0;
            _body.linearVelocity = velocity;
        }
    }

    private void RecoverFromFall()
    {
        if (_terrain == null)
            return;

        float groundHeight = _terrain.SampleHeight(_body.position) + _terrain.transform.position.y;
        if (_body.position.y >= groundHeight - respawnDepth)
            return;

        _body.position = _spawnPosition;
        _body.linearVelocity = Vector3.zero;
        _body.angularVelocity = Vector3.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
    }
}
