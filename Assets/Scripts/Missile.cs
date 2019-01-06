using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] float missileDamage = 150.0f;
    [SerializeField] float missileSpeed = 15.0f;
    const float missileTimeout = 5.5f;

    [SerializeField] GameObject explosionPrefab;


    // Use this for initialization
    private void Start()
    {
        // If missile never hits anything, delete it after timeout
        StartCoroutine(DeleteObjectAfterDelay());
    }

    private void Update()
    {
        this.transform.position += -this.transform.right * missileSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Missile collided with " + other.transform.tag);
        if (other.transform.tag == "fighterJet")
        {
            other.gameObject.GetComponent<EnemyPlane>().Damage(missileDamage);
        }
        Explode();
    }

    IEnumerator DeleteObjectAfterDelay()
    {
        yield return new WaitForSeconds(missileTimeout);
        GameObject.Destroy(this.gameObject);
    }

    void Explode()
    {

    }
}
