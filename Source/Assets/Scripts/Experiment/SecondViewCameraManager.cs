/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

public class SecondViewCameraManager : MonoBehaviour
{
    
    // Config Manager
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // Table Manager
    public TableManager tableManager;
    
    
    // Start is called before the first frame update
    void Start()
    {
        // Find Config Manager
        configManager = GameObject.FindWithTag(configManagerTag).GetComponent<ConfigManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    
    // Update the position of the head position volume 
    public void UpdateSecondViewCameraTransformAndViewport() {
        
        // Reset second camera transform excluding scale 
        transform.ResetLocalPose();
        
        // Resize and reposition
        Vector2 viewportRectPosition = GetComponent<Camera>().rect.position;
        Vector2 viewportRectSize = GetComponent<Camera>().rect.size;
        viewportRectSize.y = configManager.secondViewCameraViewportHeightPercentage / 100.0f; // Update height from config manager, keep other values
        GetComponent<Camera>().rect = new Rect(viewportRectPosition, viewportRectSize);
        
        // Get table surface center position and rotation and extents 
        Vector3 tableRotationEuler = tableManager.GetTableRotation();
        Vector3 tableSurfaceCenterPosition = tableManager.GetTableSurfaceCenterPosition();
        Vector3 tableExtents = tableManager.GetTableExtents();
        
        // Get the position of the center of the frontal table edge 
        Vector3 tableFrontalEdgeCenterPosition = tableSurfaceCenterPosition + new Vector3(tableExtents.x,0,0);
        
        // Vector from table surface center to second camera position in air 
        // Get offset from center to edge and add offset to position in air 
        Vector3 offsetTableCenterToPositionInAir = tableFrontalEdgeCenterPosition - tableSurfaceCenterPosition;
        offsetTableCenterToPositionInAir += new Vector3(configManager.secondViewCameraOffsetFromTableEdgeTowardsSubject, configManager.secondViewCameraOffsetFromTableSurfaceUpwards, configManager.secondViewCameraOffsetFromTableCenterToRight);
        
        // Rotate offset from table surface center position in air around angle 
        Vector3 rotatedOffset = Quaternion.Euler(tableRotationEuler) * offsetTableCenterToPositionInAir;
        
        // Second camera position = table surface + rotated offset vector 
        Vector3 secondCameraPosition = tableSurfaceCenterPosition + rotatedOffset;
        
        // Move second camera and make it look at tablesurfacecenter
        transform.position = secondCameraPosition;
        transform.LookAt(tableSurfaceCenterPosition);
    }
    
    
}
