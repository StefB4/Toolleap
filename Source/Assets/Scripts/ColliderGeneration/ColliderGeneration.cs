/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Leap.Unity;
using UnityEditor;
using UnityEngine;

// Mark this script as usable in Unity editor only
// to exclude it from building 
#if (UNITY_EDITOR)

public class ColliderGeneration : MonoBehaviour
{
    
    // Collider data per tool 
    private Vector3 currentLeftCenter;
    private Vector3 currentRightCenter;
    private Vector3 currentColliderSize;

    // How much bigger than mesh-perfect should the collider be, dependent on 
    public int colliderOversizePercentage; 
    
    // Relative path to directory where to store prefabs
    public string prefabSaveDirectory; 
    
    // Exclude tool id 
    public List<int> excludeToolIds;
    
    // Start is called before the first frame update
    void Start()
    {
        GeneratePrefabsWithCollidersForAllTools();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Create prefabs with colliders for all tools
    public void GeneratePrefabsWithCollidersForAllTools()
    {
        // All tools
        foreach (Transform tool in transform)
        {
            // Check if tool should be skipped
            if (excludeToolIds.Contains(tool.GetComponent<ToolInfo>().toolId))
            {
                Debug.Log("[ColliderGeneration] Skipping tool " + tool.name + " with tool ID " + tool.GetComponent<ToolInfo>().toolId);
                continue;
            }
            
            // Create prefabs
            CalculateEqualColliders(tool.gameObject);
            CreateEqualColliders(tool.gameObject);
        }
    }
    

    // Create the equally sized collider the tool 
    public void CreateEqualColliders(GameObject tool)
    {
        Debug.Log("[ColliderGeneration] Adding colliders to tool: " + tool.ToString());
        
        // Get handle position of tool
        ToolManager.HandleOrientations handleOrientation = tool.GetComponent<ToolInfo>().currentHandleOrientation;
        
        // Get prefab root directly located underneath tool gameobject
        GameObject prefabRoot = tool.transform.GetChild(0).gameObject;

        // Create new game object that holds colliders
        GameObject collidersHolder = new GameObject();
        collidersHolder.name = "CollidersCalculatedOversize" + colliderOversizePercentage.ToString() + "Percent";
        collidersHolder.transform.SetParent(prefabRoot.transform);

        // Create collider game objects
        GameObject leftCollider = new GameObject();
        leftCollider.transform.SetParent(collidersHolder.transform);
        GameObject rightCollider = new GameObject();
        rightCollider.transform.SetParent(collidersHolder.transform);

        // Set collider names 
        if (handleOrientation == ToolManager.HandleOrientations.Left)
        {
            leftCollider.name = tool.name + "HandleCollider";
            rightCollider.name = tool.name + "EffectorCollider";
        }
        else // Right
        {
            rightCollider.name = tool.name + "HandleCollider";
            leftCollider.name = tool.name + "EffectorCollider";
        }
        
        // Add collider position and size 
        rightCollider.AddComponent<BoxCollider>();
        rightCollider.GetComponent<BoxCollider>().center = currentRightCenter;
        rightCollider.GetComponent<BoxCollider>().size = currentColliderSize;
        leftCollider.AddComponent<BoxCollider>();
        leftCollider.GetComponent<BoxCollider>().center = currentLeftCenter;
        leftCollider.GetComponent<BoxCollider>().size = currentColliderSize;
        
        // Set colliders to be triggers (to prevent physics interactions)
        rightCollider.GetComponent<BoxCollider>().isTrigger = true;
        leftCollider.GetComponent<BoxCollider>().isTrigger = true;

        // Create path to store adapted prefab
        string relativePath = prefabSaveDirectory + "/" + tool.name + ".prefab";
        string prefabSavePath = Application.dataPath + relativePath;
        
        // Make sure directory of where to save prefab exists
        FileInfo fileInfo = new FileInfo(prefabSavePath);
        fileInfo.Directory.Create();
        
        // Save prefab
        Debug.Log("[ColliderGeneration] Writing prefab with added colliders to disk at path: " + prefabSavePath);
        GameObject savedPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(prefabRoot, "Assets/" + relativePath);
    }


    // Calculate two equally sized colliders for an object,
    // which split at the middle of the object and are aligned with z axis
    // Tool is already formatted correctly within hierarchy (along z-axis, handle left or right is specified) 
    public void CalculateEqualColliders(GameObject tool)
    {
        Debug.Log("[ColliderGeneration] Calculating equally sized colliders for tool: " + tool.name);
        
        // Get mesh bounds
        Bounds bounds = GetComponent<ObjectTransformHelper>().GetBoundingBox(tool);
        
        // Get center of left and right colliders
        currentLeftCenter = bounds.center;
        currentLeftCenter.z = bounds.center.z - 1 / 4.0f * bounds.size.z; // left collider center is at half of the first half
        currentRightCenter = bounds.center;
        currentRightCenter.z = bounds.center.z + 1 / 4.0f * bounds.size.z; // right collider center is at half of second half

        // Set the size of both collider 
        currentColliderSize = bounds.size;
        currentColliderSize.z = bounds.size.z / 2;
        
        // Oversize colliders, if specified 
        // Add percentage of smallest side to all sides 
        if (colliderOversizePercentage > 0)
        {
            // Get smallest side of collider
            float smallestSide = 0.0f;
            if (currentColliderSize.x <= currentColliderSize.z && currentColliderSize.x <= currentColliderSize.y) // smaller or equal as it might actually be the case that they are the same (for daisycutter x and y) 
            {
                smallestSide = currentColliderSize.x;
            }
            else if (currentColliderSize.y <= currentColliderSize.x && currentColliderSize.y <= currentColliderSize.z)
            {
                smallestSide = currentColliderSize.y;
            }
            else if (currentColliderSize.z <= currentColliderSize.x && currentColliderSize.z <= currentColliderSize.y)
            {
                smallestSide = currentColliderSize.z;
            }
            
            // Calculate size addon
            float addonSize = 0.0f;
            addonSize = smallestSide * colliderOversizePercentage / 100.0f;
            
            // Add to collider dimensions 
            currentColliderSize.x += 2 * addonSize;
            currentColliderSize.y += 2 * addonSize;
            currentColliderSize.z += addonSize; // Only once, as left and right collider touch 
            
            // Update centers
            currentLeftCenter.z -= addonSize / 2.0f;
            currentRightCenter.z += addonSize / 2.0f;
            
            Debug.Log("[ColliderGeneration] Added collider oversize percentage of " + colliderOversizePercentage.ToString() + " percent.");
        }
        
    }

}

#endif
