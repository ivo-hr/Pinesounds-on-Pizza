using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AreaDetector : MonoBehaviour
{
    public EventReference areaSound;
    public Transform player;

    public float maxDistance = 100f;
    public float occlusionIntensity = 0.5f;
    public float ambientVolumeReduction = 10f;

    //Reference to the StepDetector script to activate reverb
    public StepDetector stepDetector;
    
    //We must remember if the reverb is applied to the player, so not to remove it in the foreach loop
    private bool reverbApplied = false;


    private FMOD.Studio.EventInstance areaInstance;


    [SerializeField]
    private string currentArea = "None";


    int terrainLayer;
    FMOD.ATTRIBUTES_3D attributes;

    // Start is called before the first frame update
    void Start()
    {
        attributes = RuntimeUtils.To3DAttributes(player.position);
        
        areaInstance = RuntimeManager.CreateInstance(areaSound);

        areaInstance.set3DAttributes(attributes);

        areaInstance.start();

        terrainLayer = LayerMask.GetMask("Terrain");
    }

    // Update is called once per frame
    void Update()
    {
        attributes = RuntimeUtils.To3DAttributes(player.position);

        NearAreaSensor();
    }

    // Detect the area the player is in

    void NearAreaSensor()
    {
        //Use the stepDetector script to get the current tile
        //Translate the current area to FMOD parameters
        currentArea = AreaTranslate(stepDetector.currentTile);


        //Detect the areas near the player
        Collider[] hitColliders = Physics.OverlapSphere(player.position, maxDistance, terrainLayer);
        //Nearest different areas dictionary
        Dictionary<string, GameObject> nearestAreas = new Dictionary<string, GameObject>();

        foreach (Collider hit in hitColliders)
        {
            //Translate different areas to FMOD parameters
            string areaCode = AreaTranslate(hit.tag);



            if (areaCode != currentArea){

                //Distance between player and area hit
                float distance = Vector3.Distance(player.position, hit.gameObject.transform.position);

                if (nearestAreas.ContainsKey(areaCode))
                {
                    //Check if the new area is closer than the one in the dictionary
                    if (distance < Vector3.Distance(player.position, nearestAreas[areaCode].transform.position))
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


        //Set the reverbApplied to false to remove reverb if the player is not occluded
        reverbApplied = false;
        //Check if the nearest area is occluded
        foreach (KeyValuePair<string, GameObject> area in nearestAreas)
        {
            //Get real distance between player and area
            float distance = Vector3.Distance(player.position, area.Value.transform.position);
            //Set volume, Direction and Extent parameters
            areaInstance.setParameterByName(area.Key + "Volume", maxDistance - distance - ambientVolumeReduction);
            areaInstance.setParameterByName(area.Key + "Direction", yawAngle(player.position, area.Value.transform.position) + 180);
            areaInstance.setParameterByName(area.Key + "Distance", distance * maxDistance / 10000);
            
            //Get the top center point of the tile
            Vector3 topCenter = GetTopCenterPoint(area.Value);

            //Get the point at eye level of the tile relative to the player
            Vector3 pointAtPlayerHeight = new Vector3(area.Value.transform.position.x, 
                                                                                player.position.y + player.up.y * 1, 
                                                                                area.Value.transform.position.z
                                                                                );
            //Get distance between player and point at player height
            float distanceToPlayerHeight = Vector3.Distance(player.position + player.up * 1, pointAtPlayerHeight);

            
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
                
                    //We can suppose the player is near the occluded area, so reverberation is applied to steps depending on the occlusion intensity
                    if (!reverbApplied)
                    {
                        stepDetector.applyReverb = true;
                        reverbApplied = true;
                    }
                
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

        //Set the reverb to false if the player is not occluded (no reverberation)
        if (!reverbApplied)
        {
            stepDetector.applyReverb = false;
        }


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


    public static float yawAngle(Vector3 playerPos, Vector3 areaPos)
    {
        Vector3 direction = areaPos - playerPos;
        return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }



    public static Vector3 GetTopCenterPoint(GameObject cube)
    {
        // Get the collider of the GameObject
        Collider cubeCollider = cube.GetComponent<Collider>();
        
        if (cubeCollider != null)
        {
            // Calculate the top-center point
            Vector3 topCenter = cubeCollider.bounds.center + Vector3.up * cubeCollider.bounds.extents.y;
            return topCenter;
        }
        
        Debug.LogWarning("No collider found on the provided GameObject.");
        return Vector3.zero; // Return zero vector if no collider is found
    }
}




