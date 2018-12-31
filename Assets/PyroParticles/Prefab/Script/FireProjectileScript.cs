using UnityEngine;
using System.Collections;

namespace DigitalRuby.PyroParticles
{
    /// <summary>
    /// Handle collision of a fire projectile
    /// </summary>
    /// <param name="script">Script</param>
    /// <param name="pos">Position</param>
    public delegate void FireProjectileCollisionDelegate(FireProjectileScript script, Vector3 pos);

    /// <summary>
    /// This script handles a projectile such as a fire ball
    /// </summary>
    public class FireProjectileScript : FireBaseScript, ICollisionHandler
    {
        [Tooltip("The collider object to use for collision and physics.")]
        public GameObject ProjectileColliderObject;
        public GameObject MissileModel;

        [Tooltip("The sound to play upon collision.")]
        public AudioSource ProjectileCollisionSound;

        [Tooltip("The particle system to play upon collision.")]
        public ParticleSystem ProjectileExplosionParticleSystem;

        [Tooltip("The radius of the explosion upon collision.")]
        public float ProjectileExplosionRadius = 50.0f;

        [Tooltip("The force of the explosion upon collision.")]
        public float ProjectileExplosionForce = 50.0f;

        [Tooltip("An optional delay before the collider is sent off, in case the effect has a pre fire animation.")]
        public float ProjectileColliderDelay = 0.0f;

        [Tooltip("The speed of the collider.")]
        public float ProjectileColliderSpeed = 450.0f;

        [Tooltip("The direction that the collider will go. For example, flame strike goes down, and fireball goes forward.")]
        public Vector3 ProjectileDirection = Vector3.forward;

        [Tooltip("What layers the collider can collide with.")]
        public LayerMask ProjectileCollisionLayers = Physics.AllLayers;

        [Tooltip("Particle systems to destroy upon collision.")]
        public ParticleSystem[] ProjectileDestroyParticleSystemsOnCollision;

        [HideInInspector]
        public FireProjectileCollisionDelegate CollisionDelegate;

        private bool collided;
        GameObject clone;

        private IEnumerator SendCollisionAfterDelay()
        {
            yield return new WaitForSeconds(ProjectileColliderDelay);


            Vector3 dir = ProjectileDirection * ProjectileColliderSpeed;
            dir = ProjectileColliderObject.transform.rotation * dir;
            ProjectileColliderObject.transform.position = new Vector3(transform.position.x + dir.x / 14.0f, transform.position.y + dir.y / 14.0f, transform.position.z + dir.z / 14.0f);
            ProjectileColliderObject.GetComponent<Rigidbody>().velocity = dir;

            clone = Instantiate(MissileModel, new Vector3(transform.position.x + dir.x/14.0f, transform.position.y + dir.y/14.0f, transform.position.z + dir.z/14.0f), transform.rotation);
            
            clone.transform.eulerAngles = new Vector3(
                clone.transform.eulerAngles.z,
                clone.transform.eulerAngles.y + 90,
                clone.transform.eulerAngles.x
            );
            clone.GetComponent<Rigidbody>().velocity = dir;
        }

        private IEnumerator DeleteObjectAfterDelay() {
            yield return new WaitForSeconds(5.0f);

            if (!collided) {
                // stop the projectile
                collided = true;
                Stop();
                // destroy particle systems after a slight delay
                if (ProjectileDestroyParticleSystemsOnCollision != null) {
                    foreach (ParticleSystem p in ProjectileDestroyParticleSystemsOnCollision) {
                        GameObject.Destroy(p, 0.1f);
                    }
                }

                FireBaseScript.CreateExplosion(clone.transform.position, ProjectileExplosionRadius, ProjectileExplosionForce);

                GameObject.Destroy(clone);

                // play collision sound
                if (ProjectileCollisionSound != null) {
                    ProjectileCollisionSound.Play();
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            StartCoroutine(SendCollisionAfterDelay());
            StartCoroutine(DeleteObjectAfterDelay());
        }

        public void HandleCollision(GameObject obj, Collision c)
        {
            if (collided)
            {
                // already collided, don't do anything
                return;
            }

            // stop the projectile
            collided = true;
            Stop();

            // destroy particle systems after a slight delay
            if (ProjectileDestroyParticleSystemsOnCollision != null)
            {
                foreach (ParticleSystem p in ProjectileDestroyParticleSystemsOnCollision)
                {
                    GameObject.Destroy(p, 0.1f);
                }
            }
            GameObject.Destroy(clone);

            // play collision sound
            if (ProjectileCollisionSound != null)
            {
                ProjectileCollisionSound.Play();
            }

            // if we have contacts, play the collision particle system and call the delegate
            if (c.contacts.Length != 0)
            {
                ProjectileExplosionParticleSystem.transform.position = c.contacts[0].point;
                ProjectileExplosionParticleSystem.Play();
                FireBaseScript.CreateExplosion(c.contacts[0].point, ProjectileExplosionRadius, ProjectileExplosionForce);
                if (CollisionDelegate != null)
                {
                    CollisionDelegate(this, c.contacts[0].point);
                }
            }

            //Debug.Log("Collided with some shit");
            //Debug.Log(c.collider.gameObject.name);
            Debug.Log(c.gameObject.name);
            if(c.gameObject.tag.Equals("fighterJet")) {
                GameObject.Destroy(c.gameObject);
                //Debug.Log("Collided with the fighter jet!");
            }
        }
    }
}