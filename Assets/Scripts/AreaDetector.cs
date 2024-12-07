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

                case "RockFloor":
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
            AdjustEcho();



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
    void AdjustEcho()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.position, Vector3.down, out hit, maxDistance, terrainLayer))
        {
            if (hit.collider.tag == "RockFloor")
            { 

                LayerMask obstacleLayer;
                obstacleLayer = LayerMask.GetMask("Terrain");
                Collider[] nearbyWalls = Physics.OverlapSphere(player.position, maxDistance, obstacleLayer);                    
                float occlusionLevel = 0f;
                float reverbLevel = 0f;

                foreach (Collider wall in nearbyWalls)
                {
                    // Verificar si el objeto tiene el componente AddGeometry (solo rocas o superficies relevantes)
                    AddGeometry geometry = wall.GetComponent<AddGeometry>();
                    if (geometry != null)
                    {
                        // Calcular la distancia del jugador al punto más cercano de cada pared
                        float distance = Vector3.Distance(player.position, wall.ClosestPoint(player.position));
            
                        // Ajustar el nivel de reverb dependiendo de la proximidad de la superficie (entre 0 y 1)
                        reverbLevel += Mathf.Clamp01(1 - (distance / maxDistance));

                        // Verificar si hay obstáculos bloqueando el sonido para ajustar la oclusión
                        if (Physics.Raycast(player.position, wall.ClosestPoint(player.position) - player.position, out hit, maxDistance, obstacleLayer))
                        {
                            // Aumentar el nivel de oclusión según la distancia
                            occlusionLevel += Mathf.Clamp01(1 - (hit.distance / maxDistance));
                        }
                    }
                }

                // Escalar los valores de oclusión y reverb en función de la cantidad de obstáculos
                occlusionLevel = Mathf.Clamp(occlusionLevel, 0, 1);
                reverbLevel = Mathf.Clamp(reverbLevel, 0, 1);

                // Normalizar los valores de oclusión y reverb
                occlusionLevel = Mathf.Clamp(occlusionLevel, 0, 1);
                reverbLevel = Mathf.Clamp(reverbLevel, 0, 1);

                // Ajustar parámetros en FMOD
                stepInstance.setParameterByName("Occlusion", occlusionLevel);
                stepInstance.setParameterByName("Reverb", reverbLevel);

                Debug.Log($"Calculated ReverbLevel: {reverbLevel}");
                Debug.Log($"Calculated Occlusion: {occlusionLevel}");
            }
        }
    }
}
