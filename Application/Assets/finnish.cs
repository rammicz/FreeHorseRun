using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class finnish : MonoBehaviour {


    public TimeUpdate timer;

    // Use this for initialization
    void Start () {
        //timer = GetComponent<GameObject>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        player.Stop();
        timer.Stop();

        StartCoroutine(RestartInAWhile(10));
        

    }



    IEnumerator RestartInAWhile(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
