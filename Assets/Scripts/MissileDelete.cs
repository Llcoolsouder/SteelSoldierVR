using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileDelete : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(DeleteObjectAfterDelay());
        //Debug.Log("Starting this.");
    }

    private IEnumerator DeleteObjectAfterDelay() {
        yield return new WaitForSeconds(5.5f);
        //Debug.Log("Here1");
        GameObject.Destroy(this.gameObject);
    }
}
