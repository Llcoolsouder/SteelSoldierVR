using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlane : MonoBehaviour {

    Transform target = null; // Player
    [SerializeField]
    float speed = 100.0f;

    enum State { Seek, Straight, Dodge, Cycle, Destroyed };
    State currentState = State.Cycle;

    enum Dodge { Left = 0, Right = 1 };
    Dodge dodgeDir = Dodge.Left;
    Vector3 currentDodgeVec;
    
    // Defines when Plane state changes occur
    float DistToMaintain = 100.0f;
    const float maxDistToTarget = 100.0f;
    const float minHeight = 50.0f;
    const float seekThresholdDist = 50.0f;
    const float minDistToTarget = 10.0f;

    // Defines Plane steering in Cycle state
    enum CycleDir { CounterClockwise=0, Clockwise=1};
    CycleDir cycleDir;
    float previousErr = 0; // For calculating de/dt
    float sumErr = 0;
    [SerializeField]
    float P = -2.0f;
    [SerializeField]
    float I = -0.001f;
    [SerializeField]
    float D = 0.0f;

    // Defines other behavior
    float currentHealth = 100.0f;
    Vector3 shootDir = Vector3.one;
    const float bulletSpread = 1.0f;

    [SerializeField]
    ParticleSystem dirtParticleFX;
    [SerializeField]
    ParticleSystem smokeParticleFX;

    [FMODUnity.EventRef, SerializeField]
    string MachineGunEvent;
    FMOD.Studio.EventInstance MachineGun;

    // Use this for initialization
    void Start () {
        target = AirTrafficControl.airControl.GetTarget();
        cycleDir = (CycleDir)Random.Range((int)0, 2);
    }

    // Update is called once per frame
    void Update () {
        currentState = CheckForStateTransition();

        // Steering is determined by state
        if (currentState == State.Cycle)
        {
            // Keeps plane parallel with the ground
            Quaternion roll = Quaternion.LookRotation(transform.forward, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, roll, 10*Time.deltaTime);
            // PID controller for smooth steering
            float pid = PID();
            GraphDbg.Log("PID", pid);   // DEBUG
            pid = Mathf.Clamp(pid, 0, 2);
            if (cycleDir == CycleDir.Clockwise)
                pid *= -1;

            Vector3 seekDir = PointAtTarget();
            Quaternion yaw = new Quaternion(0, 0, 0, 0); ;
            try {
                yaw = Quaternion.LookRotation(seekDir, transform.up) * Quaternion.Euler(0, 90 * pid, 0);
            }
            catch {
                Debug.Log("Quaternion.LookRotation failed EnemyPlane line 73");
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, yaw, 10 * Time.deltaTime);
        }
        else if (currentState == State.Seek)
        {
            Vector3 seekDir = PointAtTarget();
            Quaternion rotateTo = Quaternion.LookRotation(seekDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotateTo, 10*Time.deltaTime);
        }
        else if (currentState == State.Straight)
        {
            // Planes at 1.5x speed in straight mode
            transform.position += transform.forward * (0.5f * speed) * Time.deltaTime;
            Shoot();
        }
        else if (currentState == State.Dodge)
        {
            transform.position += currentDodgeVec * speed * Time.deltaTime;
            if (dodgeDir == Dodge.Left)
            {
                transform.Rotate(Vector3.forward * 360 * Time.deltaTime, Space.Self);
            }
            else
            {
                transform.Rotate(Vector3.forward * -360 * Time.deltaTime, Space.Self);
            }
        }
        else if (currentState == State.Destroyed)
        {
            // In case plane goes down outside of play area
            if (transform.position.y <= -50.0f)
                GameObject.Destroy(this.gameObject);

            GameObject smoke = Instantiate(smokeParticleFX.gameObject, transform.position, Quaternion.identity);
            Destroy(smoke.gameObject, smokeParticleFX.main.duration + smokeParticleFX.main.startLifetime.constantMax);
            transform.Rotate(Vector3.forward * -360 * Time.deltaTime, Space.Self);
            Quaternion pitch = Quaternion.LookRotation(Vector3.down, transform.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, pitch, Time.deltaTime);
        }

        // Avoid crashing into the ground if you haven't already been destroyed
        if (currentState != State.Destroyed)
        {
            RaycastHit obstacle;
            bool isFront = Physics.Raycast(transform.position, transform.forward, out obstacle, 20);
            if (isFront)
            {
                if (obstacle.transform.tag == "Terrain")
                {
                    transform.Rotate(-750 * Time.deltaTime, 0, 0, Space.Self);
                }
            }
        }

        // Planes will always move forward
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    /*************************
     * Private Methods
     * ***********************/

    // Returns the next state if a transition condition is met
    // Otherwise, it returns the current state
    State CheckForStateTransition()
    {
        if (currentState == State.Destroyed)
            return State.Destroyed;

        if (currentState == State.Cycle)
        {
            // Only AirTrafficController should set planes to Seek Mode
            return State.Cycle;
        }
        else if (currentState == State.Seek)
        {
            if (DistToTarget() <= seekThresholdDist)
            {
                Aim();
                return State.Straight;
            }
            else
                return currentState;
        }
        else if (currentState == State.Straight)
        {
            float d = DistToTarget();
            if (d <= minDistToTarget || d >= maxDistToTarget)
            {
                bool isLeft = TargetIsLeft();
                currentDodgeVec = isLeft ? -transform.right : transform.right;
                dodgeDir = isLeft ? Dodge.Left : Dodge.Right;
                return State.Dodge;
            }  
            else
                return currentState;
        }
        else if (currentState == State.Dodge)
        {
            if (DistToTarget() >= DistToMaintain)
            {
                AirTrafficControl.airControl.ResetSeeker();
                return State.Cycle;
            }
            else
                return currentState;
        }
        else
        {
            return State.Cycle;
        }
    }

    // Returns the distance to the target
    float DistToTarget()
    {
        return (target.position - transform.position).magnitude;
    }

    // Returns True if the target is on the left or false if the target is on the right
    bool TargetIsLeft()
    {
        Vector3 toTarget = target.position - transform.position;
        float direction = Vector3.Cross(transform.forward, toTarget).z;
        return direction >= transform.position.z; 
    }

    void ResetPID()
    {
        previousErr = 0;
        sumErr = 0;
    }

    // Computes new values for all PID variables and
    // returns the value of the controller's output
    float PID()
    {
        float err = DistToTarget() - DistToMaintain;
        float derr = (err - previousErr) / Time.deltaTime;
        previousErr = err;
        sumErr += (err * Time.deltaTime);
        GraphDbg.Log("Error", err); // DEBUG
        return (P * err) + (I * sumErr) - (D * derr);
    }

    // Returns a unit Vector3 that points to the target
    Vector3 PointAtTarget()
    {
        //float targetHeight = Mathf.Max(target.position.y, minHeight);
        //Vector3 projectedTarget = new Vector3(target.position.x, targetHeight, target.position.z);
        //Vector3 seekDir = (target.position - transform.position);
        Vector3 seekDir = (new Vector3(target.position.x, target.position.y + 10.0f, target.position.z) - transform.position);
        return seekDir / seekDir.magnitude;
    }

    void Shoot()
    {
        Vector3 bulletVar = new Vector3(Random.Range(0, bulletSpread),
                                        Random.Range(0, bulletSpread),
                                        Random.Range(0, bulletSpread));

        //RaycastHit hit;
        //bool bulletHit = Physics.Raycast(transform.position, shootDir + bulletVar, out hit);

        Ray raycast = new Ray(transform.position + bulletVar, shootDir);
        Debug.DrawRay(transform.position, shootDir, Color.yellow);
        RaycastHit hit;
        bool bulletHit = Physics.Raycast(raycast, out hit);

        //bool bulletHit = Physics.Raycast(transform.position, shootDir, out hit);
        if (bulletHit)
        {
            Debug.Log("BULLET HIT! " + hit.collider.name);
            if (hit.collider.tag.Equals("Player"))
            {
                //Insert player's damage function here
                Debug.Log("Player hit!");
                hit.collider.gameObject.GetComponent<PlayerStatus>().DamagePlayer(1.0f);
            }
            if (hit.collider.tag.Equals("Terrain"))
            {
                Debug.Log("Terrain hit!");
                GameObject dirt = Instantiate(dirtParticleFX.gameObject, hit.point, Quaternion.identity);
                Destroy(dirt.gameObject, dirtParticleFX.main.duration + dirtParticleFX.main.startLifetime.constantMax);
            }
        }
      
        MachineGun = FMODUnity.RuntimeManager.CreateInstance(MachineGunEvent);
        MachineGun.start();
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(MachineGun, transform, GetComponent<Rigidbody>());
        MachineGun.release();
    }

    // Returns a Vector3 describing the direction to shoot in
    // It will aim a little bit in front of the target
    Vector3 Aim()
    {
        Vector3 toTarget = target.position - transform.position;
        //shootDir = transform.position + toTarget; // + Vector3.Scale(toTarget, new Vector3(0.9f, 1, 0.9f));
        shootDir = toTarget;
        return shootDir;
    }

    private void OnDestroy()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentState == State.Destroyed)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    /* ***********************
     * Public Methods 
     * (mostly for use by the AirTrafficController)
     * ***********************/

    public void SetDistToTarget(float d)
    {
        DistToMaintain = d;
    }

    public void SetToSeek()
    {
        currentState = State.Seek;
    }

    public void damage(float damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            AirTrafficControl.airControl.DestroyPlane(this);
            currentState = State.Destroyed;
        }
    }
}
