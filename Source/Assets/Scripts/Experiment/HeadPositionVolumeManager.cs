/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;
using Valve.VR;

public class HeadPositionVolumeManager : MonoBehaviour
{

    // Table Manager
    public TableManager tableManager;
    
    // Config Manager
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // Name of the hmd collider
    public string hmdColliderName;
    
    // Is HMD touching head position volume?
    private bool hmdIsIntersectingHeadPositionVolume; 
    
    // Start is called before the first frame update
    void Start()
    {
        
        // Find Config Manager
        configManager = GameObject.FindWithTag(configManagerTag).GetComponent<ConfigManager>();
        
        // Disable visibility by default and for e.g. scene changes
        SetHeadVolumeVisibility(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    // Update the position of the head position volume 
    public void UpdateVolumeTransform() {
        
        // Reset head volume transform excluding scale 
        transform.ResetLocalPose();
        
        // Resize 
        transform.localScale = new Vector3(configManager.headPositionVolumeSizeLookingDirection, configManager.headPositionVolumeSizeUpwardsDirection,
            configManager.headPositionVolumeSizeEarOutDirection);

        // Get table surface center position and rotation and extents 
        Vector3 tableRotationEuler = tableManager.GetTableRotation();
        Vector3 tableSurfaceCenterPosition = tableManager.GetTableSurfaceCenterPosition();
        Vector3 tableExtents = tableManager.GetTableExtents();
        
        // Get the position of the center of the frontal table edge 
        Vector3 tableFrontalEdgeCenterPosition = tableSurfaceCenterPosition + new Vector3(tableExtents.x,0,0);
        
        // Vector from table surface center to head volume position in air 
        // Get offset from center to edge and add offset to volume in air 
        Vector3 offsetTableCenterToVolumeInAir = tableFrontalEdgeCenterPosition - tableSurfaceCenterPosition;
        offsetTableCenterToVolumeInAir += new Vector3(configManager.headPositionVolumeOffsetFromTableEdgeTowardsSubject, configManager.headPositionVolumeOffsetFromTableSurfaceUpwards, 0);
        
        // Rotate offset from table surface center to head volume position in air around angle 
        Vector3 rotatedOffset = Quaternion.Euler(tableRotationEuler) * offsetTableCenterToVolumeInAir;
        
        // Head volume position = table surface + rotated offset vector 
        Vector3 headVolumePosition = tableSurfaceCenterPosition + rotatedOffset;
        
        // Move and rotate head volume 
        transform.position = headVolumePosition;
        transform.rotation = Quaternion.Euler(tableRotationEuler);
    }

    
    // Get Position of Head Volume 
    public Vector3 GetHeadVolumePosition()
    {
        return transform.position;
    }
    
    // Get AABB size of head volume  
    public Vector3 GetHeadVolumeBoundingBoxSize()
    {
        return GetComponent<ObjectTransformHelper>().GetBoundingBox(this.gameObject).size;
    }
    
    // Set visibility of Head Volume
    public void SetHeadVolumeVisibility(bool visibility)
    {
        Debug.Log("[HeadPositionVolumeManager] Setting visibility to " + visibility.ToString() + ".");
        GetComponent<Renderer>().enabled = visibility;
    }
    
    // Toggle visibility of Head Volume to opposite of current state 
    public void ToggleHeadVolumeVisibility()
    {
        Debug.Log("[HeadPositionVolumeManager] Setting visibility to " + (!GetComponent<Renderer>().enabled).ToString() + ".");
        GetComponent<Renderer>().enabled = !GetComponent<Renderer>().enabled;
    }
    

    // Volume collides with other collider, check if other is hmd, also make sure that center of hmd is inside head volume
    private void OnTriggerStay(Collider otherCollider)
    {
        // HMD is intersecting head position volume 
        if (otherCollider.name == hmdColliderName)
        {
            // Make sure hmd center is inside head volume 
            if (GetComponent<Collider>().bounds.Contains(otherCollider.bounds.center))
            {
                hmdIsIntersectingHeadPositionVolume = true;
            }
            else
            {
                hmdIsIntersectingHeadPositionVolume = false;
            }
           
        }
    }


    // Other collider stops colliding with volume, check if hmd is other and make sure intersecting indicator is set appropriately 
    private void OnTriggerExit(Collider otherCollider)
    {
        // HMD is intersecting head position volume not any longer 
        if (otherCollider.name == hmdColliderName)
        {
            hmdIsIntersectingHeadPositionVolume = false;
        }
    }


    // Check if smaller bounds are in bigger bounds by checking if min and max values of smaller are inside bigger  
    private bool AreBoundsInBounds(Bounds bigger, Bounds smaller)
    {
        return (bigger.Contains(smaller.max) && bigger.Contains(smaller.min));
    }
    
    
    // Get whether hmd is intersecting head position volume
    public bool GetHmdIsIntersectingHeadPositionVolume()
    {
        return hmdIsIntersectingHeadPositionVolume;
    }
    
}
