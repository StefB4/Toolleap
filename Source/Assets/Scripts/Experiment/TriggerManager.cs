/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using Leap.Unity.Attachments;
using Leap.Unity.Interaction;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class TriggerManager : MonoBehaviour
{
    // Experiment Manager
    public ExperimentManager experimentManager;
    
    // Table Manager 
    public TableManager tableManager;
    
    // Config Manager
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // GrabType that is the interaction with trigger (set to pinch, default)  
    public GrabTypes triggerInteractionGrabType;
    
    // Leap Motion Attachment Hands
    public AttachmentHands attachmentHands;
    
    // Interaction with trigger happened 
    private bool triggerInteractionHappened = false;
    
    // Trigger animator 
    private Animator triggerAnimator;
    
    // Trigger animation name 
    public string triggerAnimationName;
    
    // Trigger top position 
    private Vector3 triggerTopPosition;
    
    // Object Transform Tools
    private ObjectTransformHelper transformHelper;

    
    
    // Start is called before the first frame update
    void Start()
    {
        // Find config manager
        configManager = GameObject.FindWithTag(configManagerTag).GetComponent<ConfigManager>();
        
        // Get Object Transform Tools Component 
        transformHelper = GetComponent<ObjectTransformHelper>();

        // Get the Animator 
        triggerAnimator = GetComponentInChildren<Animator>();
        
        // In the case of LeapMotion, subscribe OnContactBegin method to Leap action 
        GetComponent<InteractionBehaviour>().OnContactBegin += OnContactBegin;
        
        // Store trigger top position 
        triggerTopPosition = transformHelper.GetBoundingBox(this.gameObject).center + new Vector3(0,transformHelper.GetBoundingBox(this.gameObject).extents.y,0);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
    
    

    // Update the position of the trigger 
    public void UpdateTriggerTransform() {
        
        Debug.Log("[TriggerManager] Updating trigger transform.");
        
        // Reset trigger transform including scale 
        transform.ResetLocalTransform();
        
        // Get handedness of subject 
        // If left, place trigger to the left, else (right or ambidextrous) place on the right 
        int handednessFactor = 0;
        
        if (configManager.subjectHandedness.ToLower().Contains("left"))
        {
            handednessFactor = -1;
            configManager.triggerIsOnSide = "left";
        }
        else
        {
            handednessFactor = 1;
            configManager.triggerIsOnSide = "right";
        }
        

        // Get table surface center position and rotation and table extents
        Vector3 tableRotationEuler = tableManager.GetTableRotation();
        Vector3 tableSurfaceCenterPosition = tableManager.GetTableSurfaceCenterPosition();
        Vector3 tableExtents = tableManager.GetTableExtents();
       
        // Trigger position non-rotated in world space, dependent on handedness of subject
        // From center of table specified percentage to the right and specified percentage to the front 
        Vector3 triggerPosition = tableSurfaceCenterPosition +
                                  new Vector3(tableExtents.x * configManager.triggerPositionTableFrontPercentage / 100.0f, 0,
                                      tableExtents.z * configManager.triggerPositionTableSidePercentage / 100.0f *
                                      handednessFactor);
        
        //Vector from table surface center to trigger position
        Vector3 triggerOffsetFromTableCenter = triggerPosition - tableSurfaceCenterPosition;
        
        // Rotate offset vector
        Vector3 rotatedOffsetFromTableCenter = Quaternion.Euler(tableRotationEuler) * triggerOffsetFromTableCenter;
        
        // Calculate rotated trigger position 
        Vector3 triggerPositionRotated = tableSurfaceCenterPosition + rotatedOffsetFromTableCenter;
        
        // Get trigger bounds 
        Bounds triggerBounds = transformHelper.GetBoundingBox(this.gameObject);
        
        // Trigger bottom center
        Vector3 triggerBottomCenter = triggerBounds.center - new Vector3(0, triggerBounds.extents.y, 0);
        
        // Trigger offset between pivot and bottom center, non-rotated
        Vector3 triggerPivotBottomCenterOffset = triggerBottomCenter - transform.position;
        
        // Rotate trigger pivot bottom center offset 
        Vector3 rotatedTriggerPivotBottomCenterOffset =
            Quaternion.Euler(tableRotationEuler) * triggerPivotBottomCenterOffset;
        
        // Rotate trigger to fit table rotation
        transform.rotation = Quaternion.Euler(tableRotationEuler);
        
        // Update Trigger Position 
        transform.position = triggerPositionRotated - rotatedTriggerPivotBottomCenterOffset;
        
        // Store trigger top position 
        triggerTopPosition = transformHelper.GetBoundingBox(this.gameObject).center + new Vector3(0,transformHelper.GetBoundingBox(this.gameObject).extents.y,0);
    }


    // STEAMVR
    // What to do when Hand Hovers over Trigger? 
    // Check if grab type signalling trigger interaction is performed 
    // And signal to Experiment Manager that interaction happened 
    // Requires colliders on object 
    private void HandHoverUpdate(Hand hand)
    {
        //Debug.Log("[TriggerManager] SteamVR HandHoverUpdate");
        
        // Check if in addition to hovering hand is also performing grab type that signals trigger interaction 
        if (hand.GetGrabStarting() == triggerInteractionGrabType)
        {
            // Signal to Experiment Manager that there was interaction with the trigger
            triggerInteractionHappened = true;
            Debug.Log("[TriggerManager] Trigger Interaction Happened.");
            
            // Play trigger animation 
            triggerAnimator.Play(triggerAnimationName);
        }
    }
    
    
    // LEAP MOTION 
    // Detect hand contacting button on top, set trigger interaction happened and play animation 
    void OnContactBegin()
    {
        // Make sure that hand is on top of trigger 
        if (Vector3.SqrMagnitude(triggerTopPosition - attachmentHands.attachmentHands[configManager.subjectHandednessLeapFormat].palm.transform.position) <
            configManager.triggerActivationLeapSquaredDistanceFromTopThreshold)
        {
            Debug.Log("[TriggerManager] LeapMotion OnContactBegin");

            // Signal to Experiment Manager that there was interaction with the trigger
            triggerInteractionHappened = true;
            Debug.Log("[TriggerManager] Trigger Interaction Happened.");

            // Play trigger animation 
            triggerAnimator.Play(triggerAnimationName);
        }
    }
   
    
    
    // Get whether trigger interaction happened or not 
    public bool GetInteractionHappened()
    {
        return triggerInteractionHappened;
    }
    
    // Reset Interaction happened
    public void ResetInteractionHappened()
    {
        triggerInteractionHappened = false;
    }
}
