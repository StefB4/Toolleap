/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Leap.Unity;
using LeapInternal;
using UnityEditor;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ToolManager : MonoBehaviour
{
    /*
     * Keeps track of the tools, their names and ids and
     * preprocessing for scaling/ rotation. 
     */

    // **
    // Variables related to naming and ids 

    // Tool names and ids 
    private Dictionary<string, int> toolNamesIds = new Dictionary<string, int>();

    // Tool ids and GameObjects 
    private Dictionary<int, GameObject> toolIdsGameObjects = new Dictionary<int, GameObject>();

    // Cue orientation names and ids 
    private Dictionary<string, int> cueOrientationNamesIds = new Dictionary<string, int>();

    // Offset Vectors between tool bottom center and pivot (transform.position)
    private Dictionary<int, Vector3> toolPivotOffsets = new Dictionary<int,Vector3>();
    
    // List of attachment points Steam VR 
    private Dictionary<int, List<Transform>> toolAttachmentPointsSteamVr = new Dictionary<int, List<Transform>>();
    
    // List of attachment points LeapMotion 
    private Dictionary<int, List<Transform>> toolAttachmentPointsLeapMotion = new Dictionary<int, List<Transform>>();
    
    // Dictionary of tool details 
    private Dictionary<int, ToolDetails> toolDetailsByToolId = new Dictionary<int, ToolDetails>();
    
    // Number of tools 
    private int totalNumberOfTools;

    // Number of cue Orientation Combinations 
    private int totalNumberOfCueOrientationCombinations;
    
    
    // ** 
    // Variables related to program interaction 
    [Header("Inter-Script Communication")]

    // ExperimentManager 
    public ExperimentManager experimentManager;

    // CSV IO 
    public CsvIO csvIo;

    // Table Manager 
    public TableManager tableManager;
    
    // Object Transform Tools
    private ObjectTransformHelper transformHelper; 
    
    // Config Manager
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag; 
    
    
    // **
    // Variables related to displaying tools sequentially 
    private int displayAllToolsIdx;
    private bool displayAllToolsRequestedNext;
    private bool displayAllToolsRequestedPrevious;
    private int displayAllToolsCueOrientationId;
    
    
    // ** Handle orientations 
    public enum HandleOrientations { Left, Right };
    

    // ** Struct that holds all info of about a tool 
    [Serializable]
    public struct ToolDetails
    {
        public int toolId;
        public string toolNameFromConfig;
        public string toolNameFromGameObject;
        public string toolDefaultHandleOrientation; 
        public Vector3 toolPivotBottomCenterOffset;
        public Bounds toolMeshBoundingBox;
        public SerializableTransform[] toolAttachmentPointsSteamVr;
        public SerializableTransform[] toolAttachmentPointsLeapMotion;
        public Collider toolHandleCollider;
        public Collider toolEffectorCollider;
        public SerializableCollider[] toolColliders;
    }
    
    
    // ** Serializable Collider 
    [Serializable]
    public struct SerializableCollider
    {
        public string colliderName;
        public Bounds colliderBounds;
        public Collider collider;
    }
    
    
    // ** Serializable Attachment Points 
    [Serializable]
    public struct SerializableTransform
    {
        public string transformName;
        public Vector3 transformPosition;
        public Transform transform;
    }
    
    
    // ** Serializable Cue OrientationNamePair 
    [Serializable]
    public struct SerializableCueOrientationNameIdPair
    {
        public string cueOrientationName;
        public int cueOrientationId;
    }
     

    // Use Awake to make sure components are definitely loaded before calls from outside come in 
    void Awake()
    {
        // Get Object Transform Tools Component 
        transformHelper = GetComponent<ObjectTransformHelper>();
        
        // Find Config Manager
        configManager = GameObject.FindGameObjectWithTag(configManagerTag).GetComponent<ConfigManager>();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Start DisplayingAllTools 
    public void StartDisplayingAllTools(int cueOrientationId = 1)
    {
        Debug.Log("[ToolManager] Start displaying all tools sequentially.");
        
        // Make sure cueorientations exist
        if (cueOrientationNamesIds.Count < 1)
        {
            Debug.Log("[ToolManager] No cue orientation IDs set. Not displaying tools sequentially.");
            return;
        }
        
        // Check whether cueOrientationID is valid 
        if (!cueOrientationNamesIds.Values.Contains(cueOrientationId))
        {
            Debug.Log("[ToolManager] Cue Orientation ID to display all tools is not valid. Falling back to ID " + cueOrientationNamesIds.Values.ToArray()[0].ToString() + ".");
            cueOrientationId = cueOrientationNamesIds.Values.ToArray()[0];
        }
        
        // Make sure there are tools to display 
        if (toolNamesIds.Values.Count < 1)
        {
            Debug.Log("[ToolManager] There are no tools to display.");
            return;
        }
        
        // Set Cue orientation id and reset values 
        displayAllToolsCueOrientationId = cueOrientationId;
        displayAllToolsRequestedNext = false;
        displayAllToolsRequestedPrevious = false;

        // Make sure Coroutine is not running and start 
        StopCoroutine("DisplayAllToolsSequentially");
        StartCoroutine("DisplayAllToolsSequentially");
    }
    
    
    // Stop DisplayingAllTools
    public void StopDisplayingAllTools()
    {
        Debug.Log("[ToolManager] Stop displaying all tools sequentially.");
        
        // Stop Coroutine
        StopCoroutine("DisplayAllToolsSequentially");
        
        // Hide tools
        DisplayNoTool();
        
        // Reset values 
        displayAllToolsIdx = 0;
        displayAllToolsRequestedNext = false;
        displayAllToolsRequestedPrevious = false;
        displayAllToolsCueOrientationId = 1;
    }
    
    
    // Check if there is a next tool from current tool's position in tool list 
    public bool DisplayAllToolsIsNextAvailable()
    {
        // There are tools left between current tool and list end 
        if (displayAllToolsIdx < toolNamesIds.Values.Count - 1)
        {
            return true;
        }
        else // at list end 
        {
            return false;
        }
    }
    
    // Check if there is a previous tool from current tool's position in tool list 
    public bool DisplayAllToolsIsPreviousAvailable()
    {
        // There are tools left between current tool and list beginning 
        if (displayAllToolsIdx > 0)
        {
            return true;
        }
        else // at list beginning   
        {
            return false;
        }
    }
    
    // Request next tool 
    public void DisplayAllToolsRequestNextTool()
    {
        displayAllToolsRequestedNext = true;
    }
    
    // Request previous tool 
    public void DisplayAllToolsRequestPreviousTool()
    {
        displayAllToolsRequestedPrevious = true;
    }
    

    // Show all tools after each other, specify cueOrientation optionally
    IEnumerator DisplayAllToolsSequentially()
    {
        // Start Tool Idx at 0 
        displayAllToolsIdx = 0; 
        
        // Start display with initial tool 
        int utcon = toolNamesIds.Values.ToArray()[displayAllToolsIdx] * 10 + displayAllToolsCueOrientationId;
        displayToolOnTable(utcon);
        
        // Update display of tools 
        while (true)
        {
            // Check for requested next tool 
            if (displayAllToolsRequestedNext)
            {
                // Reset request indicator 
                displayAllToolsRequestedNext = false;

                // Check if next tool is available 
                if (DisplayAllToolsIsNextAvailable())
                {
                    // Update 
                    displayAllToolsIdx += 1;
                    utcon = toolNamesIds.Values.ToArray()[displayAllToolsIdx] * 10 + displayAllToolsCueOrientationId;
                    displayToolOnTable(utcon);
                }
            }
            
            // Check for requested previous tool 
            if (displayAllToolsRequestedPrevious)
            {
                // Reset request indicator 
                displayAllToolsRequestedPrevious = false;

                // Check if previous tool is available 
                if (DisplayAllToolsIsPreviousAvailable())
                {
                    // Update 
                    displayAllToolsIdx -= 1;
                    utcon = toolNamesIds.Values.ToArray()[displayAllToolsIdx] * 10 + displayAllToolsCueOrientationId;
                    displayToolOnTable(utcon);
                }
            }
            
            // If there is no request, do nothing 
            yield return new WaitForSeconds(0.2f);
            
        }

        yield break;
    }
    
    


    // Setup the tool manager 
    public void UpdateToolData()
    {
        // Get the list of tool names and their respective ids 
        LoadToolNamesIds();

        // Get list of cue orientation combination names and ids 
        LoadCueOrientationNamesIds();

        // Generate list of tool model GameObjects belonging to tool ids 
        LoadToolIdsGameObjects();
        
        // Generate a list of offsets between bottom centers of tool and pivot (transform.position) of 
        GenerateToolPivotOffsetList();
        
        // Generate dicitonary of attachment points for each tool id
        GenerateAttachmentPointsDictionarySteamVr();
        GenerateAttachmentPointsDictionaryLeapMotion();
        
        // Generate tool details dictionary 
        GenerateToolDetailsDictionary();
    }


    // Load the Tool Names and Ids from config file 
    private void LoadToolNamesIds()
    {

        Debug.Log("[ToolManager] Loading tool names and IDs from CSV.");

        // Get path from Experiment Manager and Load 
        string csvPath = experimentManager.GetToolNameIdCsvPath();
        List<string> csvLines = csvIo.ReadCsvFromPath(csvPath);

        // Reset tool names ids list for possible later load of new csv 
        toolNamesIds.Clear();

        // Split lines at comma and save name and ids into dictionary
        int toolCount = 0;
        foreach (var line in csvLines)
        {
            // Handle misformed lines 
            try
            {
                var lineContents = line.Split(',');
                toolNamesIds.Add(lineContents[0], int.Parse(lineContents[1]));
                toolCount += 1;
            }
            catch (Exception e)
            {
                Debug.Log("[ToolManager] Malformed line " + line + ". Skipping.\n" + e.ToString());
            }
        }

        // Save total number of tools 
        totalNumberOfTools = toolCount;

        Debug.Log("[ToolManager] Loaded Tool Names IDs config.");
    }


    // Load the Cue Orientation Names and Ids from config file 
    private void LoadCueOrientationNamesIds()
    {
        Debug.Log("[ToolManager] Load Cue and Orientation Names and IDs from CSV.");

        // Get path from Experiment Manager and Load 
        string csvPath = experimentManager.GetToolOrientationCueNamesIdsCsvPath();
        List<string> csvLines = csvIo.ReadCsvFromPath(csvPath);

        // Reset cue orientation names ids list 
        cueOrientationNamesIds.Clear();

        // Split lines at comma and save name and ids into dictionary for possible later load of new csv
        int combCount = 0;
        foreach (var line in csvLines)
        {
            // Handle misformed lines 
            try
            {
                var lineContents = line.Split(',');
                cueOrientationNamesIds.Add(lineContents[0], int.Parse(lineContents[1]));
                combCount += 1;
            }
            catch (Exception e)
            {
                Debug.Log("[ToolManager] Malformed line " + line + ". Skipping.\n" + e.ToString());
            }
        }

        // Save total number of combinations 
        totalNumberOfCueOrientationCombinations = combCount;

        Debug.Log("[ToolManager] Loaded Cue Orientation Names IDs config.");
    }


    // Generate dictionary holding tool ids and corresponding GameObjects 
    // Assumes that tool models are children of the GameObject this script is attached to 
    // The tool ids need to be specified by hand for the models 
    // Checks whether all tool ids are covered by GameObjects, no wrong ids appear and no id appears twice
    // Throws an exception if not all tool ids can be matched with a model from the children 
    private void LoadToolIdsGameObjects()
    {

        Debug.Log("[ToolManager] Matching model game objects to tool IDs.");

        // List of all Tool Ids 
        List<int> toolIds = toolNamesIds.Values.ToList();

        // Access all child GameObjects 
        foreach (Transform child in this.transform.GetChildren())
        {
            // Get toolId of child 
            int toolId = -1;
            try
            {
                toolId = child.gameObject.GetComponent<ToolInfo>().toolId;
            }
            catch (Exception e)
            {
                Debug.Log("[ToolManager] Cannot access child " + child.gameObject.ToString() + ", ignoring!\n" + e);
                continue;
            }

            // Check if toolId of tool is valid, i.e. in list of toolIds 
            if (!toolIds.Contains(toolId))
            {
                Debug.Log("[ToolManager] Tool ID " + toolId + " of child " + child.gameObject.ToString() +
                          " has either already been used or is not a valid ID, ignoring!");
                continue;
            }

            // ToolID is valid, save GameObject alongside toolId into list 
            toolIdsGameObjects.Add(toolId, child.gameObject);

            // Keep track of used ids
            toolIds.Remove(toolId);
        }


        // Check whether there are Game Objects for all ids 
        if (toolIds.Count > 0)
        {
            string excMsg = "There are missing tool models for some IDs! Models are missing for IDs:";
            foreach (var id in toolIds)
            {
                excMsg = excMsg + " " + id.ToString();
            }

            excMsg = excMsg +
                     ". This will potentially lead to issues with the experiment, make sure all models exist as children!";
            throw new Exception(excMsg);
        }
        else // Models for all ids 
        {
            Debug.Log("[ToolManager] Finished matching model game objects to tool IDs.");
        }

    }
    
    
    // Generate a list of offets between tool bottom center and pivot of object (transform.position)
    private void GenerateToolPivotOffsetList()
    {
        Debug.Log("[ToolManager] Generating list of tool pivot bottom center offsets.");
        
        // Calculate for each tool, kvp.Key is id kvp.Value is GameObject 
        foreach (KeyValuePair<int, GameObject> kvp in toolIdsGameObjects)
        {
            // Get tool pivot 
            Vector3 toolPivot = kvp.Value.transform.position;
            
            // Freeze tool in space, otherwise bounds might be incorrect 
            kvp.Value.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            
            // Activate tool to get the bounds 
            kvp.Value.SetActive(true);

            // Get tool bounds
            Debug.Log(kvp.Value);
            Debug.Log(transformHelper);
            Bounds toolBounds = transformHelper.GetBoundingBox(kvp.Value);
            
            // Get tool bottom center position 
            Vector3 toolBottomCenter = toolBounds.center - new Vector3(0, toolBounds.extents.y, 0);
            
            // Calculate offset 
            Vector3 toolOffset = toolPivot -  toolBottomCenter;

            // Save offset in dictionary dependent on id 
            toolPivotOffsets.Add(kvp.Key, toolOffset);
            
            // Find tool name 
            string toolName = toolNamesIds.First(pair => pair.Value == kvp.Key).Key;

            Debug.Log("[ToolManager] Calculated Pivot Offset for tool with ID " + kvp.Key.ToString() + " and name " + toolName + " as " + toolOffset.ToString("F4"));
            
            // Deactivate tool 
            kvp.Value.SetActive(false);
            
            // Toggle to (de-)activate movement of tool
            // Unfreeze tool
            kvp.Value.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            

            // ** Debug 
            /*
            if (kvp.Key == 17)
            {
                // Generate Debug Spheres 
                GameObject debugSphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere1.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
                debugSphere1.GetComponent<Renderer>().material.color = new Color(1,0,0);
                GameObject debugSphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere2.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
                debugSphere2.GetComponent<Renderer>().material.color = new Color(1,0,0);
                GameObject debugSphere3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere3.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
                debugSphere3.GetComponent<Renderer>().material.color = new Color(1,0,0);
                GameObject debugSphere4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere4.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
                debugSphere4.GetComponent<Renderer>().material.color = new Color(1,0,0);
                
                // Move Debug Spheres to position 
                debugSphere1.transform.position = toolBottomCenter;
                debugSphere2.transform.position = toolBounds.center;
                debugSphere3.transform.position = toolBounds.center - new Vector3(0, 0, toolBounds.extents.z);
                debugSphere4.transform.position = toolBounds.center + new Vector3(0, 0, toolBounds.extents.z);
                
                // Debug Output 
                print("[ToolManager] Tool Pivot " + toolPivot.ToString("F4"));
                print("[ToolManager] Tool Bounds " + toolBounds.ToString("F4"));
                print("[ToolManager] Tool Bounds Extents " + toolBounds.extents.ToString("F4"));
                print("[ToolManager] Tool Bottom center " + toolBottomCenter.ToString("F4"));
                print("[ToolManager] Tool Offset " + toolOffset.ToString("F4"));
            }
            */ 
            
        }
    }
    
    
    // Generate list of tool attachment points SteamVR
    public void GenerateAttachmentPointsDictionarySteamVr()
    {
        Debug.Log("[ToolManager] Generating tool attachment point SteamVR dictionary.");
        
        // Reset tool attachment points dictionary
        toolAttachmentPointsSteamVr.Clear();
        
        // Generate attachment point list for each tool, kvp.Key is id kvp.Value is GameObject 
        foreach (KeyValuePair<int, GameObject> kvp in toolIdsGameObjects)
        {
            // Get tool id from key 
            int toolId = kvp.Key;

            // Get tool GameObject from value 
            GameObject tool = kvp.Value;
            
            // Find GameObject that holds AttachmentPoints, is child of child of current game object 
            Transform attachmentPoints = tool.transform.GetChild(0).Find(configManager.toolAttachmentPointsSteamVrParentName);
            
            // Create List with Transforms of attachment points
            List<Transform> attachmentTransforms = new List<Transform>();
            
            // Add transforms 
            if (attachmentPoints != null)
            {
                foreach (Transform child in attachmentPoints.GetChildren())
                {
                    attachmentTransforms.Add(child);
                }
            }

            // Add list to dictionary by toolId 
            toolAttachmentPointsSteamVr.Add(toolId,attachmentTransforms);
        }
    }
    
    // Generate list of tool attachment points LeapMotion 
    public void GenerateAttachmentPointsDictionaryLeapMotion()
    {
        Debug.Log("[ToolManager] Generating tool attachment point LeapMotion dictionary.");
        
        // Reset tool attachment points dictionary
        toolAttachmentPointsLeapMotion.Clear();
        
        // Generate attachment point list for each tool, kvp.Key is id kvp.Value is GameObject 
        foreach (KeyValuePair<int, GameObject> kvp in toolIdsGameObjects)
        {
            // Get tool id from key 
            int toolId = kvp.Key;

            // Get tool GameObject from value 
            GameObject tool = kvp.Value;
            
            // Find GameObject that holds AttachmentPoints, is child of child of current game object 
            Transform attachmentPoints = tool.transform.GetChild(0).Find(configManager.toolAttachmentPointsLeapMotionParentName);
            
            // Create List with Transforms of attachment points
            List<Transform> attachmentTransforms = new List<Transform>();
            
            // Add transforms 
            if (attachmentPoints != null)
            {
                foreach (Transform child in attachmentPoints.GetChildren())
                {
                    attachmentTransforms.Add(child);
                }
            }

            // Add list to dictionary by toolId 
            toolAttachmentPointsLeapMotion.Add(toolId,attachmentTransforms);
        }
    }
    
    
    // Get closest attachment point to position by toolId for SteamVR
    public Transform GetClosestAttachmentPointSteamVr(Vector3 position, int toolId)
    {
        // Getting tool attachment points successful  
        List<Transform> attachmentPoints;
        if (toolAttachmentPointsSteamVr.TryGetValue(toolId, out attachmentPoints))
        {
            // Find closest attachment point by reordering all points by distance from position
            // Use squared magnitude to prohibit squareroot calculation, while keeping the order of distances intact 
            Transform closestAttachmentPoint = attachmentPoints.OrderBy(point => (point.position - position).sqrMagnitude).First();
            return closestAttachmentPoint;
        }
        
        // Getting tool attachment points not successful 
        else
        {
            Debug.Log("[ToolManager] Could not extract SteamVR tool attachment points for ToolID " + toolId.ToString() + ".");
            throw new Exception();
        }
    }
    
    // Get closest attachment point to position by toolId for LeapMotion
    public Transform GetClosestAttachmentPointLeapMotion(Vector3 position, int toolId)
    {
        
        // Getting tool attachment points successful  
        List<Transform> attachmentPoints;
        if (toolAttachmentPointsLeapMotion.TryGetValue(toolId, out attachmentPoints))
        {
            // Find closest attachment point by reordering all points by distance from position
            // Use squared magnitude to prohibit squareroot calculation, while keeping the order of distances intact 
            Transform closestAttachmentPoint = attachmentPoints.OrderBy(point => (point.position - position).sqrMagnitude).First();
            return closestAttachmentPoint;
        }
        
        // Getting tool attachment points not successful 
        else
        {
            Debug.Log("[ToolManager] Could not extract LeapMotion tool attachment points for ToolID " + toolId.ToString() + ".");
            throw new Exception();
        }
    }
    
    
    // Generate list of tool infos for each tool 
    public void GenerateToolDetailsDictionary()
    {
        Debug.Log("[ToolManager] Generating tool details dictionary.");
        
        // Generate for each tool, kvp.Key is id kvp.Value is GameObject 
        foreach (KeyValuePair<int, GameObject> kvp in toolIdsGameObjects)
        {
            // Get tool id from key 
            int toolId = kvp.Key;

            // Get tool GameObject from value 
            GameObject tool = kvp.Value;
            
            // Deactivate tool as a precaution 
            tool.SetActive(false);

            // Reset pose of tool to guarantee correct bounding boxes
            tool.transform.ResetLocalPose();

            // Freeze tool in space, otherwise bounds might be incorrect 
            tool.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            // Get tool pivot 
            Vector3 toolPivot = tool.transform.position;
            
            // Activate tool to get the bounds 
            tool.SetActive(true);

            // Get tool bounds
            Bounds toolBounds = transformHelper.GetBoundingBox(tool);

            // Get tool bottom center position 
            Vector3 toolBottomCenter = toolBounds.center - new Vector3(0, toolBounds.extents.y, 0);

            // Get pivot bottom center offset 
            Vector3 toolPivotBottomCenterOffset;
            toolPivotOffsets.TryGetValue(toolId, out toolPivotBottomCenterOffset);

            // Find tool name in config 
            string toolNameConfig = toolNamesIds.First(pair => pair.Value == toolId).Key;
            
            // Get tool info 
            ToolInfo toolInfo = tool.GetComponent<ToolInfo>();

            // Find all colliders of tool
            Collider[] toolColliders = tool.GetComponentsInChildren<Collider>();

            // Create ToolDetails for current tool and add information 
            ToolDetails currentToolDetails = new ToolDetails();
            currentToolDetails.toolId = toolId;
            currentToolDetails.toolNameFromConfig = toolNameConfig;
            currentToolDetails.toolNameFromGameObject = tool.name;
            currentToolDetails.toolDefaultHandleOrientation = Enum.GetName(typeof(HandleOrientations),toolInfo.currentHandleOrientation);
            currentToolDetails.toolPivotBottomCenterOffset = toolPivotBottomCenterOffset;
            currentToolDetails.toolMeshBoundingBox = toolBounds;
            
            
            // Add colliders 
            // If colliders are deactivated, their dimension is reset to zero. So use this wrapping structure to not lose data 
            List<SerializableCollider> serializableCollidersList = new List<SerializableCollider>();
            foreach (Collider coll in toolColliders)
            {
                // Add converted collider to list 
                serializableCollidersList.Add(new SerializableCollider
                {
                    colliderName = coll.name, 
                    colliderBounds = coll.bounds,
                    collider = coll 
                });
            }
            currentToolDetails.toolColliders = serializableCollidersList.ToArray();
            
            // Set handle and effector colliders 
            currentToolDetails.toolHandleCollider =
                currentToolDetails.toolColliders
                    .Where(toolCollider => toolCollider.colliderName.ToLower().Contains("handle")).ToList()[0].collider;
            currentToolDetails.toolEffectorCollider =
                currentToolDetails.toolColliders.Where(toolCollider => toolCollider.colliderName.ToLower().Contains("effector")).ToList()[0].collider;

            // Add tool attachment points SteamVR
            List<Transform> currentToolAttachmentPointsSteamVrList;
            List<SerializableTransform> serializableToolAttachmentPointsSteamVr = new List<SerializableTransform>();
            toolAttachmentPointsSteamVr.TryGetValue(toolId, out currentToolAttachmentPointsSteamVrList);
            if (currentToolAttachmentPointsSteamVrList != null) 
            {
                // For each element in the pure transform list, generate a serializable list  
                foreach (Transform attachPoint in currentToolAttachmentPointsSteamVrList)
                {
                    serializableToolAttachmentPointsSteamVr.Add(new SerializableTransform
                    {
                        transform = attachPoint,
                        transformPosition = attachPoint.position,
                        transformName = attachPoint.name
                    });
                }
                currentToolDetails.toolAttachmentPointsSteamVr = serializableToolAttachmentPointsSteamVr.ToArray();
            }
            
            // Add tool attachment points LeapMotion
            List<Transform> currentToolAttachmentPointsLeapMotionList;
            List<SerializableTransform> serializableToolAttachmentPointsLeapMotion = new List<SerializableTransform>();
            toolAttachmentPointsLeapMotion.TryGetValue(toolId, out currentToolAttachmentPointsLeapMotionList);
            if (currentToolAttachmentPointsLeapMotionList != null) 
            {
                // For each element in the pure transform list, generate a serializable list  
                foreach (Transform attachPoint in currentToolAttachmentPointsLeapMotionList)
                {
                    serializableToolAttachmentPointsLeapMotion.Add(new SerializableTransform
                    {
                        transform = attachPoint,
                        transformPosition = attachPoint.position,
                        transformName = attachPoint.name
                    });
                }
                currentToolDetails.toolAttachmentPointsLeapMotion = serializableToolAttachmentPointsLeapMotion.ToArray();
            }

            // Deactivate tool (do this after fetching colliders! Otherwise colliders will be empty) 
            tool.SetActive(false);

            // Unfreeze tool
            tool.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            
            // Add ToolDetails to list of all tools
            toolDetailsByToolId.Add(toolId,currentToolDetails);
            Debug.Log("[ToolManager] Generated tool details dictionary for tool \"" + toolNameConfig + "\".");
            
        }
    }
    

    // Get tool details from tool id, throws exception if no details for the tool id are specified 
    public ToolDetails GetToolDetailsFromToolId(int toolId)
    {
        // Getting tool details successful
        ToolDetails toolDetails;
        if (toolDetailsByToolId.TryGetValue(toolId, out toolDetails))
        {
            return toolDetails;
        }
        
        // Getting tool details not successful 
        else
        {
            Debug.Log("[ToolManager] Could not extract tool details for ToolID " + toolId.ToString() + ".");
            throw new Exception();
        }
    }
    
    // Get all tool detail 
    public List<ToolDetails> GetAllToolDetails()
    {
        // Create list of all tool details from dictionary 
        List<ToolDetails> allToolDetails = new List<ToolDetails>();
        foreach (KeyValuePair<int, ToolDetails> kvp in toolDetailsByToolId)
        {
            // Add ToolDetails to list 
            allToolDetails.Add(kvp.Value);
        }

        return allToolDetails;
    }
    

    // Generate Tool Info File 
    public void GenerateToolDetailsFile()
    {
        Debug.Log("[ToolManager] Generating tool details file.");
        
        // Construct string to write as csv, start with header
        string toolDetailsText = "";
        toolDetailsText += "Tool ID,Tool name as specified in config file,Tool name as specified in game object";
        toolDetailsText += ",Tool default handle orientation";
        toolDetailsText += ",Tool pivot point offset from Tool bottom center point X";
        toolDetailsText += ",Tool pivot point offset from Tool bottom center point Y";
        toolDetailsText += ",Tool pivot point offset from Tool bottom center point Z";
        toolDetailsText += ",Tool bounding box of entire mesh Center-X";
        toolDetailsText += ",Tool bounding box of entire mesh Center-Y";
        toolDetailsText += ",Tool bounding box of entire mesh Center-Z";
        toolDetailsText += ",Tool bounding box of entire mesh Extents-X";
        toolDetailsText += ",Tool bounding box of entire mesh Extents-Y";
        toolDetailsText += ",Tool bounding box of entire mesh Extents-Z";
        toolDetailsText += ",Tool Handle Collider bounding box Center-X";
        toolDetailsText += ",Tool Handle Collider bounding box Center-Y";
        toolDetailsText += ",Tool Handle Collider bounding box Center-Z";
        toolDetailsText += ",Tool Handle Collider bounding box Extents-X";
        toolDetailsText += ",Tool Handle Collider bounding box Extents-Y";
        toolDetailsText += ",Tool Handle Collider bounding box Extents-Z";
        toolDetailsText += ",Tool Effector Collider bounding box Center-X";
        toolDetailsText += ",Tool Effector Collider bounding box Center-Y";
        toolDetailsText += ",Tool Effector Collider bounding box Center-Z";
        toolDetailsText += ",Tool Effector Collider bounding box Extents-X";
        toolDetailsText += ",Tool Effector Collider bounding box Extents-Y";
        toolDetailsText += ",Tool Effector Collider bounding box Extents-Z";
        toolDetailsText += ",Total number of Colliders";
        toolDetailsText += ",Total number of Tool Hand Attachment Points SteamVR"; 
        toolDetailsText += ",Total number of Tool Hand Attachment Points LeapMotion"; 
        toolDetailsText += "\n";
        
        // Add lines consisting of tool details  
        foreach (KeyValuePair<int, ToolDetails> kvp in toolDetailsByToolId)
        {
            // Get tool id and details
            int toolId = kvp.Key;
            ToolDetails toolDetails = kvp.Value;
            
            // Construct line 
            toolDetailsText += toolId.ToString() + ",";
            toolDetailsText += toolDetails.toolNameFromConfig + ",";
            toolDetailsText += toolDetails.toolNameFromGameObject + ",";
            toolDetailsText += toolDetails.toolDefaultHandleOrientation.ToString() + ",";
            toolDetailsText += toolDetails.toolPivotBottomCenterOffset.x.ToString("F12") + ",";
            toolDetailsText += toolDetails.toolPivotBottomCenterOffset.y.ToString("F12") + ",";
            toolDetailsText += toolDetails.toolPivotBottomCenterOffset.z.ToString("F12") + ",";
            toolDetailsText += toolDetails.toolMeshBoundingBox.center.x.ToString("F12") + ",";
            toolDetailsText += toolDetails.toolMeshBoundingBox.center.y.ToString("F12") + ",";
            toolDetailsText += toolDetails.toolMeshBoundingBox.center.z.ToString("F12") + ",";
            toolDetailsText += toolDetails.toolMeshBoundingBox.extents.x.ToString("F12") + ",";
            toolDetailsText += toolDetails.toolMeshBoundingBox.extents.y.ToString("F12") + ",";
            toolDetailsText += toolDetails.toolMeshBoundingBox.extents.z.ToString("F12") + ",";
            
            // Add collider details
            SerializableCollider handleCollider =
                toolDetails.toolColliders.Where(toolCollider => toolCollider.colliderName.ToLower().Contains("handle")).ToList()[0];
            SerializableCollider effectorCollider =
                toolDetails.toolColliders.Where(toolCollider => toolCollider.colliderName.ToLower().Contains("effector")).ToList()[0];
            toolDetailsText += handleCollider.colliderBounds.center.x.ToString("F12") + ",";
            toolDetailsText += handleCollider.colliderBounds.center.y.ToString("F12") + ",";
            toolDetailsText += handleCollider.colliderBounds.center.z.ToString("F12") + ",";
            toolDetailsText += handleCollider.colliderBounds.extents.x.ToString("F12") + ",";
            toolDetailsText += handleCollider.colliderBounds.extents.y.ToString("F12") + ",";
            toolDetailsText += handleCollider.colliderBounds.extents.z.ToString("F12") + ",";
            toolDetailsText += effectorCollider.colliderBounds.center.x.ToString("F12") + ",";
            toolDetailsText += effectorCollider.colliderBounds.center.y.ToString("F12") + ",";
            toolDetailsText += effectorCollider.colliderBounds.center.z.ToString("F12") + ",";
            toolDetailsText += effectorCollider.colliderBounds.extents.x.ToString("F12") + ",";
            toolDetailsText += effectorCollider.colliderBounds.extents.y.ToString("F12") + ",";
            toolDetailsText += effectorCollider.colliderBounds.extents.z.ToString("F12") + ",";
            toolDetailsText += toolDetails.toolColliders.Length + ",";
            toolDetailsText += toolDetails.toolAttachmentPointsSteamVr.Length + ",";
            toolDetailsText += toolDetails.toolAttachmentPointsLeapMotion.Length;

            
            // Finish tool line 
            toolDetailsText += "\n";
        }
    
    
        // Construct filepath
        string debugToolInfoFilepath = configManager.calibrationDataFolderPath + "/ToolDetails.csv";
        
        // Write to disk   
        GetComponent<CsvIO>().WriteArbitraryTextToDisk(toolDetailsText, debugToolInfoFilepath);
        
    }
    

    // Extract tool and cue orientation IDs from UTCON and verify they are specified
    public void ExtractVerifiedIdsFromUtcon(int utcon, out int toolId, out int cueOrientationId)
    {
        // Verify utcon has 3 characters 
        if (!(utcon > 99 && utcon < 1000))
        {
            throw new Exception("Utcon " + utcon.ToString() + " is not valid!");
        }
        
        // Extract toolid and cueorientationid  
        toolId = (int) utcon / 10;
        cueOrientationId = utcon % 10;
        
        // Check if toolid and cueorientationid are valid ids, i.e. were provided in config files
        if (!toolNamesIds.Values.ToList().Contains(toolId))
        {
            throw new Exception("ToolID " + toolId.ToString() + " is not valid!");
        }
        if (!cueOrientationNamesIds.Values.ToList().Contains(cueOrientationId))
        {
            throw new Exception("CueOrientationID " + cueOrientationId.ToString() + " is not valid!");
        }
    }
    
    
    // Extract tool name and cue orientation from utcon 
    public void NamesFromUtcon(int utcon, out string toolName, out string cueOrientationName)
    {
        // Get verified tool ID and cue orientation ID
        int toolId;
        int cueOrientationId;
        ExtractVerifiedIdsFromUtcon(utcon, out toolId, out cueOrientationId);
        
        // Find cueorientationid's name 
        cueOrientationName = cueOrientationNamesIds.First(pair => pair.Value == cueOrientationId).Key;

        // Find tool name 
        toolName = toolNamesIds.First(pair => pair.Value == toolId).Key;
    }
    

    // Display tool on table depending on Unique tool-cue-orientation-number, utcon
    // Make up of number: First two characters are toolID, last number is CueOrientationId 
    public void displayToolOnTable(int utcon)
    {
        Debug.Log("[ToolManager] Displaying tool on table from utcon " + utcon.ToString());

        // Get verified tool ID and cue orientation ID
        int toolId;
        int cueOrientationId;
        ExtractVerifiedIdsFromUtcon(utcon, out toolId, out cueOrientationId);
        
        // Find tool's GameObject 
        GameObject currentTool;
        toolIdsGameObjects.TryGetValue(toolId, out currentTool);

        // Find cueorientationid's text 
        string cueOrientationText = cueOrientationNamesIds.First(pair => pair.Value == cueOrientationId).Key;

        // Find tool name 
        string toolName = toolNamesIds.First(pair => pair.Value == toolId).Key;

        // Log ids and names 
        Debug.Log("[ToolManager] Tool ID: " + toolId.ToString() + ", Tool Name: " + toolName
                  + ", Cue Orientation ID: " + cueOrientationId.ToString() + ", Cue Orientation Name: " +
                  cueOrientationText.ToString());
        
        // Reset orientation and position to make sure that the tool gets rotated correctly on the table
        // Reset the indicator of having table rotation applied to it and reset orientation indicator to default
        currentTool.transform.ResetLocalPose();
        currentTool.GetComponent<ToolInfo>().hasTableRotation = false;
        currentTool.GetComponent<ToolInfo>().ResetHandleRotationIndicatorToDefault();
        
        // Move tool to display position and regard the tool pivot-bottomcenter offset 
        Vector3 tableSurfaceCenterPosition = tableManager.GetTableSurfaceCenterPosition();
        Vector3 toolPivotOffset;
        toolPivotOffsets.TryGetValue(toolId, out toolPivotOffset);
        Vector3 toolPositionWithOffset = tableSurfaceCenterPosition + toolPivotOffset;
        currentTool.transform.position = toolPositionWithOffset;
        
        // Orient tool depending on cueorientationid's text 
        if (cueOrientationText.ToLower().Contains("left"))
        {
            orientHandle(toolId, HandleOrientations.Left); // accounts for table rotation
        }
        else if (cueOrientationText.ToLower().Contains("right"))
        {
            orientHandle(toolId, HandleOrientations.Right); // accounts for table rotation
        }
        else // Should not happen, config files need to be created properly 
        {
            Debug.Log("[ToolManager] Error in the cue orientation text, it does not hold orientation information!");
        }
        
        // Deactivate all other Tools 
        foreach (GameObject tool in toolIdsGameObjects.Values.ToList())
        {
            tool.SetActive(false);
        }
        
        // Freeze tool in space to prevent tumbling over of tool with rigidbodies that do not align with ground
        // Deactivate on touch of hand 
        currentTool.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        

        // Activate current tool 
        currentTool.SetActive(true);
        
        // Update config manager
        configManager.isToolDisplayedOnTable = true;
        
        // ** Debug 
        /*
        // Move debug sphere to table surface center 
        GameObject debugSphere5 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere5.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        debugSphere5.GetComponent<Renderer>().material.color = new Color(1,0,0);
        debugSphere5.transform.position = tableSurfaceCenterPosition;
        */

    }


    // Deactivate all tools 
    public void DisplayNoTool()
    {
        foreach (GameObject tool in toolIdsGameObjects.Values.ToList())
        {
            tool.SetActive(false);
        }
        
        // Update config manager
        configManager.isToolDisplayedOnTable = false;
    }
    

    // Change HandleOrientation of tool to left or right 
    // Takes into account table orientation 
    public void orientHandle(int toolId, HandleOrientations handleOrientation)
    {
        
        Debug.Log("[ToolManager] Orienting handle of tool with ID " + toolId.ToString() + " to the " + handleOrientation.ToString() + ".");
        
        // Get tool 
        GameObject tool;
        toolIdsGameObjects.TryGetValue(toolId, out tool);
        
        // Get tool state 
        HandleOrientations currentOrientation = tool.GetComponent<ToolInfo>().currentHandleOrientation;

        // Get whether tool has table rotation applied or not 
        bool hasTableRotation = tool.GetComponent<ToolInfo>().hasTableRotation;
        
        // Get table surface 
        Vector3 tableSurfaceCenter = tableManager.GetTableSurfaceCenterPosition();

        
        // Apply table rotation to tool, if not yet applied 
        if (!hasTableRotation)
        {
            // Get table rotation around y axis (up)
            Vector3 tableRotation = tableManager.GetTableRotation();
            
            // Rotate tool around table surface center to keep correct positioning of tool center on table 
            tool.GetComponent<Transform>().RotateAround(tableSurfaceCenter, Vector3.up, tableRotation.y);
            
            // Update indicator of table rotation applied 
            tool.GetComponent<ToolInfo>().hasTableRotation = true;
        }
        
        
        // Check if handle orientation is already correct 
        if (currentOrientation == handleOrientation)
        {
            Debug.Log("[ToolManager] Handle already oriented correctly. Not reorienting.");
        }
        else // rotation needs to be adjusted 
        {
            // Rotate tool around table surface center to keep correct positioning of tool center on table 
            tool.GetComponent<Transform>().RotateAround(tableSurfaceCenter, Vector3.up, 180);

            // Update rotation state
            tool.GetComponent<ToolInfo>().currentHandleOrientation = handleOrientation;
        }
      
    }

    
    // Get the cue orientation name from the cue orientation id 
    public string GetCueOrientationNameFromUtcon(int utcon)
    {
        // Return "" if id is no name exists with provided id, else return name 
        string cueOrientationName = "";
        try
        {
            // Extract cue orientation id from utcon 
            int cueOrientationId = utcon % 10;
            
            // Get name from cue orientation id 
            cueOrientationName = cueOrientationNamesIds.First(pair => pair.Value == cueOrientationId).Key;
        }
        catch (Exception e)
        {
            Debug.Log("[ToolManager] Could not obtain Cue Orientation Name from utcon: " + utcon.ToString() + "\n" + e.ToString());
        }
        return cueOrientationName;
    }
    
    
    // Get array of tool ids 
    public int[] GetToolIds()
    {
        return toolIdsGameObjects.Keys.ToArray();
    }


    
    
    // Get cue orientation names ids 
    public List<SerializableCueOrientationNameIdPair> GetCueOrientationNamesIds()
    {
        List<SerializableCueOrientationNameIdPair> serializableList = new List<SerializableCueOrientationNameIdPair>();

        // For all elements in the dictionary add serializable element to list 
        foreach (KeyValuePair<string, int> kvp in cueOrientationNamesIds)
        {
            serializableList.Add(new SerializableCueOrientationNameIdPair
            {
                cueOrientationName = kvp.Key, // key is name
                cueOrientationId = kvp.Value // value is id 
            });
        }
        
        return serializableList;
    }
    
    // Get list of cue orientation ids 
    public int[] GetCueOrientationIds()
    {
        return cueOrientationNamesIds.Values.ToArray();
    }
    
    
}
