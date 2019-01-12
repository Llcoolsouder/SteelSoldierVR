using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

    [FMODUnity.EventRef, SerializeField] string explosionAudio;

	// Use this for initialization
	void Start ()
    {
        FMODUnity.RuntimeManager.PlayOneShot(explosionAudio, transform.position);
	}
}
