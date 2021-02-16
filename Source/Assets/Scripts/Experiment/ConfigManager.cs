/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Leap;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using Valve.VR;

public class ConfigManager : MonoBehaviour
{
    
    // ** 
    // Config File IO
    [Header("Configuration File IO - Set here")] // Experiment variables that need to be changed only in editor 

    // ConfigManager configuration file name
    public string filenameConfigManagerConfiguration;
    
    // Filename of Tool name and ID csv
    public string filenameToolNameIdCsv;
    
    // Filename of Orientation/ cue names and ids csv 
    public string filenameToolOrientationCueNamesIdsCsv;
    
    // Filename of csv including Unique tool-cue-orientation-numbers (utcons) for experiment flow
    public string filenameExperimentFlowUtconsCsv; 
    
    // Filename of csv including Unique tool-cue-orientation-numbers (utcons) for practice flow
    public string filenamePracticeFlowUtconsCsv; 
    
    // Name of the folder containing the config files 
    public string configFolderName;
    
    // Directory name of stored table and floor calibrations
    public string calibrationDataFolderName;
    
    // Directory name of subject data outputs 
    public string subjectDataFolderName;
    
    // Experiment internal variables
    [Header("Configuration File IO - Set by code at runtime")]
    
    // Path to the folder containing the config files 
    public string configFolderPath;

    // Path to the folder containing the calibration files
    public string calibrationDataFolderPath;

    // Path to the folder containing the subject data files 
    public string subjectDataFolderPath;
    
    // ConfigManager Setup went smooth
    public bool configurationSucceededGracefully;

    
    // **
    // Table & Floor Calibration Settings
    
    [Header("Table Calibration Settings - Set in config files")] 
    [Space(20)]
    
    // For a time window how long should table/ floor calibration values be regarded (in seconds)?
    public float calibrationTimeWindow;
    
    // Height offset to account for tip coordinates not being exactly flush with controller bottom (found -0.01f to be good approximate)
    public float calibrationHeightOffset;
    
    // Depth of the table
    public float calibrationTableDepth;
    
    // Experiment internal variables
    [Header("Table Calibration Settings - Set by code at runtime")]

    // Player Rotation
    public float playerRotationDegrees;
    
    // Current table position
    public Vector3 tablePosition;
    
    // Current table rotation
    public Vector3 tableRotation;
    
    // Current table scale
    public Vector3 tableScale;
    
    // Current floor height 
    public float floorHeight;
    
    // Table is calibrated 
    public bool tableIsCalibrated;
    
    // Floor is calibrated 
    public bool floorIsCalibrated;

    
    // **
    // Cue Settings
    [Header("Cue Settings - Set in config files")] 
    [Space(20)]
    
    // Position offset of the cue in x direction 
    public float cuePositionOffsetTowardsSubject;
    
    // Position offset of the cue in y direction 
    public float cuePositionOffsetUpwards;
    
    // **
    // Cue Settings
    [Header("Cue Settings - Set here")] [Space(20)]

    // Percentage of oversize for the cue collider
    public float cueColliderPercentageOversize;
    
    
    // **
    // Head Position Volume Settings
    [Header("Head Position Volume Settings - Set in config files")] 
    [Space(20)]
    
    // Position offset of the head position volume in x direction 
    public float headPositionVolumeOffsetFromTableEdgeTowardsSubject;
    
    // Position offset of the head position volume in y direction 
    public float headPositionVolumeOffsetFromTableSurfaceUpwards;

    // Head Volume size in x direction 
    public float headPositionVolumeSizeLookingDirection;

    // Head Volume size in z direction 
    public float headPositionVolumeSizeEarOutDirection;

    // Head Volume size in y direction 
    public float headPositionVolumeSizeUpwardsDirection;
    
    
    // **
    // Second View Camera Settings 
    [Header("Second View Camera Settings - Set here")] 
    [Space(20)]
    
    // Second view camera viewport rect height percentage
    public float secondViewCameraViewportHeightPercentage;
    
    [Header("Second View Camera Settings - Set in config files")] 
    [Space(20)]
    
    // Position offset of the second view camera in x direction 
    public float secondViewCameraOffsetFromTableEdgeTowardsSubject;
    
    // Position offset of the second view camera in y direction 
    public float secondViewCameraOffsetFromTableSurfaceUpwards;
    
    // Position offset of the second view camera in z direction 
    public float secondViewCameraOffsetFromTableCenterToRight;
    
    
    // **
    // Trigger Settings
    [Header("Trigger Settings - Set here")] 
    [Space(20)]
    
    // Leap Trigger activation squared distance threshold
    public float triggerActivationLeapSquaredDistanceFromTopThreshold;
    
    // **
    // Trigger Settings
    [Header("Trigger Settings - Set in config files")]

    // Percentage of trigger position on table from center line towards front 
    public float triggerPositionTableFrontPercentage;
    
    // Percentage of trigger position on table from center line towards side 
    public float triggerPositionTableSidePercentage;
    
    
    // **
    // Eye Tracking Calibration Settings

    [Header("Eye Tracking Settings - Set by code at runtime")] [Space(20)]

    // Is Eye Tracking calibrated 
    public bool eyeTrackingIsCalibrated;
    
    // Is Eye Tracking validated
    public bool eyeTrackingIsValidated;
    
    // Latest eye tracking validation results 
    public Vector3 latestEyeTrackingValidationResults;
    
    // Keep track for experiment manager of whether experiment progression just returned from eye calibration
    public bool resumeExperimentAfterEyeTrackerCalibrationValidation;
    
    
    // **
    // Experiment settings
    [Header("Experiment Settings - Set in Config Files")]
    [Space(20)]

    // Max Distance between SteamVR hand and object for hover (sphere hover deactivated, finger joint hover activated)
    public float fingerJointHoverRadiusSteamVr;

    // Sampling rate (Hz)
    public float samplingRate;
    
    // Time in seconds between trigger interaction and cue presentation
    public float delayBetweenTriggerInteractionAndCuePresentation;
    
    // Time that cue is shown before disappearing again 
    public float cuePresentationDuration;
    
    // Time in seconds between cue presentation and tool display
    public float delayBetweenCueAndToolPresentation;
    
    // Time in seconds information should be presented 
    public float informationalCuePresentationDuration;
    
    // Time in seconds that tool is displayed before beep sound is played
    public float toolPresentationDurationBeforeBeep;
    
    // Experiment variables that need to be changed only in editor 
    [Header("Experiment Settings - Set here")]

    // Tool attachment points SteamVR game object name 
    public string toolAttachmentPointsSteamVrParentName;
    
    // Tool attachment points LeapMotion game object name 
    public string toolAttachmentPointsLeapMotionParentName;

    // Exclude ids for utcon flow creation 
    public List<int> excludeToolIdsForUtconFlowCreation = new List<int>();
    
    // Block Pause Indicator in Utcon csvs 
    public string blockPauseIndicator;
    
    // Manual collider included string 
    public string manualColliderIncludedStringIndication;
    
    // Name of the Table and Floor Calibration Scene
    public string calibrationSceneTableFloorName;
    
    // Name of the Eye Tracker Calibration Scene
    public string calibrationSceneEyeTrackerName;
    
    // Name of the Experiment Scene
    public string experimentSceneName;
    
    // Padding between text overlay element and screen edge in x direction
    public float paddingForTextOverlayWidth;
    
    // Padding between text overlay element and screen edge in y direction
    public float paddingForTextOverlayHeight;
    
    // Text overlay width percentage
    public float textOverlayWidthWindowPercentage;
    
    // Time Period to disable Leap Hand Contact with tool to prevent sticking
    public float timeLeapDisableHandContactAfterGraspRelease;
    
    // Measure left and right gaze ray individually in addition to combined eyes 
    public bool measureLeftAndRightEyesAdditionallyToCombinedGazeRay;
    
    // FPS Counter refresh rate in seconds 
    public float fpsCounterRefreshRateInSeconds;
    
  
    
    // Experiment internal variables
    [Header("Experiment Settings - Set by code at runtime")]
    
    // Current block, counting starts at 1
    public int currentBlock;
    
    // Current trial, counting starts at 1
    public int currentTrial; 
    
    // Current UTCON 
    public int currentUtcon;
    
    // UTCON index of experiment flow utcons 
    public int experimentUtconIdx;
    
    // UTCON index of practice flow utcons 
    public int practiceUtconIdx;

    // Is experiment running? 
    public bool experimentIsRunning;
    
    // Current subject data to allow resume of data generation after eye tracking 
    public SubjectMetaData currentSubjectData; 
    
    // Is practice or experiment section running? 
    public bool isInPractice;
    
    // Trigger on which side 
    public string triggerIsOnSide;
    
    // Is LeapMotion enabled or not 
    public bool isUsingLeap;
    
    // Is tool displaied on table 
    public bool isToolDisplayedOnTable;
    
    // Is tool crrently attached to hand?
    public bool isToolCurrentlyAttachedToHand;
    
    // Transform of tool attachment point that is currently closest to hand
    public Transform currentClosestToolAttachmentPointTransform;
    
    // Block number that was last written to 
    public int blockNumberLastWrittenToOnDisk;
    
    // File name of current subject metadata 
    public string subjectMetaDataFileName;
    
    // File name of current subject block data 
    public string subjectCurrentBlockDataFileName;
    
    // Current block data 
    public ExperimentBlockData currentBlockData;
    
    


    // **
    // Subject settings
    [Header("General Subject Settings - Set here")]
    [Space(20)]
    // Available Genders
    public List<string> genders = new List<string>();
    
    // Available Handedness Options
    public List<string> handednessOptions = new List<string>();
    
    // Subject data that is generated at runtime
    [Header("Current Subject Settings - Set by code at runtime")]
    
    // Subject data is set  
    public bool subjectDataIsSet;
    
    // Subject ID 
    public int subjectId;
    
    // Subject Gender
    public string subjectGender;
    
    // Subject Age 
    public int subjectAge; 
    
    // Subject handedness
    public string subjectHandedness;
    
    // Subject handedness in SteamVR format
    public SteamVR_Input_Sources subjectHandednessSteamVrFormat;
    
    // Subject handedness in Leap Motion format 
    public int subjectHandednessLeapFormat;
    
    
    // ** 
    // More code components
    [Header("Other code components - Set here")] 
    [Space(20)]

    // UI Manager Tag
    public string uiManagerTag;
    
    // UI Manager
    private UiManager uiManager;
    
    
    // **
    // Setting up
    
    // Did the setup error while running
    private bool errorWhileSettingUp;
    
    // Error message of setup 
    private string errorMessage;
    
    
    // Called once at the very start
    void Awake()
    {
        // Find UiManager at every scene load
        uiManager = GameObject.FindGameObjectWithTag(uiManagerTag).GetComponent<UiManager>();
        
        // Make sure config manager does not get duplicated between loads 
        // For first instance setup ConfigManager 
        GameObject[] objs = GameObject.FindGameObjectsWithTag(this.gameObject.tag);
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else // first instance of ConfigManager
        {
            // Make sure config manager does not get destroied between scene loads 
            DontDestroyOnLoad(this.gameObject);
            
            // Setup config manager
            SetupConfigManager();
            
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Found error during setup, quit through UiManager
        // Run during update to guarantee all dependent code loaded
        if (errorWhileSettingUp)
        {
            // Error message will be displayed once 
            errorWhileSettingUp = false;
            
            // Quit Experiment through UiManager 
            uiManager.DisplayErrorMessageMenuWithMessage(errorMessage); 
        }
    }
    
    
    // Config Manager Settings that concern the setup of the experiment 
    [Serializable]
    public struct ConfigManagerSettings
    {
        // Files 
        public string filenameConfigManagerConfiguration;
        public string filenameToolNameIdCsv;
        public string filenameToolOrientationCueNamesIdsCsv;
        public string filenameExperimentFlowUtconsCsv; 
        public string filenamePracticeFlowUtconsCsv; 
        public string configFolderName;
        public string calibrationDataFolderName;
        public string subjectDataFolderName;
        public string configFolderPath;
        public string calibrationDataFolderPath;
        public string subjectDataFolderPath;
        
        // Table and floor
        public float tableFloorCalibrationTimeWindow;
        public float tableFloorCalibrationHeightOffset;
        public float tableFloorCalibrationTableDepth;
        public float tableFloorPlayerRotationDegrees;
        public Vector3 tablePosition;
        public Vector3 tableRotation;
        public Vector3 tableScale;
        public float floorHeight;
    
        // Cue
        public float cuePositionOffsetTowardsSubject;
        public float cuePositionOffsetUpwards;
        public float cueColliderPercentageOversize;
    
        // Head Position Volume
        public float headPositionVolumeOffsetFromTableEdgeTowardsSubject;
        public float headPositionVolumeOffsetFromTableSurfaceUpwards;
        public float headPositionVolumeSizeLookingDirection;
        public float headPositionVolumeSizeEarOutDirection;
        public float headPositionVolumeSizeUpwardsDirection;

        // Second View Camera
        public float secondViewCameraViewportHeightPercentage;
        public float secondViewCameraOffsetFromTableEdgeTowardsSubject;
        public float secondViewCameraOffsetFromTableSurfaceUpwards;
        public float secondViewCameraOffsetFromTableCenterToRight;
    
        // Trigger
        public float triggerActivationLeapSquaredDistanceFromTopThreshold;
        public float triggerPositionTableFrontPercentage;
        public float triggerPositionTableSidePercentage;
        
        
        // Experiment settings
        public float fingerJointHoverRadiusSteamVr;
        public float samplingRate;
        public float delayBetweenTriggerInteractionAndCuePresentation;
        public float cuePresentationDuration;
        public float delayBetweenCueAndToolPresentation;
        public float informationalCuePresentationDuration;
        public float toolPresentationDurationBeforeBeep;
        public string toolAttachmentPointsSteamVrParentName;
        public string toolAttachmentPointsLeapMotionParentName;
        public List<int> excludeToolIdsForUtconFlowCreation;
        public string blockPauseIndicator;
        public string manualColliderIncludedStringIndication;
        public string calibrationSceneTableFloorName;
        public string calibrationSceneEyeTrackerName;
        public string experimentSceneName;
        public float paddingForTextOverlayWidth;
        public float paddingForTextOverlayHeight;
        public float textOverlayWidthWindowPercentage;
        public float timeLeapDisableHandContactAfterGraspRelease;
        public bool measureLeftAndRightEyesAdditionallyToCombinedGazeRay;
        public float fpsCounterRefreshRateInSeconds;
        public string triggerIsOnSide;
        public bool isUsingLeap;
        
        // Subject settings
        public List<string> genders;
        public List<string> handednessOptions;
      
    }


    // Get Config Manager Settings that concern the setup of the experiment 
    public ConfigManagerSettings GetConfigManagerSettings()
    {
        
        // Init 
        ConfigManagerSettings settings = new ConfigManagerSettings
        {
            filenameConfigManagerConfiguration = filenameConfigManagerConfiguration,
            filenameToolNameIdCsv = filenameToolNameIdCsv,
            filenameToolOrientationCueNamesIdsCsv = filenameToolOrientationCueNamesIdsCsv,
            filenameExperimentFlowUtconsCsv = filenameExperimentFlowUtconsCsv,
            filenamePracticeFlowUtconsCsv = filenamePracticeFlowUtconsCsv,
            configFolderName = configFolderName,
            calibrationDataFolderName = calibrationDataFolderName,
            subjectDataFolderName = subjectDataFolderName,
            configFolderPath =  configFolderPath,
            calibrationDataFolderPath = calibrationDataFolderPath,
            subjectDataFolderPath = subjectDataFolderPath,
            
            // Table and floor
            tableFloorCalibrationTimeWindow = calibrationTimeWindow,
            tableFloorCalibrationHeightOffset = calibrationHeightOffset,
            tableFloorCalibrationTableDepth = calibrationTableDepth,
            tableFloorPlayerRotationDegrees = playerRotationDegrees,
            tablePosition = tablePosition,
            tableRotation = tableRotation,
            tableScale = tableScale,
            floorHeight = floorHeight,
        
            // Cue
            cuePositionOffsetTowardsSubject = cuePositionOffsetTowardsSubject,
            cuePositionOffsetUpwards = cuePositionOffsetUpwards,
            cueColliderPercentageOversize = cueColliderPercentageOversize,
        
            // Head Position Volume
            headPositionVolumeOffsetFromTableEdgeTowardsSubject = headPositionVolumeOffsetFromTableEdgeTowardsSubject,
            headPositionVolumeOffsetFromTableSurfaceUpwards = headPositionVolumeOffsetFromTableSurfaceUpwards,
            headPositionVolumeSizeLookingDirection = headPositionVolumeSizeLookingDirection,
            headPositionVolumeSizeEarOutDirection = headPositionVolumeSizeEarOutDirection,
            headPositionVolumeSizeUpwardsDirection = headPositionVolumeSizeUpwardsDirection,

            // Second View Camera
            secondViewCameraViewportHeightPercentage = secondViewCameraViewportHeightPercentage,
            secondViewCameraOffsetFromTableEdgeTowardsSubject = secondViewCameraOffsetFromTableEdgeTowardsSubject,
            secondViewCameraOffsetFromTableSurfaceUpwards = secondViewCameraOffsetFromTableSurfaceUpwards,
            secondViewCameraOffsetFromTableCenterToRight = secondViewCameraOffsetFromTableCenterToRight,
        
            // Trigger
            triggerActivationLeapSquaredDistanceFromTopThreshold = triggerActivationLeapSquaredDistanceFromTopThreshold,
            triggerPositionTableFrontPercentage = triggerPositionTableFrontPercentage,
            triggerPositionTableSidePercentage = triggerPositionTableSidePercentage,
            
            // Experiment settings
            fingerJointHoverRadiusSteamVr = fingerJointHoverRadiusSteamVr,
            samplingRate = samplingRate,
            delayBetweenTriggerInteractionAndCuePresentation = delayBetweenTriggerInteractionAndCuePresentation,
            cuePresentationDuration = cuePresentationDuration,
            delayBetweenCueAndToolPresentation =  delayBetweenCueAndToolPresentation,
            informationalCuePresentationDuration = informationalCuePresentationDuration,
            toolPresentationDurationBeforeBeep = toolPresentationDurationBeforeBeep,
            toolAttachmentPointsSteamVrParentName =  toolAttachmentPointsSteamVrParentName,
            toolAttachmentPointsLeapMotionParentName =  toolAttachmentPointsLeapMotionParentName,
            excludeToolIdsForUtconFlowCreation = excludeToolIdsForUtconFlowCreation,
            blockPauseIndicator = blockPauseIndicator,
            manualColliderIncludedStringIndication = manualColliderIncludedStringIndication,
            calibrationSceneTableFloorName = calibrationSceneTableFloorName,
            calibrationSceneEyeTrackerName = calibrationSceneEyeTrackerName,
            experimentSceneName = experimentSceneName,
            paddingForTextOverlayWidth = paddingForTextOverlayWidth,
            paddingForTextOverlayHeight =  paddingForTextOverlayHeight,
            textOverlayWidthWindowPercentage = textOverlayWidthWindowPercentage,
            timeLeapDisableHandContactAfterGraspRelease = timeLeapDisableHandContactAfterGraspRelease,
            measureLeftAndRightEyesAdditionallyToCombinedGazeRay = measureLeftAndRightEyesAdditionallyToCombinedGazeRay,
            fpsCounterRefreshRateInSeconds = fpsCounterRefreshRateInSeconds,
            triggerIsOnSide = triggerIsOnSide,
            isUsingLeap = isUsingLeap,
            
            // Subject settings
            genders = genders,
            handednessOptions = handednessOptions
        };

        // Return 
        return settings;
    }

    

    // Setup ConfigManager with initial values 
    // Sets errorWhileSettingUp to true if the setting up encounters an error,
    // the error will then be worked on in Update() to make sure all depending code is loaded 
    private void SetupConfigManager()
    {
        Debug.Log("[ConfigManager] Setting up ConfigManager.");
        
        // Initial values 
        configurationSucceededGracefully = false;
        isUsingLeap = false;
        tableIsCalibrated = false;
        floorIsCalibrated = false;
        experimentIsRunning = false;
        subjectDataIsSet = false;
        subjectHandednessLeapFormat = 1;
        subjectHandednessSteamVrFormat = SteamVR_Input_Sources.RightHand;
        configFolderPath = Application.dataPath + "/../" + configFolderName;
        calibrationDataFolderPath = Application.dataPath + "/../" + calibrationDataFolderName;
        subjectDataFolderPath = Application.dataPath + "/../" + subjectDataFolderName;
        

        // Create folders for calibration data and subject data if not yet existent
        DirectoryInfo fileInfo = new DirectoryInfo(calibrationDataFolderPath);
        fileInfo.Create(); // does nothing if already exists
        fileInfo = new DirectoryInfo(subjectDataFolderPath);
        fileInfo.Create(); // does nothing if already exists

        // Read configs from files
        ReadConfigFromFile();
        
        // Configuration went well  
        if (!errorWhileSettingUp)
        {
            configurationSucceededGracefully = true;
            Debug.Log("[ConfigManager] Finished setting up ConfigManager successfully.");
        }
    }
        
    

    
    // Load configuration parameters from file 
    // If file cannot be found or variables are not specified, quit application after displaying error message 
    private void ReadConfigFromFile()
    {
        Debug.Log("[ConfigManager] Reading ConfigManager configuration file.");
        
        // Construct path to config file
        string configFilePath = configFolderPath + "/" + filenameConfigManagerConfiguration;
        
        // Open file 
        List<string> configLines = GetComponent<CsvIO>()
            .ReadCsvFromPath(configFilePath);
        
        // Check if opening config file was successfull, exit application if not successfull
        if (configLines == null)
        {
            errorMessage = "[ConfigManager] Could not find ConfigManager configuration file at \"" +
                                  configFilePath + "\". Exiting.";
            Debug.Log(errorMessage);
            errorWhileSettingUp = true;
            return;
        }
        
        // Config file lines with config content
        List < string[] > configContentLines = new List<string[]>();

        // Opening was successfull, process lines and create list of content lines 
        foreach (string line in configLines)
        {
            // Ignore empty lines 
            if (String.IsNullOrWhiteSpace(line) || String.IsNullOrEmpty(line))
            {
                continue; // to next line
            }
            
            // Ignore lines beginning with '#'
            if (line.StartsWith("#"))
            {
                continue; // to next line
            }
            
            // Line has content, add to content list 
            try
            {
                // Split data at "=" 
                string[] allSplits = line.Split('=');
    
                // There is not exactly one "=" in the line 
                if (allSplits.Length != 2)
                {
                    throw new Exception();
                }
                
                // Line has exactly one "="
                string[] splitData = new string[2];
                splitData[0] = allSplits[0];
                splitData[1] = allSplits[1];
                configContentLines.Add(splitData);
            }
            catch (Exception e)
            {
                // Line was not formatted properly, quitting 
                errorMessage = "[ConfigManager] Configuration file line \"" + line +
                               "\" seems to be malformed. Exiting.";
                Debug.Log(errorMessage);
                errorWhileSettingUp = true;
                return;
            }
            
        } // Read all file lines
        
        // Set calibrationTimeWindow
        if (!ParseConfigVariableData(configContentLines, "calibrationTimeWindow", out calibrationTimeWindow))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set calibrationHeightOffset
        if (!ParseConfigVariableData(configContentLines, "calibrationHeightOffset", out calibrationHeightOffset))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set calibrationTableDepth
        if (!ParseConfigVariableData(configContentLines, "calibrationTableDepth", out calibrationTableDepth))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set cuePositionOffsetTowardsSubject
        if (!ParseConfigVariableData(configContentLines, "cuePositionOffsetTowardsSubject", out cuePositionOffsetTowardsSubject))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set cuePositionOffsetUpwards
        if (!ParseConfigVariableData(configContentLines, "cuePositionOffsetUpwards", out cuePositionOffsetUpwards))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set headPositionVolumeOffsetFromTableEdgeTowardsSubject
        if (!ParseConfigVariableData(configContentLines, "headPositionVolumeOffsetFromTableEdgeTowardsSubject", out headPositionVolumeOffsetFromTableEdgeTowardsSubject))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set headPositionVolumeOffsetFromTableSurfaceUpwards
        if (!ParseConfigVariableData(configContentLines, "headPositionVolumeOffsetFromTableSurfaceUpwards", out headPositionVolumeOffsetFromTableSurfaceUpwards))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set headPositionVolumeSizeLookingDirection
        if (!ParseConfigVariableData(configContentLines, "headPositionVolumeSizeLookingDirection", out headPositionVolumeSizeLookingDirection))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set headPositionVolumeSizeEarOutDirection
        if (!ParseConfigVariableData(configContentLines, "headPositionVolumeSizeEarOutDirection", out headPositionVolumeSizeEarOutDirection))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set headPositionVolumeSizeUpwardsDirection
        if (!ParseConfigVariableData(configContentLines, "headPositionVolumeSizeUpwardsDirection", out headPositionVolumeSizeUpwardsDirection))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
       
        // Set secondViewCameraOffsetFromTableEdgeTowardsSubject
        if (!ParseConfigVariableData(configContentLines, "secondViewCameraOffsetFromTableEdgeTowardsSubject", out secondViewCameraOffsetFromTableEdgeTowardsSubject))
        {
             return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set secondViewCameraOffsetFromTableSurfaceUpwards
        if (!ParseConfigVariableData(configContentLines, "secondViewCameraOffsetFromTableSurfaceUpwards", out secondViewCameraOffsetFromTableSurfaceUpwards))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
    
        // Set secondViewCameraOffsetFromTableCenterToRight
        if (!ParseConfigVariableData(configContentLines, "secondViewCameraOffsetFromTableCenterToRight", out secondViewCameraOffsetFromTableCenterToRight))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
    
        // Set triggerPositionTableFrontPercentage
        if (!ParseConfigVariableData(configContentLines, "triggerPositionTableFrontPercentage", out triggerPositionTableFrontPercentage))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set triggerPositionTableSidePercentage
        if (!ParseConfigVariableData(configContentLines, "triggerPositionTableSidePercentage", out triggerPositionTableSidePercentage))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set fingerJointHoverRadiusSteamVr
        if (!ParseConfigVariableData(configContentLines, "fingerJointHoverRadiusSteamVr", out fingerJointHoverRadiusSteamVr))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set samplingRate 
        if (!ParseConfigVariableData(configContentLines, "samplingRate", out samplingRate))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set delayBetweenTriggerInteractionAndCuePresentation
        if (!ParseConfigVariableData(configContentLines, "delayBetweenTriggerInteractionAndCuePresentation", out delayBetweenTriggerInteractionAndCuePresentation))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set cuePresentationDuration
        if (!ParseConfigVariableData(configContentLines, "cuePresentationDuration", out cuePresentationDuration))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set delayBetweenCueAndToolPresentation
        if (!ParseConfigVariableData(configContentLines, "delayBetweenCueAndToolPresentation", out delayBetweenCueAndToolPresentation))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set informationalCuePresentationDuration
        if (!ParseConfigVariableData(configContentLines, "informationalCuePresentationDuration", out informationalCuePresentationDuration))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Set toolPresentationDurationBeforeBeep
        if (!ParseConfigVariableData(configContentLines, "toolPresentationDurationBeforeBeep", out toolPresentationDurationBeforeBeep))
        {
            return; // Parsing failed, error messages already handled during parse 
        }
        
        // Read and parsed file successfully 
        Debug.Log("[ConfigManager] Finished reading ConfigManager configuration file successfully.");
        
    }

    // Parse the data for variable name to variable from provided list with config content lines 
    private bool ParseConfigVariableData(List<string[]> configContentLines, string configVariableName, out float configVariable)
    {
        // Get list with only the items that have intended variable 
        // Pick from the config content list those lines, where the first element is the intended variable  
        // Remove whitespaces from variable 
        List<string[]> linesWithWantedVariable =
            (from line in configContentLines where line[0].Trim() == configVariableName select line).ToList();
        
        // Elements with inteded variable exist
        if (linesWithWantedVariable.Count >= 1)
        {
            // Get the value, remove whitespaces from value 
            string configValue =
                linesWithWantedVariable[0][1].Trim(); // from first element of list with elements that have desired variable name in it, select second element which holds the value  
            
            // Try to parse the string 
            if (!float.TryParse(configValue, out configVariable))
            {
                errorMessage =
                    "[ConfigManager] Parsing data for \"" + configVariableName + "\" was unsuccessful. Exiting.";
                Debug.Log(errorMessage);
                errorWhileSettingUp = true;
                return false;
            }
        }
        else // no line has desired variable 
        {
            errorMessage =
                "[ConfigManager] Data for \"" + configVariableName + "\" not specified in configuration file. Exiting.";
            Debug.Log(errorMessage);
            errorWhileSettingUp = true;
            configVariable = 0; // dummy, exiting anyways 
            return false;
        }

        // Parsing worked 
        return true;
    }
    
    
    
}
