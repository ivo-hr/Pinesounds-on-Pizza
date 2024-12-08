using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class ReverbOcclusionCalc : MonoBehaviour
{
    // Add your variables and methods here

    [SerializeField]
    private StudioEventEmitter objectEmitter;
    [SerializeField] 
    private float occlusionIntensity = 0.5f;
    [SerializeField] 
    private float reverbIntensity = 60f;
    [SerializeField]
    private float maxDistance = 100f;
    [SerializeField]
    private GameObject targetListener;
    [SerializeField]
    private LayerMask occlusionLayers;


    void Start()
    {
        //Search for the player
        if (targetListener == null)
        {
            targetListener = GameObject.FindWithTag("Player");
        }

    }

    void Update()
    {
        // Update code here
        IsTargetOccluded();
    }

    void IsTargetOccluded()
    {
        //Use raycast to check if the player is occluded
        RaycastHit hit;
        Vector3 direction = targetListener.transform.position - transform.position;
        if (Physics.Raycast(transform.position, direction, out hit, maxDistance, occlusionLayers))
        {
            //If it's the player, no occlusion or reverb
            if (hit.collider.gameObject == targetListener)
            {
                objectEmitter.SetParameter("Occlusion", 0);
                objectEmitter.SetParameter("Reverb", 0);
            }
            else
            {
                //If it's not the player, apply occlusion and reverb
                objectEmitter.SetParameter("Occlusion", occlusionIntensity);
                objectEmitter.SetParameter("Reverb", reverbIntensity);
            }
        }
        else
        {
            //If there's no occlusion, apply reverb
            objectEmitter.SetParameter("Occlusion", 0);
            objectEmitter.SetParameter("Reverb", reverbIntensity);
        }
    }
}