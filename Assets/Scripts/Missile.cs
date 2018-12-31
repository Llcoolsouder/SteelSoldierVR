using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Missile : MonoBehaviour {
    private Rigidbody body;
    public Transform Dad;
    public GameObject missileRocket;
    // Use this for initialization
    void Start () 
    {
        Dad = body.GetComponentInParent<Transform>();
        body = GetComponent<Rigidbody>();
        body.AddForce(Dad.forward);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
