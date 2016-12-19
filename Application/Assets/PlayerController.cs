using System;
using System.Collections;
using System.Linq;
using System.Timers;
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
    private float _startingLinePosition;
    private Timer _jumpTimer = new System.Timers.Timer(50);


    public float startingPositionX;
    private bool _isInJump = false;

    // Use this for initialization
    private void Start()
    {
        _body = GetComponentInChildren<Rigidbody>();
        _animator = GetComponent<Animator>();
        int walkhash = Animator.StringToHash("a");

        _startingLinePosition = transform.position.z;

        _jumpTimer.Elapsed += _jumpTimer_Elapsed;
        //Debug.Log(_body.velocity.x);

    }

    private void _jumpTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        _jumpTimer.Stop();
        _isInJump = false;
    }

    private void FixedUpdate()
    {
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
             speed * 4 * (_body.velocity.x >= 5 ? 0 : 1) - powerHandicap,
             0,
             positionZ * 5);

        //kdyz je ve vzduchu

        if (!IsOnGround)
        {
            power = power / 3;
        }

        _body.AddForce(power);

        _body.rotation = Quaternion.Slerp(_body.rotation, Quaternion.Euler(0,
            0,
            0), Time.fixedDeltaTime * 2.3f);
    }

    public void Jump()
    {
        // If on the ground and jump is pressed...
        if (IsOnGround && !_isInJump)
        {
            // ... add force in upwards.
            _body.AddForce(Vector3.up * 6, ForceMode.Impulse);
            _isInJump = true;
            _jumpTimer.Start();
        }
    }

    private bool IsOnGround
    {
        get
        {
            Vector3 startcast = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            Debug.DrawLine(startcast, startcast - (Vector3.up*100));

            return Physics.Raycast(transform.position, -Vector3.up, 0.01f);
        }
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