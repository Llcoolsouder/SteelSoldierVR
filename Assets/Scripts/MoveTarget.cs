using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTarget : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float x = 100*Mathf.Cos(0.25f * Time.time);
        float z = 100*Mathf.Sin(0.5f * Time.time);
        transform.position = new Vector3(x, 1, z);
	}
}
