using UnityEngine;
using System.Collections;
using Valve.VR;


public class PlayerInput : MonoBehaviour {
    public enum AxisType {
        XAxis,
        ZAxis
    }


    public bool rightHand;
    public Color color;
    public float thickness = 0.002f;
    public AxisType facingAxis = AxisType.XAxis;
    public float length = 100f;
    public bool showCursor = true;
    private Rigidbody body;
    public float thrust;

    float cooldownTime = 0;
    float cooldownTimeLaser = 0;
    float cooldownTimeLaserExplosion = 0;

    public GameObject missilePrefab;
    public GameObject explosionPrefab;

    // Steam VR trigger action
    public SteamVR_Action_Single flyAction;
    public SteamVR_Action_Boolean shootAction;
    public SteamVR_Action_Boolean missileAction;

    public Transform myParent;
    Rigidbody controllerRB;

    GameObject holder;
    GameObject pointer;
    GameObject cursor;

    [FMODUnity.EventRef]
    public string laserAudioPath;
    FMOD.Studio.EventInstance laserAudio;
    [FMODUnity.EventRef]
    public string thrusterAudioPath;
    FMOD.Studio.EventInstance thrusterAudio;
    FMOD.Studio.ParameterInstance thrusterVol;

    bool audioLaserPlaying = false;
    bool audioThrusterPlaying = false;

    public float laserDamage = 0.01f;
    public AudioSource audioLaser;
    public AudioSource audioThruster;

    Vector3 cursorScale = new Vector3(0.05f, 0.05f, 0.05f);
    float contactDistance = 0f;
    Transform contactTarget = null;

    void SetPointerTransform(float setLength, float setThicknes) {
        //if the additional decimal isn't added then the beam position glitches
        float beamPosition = setLength / (2 + 0.00001f);

        if (facingAxis == AxisType.XAxis) {
            pointer.transform.localScale = new Vector3(setLength, setThicknes, setThicknes);
            pointer.transform.localPosition = new Vector3(beamPosition, 0f, 0f);
            if (showCursor) {
                cursor.transform.localPosition = new Vector3(setLength - cursor.transform.localScale.x, 0f, 0f);
            }
        }
        else {
            pointer.transform.localScale = new Vector3(setThicknes, setThicknes, setLength);
            pointer.transform.localPosition = new Vector3(0f, 0f, beamPosition);

            if (showCursor) {
                cursor.transform.localPosition = new Vector3(0f, 0f, setLength - cursor.transform.localScale.z);
            }
        }
    }

    // Use this for initialization
    void Start() {
        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);

        holder = new GameObject();
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;

        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.transform.parent = holder.transform;
        pointer.GetComponent<MeshRenderer>().material = newMaterial;

        pointer.GetComponent<BoxCollider>().isTrigger = true;
        pointer.AddComponent<Rigidbody>().isKinematic = true;
        pointer.layer = 2;

        if (showCursor) {
            cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cursor.transform.parent = holder.transform;
            cursor.GetComponent<MeshRenderer>().material = newMaterial;
            cursor.transform.localScale = cursorScale;

            cursor.GetComponent<SphereCollider>().isTrigger = true;
            cursor.AddComponent<Rigidbody>().isKinematic = true;
            cursor.layer = 2;
        }
        myParent = transform.parent;
        body = transform.GetComponentInParent<Rigidbody>();
        //SetPointerTransform(length, thickness);

        // Audio setup
        thrusterAudio = FMODUnity.RuntimeManager.CreateInstance(thrusterAudioPath);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(thrusterAudio, transform, body);
        laserAudio = FMODUnity.RuntimeManager.CreateInstance(laserAudioPath);
        laserAudio.getParameter("trigger", out thrusterVol);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(laserAudio, transform, body);

    }

    float GetBeamLength(bool bHit, RaycastHit hit) {
        float actualLength = length;

        //reset if beam not hitting or hitting new target
        if (!bHit || (contactTarget && contactTarget != hit.transform)) {
            contactDistance = 0f;
            contactTarget = null;
        }

        //check if beam has hit a new target
        if (bHit) {
            if (hit.distance <= 0) {

            }
            contactDistance = hit.distance;
            contactTarget = hit.transform;
        }

        //adjust beam length if something is blocking it
        if (bHit && contactDistance < length) {
            actualLength = contactDistance;
        }

        if (actualLength <= 0) {
            actualLength = length;
        }
        thrust = 5.0f;
        return actualLength;
    }

    void Update() {
        // Shoot a laser beam on track pad button press
        audioThruster.volume = flyAction.GetAxis(SteamVR_Input_Sources.Any);
        if ((rightHand && shootAction.GetState(SteamVR_Input_Sources.RightHand) == true)
            || (!rightHand && shootAction.GetState(SteamVR_Input_Sources.LeftHand) == true)) {

            Ray raycast = new Ray(transform.position, transform.forward);
            //Vector3 direction = -raycast.direction;

            RaycastHit hitObject;
            bool rayHit = Physics.Raycast(raycast, out hitObject);
            //GameObject hitGameObject = hitObject.collider.gameObject;
            if (rayHit) {
                Debug.Log("Ray hit!");
                if (hitObject.collider.tag.Equals("fighterJet")) {
                    if (Time.time > cooldownTimeLaser + 5.0f) {
                        cooldownTimeLaser = Time.time;
                        Debug.Log("Hit a fighter jet!!");
                        Debug.Log(hitObject.collider.name);
                        EnemyPlane health = hitObject.collider.GetComponent<EnemyPlane>();
                        health.damage(laserDamage);
                    }
                    if(Time.time > cooldownTimeLaserExplosion + 0.1f) {
                        cooldownTimeLaserExplosion = Time.time;
                        Instantiate(explosionPrefab, hitObject.collider.transform.position, hitObject.collider.transform.rotation);
                    }
                }
            }

            float beamLength = GetBeamLength(rayHit, hitObject);
            SetPointerTransform(beamLength, thickness);

            FMOD.Studio.PLAYBACK_STATE laserAudioState;
            laserAudio.getPlaybackState(out laserAudioState);
            if (laserAudioState != FMOD.Studio.PLAYBACK_STATE.PLAYING) {
                laserAudio.start();
            }
            if (!audioLaserPlaying) {
                audioLaserPlaying = true;
                audioLaser.Play();
            }
        }
        else {
            FMOD.Studio.PLAYBACK_STATE laserAudioState;
            laserAudio.getPlaybackState(out laserAudioState);
            if (laserAudioState == FMOD.Studio.PLAYBACK_STATE.PLAYING) {
                laserAudio.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
            if (audioLaserPlaying) {
                audioLaserPlaying = false;
                audioLaser.Stop();
            }
            pointer.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
            cursor.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        }

        if ((rightHand && missileAction.GetState(SteamVR_Input_Sources.RightHand) == true)
            || (!rightHand && missileAction.GetState(SteamVR_Input_Sources.LeftHand) == true)) {

            if (Time.time > cooldownTime + 1.0f) {

                cooldownTime = Time.time;
                /*Vector3 createVector = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

                Ray raycast = new Ray(transform.position, transform.forward);
                Vector3 missileDirection = raycast.direction;
                Vector3 rayPoint = raycast.GetPoint(10);
                Vector3 relativePos = rayPoint - transform.position;*/

                //Ray raycast = new Ray(transform.position, transform.forward);
                //Vector3 rayPoint = raycast.GetPoint(10);

                Instantiate(missilePrefab, this.transform.position, this.transform.rotation);
                //Instantiate(missilePrefab, rayPoint, this.transform.rotation);
            }
        }
    }

    private void FixedUpdate() {
        float flyValue = flyAction.GetAxis(SteamVR_Input_Sources.Any);
        FMOD.Studio.PLAYBACK_STATE thrusterAudioState;
        thrusterAudio.getPlaybackState(out thrusterAudioState);
        if (flyValue > 0.0f) {
            thrusterVol.setValue(flyValue);
            if (!audioThrusterPlaying) {
                audioThrusterPlaying = true;
                //audioThruster.volume = flyValue;
                audioThruster.Play();
            }
            if (thrusterAudioState != FMOD.Studio.PLAYBACK_STATE.PLAYING) {
                thrusterAudio.start();
            }
            Vector3 forceVector = -transform.forward * 10.0f * flyValue;
            if ((transform.position.x >= 0.0f && transform.position.x < 500.0f) &&
                (transform.position.y >= 0.0f && transform.position.y < 500.0f)) { // Out of bounds calculation
                if (transform.position.y <= 70) {
                    Vector3 newForce = forceVector;
                    //if (body.velocity.x >= 100.0f) newForce.x = 100.0f;
                    //if (body.velocity.y >= 100.0f) newForce.y = 100.0f;
                    //if (body.velocity.z >= 100.0f) newForce.z = 100.0f;
                    body.AddForce(newForce);
                }
            }
            else {
                Debug.Log("Going out of bounds. Bouncing back.");
                Vector3 oldVelocity = body.velocity;
                body.velocity = new Vector3(-oldVelocity.x, -oldVelocity.y, Mathf.Abs(oldVelocity.z));
            }
        }
        else {
            if (audioThrusterPlaying) {
                audioThrusterPlaying = false;
                audioThruster.Stop();
            }
            if (thrusterAudioState == FMOD.Studio.PLAYBACK_STATE.PLAYING) {
                thrusterAudio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

    }
}