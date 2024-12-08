using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class StepDetector : MonoBehaviour
{

    public EventReference stepSound;
    public Transform player;
    private Rigidbody playerRigidbody;
    public GroundCheck groundCheck;
    
    public float maxDistance = 30f;
    public float reverbIntensity = 60f;
    public float ambientVolumeReduction = 10f;
    public float stepVolume = 0.5f;

    private FMOD.Studio.EventInstance stepInstance;

    [SerializeField]
    public string currentTile = "None";


    //We must remember if the reverb is applied to the player, so not to remove it in the foreach loop
    public bool applyReverb = false;

    int terrainLayer;
    FMOD.ATTRIBUTES_3D attributes;

    // Start is called before the first frame update
    void Start()
    {
        attributes = RuntimeUtils.To3DAttributes(player.position);

        stepInstance = RuntimeManager.CreateInstance(stepSound);
        stepInstance.set3DAttributes(attributes);

        //stepInstance.start();

        stepInstance.setParameterByName("Volume", stepVolume);

        terrainLayer = LayerMask.GetMask("Terrain");

        playerRigidbody = player.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        attributes = RuntimeUtils.To3DAttributes(player.position);

        FloorDetector();
        MovementDetector();

        if (applyReverb)
            stepInstance.setParameterByName("Reverb", reverbIntensity);
        else
            stepInstance.setParameterByName("Reverb", 0);
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
    void MovementDetector()
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
}