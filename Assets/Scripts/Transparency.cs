using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transparency : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var trans = 0.0f;
        var col = gameObject.GetComponent<Renderer>().material.color;
        col.a = trans;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
