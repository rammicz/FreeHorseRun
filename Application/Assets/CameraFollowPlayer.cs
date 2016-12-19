using UnityEngine;
using System.Collections;

public class CameraFollowPlayer : MonoBehaviour {
    private GameObject _player;
    private float _Smooth = 5f;

	// Use this for initialization
	void Start () {
        _player = GameObject.Find("Player");
	}
	
	// Update is called once per frame
	void Update () {

        var newPos = new Vector3(_player.transform.position.x+3.5f, _player.transform.position.y+3, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, newPos, _Smooth);

         
	}
}
