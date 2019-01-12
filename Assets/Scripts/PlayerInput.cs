using UnityEngine;
using System.Collections;
using Valve.VR;


public class PlayerInput : MonoBehaviour {
    public enum AxisType {
        XAxis,
        ZAxis
    }

    // Controller
    [SerializeField] bool rightHand;
    SteamVR_Input_Sources VR_Controller;
    [SerializeField] SteamVR_Action_Single flyAction;
    [SerializeField] SteamVR_Action_Boolean laserAction;
    [SerializeField] SteamVR_Action_Boolean missileAction;
    [SerializeField] SteamVR_Action_Vibration haptics;

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

    // Thruster
    [FMODUnity.EventRef]
    public string thrusterAudioPath;
    FMOD.Studio.EventInstance thrusterAudio;
    private Rigidbody playerRigidbody;

    // Missile
    public GameObject missilePrefab;
    public GameObject explosionPrefab;
    float missileCooldownTime = 0;
    

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
        
        // Find player's rigidbody
        playerRigidbody = transform.GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        DoLaser();
        DoMissile();
    }

    private void FixedUpdate()
    {
        DoThruster();    
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
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(laserAudio, transform, playerRigidbody);
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
                    hitObject.collider.GetComponent<EnemyPlane>().Damage(laserDamage);
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
        else // ZAxis
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

    // Handles controller input/output, audio, and forces for thrusters
    void DoThruster()
    {
        float flyValue = flyAction.GetAxis(VR_Controller);
        FMOD.Studio.PLAYBACK_STATE thrusterAudioState;
        thrusterAudio.getPlaybackState(out thrusterAudioState);
        if (flyValue > 0.01f)
        {
            if (thrusterAudioState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                thrusterAudio = FMODUnity.RuntimeManager.CreateInstance(thrusterAudioPath);
                thrusterAudio.start();
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(thrusterAudio, transform, playerRigidbody);
            }
            thrusterAudio.setParameterValue("Trigger", flyValue);
            haptics.Execute(0.0f, 0.005f, 50.0f, flyValue, VR_Controller);

            // Calculate thruster force and check vertical bound
            Vector3 thrusterForce = -transform.forward * 10.0f * flyValue;
            if ((transform.position.x >= 0.0f && transform.position.x < 500.0f) &&
                (transform.position.y >= 0.0f && transform.position.y < 500.0f))
            {
                if (transform.position.y <= 70)
                {
                    playerRigidbody.AddForce(thrusterForce);
                }
            }
            else
            {
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, -Mathf.Abs(playerRigidbody.velocity.y), playerRigidbody.velocity.z);
            }
        }
        else
        {
            if (thrusterAudioState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                thrusterAudio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                thrusterAudio.release();
            }
        }
    }

    // Handles missile inputs
    void DoMissile()
    {
        if (missileAction.GetState(VR_Controller))
        {
            if (Time.time > missileCooldownTime + 1.0f)
            {
                missileCooldownTime = Time.time;
                Instantiate(missilePrefab, transform.position + transform.forward * 0.5f, this.transform.rotation*Quaternion.Euler(0, 90, 0));
            }
        }
    }
}