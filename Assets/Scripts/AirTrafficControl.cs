using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirTrafficControl : MonoBehaviour {

    public static AirTrafficControl airControl;
    //int numberOfClassObjects = //GameObject.FindObjectsOfType(typeof(EnemyPlane)).Length;
    public int totalPlanes = 20;
    public int planeSpawnPer = 5;
    public int planeCount = 0;
    [SerializeField]
    GameObject Plane;
    [SerializeField]
    Transform target;

    List<EnemyPlane> allPlanes;
    EnemyPlane seekingPlane = null;

	// Use this for initialization
	void Awake () {
        enabled = false;
        if (airControl == null)
            airControl = this;
        else
            UnityEngine.Debug.LogWarning("WARNING: There are two or more AirTrafficControllers.");

        allPlanes = new List<EnemyPlane>();
        
        StartCoroutine(SpawnPlanes(planeSpawnPer));
        //planeCount += 5;
    }

    void Start()
    {
        enabled = true;
    }

    // Update is called once per frame
    void Update () {
		if (seekingPlane == null && allPlanes.Count >= 1)
        {
            seekingPlane = allPlanes[Random.Range(0, allPlanes.Count-1)];
            seekingPlane.SetToSeek();
            Debug.Log("Set seek");
        }

        if ((planeCount < (totalPlanes - planeSpawnPer)) && allPlanes.Count < 10)
        {
            StartCoroutine(SpawnPlanes(planeSpawnPer));
            //planeCount += 5;
        }

        // DEBUG: MAKE SURE THIS GETS REMOVED
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            EnemyPlane plane = allPlanes[Random.Range(0, allPlanes.Count - 1)];
            plane.damage(150.0f);  //Should kill plane immediately
        }
        // DEBUG: MAKE SURE THIS GETS REMOVED
    }

    public Transform GetTarget()
    {
        return target;
    }

    IEnumerator SpawnPlanes(int numPlanes)
    {
        GameObject planeObj = null;
        EnemyPlane plane = null;
        planeCount += numPlanes;
        for (int i = 0; i < numPlanes; i++)
        {
            planeObj = Instantiate(Plane, transform.position, Quaternion.identity);
            plane = planeObj.GetComponent<EnemyPlane>();
            plane.SetDistToTarget(Random.Range(90.0f, 200.0f));
            allPlanes.Add(plane);
            yield return new WaitForSeconds(2);
        }
        enabled = true;
    }

    public void ResetSeeker()
    {
        seekingPlane = null;
    }

    public void setPlaneData(int planecount, int totalplanes, int spawnper) {
        planeCount = planecount;
        totalPlanes = totalplanes;
        planeSpawnPer = spawnper;
    }

    public void DestroyPlane(EnemyPlane plane)
    {
        allPlanes.Remove(plane);
    }
}
