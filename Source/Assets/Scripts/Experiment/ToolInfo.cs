/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolInfo : MonoBehaviour
{
    
    // Tool ID for identification 
    public int toolId;

    // Handle orientation, is right at start for most prefabs, can be set in inspector before starting   
    public ToolManager.HandleOrientations currentHandleOrientation = ToolManager.HandleOrientations.Right;
    
    // Bool to indicate whether table rotation is regarded or not 
    public bool hasTableRotation = false;
    
    // Default handle rotation, get from setting of current handle rotation at start 
    private ToolManager.HandleOrientations defaultHandleOrientation;

    
    // Run before other start methods 
    // Run in awake, as start might not be called
    // during the small time window, when tool manager sets up the tools  
    void Awake()
    {
        // Default is orientation at start 
        defaultHandleOrientation = currentHandleOrientation;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    // Reset current handle rotation to default 
    public void ResetHandleRotationIndicatorToDefault()
    {
        // Default taken from current at start  
        currentHandleOrientation = defaultHandleOrientation;
    }
    
}
