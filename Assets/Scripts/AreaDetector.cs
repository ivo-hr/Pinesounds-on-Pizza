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
    private string currentTile = "None";
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

        areaInstance = RuntimeManager.CreateInstance(areaSound);
        stepInstance = RuntimeManager.CreateInstance(stepSound);
        areaInstance.start();
        stepInstance.start();

        stepInstance.setParameterByName("Volume", stepVolume);

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

            currentTile = hit.collider.tag;
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
        currentArea = AreaTranslate(currentTile);


        //Detect the areas near the player
        Collider[] hitColliders = Physics.OverlapSphere(player.position, maxDistance, terrainLayer);
        //Nearest different areas dictionary
        Dictionary<string, GameObject> nearestAreas = new Dictionary<string, GameObject>();

        foreach (Collider hit in hitColliders)
        {
            //Translate different areas to FMOD parameters
            string areaCode = AreaTranslate(hit.tag);



            if (areaCode != currentTile){

                //Distance between player and area hit
                float distance = UnityEngine.Vector3.Distance(player.position, hit.gameObject.transform.position);

                if (nearestAreas.ContainsKey(areaCode))
                {
                    //Check if the new area is closer than the one in the dictionary
                    if (distance < UnityEngine.Vector3.Distance(player.position, nearestAreas[areaCode].transform.position))
                    {
                        nearestAreas[areaCode] = hit.gameObject;
                    }
                }
                else if (distance <= maxDistance)
                {
                    nearestAreas.Add(areaCode, hit.gameObject);
                }
            }   
        }



        //Check if the nearest area is occluded
        foreach (KeyValuePair<string, GameObject> area in nearestAreas)
        {
            //Get real distance between player and area
            float distance = UnityEngine.Vector3.Distance(player.position, area.Value.transform.position);
            //Set volume, Direction and Extent parameters
            areaInstance.setParameterByName(area.Key + "Volume", maxDistance - distance - ambientVolumeReduction);
            areaInstance.setParameterByName(area.Key + "Direction", yawAngle(player.position, area.Value.transform.position) + 180);
            areaInstance.setParameterByName(area.Key + "Distance", distance * maxDistance / 10000);
            
            //Get the top center point of the tile
            UnityEngine.Vector3 topCenter = GetTopCenterPoint(area.Value);

            //Get the point at eye level of the tile relative to the player
            UnityEngine.Vector3 pointAtPlayerHeight = new UnityEngine.Vector3(area.Value.transform.position.x, 
                                                                                player.position.y + player.up.y * 1, 
                                                                                area.Value.transform.position.z
                                                                                );
            //Get distance between player and point at player height
            float distanceToPlayerHeight = UnityEngine.Vector3.Distance(player.position + player.up * 1, pointAtPlayerHeight);

            
            RaycastHit hit;
            if (Physics.Raycast(player.position + player.up * 1, pointAtPlayerHeight - (player.position + player.up * 1), out hit, distanceToPlayerHeight, terrainLayer))
            {
                

                //What the ray hits
                string rayHitArea = AreaTranslate(hit.collider.tag);

                

                //If the ray hits the same area, no occlusion
                if (rayHitArea == area.Key)
                {
                    areaInstance.setParameterByName(area.Key + "Occlusion", 0);

                    Debug.Log("Not Occluded " + area.Key);
                    Debug.DrawRay(player.position + player.up * 1, ( pointAtPlayerHeight - (player.position + player.up * 1)).normalized * distanceToPlayerHeight, Color.blue, 10f);
                }
                //If the ray hits a different area, occlusion!
                else
                {
                    areaInstance.setParameterByName(area.Key + "Occlusion", occlusionIntensity); 

                    Debug.Log("Occluded " + area.Key + " by " + rayHitArea);   
                    Debug.DrawRay(player.position + player.up * 1, ( pointAtPlayerHeight - (player.position + player.up * 1)).normalized * distanceToPlayerHeight, Color.red, 10f);
                }
    
                
            }
            //If the ray doesn't hit anything, no occlusion! 
            else
            {
                Debug.DrawRay(player.position + player.up * 1, ( pointAtPlayerHeight - (player.position + player.up * 1)).normalized * distanceToPlayerHeight, Color.green, 10f);
                areaInstance.setParameterByName(area.Key + "Occlusion", 0);
                Debug.Log("Not Occluded " + area.Key);
            }

        }

        //Play the current area sound were the player is

        areaInstance.setParameterByName(currentArea + "Volume", 100 - ambientVolumeReduction);
        areaInstance.setParameterByName(currentArea + "Direction", 0);
        areaInstance.setParameterByName(currentArea + "Distance", 0);
        areaInstance.setParameterByName(currentArea + "Occlusion", 0);


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


    public static float yawAngle(UnityEngine.Vector3 playerPos, UnityEngine.Vector3 areaPos)
    {
        UnityEngine.Vector3 direction = areaPos - playerPos;
        return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }



    public static UnityEngine.Vector3 GetTopCenterPoint(GameObject cube)
    {
        // Get the collider of the GameObject
        Collider cubeCollider = cube.GetComponent<Collider>();
        
        if (cubeCollider != null)
        {
            // Calculate the top-center point
            UnityEngine.Vector3 topCenter = cubeCollider.bounds.center + UnityEngine.Vector3.up * cubeCollider.bounds.extents.y;
            return topCenter;
        }
        
        Debug.LogWarning("No collider found on the provided GameObject.");
        return UnityEngine.Vector3.zero; // Return zero vector if no collider is found
    }
}




