using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODTest : MonoBehaviour {

    [FMODUnity.EventRef]
    public string thrusterPath;
    FMOD.Studio.EventInstance thrusterEvent;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            thrusterEvent = FMODUnity.RuntimeManager.CreateInstance(thrusterPath);
            thrusterEvent.start();
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(thrusterEvent, transform, GetComponent<Rigidbody>());
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            thrusterEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            thrusterEvent.release();
        }
		
	}
}
