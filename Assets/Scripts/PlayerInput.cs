using UnityEngine;
using System.Collections;
using Valve.VR;


public class PlayerInput : MonoBehaviour {
    public enum AxisType {
        XAxis,
        ZAxis
    }

    // Controller
    public bool rightHand;
    SteamVR_Input_Sources VR_Controller;
    public SteamVR_Action_Single flyAction;
    public SteamVR_Action_Boolean laserAction;
    public SteamVR_Action_Boolean missileAction;

    // Laser
    bool shootingLaser = false;
    [FMODUnity.EventRef]
    public string laserAudioPath;
    FMOD.Studio.EventInstance laserAudio;
    public Color color;
    public float laserThickness = 0.02f;
    public AxisType facingAxis = AxisType.XAxis;
    public float laserLength = 100f;
    public bool showCursor = true;
    float laserCooldownTime = 0;
    public float laserDamage = 0.01f;
    GameObject laserGraphic;

    private Rigidbody body;

    public GameObject missilePrefab;
    public GameObject explosionPrefab;
    float cooldownTime = 0;
    
    
    [FMODUnity.EventRef]
    public string thrusterAudioPath;
    FMOD.Studio.EventInstance thrusterAudio;
    FMOD.Studio.ParameterInstance thrusterVol;


    // Use this for initialization
    void Start() {
        // Specify which controller
        VR_Controller = rightHand ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand;

        // Instantiate laser graphic
        Material laserMaterial = new Material(Shader.Find("Unlit/Color"));
        laserMaterial.SetColor("_Color", color);
        laserGraphic = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        laserGraphic.transform.parent = this.transform;
        laserGraphic.GetComponent<MeshRenderer>().material = laserMaterial;
        laserGraphic.GetComponent<CapsuleCollider>().isTrigger = true;
        laserGraphic.AddComponent<Rigidbody>().isKinematic = true;
        laserGraphic.layer = 2;
        
        body = transform.GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        // Shoot laser
        DoLaser();

        // Shoot missiles
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
        thrusterVol.setValue(flyValue);
        if (flyValue > 0.0f) {
            if (thrusterAudioState != FMOD.Studio.PLAYBACK_STATE.PLAYING) {
                thrusterAudio = FMODUnity.RuntimeManager.CreateInstance(thrusterAudioPath);
                thrusterAudio.start();
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(thrusterAudio, transform, body);
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
            if (thrusterAudioState == FMOD.Studio.PLAYBACK_STATE.PLAYING) {
                thrusterAudio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

    }

    // Handles input actions, graphics, and audio for the laser
    void DoLaser()
    {   
        // Sound and controller input
        FMOD.Studio.PLAYBACK_STATE laserAudioState = 0;
        laserAudio.getPlaybackState(out laserAudioState);
        if (laserAction.GetLastStateDown(VR_Controller))
        {
            shootingLaser = true;
            if (laserAudioState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                laserAudio = FMODUnity.RuntimeManager.CreateInstance(laserAudioPath);
                laserAudio.start();
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(laserAudio, transform, body);
            }
        }
        else if (laserAction.GetLastStateUp(VR_Controller))
        {
            shootingLaser = false;
            laserAudio.getPlaybackState(out laserAudioState);
            if (laserAudioState != FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                laserAudio.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                laserAudio.release();
            }
        }

        // Graphics and damage
        if (shootingLaser)
        {
            Ray raycast = new Ray(transform.position, transform.forward);
            RaycastHit hitObject;
            bool rayHit = Physics.Raycast(raycast, out hitObject);
            if (rayHit && hitObject.collider.tag.Equals("fighterJet"))
            {
                if (Time.time > laserCooldownTime + 0.1f)
                {
                    laserCooldownTime = Time.time;
                    hitObject.collider.GetComponent<EnemyPlane>().damage(laserDamage);
                    Instantiate(explosionPrefab, hitObject.collider.transform.position, hitObject.collider.transform.rotation);
                }
            }
            float beamLength = CalculateLaserGraphicLength(rayHit, hitObject);
            SetLaserGraphicTransform(beamLength, laserThickness);
        }
        else
        {
            laserGraphic.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    // Sets the position, scale, and rotation of the laser's graphic
    void SetLaserGraphicTransform(float setLength, float setThickness)
    {
        if (facingAxis == AxisType.XAxis)
        {
            laserGraphic.transform.localRotation = Quaternion.Euler(90, 0, 0);   // This line has not been tested
            laserGraphic.transform.localScale = new Vector3(setLength, setThickness, setThickness);
            laserGraphic.transform.localPosition = new Vector3(setLength, 0f, 0f);
        }
        else
        {
            laserGraphic.transform.localRotation = Quaternion.Euler(90, 0, 0);
            laserGraphic.transform.localScale = new Vector3(setThickness, setLength, setThickness);
            laserGraphic.transform.localPosition = new Vector3(0f, 0f, setLength);
        }
    }

    // Calculates and returns the correct length for the laser's graphic
    float CalculateLaserGraphicLength(bool bHit, RaycastHit hit)
    {
        if (bHit)
        {
            return hit.distance;
        }
        else
        {
            return laserLength;
        }
    }
}