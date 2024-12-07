using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

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
    public LayerMask obstacleLayer;

    private FMOD.Studio.EventInstance areaInstance;
    private FMOD.Studio.EventInstance stepInstance;

    int terrainLayer;
    FMOD.ATTRIBUTES_3D attributes;

    // Start is called before the first frame update
    void Start()
    {
        attributes = RuntimeUtils.To3DAttributes(player.position);
        stepInstance.set3DAttributes(attributes);

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
        

        FloorDetector();
        StepDetector();
    }


    // Cast a ray to the floor to see on what terrain the player is standing
    void FloorDetector()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.position, Vector3.down, out hit, maxDistance, terrainLayer))
        {
            Debug.Log(hit.collider.tag);
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
        }
    }

    // Check if the player is moving
    bool ismoving = false;
    void StepDetector()
    {
        if (playerRigidbody.velocity.magnitude > 0.1f && groundCheck.isGrounded)
        {
            //play the step sound
            attributes = RuntimeUtils.To3DAttributes(player.position);
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
}
