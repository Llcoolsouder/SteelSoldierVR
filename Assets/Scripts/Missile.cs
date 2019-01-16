using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] float missileDamage = 150.0f;
    [SerializeField] float missileSpeed = 15.0f;
    const float missileTimeout = 5.5f;
    [SerializeField, FMODUnity.EventRef] string missileFirePath;
    FMOD.Studio.EventInstance missileFireSound;
    [SerializeField] GameObject explosionPrefab;
    

    // Use this for initialization
    private void Start()
    {
        // If missile never hits anything, delete it after timeout
        StartCoroutine(DeleteObjectAfterDelay());

        missileFireSound = FMODUnity.RuntimeManager.CreateInstance(missileFirePath);
        missileFireSound.start();
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(missileFireSound, this.transform, this.GetComponent<Rigidbody>());
    }

    private void Update()
    {
        this.transform.position += -this.transform.right * missileSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Missile collided with " + other.transform.tag);
        if (other.transform.tag == "Player")
        {
            return;
        }
        else if (other.transform.tag == "fighterJet")
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
        FMOD.Studio.PLAYBACK_STATE missileFireState;
        missileFireSound.getPlaybackState(out missileFireState);
        if (missileFireState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            missileFireSound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        missileFireSound.release();

        DeleteMissile();
        GameObject explosion = Instantiate(explosionPrefab, transform.position, transform.rotation, transform);
        ParticleSystem shrapnel = explosion.transform.Find("FireShrapnel").GetComponent<ParticleSystem>();
        Destroy(this.gameObject, shrapnel.main.duration + shrapnel.main.startLifetime.constantMax);
    }

    void DeleteMissile()
    {
        foreach (Transform child in transform)
        {
            if (child.name != "RocketTrailFire" && child.name != "RocketTrailSmoke")
            {
                Destroy(child.gameObject);
            }
            else
            {
                // Allows trail time to play out. Will be deleted when the rest of this object is Destroyed
                child.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
