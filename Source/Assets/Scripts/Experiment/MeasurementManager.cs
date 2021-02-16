/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using ViveSR.anipal.Eye;
using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;
using LeapInternal;
using UnityEditor;
using UnityEngine.UIElements;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Hand = Valve.VR.InteractionSystem.Hand;


public class MeasurementManager : MonoBehaviour
{
    
    // Tool Manager 
    public ToolManager toolManager;
    
    // Table Manager
    public TableManager tableManager;
    
    // Head Volume Manager
    public HeadPositionVolumeManager headVolumeManager;
    
    // Experiment Manager
    public ExperimentManager experimentManager; 
    
    // Config Manager 
    private ConfigManager configManager;
    
    // Config Manager Tag 
    public string configManagerTag;
    
    // Keep track of last gaze ray casting timestamp to get closer to framerate
    private double lastGazeRayTimeStamp; 
    
    // Sampling interval in seconds 
    private float samplingInterval;
    
    // Handedness in SteamVR format 
    private SteamVR_Input_Sources handednessOfPlayerSteamVrFormat;
    
    // SteamVR 
    public SteamVR_Action_Boolean steamVrAction;
    public Hand steamVrLeftHand;
    public Hand steamVrRightHand;
    public Player steamVrPlayer;
    
    // Leap Motion 
    public GameObject leapMainCamera;
    public InteractionHand leapLeftInteractionHand;
    public InteractionHand leapRightInteractionHand;
    
    
    // *** Debug 
    public LineRenderer debugLineRenderer; 
    
    
    
    

    // Start is called before the first frame update
    void Start()
    {
        // Find Config Manager 
        configManager = GameObject.FindGameObjectWithTag(configManagerTag).GetComponent<ConfigManager>();
        
        // Set sampling rate, convert Hz to s 
        if (configManager.samplingRate < 0.01f) // if not set
        {
            Debug.Log("[MeasurementManager] Sampling Rate not set, falling back to 90Hz.");
            configManager.samplingRate = 90;
        }
        samplingInterval = 1.0f / configManager.samplingRate;
        
    }
    
    
    // Update is called once per frame
    void Update()
    {
        /*
        // Debug 
        if (Input.GetKeyDown("up"))
        {
            print("up");
            configManager.currentUtcon = 111;
            InitSubjectData();
            debugLineRenderer.gameObject.SetActive(true);
            debugLineRenderer.startWidth = .002f;
            debugLineRenderer.endWidth = .002f;
            StartMeasurement();
        }
        if (Input.GetKeyDown("down"))
        {
            debugLineRenderer.gameObject.SetActive(false);
            StopMeasurement();
        }
        if (Input.GetKeyDown("r"))
        {
            StartCoroutine("DebugFileStructure");
        }
        */
        
    }
    
    
    
    // Debug 
    IEnumerator DebugRayTest()
    {
        while (true)
        {
            Ray ray;
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out ray);
            Debug.Log(ray);
            yield return new WaitForSeconds(1);
            
            VerboseData verboseData;
            SRanipal_Eye_v2.GetVerboseData(out verboseData);
            ViveSR.anipal.Eye.SingleEyeData eyeData = verboseData.combined.eye_data;
            print(eyeData.gaze_origin_mm);


        }
        yield break;
    }
    
    // Debug 
    IEnumerator DebugFileStructure()
    {
        configManager.subjectId = 42;
        configManager.currentUtcon = 111;
        InitSubjectData();
        WriteSubjectMetaDataToDisk();
        StartMeasurement();
        yield return new WaitForSeconds(0.01f);
        StopMeasurement();
        yield return new WaitForSeconds(0.01f);
        StartMeasurement();
        yield return new WaitForSeconds(0.01f);
        StopMeasurement();
        configManager.currentBlock = 2;
        yield return new WaitForSeconds(0.01f);
        StartMeasurement();
        yield return new WaitForSeconds(0.01f);
        StopMeasurement();
        yield return new WaitForSeconds(0.01f);
        StartMeasurement();
        yield return new WaitForSeconds(0.01f);
        StopMeasurement();
        FinishSubjectData();
    }
    
    

    // Inititalize subject data 
    public void InitSubjectData()
    {
        Debug.Log("[MeasurementManager] Initializing new subject data.");

        // Reset block; set below 
        configManager.currentBlock = 1;
        configManager.currentTrial = 1;
        configManager.currentBlockData = new ExperimentBlockData();
        configManager.blockNumberLastWrittenToOnDisk = -1; // none written to yet 
       
        // Create Subject Data 
        configManager.currentSubjectData = new SubjectMetaData();

        // Set subject data config from config manager
        configManager.currentSubjectData.subjectAge = configManager.subjectAge;
        configManager.currentSubjectData.subjectGender = configManager.subjectGender;
        configManager.currentSubjectData.subjectHandedness = configManager.subjectHandedness;
        configManager.currentSubjectData.subjectId = configManager.subjectId;
        
        // Set subject data time  
        configManager.currentSubjectData.dateTimeCreated = System.DateTime.Now.ToString("yyyy-MM-dd HH-mm");
        
        // Set table data
        configManager.currentSubjectData.tableSurfaceCenterPosition = tableManager.GetTableSurfaceCenterPosition();

        // Set head volume data
        configManager.currentSubjectData.headVolumeCenterPosition = headVolumeManager.GetHeadVolumePosition();
        configManager.currentSubjectData.headVolumeAxisAlignedBoundingBoxSize = headVolumeManager.GetHeadVolumeBoundingBoxSize(); 

        // Set input mode 
        configManager.currentSubjectData.isUsingLeapMotion = configManager.isUsingLeap;
        
        // Set tool data
        configManager.currentSubjectData.allToolDetails = toolManager.GetAllToolDetails();
        
        // Set cue orientation names ids  
        configManager.currentSubjectData.cueOrientationNamesIds = toolManager.GetCueOrientationNamesIds();
        
        // Set config manager settings 
        configManager.currentSubjectData.configManagerSettings = configManager.GetConfigManagerSettings();
        
        // Set experiment data
        configManager.currentSubjectData.experimentUtconInfo = experimentManager.GetExperimentUtconInfo();
        
        // Set filenames 
        configManager.subjectMetaDataFileName = "\\SubjectID_" + configManager.currentSubjectData.subjectId + "_MetaData_Datetime_" + configManager.currentSubjectData.dateTimeCreated + ".json";
        configManager.subjectMetaDataFileName = configManager.subjectMetaDataFileName.Replace(" ", "_");
        configManager.subjectCurrentBlockDataFileName = "\\SubjectID_" + configManager.currentSubjectData.subjectId + "_DataOfBlock_" + configManager.currentBlock.ToString() + "_Datetime_" + configManager.currentSubjectData.dateTimeCreated + ".json";
        configManager.subjectCurrentBlockDataFileName = configManager.subjectCurrentBlockDataFileName.Replace(" ", "_");
        
        // Write meta data to disk 
        WriteSubjectMetaDataToDisk();
        
    }
    
    
    // Initialize Block Data and Block Data on Disk 
    public void InitBlockData()
    {
        Debug.Log("[MeasurementManager] Initialize block data.");
        
        // Reset and init local block data
        configManager.currentBlockData = new ExperimentBlockData();
        configManager.currentBlockData.subjectId = configManager.subjectId;
        configManager.currentBlockData.dateTimeSubjectMetaDataCreated = configManager.currentSubjectData.dateTimeCreated;
        configManager.currentBlockData.eyeTrackingOverallCombinedValidationResults =
            configManager.latestEyeTrackingValidationResults;
        configManager.currentBlockData.blockNumber = configManager.currentBlock;
        
        // Init on disk 
        InitBlockDataOnDisk();
        
    }
    
    
    // Finish Subject data to finish written blocks 
    public void FinishSubjectData()
    {
        FinishBlockDataOnDisk();
    }
    
    
    // Write the subject's meta data 
    private void WriteSubjectMetaDataToDisk()
    {
        // Get the directory for the subject meta data from the config manager 
        string jsonDirectoryPath = configManager.subjectDataFolderPath;
        
        // Full filepath 
        string filePath = jsonDirectoryPath + configManager.subjectMetaDataFileName;
        
        // Make sure directory for file exists 
        FileInfo fileInfo = new FileInfo(filePath);
        fileInfo.Directory.Create();

        // Serialize subject meta data 
        string serialized = JsonUtility.ToJson(configManager.currentSubjectData,true);
        
        // Write subject meta data 
        Debug.Log("[MeasurementManager] Writing subject meta data at " + filePath + " to disk.");
        File.WriteAllText(filePath,serialized);
        Debug.Log("[MeasurementManager] Writing subject meta data to disk finished.");
    }
    

    // Init the subjects block data, write block header and keep parentheses open 
    // Make sure that current Block Data does not have trials already, else new trials will be appended
    private void InitBlockDataOnDisk()
    {
        // Get the directory for the subject block data from the config manager 
        string jsonDirectoryPath = configManager.subjectDataFolderPath;

        // Full filepath 
        string filePath = jsonDirectoryPath + configManager.subjectCurrentBlockDataFileName;
        
        // Make sure directory for file exists 
        FileInfo fileInfo = new FileInfo(filePath);
        fileInfo.Directory.Create();

        // Serialize block data, which at this point should only have header data   
        string serialized = JsonUtility.ToJson(configManager.currentBlockData,true);
        
        // Remove last few characters to append trials to file at runtime; add new line 
        // Removed "]}"
        // Added "\n"
        serialized = serialized.Substring(0, serialized.Length - 3) + Environment.NewLine;


        // Write subject data 
        Debug.Log("[MeasurementManager] Writing block data header of block " + configManager.currentBlock.ToString()  + " at " + filePath + " to disk.");
        File.WriteAllText(filePath,serialized);
        Debug.Log("[MeasurementManager] Writing block data header to disk finished.");
    }
    
    
    // Write Subject Trial Data of one trial specified by the trial index to disk 
    private void AppendTrialDataToSubjectDataOnDisk(int trialIndex)
    {
        // Get the directory for the subject data from the config manager 
        string jsonDirectoryPath = configManager.subjectDataFolderPath;
        
        // Full filepath 
        string filePath = jsonDirectoryPath + configManager.subjectCurrentBlockDataFileName;
        
        // Make sure directory for file exists 
        FileInfo fileInfo = new FileInfo(filePath);
        fileInfo.Directory.Create();

        // Init data that will be written to disk 
        string serialized = "";

        // In case of trial that is not first trial, add ",\n" to separate between trials
        if (trialIndex > 0)
        {
            serialized = "," + Environment.NewLine;
        }
        
        // Append serialized trial data 
        serialized = serialized + JsonUtility.ToJson(configManager.currentBlockData.blockTrials[trialIndex],true);
        
        // Write trial data 
        Debug.Log("[MeasurementManager] Append trial data of trial " + (trialIndex + 1).ToString()  + " in block " + configManager.currentBlock.ToString() + " at " + filePath + " to block data on disk.");
        File.AppendAllText(filePath,serialized);
        Debug.Log("[MeasurementManager] Appending trial data finished.");
        
    }
    
    
    // Finish a subject block data file with closing parentheses 
    private void FinishBlockDataOnDisk()
    {
        // Get the directory for the subject block data from the config manager 
        string jsonDirectoryPath = configManager.subjectDataFolderPath;
        
        // Full filepath 
        string filePath = jsonDirectoryPath + configManager.subjectCurrentBlockDataFileName;
        
        // Make sure directory for file exists 
        FileInfo fileInfo = new FileInfo(filePath);
        fileInfo.Directory.Create();

        // Init data that will be written to disk 
        // Append "]\n}\n" to finish block 
        string blockFinishSyntax = Environment.NewLine + "]" + Environment.NewLine + "}" + Environment.NewLine;
        
        // Write data 
        Debug.Log("[MeasurementManager] Append finishing syntax of block " + configManager.blockNumberLastWrittenToOnDisk.ToString()  + " at " + filePath + " to block data on disk.");
        File.AppendAllText(filePath,blockFinishSyntax);
        Debug.Log("[MeasurementManager] Finished finishing block.");
    }
  
    
    
    // Start measurement
    public void StartMeasurement()
    {
        Debug.Log("[MeasurementManager] Starting measuring trial in block " + configManager.currentBlock.ToString());

        // Init new trial data 
        ExperimentTrialData currentTrialData = new ExperimentTrialData();
        
        // Set data from utcon 
        currentTrialData.utcon = configManager.currentUtcon;
        toolManager.ExtractVerifiedIdsFromUtcon(currentTrialData.utcon, out currentTrialData.toolId, out currentTrialData.cueOrientationId );
        toolManager.NamesFromUtcon(currentTrialData.utcon, out currentTrialData.toolName, out currentTrialData.cueOrientationName);
        if (currentTrialData.cueOrientationName.ToLower().Contains("left"))
        {
            currentTrialData.toolHandleOrientation = "left";
            currentTrialData.cueName = currentTrialData.cueOrientationName.ToLower().Replace("left", "");
        }
        else
        {
            currentTrialData.toolHandleOrientation = "right";
            currentTrialData.cueName = currentTrialData.cueOrientationName.ToLower().Replace("right", "");
        }
        
        
        // Check if current block exists, create it otherwise  
        if (configManager.currentBlock != configManager.blockNumberLastWrittenToOnDisk)
        {
            // Finish previous block's file if previous block existed 
            if (configManager.currentBlock >= 1)
            {
                FinishBlockDataOnDisk();
            }
            
            // Update Block file name
            configManager.subjectCurrentBlockDataFileName = "\\SubjectID_" + configManager.currentSubjectData.subjectId + "_DataOfBlock_" + configManager.currentBlock.ToString() + "_Datetime_" + configManager.currentSubjectData.dateTimeCreated.Replace(" ","_") + ".json";
            
            // Init block data and file
            InitBlockData();
            
            // Update last written to 
            configManager.blockNumberLastWrittenToOnDisk = configManager.currentBlock;
        }
        
        // Append current trial to current block
        configManager.currentBlockData.blockTrials.Add(currentTrialData);
        
        // Update current trial number, is reset to in case of new block data 
        configManager.currentTrial = configManager.currentBlockData.blockTrials.Count;
        
        
        // Set handedness in SteamVR format for faster access, avoiding string compare 
        if (configManager.subjectHandedness.ToLower().Contains("left"))
        {
            handednessOfPlayerSteamVrFormat = SteamVR_Input_Sources.LeftHand;
        }
        else
        {
            handednessOfPlayerSteamVrFormat = SteamVR_Input_Sources.RightHand;
        }
        
        
        // Reset gaze ray timestamp 
        lastGazeRayTimeStamp = 0;
        
        // Start the actual measuring 
        StartCoroutine("RecordData");


    }
    
    // Stop measurement
    public void StopMeasurement()
    {
        Debug.Log("[MeasurementManager] Stopping Measuring.");
        
        // Stop the measurement coroutine
        StopCoroutine("RecordData");
        
        // Write the trial's data to disk, depending on trial index 
        AppendTrialDataToSubjectDataOnDisk(configManager.currentTrial - 1);
    }
    
    // Add data point to measurement data 
    public void AddDataPointToTrialData(ExperimentDataPoint dp)
    {
        // Append data point to latest trial (which was added when starting measurement) of current block
        // Index starts at 0, count starts at 1, so -1 for block and trial 
        configManager.currentBlockData.blockTrials[configManager.currentTrial - 1].dataPoints.Add(dp);
        
    }
    
    //get current u long timestamp
    private double GetCurrentTimestampInSeconds()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (System.DateTime.UtcNow - epochStart).TotalSeconds;
    }
    
    // Record Data 
    private IEnumerator RecordData()  // orig: RecordControllerTriggerAndPositionData
    {
        Debug.Log("[MeasurementManager] Started Coroutine to Record Data.");
        
        // Measure until stopped
        while (true)
        {
            // Create new data point for current measurement data point 
            ExperimentDataPoint dataPoint = new ExperimentDataPoint(); // orig: FrameData(), custom class do not confuse with Unity class  
            
            
            //// ** 
            // Add supplementary info to data point before raycasting 

            // TimeStamp at start 
            double timeAtStart = GetCurrentTimestampInSeconds();
            dataPoint.timeStampDataPointStart = timeAtStart;
            
            // HMD existent for both Leap and SteamVR 
            Transform hmdTransform;
                
            // Using Leap Motion    
            if (configManager.isUsingLeap)
            {
                // Hands, dependent on handedness 
                Transform handTransform;
                Leap.Hand leapHand;
                InteractionHand leapUsedInteractionHand;
                
                // VR Glasses Transform
                hmdTransform = leapMainCamera.transform;
                
                // Hand transform 
                if (handednessOfPlayerSteamVrFormat == SteamVR_Input_Sources.LeftHand)
                {
                    leapUsedInteractionHand = leapLeftInteractionHand; // left hand
                }
                else
                {
                    leapUsedInteractionHand = leapRightInteractionHand; // right hand (right or ambidextrous) 
                }
                leapHand = leapUsedInteractionHand.leapHand;
                handTransform = leapUsedInteractionHand.transform;

                // In case of leap, set SteamVR values to default 
                dataPoint.controllerTriggerPressed = false;
                dataPoint.controllerTransform = null;
                dataPoint.controllerPosition = Vector3.zero;
                dataPoint.controllerRotation = Vector3.zero;
                dataPoint.controllerScale = Vector3.zero;
                
                // Set leap specific values 
                dataPoint.leapIsGrasping = leapUsedInteractionHand.isGraspingObject;
                dataPoint.leapGrabStrength = leapHand.GrabStrength;
                dataPoint.leapGrabAngle = leapHand.GrabAngle;
                dataPoint.leapPinchStrength = leapHand.PinchStrength;
                dataPoint.leapHandPosition = handTransform.position;
                dataPoint.leapHandPalmPosition = leapHand.PalmPosition.ToVector3();
                dataPoint.leapHandRotation = leapHand.Rotation.ToQuaternion().eulerAngles;
            }
            
            // Using SteamVR
            else
            {
                // Hand Transform depending on handedness
                Transform handTransform;
                if (handednessOfPlayerSteamVrFormat == SteamVR_Input_Sources.LeftHand)
                {
                   handTransform = steamVrLeftHand.transform; // left hand
                }
                else
                {
                    handTransform = steamVrRightHand.transform; // right hand
                }

                // VR Glasses Transform 
                hmdTransform = Player.instance.hmdTransform;
                
                // Set SteamVR specific values 
                dataPoint.controllerTriggerPressed = steamVrAction.state;
                dataPoint.controllerTransform = handTransform; 
                dataPoint.controllerPosition = handTransform.position; 
                dataPoint.controllerRotation = handTransform.rotation.eulerAngles; 
                dataPoint.controllerScale = handTransform.lossyScale;
                
                // In case of SteamVR set Leap values to default
                dataPoint.leapIsGrasping = false;
                dataPoint.leapGrabStrength = 0;
                dataPoint.leapGrabAngle = 0;
                dataPoint.leapPinchStrength = 0 ;
                dataPoint.leapHandPosition = Vector3.zero;
                dataPoint.leapHandPalmPosition = Vector3.zero;
                dataPoint.leapHandRotation = Vector3.zero;
            }

            
            // Set values existent for both, Leap and SteamVR  
            dataPoint.hmdPos = hmdTransform.position; 
            dataPoint.hmdDirectionForward = hmdTransform.forward; 
            dataPoint.hmdDirectionUp = hmdTransform.up; 
            dataPoint.hmdDirectionRight = hmdTransform.right; 
            dataPoint.hmdRotation = hmdTransform.rotation.eulerAngles; 

            // End of supplementary data
            //// ** 
            
            
            
            //// **
            // Wait time before Gaze Ray Casting 
            
            // Time stamp before gaze ray casting to get time that needs to be waited 
            double timeBeforeGazeRayCasting = GetCurrentTimestampInSeconds(); // In seconds 
            
            // 
            // Check how much time needs to be waited to meet sampling rate before doing the next GazeRay Casting 
            // (If lastGazeRayTimeStamp is not yet set, i.e. 0, timeBeforeGazeRayCasting will be greater than samplingInterval so no waiting will occur)  

            // Computation was faster than sampling rate, i.e. wait to match sampling rate
            // Else: Computation was slower, i.e. continue directly with next data point 
            if ((timeBeforeGazeRayCasting - lastGazeRayTimeStamp) < samplingInterval) 
            {
                // Debug.Log("waiting for " + (float)(samplingInterval - (timeBeforeGazeRayCasting - lastGazeRayTimeStamp)));
                // Debug.Log(getCurrentTimestamp());

                // Wait for seconds that fill time between last and current gaze ray casting to meet sampling interval 
                yield return new WaitForSeconds((float)(samplingInterval - (timeBeforeGazeRayCasting - lastGazeRayTimeStamp)));
            }

            // Update time before gaze ray casting and save to data point
            timeBeforeGazeRayCasting = GetCurrentTimestampInSeconds();
            dataPoint.timeDataPointBeforeGazeRayCasting = timeBeforeGazeRayCasting;
            
            //Debug.Log("Real Framerate: " + 1 / (timeBeforeGazeRayCasting - lastGazeRayTimeStamp));
            
            // Update last Gaze Ray Time Stamp
            lastGazeRayTimeStamp = timeBeforeGazeRayCasting;
            
            // Wait time before Gaze Ray Casting End
            //// ** 

            
            
            //// **
            // GazeRay Casting 

            
            // ** Raycast Combined Eyes
            // Get Eye Position and Gaze Direction 
            Ray rayCombineEye; // origin has same value (in m) as verboseData.combined.eye_data.gaze_origin_mm (in mm)
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out rayCombineEye); 
            dataPoint.eyePositionCombinedWorld = hmdTransform.position + rayCombineEye.origin; // ray origin is at transform of hmd + offset 
            dataPoint.eyeDirectionCombinedWorld = hmdTransform.rotation * rayCombineEye.direction; // ray direction is local, so multiply with hmd transform to get world direction 
            

            RaycastHit[] raycastHitsCombined;
            raycastHitsCombined = Physics.RaycastAll(dataPoint.eyePositionCombinedWorld, dataPoint.eyeDirectionCombinedWorld,Mathf.Infinity);
            
            // Make sure something was hit 
            if (raycastHitsCombined.Length > 0)
            {
                // Sort by distance
                raycastHitsCombined = raycastHitsCombined.OrderBy(x=>x.distance).ToArray();

                // Make sure hit was not on manual collider 
                int combinedHitsIndex = 0; 
                while (raycastHitsCombined[combinedHitsIndex].collider.name.ToLower().Contains(configManager.manualColliderIncludedStringIndication))
                {
                    combinedHitsIndex += 1;
                }

                // Save hit on collider 
                if (!(raycastHitsCombined[combinedHitsIndex].collider.name.ToLower().Contains(configManager.manualColliderIncludedStringIndication)))
                {
                    dataPoint.hitObjectNameCombinedEyes = raycastHitsCombined[combinedHitsIndex].collider.name;
                    dataPoint.hitPointOnObjectCombinedEyes = raycastHitsCombined[combinedHitsIndex].point;
                    dataPoint.hitObjectCenterInWorldCombinedEyes = raycastHitsCombined[combinedHitsIndex].collider.bounds.center;
                    
                    // Debug
                    /*Debug.Log("***");
                    Debug.Log("Combined: " + raycastHitsCombined[combinedHitsIndex].collider.name);
                    //Debug.DrawRay(dataPoint.eyePositionCombinedWorld, dataPoint.eyeDirectionCombinedWorld, Color.red, 2.0f, false);
                    debugLineRenderer.SetPosition(0,dataPoint.eyePositionCombinedWorld);
                    debugLineRenderer.SetPosition(1, raycastHitsCombined[combinedHitsIndex].point);
                    Debug.Log("***");*/
                }
                
            }
            
            
            // ** Ray cast additionally left and right if wished
            if (configManager.measureLeftAndRightEyesAdditionallyToCombinedGazeRay)
            {
                // Raycast Left Eye

                // Get Eye Position and Gaze Direction 
                Ray rayLeftEye;
                SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out rayLeftEye);
                dataPoint.eyePositionLeftWorld =
                    hmdTransform.position + rayLeftEye.origin; // ray origin is at transform of hmd + offset 
                dataPoint.eyeDirectionLeftWorld =
                    hmdTransform.rotation *
                    rayLeftEye
                        .direction; // ray direction is local, so multiply with hmd transform to get world direction 

                
                RaycastHit[] raycastHitsLeft;
                raycastHitsLeft = Physics.RaycastAll(dataPoint.eyePositionLeftWorld, dataPoint.eyeDirectionLeftWorld,Mathf.Infinity);
            
                // Make sure something was hit 
                if (raycastHitsLeft.Length > 0)
                {
                    // Sort by distance
                    raycastHitsLeft = raycastHitsLeft.OrderBy(x=>x.distance).ToArray();

                    // Make sure hit was not on manual collider 
                    int leftHitsIndex = 0; 
                    while (raycastHitsLeft[leftHitsIndex].collider.name.ToLower().Contains(configManager.manualColliderIncludedStringIndication))
                    {
                        leftHitsIndex += 1;
                    }

                    // Save hit on collider 
                    if (!(raycastHitsLeft[leftHitsIndex].collider.name.ToLower().Contains(configManager.manualColliderIncludedStringIndication)))
                    {
                        dataPoint.hitObjectNameLeftEye = raycastHitsLeft[leftHitsIndex].collider.name;
                        dataPoint.hitPointOnObjectLeftEye = raycastHitsLeft[leftHitsIndex].point;
                        dataPoint.hitObjectCenterInWorldLeftEye = raycastHitsLeft[leftHitsIndex].collider.bounds.center;
                        
                        // Debug
                        /*Debug.Log("***");
                        Debug.Log("Left: " + raycastHitsLeft[leftHitsIndex].collider.name);
                        Debug.Log("***");*/
                    }
                
                }
  
                
                // Raycast Right Eye

                // Get Eye Position and Gaze Direction 
                Ray rayRightEye;
                SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out rayRightEye);
                dataPoint.eyePositionRightWorld =
                    hmdTransform.position + rayRightEye.origin; // ray origin is at transform of hmd + offset 
                dataPoint.eyeDirectionRightWorld =
                    hmdTransform.rotation *
                    rayRightEye
                        .direction; // ray direction is local, so multiply with hmd transform to get world direction 
                
                
                RaycastHit[] raycastHitsRight;
                raycastHitsRight = Physics.RaycastAll(dataPoint.eyePositionRightWorld, dataPoint.eyeDirectionRightWorld,Mathf.Infinity);
            
                // Make sure something was hit 
                if (raycastHitsRight.Length > 0)
                {
                    // Sort by distance
                    raycastHitsRight = raycastHitsRight.OrderBy(x=>x.distance).ToArray();

                    // Make sure hit was not on manual collider 
                    int rightHitsIndex = 0; 
                    while (raycastHitsRight[rightHitsIndex].collider.name.ToLower().Contains(configManager.manualColliderIncludedStringIndication))
                    {
                        rightHitsIndex += 1;
                    }

                    // Save hit on collider 
                    if (!(raycastHitsRight[rightHitsIndex].collider.name.ToLower().Contains(configManager.manualColliderIncludedStringIndication)))
                    {
                        dataPoint.hitObjectNameRightEye = raycastHitsRight[rightHitsIndex].collider.name;
                        dataPoint.hitPointOnObjectRightEye = raycastHitsRight[rightHitsIndex].point;
                        dataPoint.hitObjectCenterInWorldRightEye = raycastHitsRight[rightHitsIndex].collider.bounds.center;
                        // Debug
                        /*Debug.Log("***");
                        Debug.Log("Right: " + raycastHitsRight[rightHitsIndex].collider.name);
                        Debug.Log("***");*/
                    }
                
                }
                
            }
            
            //// **
            // GazeRay Casting End 

            
            
            //// **
            // Additional data
            
            // Eye Openness
            SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.LEFT, out dataPoint.eyeOpennessLeft);
            SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.RIGHT, out dataPoint.eyeOpennessRight);
           
            // Pupil Diameter
            ViveSR.anipal.Eye.VerboseData verboseData;
            SRanipal_Eye_v2.GetVerboseData(out verboseData); 
            dataPoint.pupilDiameterMillimetersLeft = verboseData.left.pupil_diameter_mm;
            dataPoint.pupilDiameterMillimetersRight = verboseData.right.pupil_diameter_mm;
            
            
            // Tool data 
            dataPoint.toolIsCurrentlyAttachedToHand = configManager.isToolCurrentlyAttachedToHand;
            if (configManager.currentClosestToolAttachmentPointTransform != null)
            {
                dataPoint.closestAttachmentPointOnToolToHand = configManager.currentClosestToolAttachmentPointTransform.name;
            }
            else
            {
                dataPoint.closestAttachmentPointOnToolToHand = "";
            }

            dataPoint.toolIsCurrentlyDisplayedOnTable = configManager.isToolDisplayedOnTable;

            // TimeStamp at end 
            double timeAtEnd = GetCurrentTimestampInSeconds();
            dataPoint.timeStampDataPointEnd = timeAtEnd;
            
            //// **
            // Additional data end
            
            
            
            // Add data point to current subject data 
            AddDataPointToTrialData(dataPoint);

        }

        yield break;  // coroutine stops, when loop breaks 

    }
    
}




// Holds all measured data per subject 
[Serializable]
public class SubjectMetaData 
{
    // Subject 
    public int subjectId;
    public int subjectAge; 
    public string subjectGender; 
    public string subjectHandedness;
   
    // Time 
    public string dateTimeCreated; 
    
    // Input Mode 
    public bool isUsingLeapMotion; 
    
    // Table
    public Vector3 tableSurfaceCenterPosition;
    
    // Head Volume
    public Vector3 headVolumeCenterPosition;
    public Vector3 headVolumeAxisAlignedBoundingBoxSize;
    
    // ConfigManager Settings
    public ConfigManager.ConfigManagerSettings configManagerSettings;
    
    // Cues and orientations 
    public List<ToolManager.SerializableCueOrientationNameIdPair> cueOrientationNamesIds;
    
    // Tools
    public List<ToolManager.ToolDetails> allToolDetails; 
    
    // Experiment Structure in practice and measurement section 
    public ExperimentManager.ExperimentUtconInfo experimentUtconInfo;
    
}

// Holds all data per measured block 
[Serializable]
public class ExperimentBlockData
{
    // Subject ID to make sure no blocks get lost if filename is changed 
    public int subjectId;

    // Date Time Subject Meta Data was created
    public string dateTimeSubjectMetaDataCreated;
    
    // Current block 
    public int blockNumber; 
    
    // Eye tracking validation result immediately before current block 
    public Vector3 eyeTrackingOverallCombinedValidationResults;
    
    // Trials per current block 
    public List<ExperimentTrialData> blockTrials;  

    // Constructor 
    public ExperimentBlockData()
    {
        // Automatically create list of experiment trials 
        blockTrials = new List<ExperimentTrialData>();
    }
}

// Holds all data per measured trial 
[Serializable]
public class ExperimentTrialData 
{
    // Tool, cue, orientation info 
    public int utcon; 
    public int toolId; 
    public string toolName;
    public int cueOrientationId; 
    public string cueOrientationName; 
    public string cueName;
    public string toolHandleOrientation;
    
    // Data points per current trial
    public List<ExperimentDataPoint> dataPoints;
    
    // Constructor
    public ExperimentTrialData()
    {
        // Automatically create list of experiment data points 
        dataPoints = new List<ExperimentDataPoint>();
    }
}


// Holds data of one data point  
[Serializable]
public class ExperimentDataPoint
{
    // TimeStamps 
    public double timeStampDataPointStart;
    public double timeStampDataPointEnd;
    public double timeDataPointBeforeGazeRayCasting;

    // EyeTracking 
    public float eyeOpennessLeft;
    public float eyeOpennessRight;
    public float pupilDiameterMillimetersLeft;
    public float pupilDiameterMillimetersRight;
    public Vector3 eyePositionCombinedWorld;
    public Vector3 eyeDirectionCombinedWorld;
    public Vector3 eyePositionLeftWorld;
    public Vector3 eyeDirectionLeftWorld;
    public Vector3 eyePositionRightWorld;
    public Vector3 eyeDirectionRightWorld;
    
    // GazeRay hit object 
    public string hitObjectNameCombinedEyes;
    public Vector3 hitPointOnObjectCombinedEyes;
    public Vector3 hitObjectCenterInWorldCombinedEyes;
    public string hitObjectNameLeftEye;
    public Vector3 hitPointOnObjectLeftEye;
    public Vector3 hitObjectCenterInWorldLeftEye;
    public string hitObjectNameRightEye;
    public Vector3 hitPointOnObjectRightEye;
    public Vector3 hitObjectCenterInWorldRightEye;
    
    // HMD 
    public Vector3 hmdPos;
    public Vector3 hmdDirectionForward;
    public Vector3 hmdDirectionRight;
    public Vector3 hmdRotation;
    public Vector3 hmdDirectionUp;
    
    // SteamVR input 
    public bool controllerTriggerPressed;
    public Transform controllerTransform;
    public Vector3 controllerPosition;
    public Vector3 controllerRotation;
    public Vector3 controllerScale;

    // LeapMotion Input 
    public bool leapIsGrasping;
    public float leapGrabStrength;
    public float leapGrabAngle; 
    public float leapPinchStrength;
    public Vector3 leapHandPosition;
    public Vector3 leapHandPalmPosition;
    public Vector3 leapHandRotation;
    
    // Tool 
    public bool toolIsCurrentlyAttachedToHand;
    public string closestAttachmentPointOnToolToHand;
    public bool toolIsCurrentlyDisplayedOnTable;
}


