/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    // Object Transform Tools
    private ObjectTransformHelper transformHelper; 
    
    // Start is called before the first frame update
    void Start()
    {
        // Get Object Transform Tools Component 
        transformHelper = GetComponent<ObjectTransformHelper>();
    }

    
    // Update is called once per frame
    void Update()
    {
        /*
        // Debug 
        if (Input.GetKeyDown("up"))
        {
            GetTableSurfaceCenterPosition();
        }
        */
    }
    
    
    // Set the table position and rotation and scale
    public void SetTableTransform(Vector3 position, Vector3 eulerAngles, Vector3 scale)
    {
        Debug.Log("[TableManager] Setting table transform.");
        transform.position = position;
        transform.rotation = Quaternion.Euler(eulerAngles);
        transform.localScale = scale;
    }
    
    
    // Get table surface center position 
    public Vector3 GetTableSurfaceCenterPosition()
    {
        
        Debug.Log("[TableManager] Getting table surface center position.");

        // Get bounds 
        Bounds bounds = transformHelper.GetBoundingBox(this.gameObject);
       
        // Calculate surface center 
        Vector3 surfaceCenter = bounds.center;
        surfaceCenter.y += bounds.extents.y;

        /*
        // Debug 
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = surfaceCenter;
        sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere2.transform.position = bounds.center;
        sphere2.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        */ 

        
       // Return surface center transform 
        return surfaceCenter;
    }
    
    
    // Get table center position
    public Vector3 GetTableCenterPosition()
    {
        // Get bounds 
        Bounds bounds = transformHelper.GetBoundingBox(this.gameObject);

        // Return center 
        return bounds.center;
    }
    
    
    // Get table extents 
    public Vector3 GetTableExtents()
    {
        // Save current transform 
        Vector3 origPosition = GetComponent<Transform>().position;
        Quaternion origRotation = GetComponent<Transform>().rotation;

        // Reset transform to get bounding box that aligns with x and z axis to get true extents
        // (This requires that the prefab is aligned properly) 
        // Keep scale 
        transform.ResetLocalPose();
        
        // Get bounds 
        Bounds bounds = transformHelper.GetBoundingBox(this.gameObject);
        
        // Get the extents of the xz-aligned table
        Vector3 extents = bounds.extents;
        
        // Restore rest of transform  
        transform.position = origPosition;
        transform.rotation = origRotation;
        
        // Return extents 
        return extents;

    }
    
    
    // Get table rotation 
    public Vector3 GetTableRotation()
    {
        return transform.rotation.eulerAngles;
    }
    
}
