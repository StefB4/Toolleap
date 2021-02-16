/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using TMPro;
using UnityEngine;
using Valve.VR;

public class CueManager : MonoBehaviour
{

    // Table Manager 
    public TableManager tableManager;

    // Tool Manager 
    public ToolManager toolManager;
    
    // Config Manager 
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // Object Transform Helper
    private ObjectTransformHelper objectTransformHelper;
    
    // Start is called before the first frame update
    void Start()
    {
        // Find config manager 
        configManager = GameObject.FindGameObjectWithTag(configManagerTag).GetComponent<ConfigManager>();
        
        // Get Object Transform Helper 
        objectTransformHelper = GetComponent<ObjectTransformHelper>();

        
    }

    
    // Update is called once per frame
    void Update()
    {
        // Debug 
        /*
        if (Input.GetKeyDown("w"))
        {
            print("w");
            UpdateCueText(ExperimentManager.CueStates.Lift);
        }

        if (Input.GetKeyDown("s"))
        {
            print("s");
            UpdateCueText(ExperimentManager.CueStates.Use);
        }
        
        if (Input.GetKeyDown("d"))
        {
            print("d");
            UpdateCueText(ExperimentManager.CueStates.Empty);
        }
        */
        
    }
    


    // Change cue text or disable 
    public void UpdateCueText(ExperimentManager.CueStates cueState)
    {
        
        Debug.Log("[CueManager] Updating cue text to " + cueState.ToString());
        
        // Display text depending on CueState 

        if (cueState == ExperimentManager.CueStates.Start)
        {
            ChangeCueFontSize(15);
            ChangeCueText("Interact with\ntrigger to begin.");
        }
        else if (cueState == ExperimentManager.CueStates.StartMoveHead)
        {
            ChangeCueFontSize(15);
            ChangeCueText("Move head\nto start position.");
        }
        else if (cueState == ExperimentManager.CueStates.Lift)
        {
            ChangeCueFontSize(60);
            ChangeCueText("Lift");
        }
        else if (cueState == ExperimentManager.CueStates.Use)
        {
            ChangeCueFontSize(60);
            ChangeCueText("Use");
        }
        else if (cueState == ExperimentManager.CueStates.Pause)
        {
            ChangeCueFontSize(15);
            ChangeCueText("Block Pause.\nInteract with trigger to\nstart eye tracker calibration.");
        }
        else if (cueState == ExperimentManager.CueStates.End)
        {
            ChangeCueFontSize(15);
            ChangeCueText("Experiment is over.");
        }
        else if (cueState == ExperimentManager.CueStates.PracticeStart)
        {
            ChangeCueFontSize(15);
            ChangeCueText("This is the\npractice section.");
        }
        else if (cueState == ExperimentManager.CueStates.PracticeEnd)
        {
            ChangeCueFontSize(15);
            ChangeCueText("This is the end\nof the practice section.\nThe measured experiment\nstarts after eye calibration.");
        }
        else if (cueState == ExperimentManager.CueStates.Empty)
        {
            ChangeCueFontSize(60);
            ChangeCueText("");
        }
        else
        {
            Debug.Log("Got invalid cue state, not updating cue text.");
        }
        
        // Update Cue Collider size 
        UpdateCueColliderSize(cueState); 

    }

    // Change font size of cue 
    public void ChangeCueFontSize(int size)
    {
        transform.GetChild(0).GetComponent<TextMeshPro>().fontSize = size;
    }
    
    // Change text of cue  
    public void ChangeCueText(string text)
    {
       transform.GetChild(0).GetComponent<TextMeshPro>().text = text;
    }
    
    
    // Change cue text depending on utcon 
    public void UpdateCueTextFromUtcon(int utcon)
    {
        // Get cue orientation name of current utcon 
        string cueOrientationName = toolManager.GetCueOrientationNameFromUtcon(utcon);
        
        // Extract lift or use from cue orientation name 
        if (cueOrientationName.ToLower().Contains("lift"))
        {
            UpdateCueText(ExperimentManager.CueStates.Lift); // change cue to lift 
        }
        else if (cueOrientationName.ToLower().Contains("use"))
        {
            UpdateCueText(ExperimentManager.CueStates.Use); // change cue to use 
        }
        else // Should not happen, config files need to be created properly 
        {
            Debug.Log("[CueManager] Error in the cue orientation name, it does not hold use or lift information!");
        }
    }
    
    
    // Update Cue Collider size 
    private void UpdateCueColliderSize(ExperimentManager.CueStates cueState)
    {
        
        // If cue does not display anything, set empty collider 
        if (cueState == ExperimentManager.CueStates.Empty)
        {
            GetComponent<BoxCollider>().size = Vector3.zero;
        }

        // Cue has text, i.e. collider size should be != 0 
        else
        {
            
            // Run in coroutine, as size check sometimes fails when run immediately after changing text, so make it possible to wait a while 
            StartCoroutine("SetCueColliderSize");
        }
    }

    // Update cue collider size 
    private IEnumerator SetCueColliderSize()
    {
        // Get Bounding Box of Cue 
        Bounds cueBoundingBox = objectTransformHelper.GetBoundingBox(this.gameObject);

        // Retry to get bounding box if size is zero, as sometimes mesh does not get updated immediately 
        while (cueBoundingBox.size == Vector3.zero)
        {
            cueBoundingBox = objectTransformHelper.GetBoundingBox(this.gameObject);
            yield return new WaitForSeconds(0.000001f);
        }
        
        // Update Collider size, keep x size (depth), add percentage of smallest side to width and height, collider size depends on local scale so factor that in  
        float smallestColliderSide = Mathf.Min(cueBoundingBox.size.y / transform.localScale.y, cueBoundingBox.size.z / transform.localScale.z);
        float colliderAdditionalSize = smallestColliderSide * configManager.cueColliderPercentageOversize / 100.0f;
        GetComponent<BoxCollider>().size = new Vector3(GetComponent<BoxCollider>().size.x,
            cueBoundingBox.size.y / transform.localScale.y + colliderAdditionalSize, cueBoundingBox.size.z / transform.localScale.z + colliderAdditionalSize);

        yield break;
    }


    // Set the position and rotation of the cue 
    public void UpdateCueTransform()
    {
        Debug.Log("[CueManager] Updating Cue Transform.");
        
        // After updating position, start with an empty display 
        UpdateCueText(ExperimentManager.CueStates.Empty);
        
        // Reset cue transform 
        transform.ResetLocalPose();
        
        // Get table surface center position and rotation 
        Vector3 tableRotationEuler = tableManager.GetTableRotation();
        Vector3 tableSurfaceCenterPosition = tableManager.GetTableSurfaceCenterPosition();
        
        // Vector from table surface center to cue position in air 
        Vector3 offsetTableCenterToCueInAir = new Vector3(configManager.cuePositionOffsetTowardsSubject, configManager.cuePositionOffsetUpwards, 0);
        
        // Rotate offset from table surface center to cue position in air around angle 
        Vector3 rotatedOffset = Quaternion.Euler(tableRotationEuler) * offsetTableCenterToCueInAir;
        
        // Cue position = table surface + rotated offset vector 
        Vector3 cuePosition = tableSurfaceCenterPosition + rotatedOffset;
        
        // Move and rotate cue
        transform.position = cuePosition;
        transform.rotation = Quaternion.Euler(tableRotationEuler);
        
    }
    
}
