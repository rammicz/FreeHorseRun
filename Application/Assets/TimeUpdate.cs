using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class TimeUpdate : MonoBehaviour {
    private Text _timeText;
    private float? _finalTime = null;

	// Use this for initialization
	void Start () {
        _timeText = this.GetComponent<Text>();
	}
	
    void FixedUpdate()
    {
        float time = _finalTime ?? Time.timeSinceLevelLoad;
        float seconds = time % 60;
        var setiny = (time - Math.Truncate(time)) * 100;
        _timeText.text = "Time: " + String.Format("{0:0}:{1:00}.{2:00}", Mathf.Floor(time / 60), seconds, setiny);
        //Time.timeSinceLevelLoad.ToString("##.##");
    }

	// Update is called once per frame
	void Update () {
	
	}

    public void Stop()
    {
        _finalTime = Time.timeSinceLevelLoad;
    }
}
