/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;
using UnityEngine.Experimental.U2D;

public class CalibrationTableManipulator : MonoBehaviour
{

    // Floor 
    public GameObject floor;

    // Object Transform Helper
    private ObjectTransformHelper transformHelper;
    
    // For debug purposes
    private GameObject debugSphere1;
    private GameObject debugSphere2;
    private GameObject debugSphere3;
    private GameObject debugSphere4;
    private GameObject debugSphere5;

    
    // Start is called before the first frame update
    void Start()
    {
        // Get Object Transform Helper
        transformHelper = GetComponent<ObjectTransformHelper>();
        
        // Create debug spheres
        debugSphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere1.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        debugSphere1.GetComponent<Renderer>().material.color = new Color(1,0,0);
        debugSphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere2.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        debugSphere2.GetComponent<Renderer>().material.color = new Color(1,0,0);
        debugSphere3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere3.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        debugSphere3.GetComponent<Renderer>().material.color = new Color(1,0,0);
        debugSphere4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere4.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        debugSphere4.GetComponent<Renderer>().material.color = new Color(1,0,0);
        debugSphere5 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere5.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        debugSphere5.GetComponent<Renderer>().material.color = new Color(1,0,0);
    }


    // Update is called once per frame
    void Update()
    {
    }


    // Test repositioning
    public void Test()
    {
        Vector3 left = new Vector3(1.484665f, 1.145234f, -0.179664f);
        Vector3 right = new Vector3(1.478626f, 1.139808f, 0.446821f);

        left.y += -0.01f;
        right.y += -0.01f;

        float tableDep = 0.5f;

        print("Running test table calibration with left " + left.ToString() + " right " + right.ToString());
        FitToFrontCornerPositions(left, right, tableDep);
    }


    // Manipulate table position, width and height to have the same frontal corners as provided
    // Takes into account floor height and pivot of table 
    // Input data should have offset between controller-base and actual lowest point already applied
    // Virtual table's transform will be reset at beginning and new transform will be applied 
    public void FitToFrontCornerPositions(Vector3 leftCornerMeasured, Vector3 rightCornerMeasured, float tableDepth)
    {

        Debug.Log("Transforming table to fit measured corners.");

        // Reset table transform (such that especially scales are referring to identity scale) 
        transform.ResetLocalTransform();
        
        // Get bounds of table
        Bounds bounds = transformHelper.GetBoundingBox(this.gameObject);

        // Get current anchorpoint of table 
        Vector3 currentAnchor = GetComponent<Transform>().position;

        // Get floor height 
        float floorHeightY = floor.GetComponent<Transform>().position.y;

        // Do sanity check on data
        // Swap left and right corners, if input z of left is smaller than input z of right 
        if (leftCornerMeasured.z > rightCornerMeasured.z)
        {
            Debug.Log("Left and right measurement seem to be swapped, swapping back.");
            Vector3 tempSwap = leftCornerMeasured;
            leftCornerMeasured = rightCornerMeasured;
            rightCornerMeasured = tempSwap;
        }

        // Calculate position of current left frontal corner
        Vector3 currentLeftCorner = bounds.center;
        currentLeftCorner.y += bounds.extents.y;
        currentLeftCorner.z -= bounds.extents.z;
        currentLeftCorner.x += bounds.extents.x;

        // ** Debug 
        // Move debug spheres to corners table left corner and anchor 
        debugSphere1.SetActive(true);
        debugSphere2.SetActive(true);
        debugSphere1.transform.position = currentAnchor;
        debugSphere2.transform.position = currentLeftCorner;

        // Get to-be values (y height of table, x depth of table, z frontal edge) 
        float toBeY = (leftCornerMeasured.y + rightCornerMeasured.y) / 2; // height
        toBeY = toBeY - floorHeightY; // take floor height into account (total height of table is height of measured point - height of floor)
        float toBeZ = Vector2.Distance(new Vector2(leftCornerMeasured.x, leftCornerMeasured.z),
            new Vector2(rightCornerMeasured.x,
                rightCornerMeasured
                    .z)); // z is frontal edge of table, i.e. distance between left corner and right corner in xz plane

        // Calculate transform scale factors (to be values / current values) 
        float scaleY = toBeY / bounds.size.y;
        float scaleX = tableDepth / bounds.size.x;
        float scaleZ = toBeZ / bounds.size.z;

        // Rescale
        GetComponent<Transform>().localScale = new Vector3(scaleX, scaleY, scaleZ);
        
        // Get new current table bounds 
        bounds = transformHelper.GetBoundingBox(this.gameObject);
        
        // Recalculate anchor and left corner after rescale
        currentAnchor = GetComponent<Transform>().position;
        currentLeftCorner = bounds.center;
        currentLeftCorner.y += bounds.extents.y;
        currentLeftCorner.z -= bounds.extents.z;
        currentLeftCorner.x += bounds.extents.x;

        // ** Debug 
        // Move debug spheres to corners table left corner and anchor 
        debugSphere3.SetActive(true);
        debugSphere4.SetActive(true);
        debugSphere3.transform.position = currentAnchor;
        debugSphere4.transform.position = currentLeftCorner;

        // Calculate offset between anchorpoint and left frontal corner
        // In 3d space: a = b - c ; in the next line: c_new = b_new - a
        Vector3 anchorLeftCornerOffset = currentLeftCorner - currentAnchor;

        // Calculate position of new anchor to match correct left frontal corner
        Vector3 anchorUpdated = leftCornerMeasured - anchorLeftCornerOffset;

        // Update new anchor 
        GetComponent<Transform>().position = anchorUpdated;

        // 
        // Rotate table around y axis in case frontal edge is not aligned with z-axis 

        // Calculate angle between the two measured points for cases, where table is not aligned with z axis
        // In the formed triangle: alpha = tan^-1 (delta_x / delta_z) 
        Vector2 leftProjected = new Vector2(leftCornerMeasured.x, leftCornerMeasured.z);
        Vector2 rightProjected = new Vector2(rightCornerMeasured.x, rightCornerMeasured.z);
        double angleProjected = Math.Atan(Math.Abs(leftCornerMeasured.x - rightCornerMeasured.x) /
                                          Math.Abs(leftCornerMeasured.z - rightCornerMeasured.z)) * 180 / Math.PI;

        // If the right corner is more to the back of the table (i.e. in negative x direction), Unity angle is negative
        if (leftCornerMeasured.x > rightCornerMeasured.x)
        {
            angleProjected *= -1;
        }

        Debug.Log("Table is tilted " + angleProjected.ToString() + " degrees.");

        // Rotate table according to calculated angle 
        GetComponent<Transform>().rotation = Quaternion.Euler(0, (float) angleProjected, 0);

        // Rotate the vector between anchor point and left corner around y-axis in direction of calculated angle 
        Vector3 rotatedAnchorOffset = Quaternion.Euler(0, (float) angleProjected, 0) * anchorLeftCornerOffset;

        // Get rotated left corner coordinates (anchor + rotated distance between anchor and non-rotated left corner)
        Vector3 rotatedLeftCorner = anchorUpdated + rotatedAnchorOffset;

        // ** Debug 
        // Show sphere at rotatedLeftCorner
        debugSphere5.SetActive(true);
        debugSphere5.transform.position = rotatedLeftCorner;

        // Calculate offset between rotated and measured left corner in x-z-plane 
        Vector3 rotatedCornerOffset = leftCornerMeasured - rotatedLeftCorner;
        rotatedCornerOffset.y = 0.0f;

        // Calculate new anchor point taking regard of the rotation 
        Vector3 anchorAfterRotationUpdated = anchorUpdated + rotatedCornerOffset;

        // Update new anchor 
        GetComponent<Transform>().position = anchorAfterRotationUpdated;

        // Rotation complete 
        //

        // Debug
        // Deactivate debugSpheres
        if (true)
        {
            debugSphere1.SetActive(false);
            debugSphere2.SetActive(false);
            debugSphere3.SetActive(false);
            debugSphere4.SetActive(false);
            debugSphere5.SetActive(false);
        }
    }
}