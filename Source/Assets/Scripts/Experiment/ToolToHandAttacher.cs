/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leap.Unity.Attachments;
using Leap.Unity.Interaction;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ToolToHandAttacher : MonoBehaviour
{
    // Config Manager
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // Tool Manager
    public ToolManager toolManager;
    
    // Tool Info 
    private ToolInfo toolInfo;
    
    // SteamVR Skeleton Poser 
    private SteamVR_Skeleton_Poser poser;

    // Leap Motion Attachment Hands
    public AttachmentHands attachmentHands;
    
    // Leap Hand Attach Point to move towards when tool is moved out of hand 
    private GameObject attachPointLeapMoveTowards;
    
    // Tool Details
    private ToolManager.ToolDetails toolDetails;
    
    // Original parent of this tool 
    private Transform originalParent;
    
    // Setup needed? 
    private bool isSetup; 
    
    // ** Debug
    // private GameObject sphereOne;
    // private GameObject sphereTwo;
     

    // Start is called before the first frame update
    void Start()
    {
        // Get Config Manager
        configManager = GameObject.FindWithTag(configManagerTag).GetComponent<ConfigManager>();
        
        // Get ToolInfo 
        toolInfo = GetComponent<ToolInfo>();
     
        // In the case of LeapMotion, subscribe to methods 
        GetComponent<InteractionBehaviour>().OnHoverStay += OnHoverStay;
        GetComponent<InteractionBehaviour>().OnGraspedMovement += OnGraspedMovement;
        GetComponent<InteractionBehaviour>().OnGraspStay += OnGraspStay;
        GetComponent<InteractionBehaviour>().OnGraspBegin += OnGraspBegin;
        GetComponent<InteractionBehaviour>().OnGraspEnd += OnGraspEnd;
        GetComponent<InteractionBehaviour>().OnContactBegin += OnContactBegin;
        
        // In the case of SteamVR, subscribe to methods
        GetComponent<Throwable>().onPickUp.AddListener(OnPickUp);
        
        // In the case of SteamVR get Skeleton Poser Component 
        poser = GetComponent<SteamVR_Skeleton_Poser>();
        
        // Get Parent 
        originalParent = this.transform.parent;

        // Create New Attach Point to move towards for Leap Grabs 
        attachPointLeapMoveTowards = new GameObject();
        
        
        // Debug 
        /*
        sphereOne = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereOne.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
        sphereOne.GetComponent<Collider>().enabled = false;
        sphereTwo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereTwo.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
        sphereTwo.GetComponent<Collider>().enabled = false;
        */ 
    }

    
    
    // Set configuration
    // Run this in update to guarantee, that tool manager has everything setup already
    void Setup()
    {
        // Get tool Details
        toolDetails = toolManager.GetToolDetailsFromToolId(toolInfo.toolId);
        
        // Set Leap Attach Point Move towards point name
        attachPointLeapMoveTowards.name = toolDetails.toolNameFromGameObject + "LeapMoveTowardsPoint";

    }
    
    

    void Update()
    {
        // Setup if not done already; setup 
        if (!isSetup)
        {
            Setup();
            isSetup = true;
        }
        
    }
    
  
    
    
    // ##################### 
    // ## STEAMVR METHODS 
    // #####################
    
    
    // SteamVR 
    void OnPickUp()
    {
        // Update config manager, is a little faster than OnAttachedToHand 
        configManager.isToolCurrentlyAttachedToHand = true;
        
    }
    
    // SteamVR
    // Tool attached to hand 
    void OnAttachedToHand(Hand hand)
    {
      
    }

    // SteamVR 
    // Tool detached from hand
    void OnDetachedFromHand(Hand hand)
    {
        // Update config manager
        configManager.isToolCurrentlyAttachedToHand = false;
    }
    
    // STEAMVR
    // Hand hover over tool starts
    void OnHandHoverBegin(Hand hand)
    {
        // Unfreeze tool that was freezed at start of displaying 
        // To enable moving object with hand 
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        
    }
    
    // STEAMVR
    // Indicates that hand is hovering over object 
    // Prepare hand pose update 
    void HandHoverUpdate(Hand hand)
    {
        // Prepare hand for updated hand pose SteamVR 
        // Make sure mode is not Leap and hand is correct one to prevent second hand interfering with selected attachment point 
        if (!configManager.isUsingLeap && hand.handType == configManager.subjectHandednessSteamVrFormat)
        {
            PrepareUpdatedHandPoseSteamVr(hand);
        }
    }
    
    // STEAMVR 
    // Update Hand Skeletal Poser depending on closest attachment point and hand orientation  
    void PrepareUpdatedHandPoseSteamVr(Hand hand)
    {
        // Get the position of the hand's palm 
        Vector3 middleMetacarpalPosition = hand.skeleton.GetBonePosition((int)SteamVR_Skeleton_JointIndexEnum.middleMetacarpal);
        Vector3 middleProximalPosition = hand.skeleton.GetBonePosition((int)SteamVR_Skeleton_JointIndexEnum.middleProximal);
        Vector3 palmPosition = (middleMetacarpalPosition + middleProximalPosition) / 2;
        
        // Get closest attachment point's transform
        Transform closestAttachmentPointTransform =  toolManager.GetClosestAttachmentPointSteamVr(palmPosition, toolInfo.toolId);
        
        // Save to ConfigManager
        configManager.currentClosestToolAttachmentPointTransform = closestAttachmentPointTransform;
        
        // Get Tool Attachment Point Info of Point 
        ToolAttachmentPointInfo tapInfo = closestAttachmentPointTransform.gameObject.GetComponent<ToolAttachmentPointInfo>();
        
        
        // Determine whether hand has different pose depending on side of point of interest (i.e. handle / effector)
        if (!tapInfo.poseIsDependentOnPointOfInterestSide)
        {
            // Pose is the same independent of position of point of interest 
            
            
            // Update hand skeleton poser with pose id 
            poser.blendingBehaviours[0].pose = tapInfo.attachmentPointIdPointOfInterestIndependent;
        }
        
        // Attachment pose is dependent on where the rest of the tool is located relative to the hand 
        else
        {
            // Pose is dependent on position of point of interest 

            // Get position of point of interest for distance compare
            Vector3 pointOfInterest = new Vector3(0,0,0);
            
            // Point of interest is effector, use center
            if (tapInfo.pointOfInterestIsEffector)
            {
                pointOfInterest = toolDetails.toolEffectorCollider.bounds.center;
            }

            // Point of interest is handle, use center
            else
            {
                pointOfInterest = toolDetails.toolHandleCollider.bounds.center; 
            }

            
            // Calculate distance depending on used hand 
            if (hand.handType == SteamVR_Input_Sources.RightHand)
            {
                // Hand is right hand, i.e. thumb is left and pinky right 

                // Get squared distance (for faster computation) between index proximal phalanges and point of interest 
                float indexPoiDistance =
                    (hand.skeleton.GetBonePosition((int) SteamVR_Skeleton_JointIndexEnum.indexProximal) -
                     pointOfInterest).sqrMagnitude;
                
                // Get squared distance between pinky proximal phalanges and point of interest 
                float pinkyPoiDistance =
                    (hand.skeleton.GetBonePosition((int) SteamVR_Skeleton_JointIndexEnum.pinkyProximal) -
                     pointOfInterest).sqrMagnitude;

                // Check whether poi is closer to index or pinky
                if (indexPoiDistance < pinkyPoiDistance)
                {
                    // Poi is to the left of index 
                    
                    // Use Id of point of interest being to the left to update pose 
                    poser.blendingBehaviours[0].pose = tapInfo.attachmentPointIdPointOfInterestLeft;
                }
                else
                {
                    // Poi is to the right of pinky
                    
                    // Use Id of point of interest being to the right to update pose 
                    poser.blendingBehaviours[0].pose = tapInfo.attachmentPointIdPointOfInterestRight;

                }
            }
            
            else
            {
                // Left hand 

                // Get squared distance (for faster computation) between index proximal phalanges and point of interest 
                float indexPoiDistance =
                    (hand.skeleton.GetBonePosition((int) SteamVR_Skeleton_JointIndexEnum.indexProximal) -
                     pointOfInterest).sqrMagnitude;

                // Get squared distance between pinky proximal phalanges and point of interest 
                float pinkyPoiDistance =
                    (hand.skeleton.GetBonePosition((int) SteamVR_Skeleton_JointIndexEnum.pinkyProximal) -
                     pointOfInterest).sqrMagnitude;

                // Check whether poi is closer to index or pinky
                if (indexPoiDistance < pinkyPoiDistance)
                {
                    // Poi is to the right of index 

                    // Use Id of point of interest being to the right to update pose 
                    poser.blendingBehaviours[0].pose = tapInfo.attachmentPointIdPointOfInterestRight;
                }
                else
                {
                    // Poi is to the left of pinky

                    // Use Id of point of interest being to the left to update pose 
                    poser.blendingBehaviours[0].pose = tapInfo.attachmentPointIdPointOfInterestLeft;

                }
            }
        }
        
        
        // ** Debug
        // sphereOne.transform.position = closestAttachmentPointTransform.position;
        // sphereOne.transform.position = hand.skeleton.GetBonePosition((int) SteamVR_Skeleton_JointIndexEnum.indexProximal);
        // sphereTwo.transform.position = hand.skeleton.GetBonePosition((int) SteamVR_Skeleton_JointIndexEnum.pinkyProximal);
        
    }

    
    
    // ########################
    // ## LEAPMOTION METHODS 
    // ########################
    
    // LeapMotion 
    // Hand Grasp starts
    // Set constraints and attach hand to closest attach point and update config manager 
    void OnGraspBegin()
    {
        // Position of palm depending on handedness 
        Transform palmTransform = attachmentHands.attachmentHands[configManager.subjectHandednessLeapFormat].palm.transform;
        
        // Get closest attachment point's transform
        Transform closestAttachmentPointTransformLeap = toolManager.GetClosestAttachmentPointLeapMotion(palmTransform.position, toolInfo.toolId);
        
        // Disable gravity and prevent additional rotation of tool 
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation; // Still allows for positional changes, aka collision is possible 
        GetComponent<Rigidbody>().useGravity = false;

        // Calculate the offset from the tool's pivot to the attachment point and transform tool such that hand attaches to attachpoint
        Vector3 pivotAttachPointOffset = closestAttachmentPointTransformLeap.position - transform.position;
        transform.position = palmTransform.position - pivotAttachPointOffset;
        
        // Attach the tool to the attachment hands 
        this.transform.SetParent(palmTransform);
       
        // Set point that tool should move towards, if it is pushed out of hand due to collision
        attachPointLeapMoveTowards.transform.position = transform.position;
        attachPointLeapMoveTowards.transform.SetParent(palmTransform);
        
        // Update config manager
        configManager.isToolCurrentlyAttachedToHand = true;
        
        
        // ** Debug 
        // sphereOne.transform.position = palmTransform.position;
        // sphereTwo.transform.position = attachPointLeapMoveTowards.transform.position;
    }
    
    
    // LeapMotion 
    // Grasp in progress
    // Disable velocities to prevent tool from drifting away 
    // Move tool towards attachment point on hand if moved out of hand 
    void OnGraspStay()
    {
        // Remove any leftover speeds from the transform and upcoming velocities during grasp 
        GetComponent<Rigidbody>().velocity = Vector3.zero;
       
        // Move the tool back to the attached position at grasp onset, in case object moved out of hand due to collision 
        transform.position = Vector3.MoveTowards(transform.position, attachPointLeapMoveTowards.transform.position, Time.deltaTime);
    }
    
    
    // LeapMotion 
    // Hand Grasp ends
    // Reset constraints and update config manager 
    void OnGraspEnd()
    {
        // Reset Parent
        this.transform.SetParent(originalParent);
        
        // Unfreeze and reenable gravity 
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        GetComponent<Rigidbody>().useGravity = true;
        
        // Update config manager
        configManager.isToolCurrentlyAttachedToHand = false;
        
        // Disable collision of hand with tool for a short while to prevent sticking of the hand to the tool
        StartCoroutine("LeapDisableHandContactForShortTime");
    }
    
    
    // LeapMotion
    // Disable hand contact for a short time, e.g. after grasp to prevent sticking of tool to hand
    private IEnumerator LeapDisableHandContactForShortTime()
    {
        // Disable contact
        GetComponent<InteractionBehaviour>().ignoreContact = true;
        
        // Wait a short time and reenable
        yield return new WaitForSeconds(configManager.timeLeapDisableHandContactAfterGraspRelease);
        GetComponent<InteractionBehaviour>().ignoreContact = false;
    }
    
    
    // LeapMotion 
    void OnGraspedMovement(Vector3 preSolvedPos, Quaternion preSolvedRot, Vector3 solvedPos, Quaternion solvedRot, List<InteractionController> graspingControllers)
    {
       
    }
    
    
    // LeapMotion
    // Hand contacts tool
    // Disable constraints 
    void OnContactBegin()
    {
        // Unfreeze tool that was freezed at start of displaying 
        // To enable moving object with hand 
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
    
    
    // LEAP MOTION
    // Hand hovers over tool
    // Calcuate closest tool attachment point and save to config manager 
    void OnHoverStay()
    {
        // Position of palm depending on handedness 
        Transform palmTransform = attachmentHands.attachmentHands[configManager.subjectHandednessLeapFormat].palm.transform;
        
        // Get closest attachment point's transform
        Transform closestAttachmentPointTransformLeap = toolManager.GetClosestAttachmentPointLeapMotion(palmTransform.position, toolInfo.toolId);
        
        // Save to ConfigManager
        configManager.currentClosestToolAttachmentPointTransform = closestAttachmentPointTransformLeap;
    }
}
