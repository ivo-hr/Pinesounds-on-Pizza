using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Numerics;

public class AreaDetector : MonoBehaviour
{
    public EventReference areaSound;
    public EventReference stepSound;
    public Transform player;
    private Rigidbody playerRigidbody;
    public GroundCheck groundCheck;
    public float maxDistance = 100f;
    public float occlusionThreshold = 0.5f;
    public float occlusionIntensity = 0.5f;
    public float ambientVolumeReduction = 10f;
    public float stepVolume = 0.5f;

    private FMOD.Studio.EventInstance areaInstance;
    private FMOD.Studio.EventInstance stepInstance;

    [SerializeField]
    private string currentArea = "None";

    int terrainLayer;
    FMOD.ATTRIBUTES_3D attributes;

    // Start is called before the first frame update
    void Start()
    {
        attributes = RuntimeUtils.To3DAttributes(player.position);
        stepInstance.set3DAttributes(attributes);
        areaInstance.set3DAttributes(attributes);

        stepInstance.setParameterByName("Volume", stepVolume);

        areaInstance = RuntimeManager.CreateInstance(areaSound);
        stepInstance = RuntimeManager.CreateInstance(stepSound);
        areaInstance.start();
        stepInstance.start();

        terrainLayer = LayerMask.GetMask("Terrain");

        playerRigidbody = player.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        attributes = RuntimeUtils.To3DAttributes(player.position);

        FloorDetector();
        StepDetector();
        NearAreaSensor();
    }


    // Cast a ray to the floor to see on what terrain the player is standing
    void FloorDetector()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.position, UnityEngine.Vector3.down, out hit, maxDistance, terrainLayer))
        {
            // Check if the player is standing on a terrain
           switch (hit.collider.tag)
            {
                case "Grass":
                    stepInstance.setParameterByName("Surface", 1);

                    break;

                case "Rock":
                    stepInstance.setParameterByName("Surface", 3);
                    
                    break;

                case "Wood":
                    stepInstance.setParameterByName("Surface",0);

                    break;

                case "Water":
                    stepInstance.setParameterByName("Surface", 5);
                    break;

                case "Sand":
                    stepInstance.setParameterByName("Surface", 4);

                    break;

                default:
                    break;
            }

            currentArea = hit.collider.tag;
        }
    }

    // Check if the player is moving
    bool ismoving = false;
    void StepDetector()
    {
        if (playerRigidbody.velocity.magnitude > 0.1f && groundCheck.isGrounded)
        {
            //play the step sound
            stepInstance.set3DAttributes(attributes);

            if (!ismoving) {
                ismoving = true;
                stepInstance.start();
            }
        }
        else
        {
            if (ismoving)
            {
                stepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                ismoving = false;
            }
        }
    }


    // Detect the area the player is in

    void NearAreaSensor()
    {
        //Translate the current area to FMOD parameters
        string currentAreaCode = AreaTranslate(currentArea);


        //Detect the areas near the player
        Collider[] hitColliders = Physics.OverlapSphere(player.position, maxDistance, terrainLayer);
        //Nearest different areas dictionary
        Dictionary<string, GameObject> nearestAreas = new Dictionary<string, GameObject>();

        foreach (Collider hit in hitColliders)
        {
            //Translate different areas to FMOD parameters
            string areaCode = AreaTranslate(hit.tag);



            if (areaCode != currentAreaCode){



                if (nearestAreas.ContainsKey(areaCode))
                {
                    //Check if the new area is closer than the one in the dictionary
                    if (UnityEngine.Vector3.Distance(player.position, hit.transform.position) < UnityEngine.Vector3.Distance(player.position, nearestAreas[areaCode].transform.position))
                    {
                        nearestAreas[areaCode] = hit.gameObject;
                    }
                }
                else
                {
                    nearestAreas.Add(areaCode, hit.gameObject);
                }
            }   
        }

        

        //Check if the nearest area is occluded
        foreach (KeyValuePair<string, GameObject> area in nearestAreas)
        {

            //Set volume, Direction and Extent parameters
            areaInstance.setParameterByName(area.Key + "Volume", maxDistance - UnityEngine.Vector3.Distance(player.position, area.Value.transform.position) - ambientVolumeReduction);
            areaInstance.setParameterByName(area.Key + "Direction", yawAngle(player.position, area.Value.transform.position) + 180);
            areaInstance.setParameterByName(area.Key + "Distance", UnityEngine.Vector3.Distance(player.position, area.Value.transform.position) * maxDistance / 10000);
            
            
            RaycastHit hit;
            if (Physics.Raycast(player.position + player.up * 1, area.Value.transform.position - (player.position + player.up * 1), out hit, maxDistance, terrainLayer))
            {
                Debug.DrawRay(player.position + player.up * 1, (area.Value.transform.position - (player.position + player.up * 1)).normalized * maxDistance, Color.red, 10f);



                string areaCode = AreaTranslate(area.Value.tag);

                //If the ray hits the same area, no occlusion
                if (areaCode == area.Key)
                {
                    areaInstance.setParameterByName(area.Key + "Occlusion", 0);
                }
                else
                    areaInstance.setParameterByName(area.Key + "Occlusion", occlusionIntensity); 

                    Debug.Log("Occluded " + area.Key);       
                
            }

        }

        //Play the current area sound were the player is

        areaInstance.setParameterByName(currentAreaCode + "Volume", 100 - ambientVolumeReduction);
        areaInstance.setParameterByName(currentAreaCode + "Direction", 0);
        areaInstance.setParameterByName(currentAreaCode + "Distance", 0);
        areaInstance.setParameterByName(currentAreaCode + "Occlusion", 0);


        areaInstance.set3DAttributes(attributes);


    }

    string AreaTranslate(string area)
    {
        switch (area)
        {
            case "Grass":
                return "grass";
            case "Rock":
                return "wind";
            case "Water":
                return "shoreline";
            case "Sand":
                return "grass";
            default:
                return "None";
        }
    }


    float yawAngle(UnityEngine.Vector3 playerPos, UnityEngine.Vector3 areaPos)
    {
        UnityEngine.Vector3 direction = areaPos - playerPos;
        return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }
}



