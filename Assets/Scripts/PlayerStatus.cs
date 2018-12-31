using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour {

    public float playerHealth = 100.0f;
    public Slider healthSlider;

    public Camera cameraObject;

    public GameObject playerPrefab;
    public GameObject objectiveManager;

    //private float redCoolDown = 0.0f;

	// Use this for initialization
	void Start () {
        playerHealth = 100.0f;
	}

    public void DamagePlayer(float amount) {
        Debug.Log("Damage taken!");
        playerHealth -= amount;
        healthSlider.value = playerHealth;

        //GameObject firstCamera = GameObject.Find("Hand");

        D2FogsPE myCamera = cameraObject.GetComponent<D2FogsPE>();

        //D2FogsPE myCamera = this.gameObject.GetComponentInChildren<D2FogsPE>();
        if (!myCamera.gotHit) { 
            myCamera.gotHit = true;
            myCamera.RedMultiplier = 5.0f;
            StartCoroutine(endHit());
        }

        if(playerHealth <= 0) {
            //rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            playerPrefab.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezePositionZ;
            ObjectiveManager objectiveScript = objectiveManager.GetComponent<ObjectiveManager>();
            objectiveScript.GameOver();
            //objectiveScript.gameOver = true;

        }
        //myCamera.lastGotHit = Time.time - 2.0f;
    }

    IEnumerator endHit() {
        yield return new WaitForSeconds(2.5f);
        D2FogsPE myCamera = cameraObject.GetComponent<D2FogsPE>();
        myCamera.gotHit = false;
        myCamera.RedMultiplier = 5.0f;
    }

    void Update() {
        D2FogsPE myCamera = cameraObject.GetComponent<D2FogsPE>();
        if(myCamera.gotHit) {
            if (myCamera.RedMultiplier > 1.0f) myCamera.RedMultiplier -= 0.1f;
            //myCamera.RedMultiplier = myCamera.RedMultiplier - 0.001f;
            /*if (Time.time > redCoolDown + 0.1f) {
                redCoolDown = Time.time;
                if(myCamera.RedMultiplier >= 0.05) myCamera.RedMultiplier -= 0.01f;
            }*/
        }
    }
}
