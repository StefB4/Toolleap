/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class ExperimentManager : MonoBehaviour
{
    
    // ** 
    // Variables related to GameObjects 
    [Header("Game Objects")]

    // Table manager 
    public TableManager tableManager;
    
    // Floor (and walls)
    public GameObject room; 
    
    // Cue 
    public CueManager cueManager; 
    
    // Tools
    public ToolManager toolManager; 
    
    // Trigger 
    public TriggerManager triggerManager; 
    
    // Player
    public PlayerManager playerManager;
    
    // Measurements 
    public MeasurementManager measurementManager;
    
    // UI Manager
    public UiManager uiManager;
    
    // Head position volume manager 
    public HeadPositionVolumeManager headPositionVolumeManager;
    
    // Second view camera manager
    public SecondViewCameraManager secondViewCameraManager;
    
    // Audio Manager
    public AudioManager audioManager;
    
    // Config Manager
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // **
    // Variables related to state of experiment 
    
    // Number of individual UTCONs
    private int utconCount = 0;
    
    // Array of experiment flow utcons 
    private int[] experimentFlowUtcons;
    
    // UTCON index of experiment flow utcons 
    //private int experimentUtconIdx = 0;
    
    // List of utcon indices INFRONT OF which experiment block pause happens, indexing of utcons starts at 0 
    private int[] experimentBlockPausesIdx; 
    
    // Array of practice flow utcons 
    private int[] practiceFlowUtcons;
    
    // UTCON index of practice flow utcons 
    //private int practiceUtconIdx = 0;
    
    // List of utcon indices INFRONT OF which practice block pause happens, indexing of utcons starts at 0 
    private int[] practiceBlockPausesIdx; 
    
    // Enum for the current Cue State 
    public enum CueStates { PracticeStart, PracticeEnd, Start, StartMoveHead, Use, Lift, Pause, End, Empty };
    
    // Enum for the current experiment state 
    public enum ExperimentStates { Idle, Init, PracticeStart, Practice, PracticeEnd, Start, Measuring, BlockPause, End };
    private ExperimentStates experimentState; 
    
    // Is coroutine started? 
    private bool experimentStateCoroutineIsStarted = false;
    
    // Did a resume from block pause just happen?
    private bool resumingFromBlockPause = false;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("[ExperimentManager] Starting Experiment Scene.");
        
        // Find config manager
        configManager = GameObject.FindWithTag(configManagerTag).GetComponent<ConfigManager>();
        
        // Do nothing if the configuration of ConfigManager did not go smooth 
        if (!configManager.configurationSucceededGracefully)
        {
            return;
        }
        
        // Update the scene 
        UpdateSceneConfiguration();
        
        // Check whether eye calibration/ validation just took place, resume experiment if that's the case 
        if (configManager.resumeExperimentAfterEyeTrackerCalibrationValidation)
        {
            // Resume experiment  
            Debug.Log("[Experiment Manager] Resuming experiment after Eye Tracker calibration and validation.");
            configManager.resumeExperimentAfterEyeTrackerCalibrationValidation = false; // Disable return to experiment 
            resumingFromBlockPause = true; // activate block pause lock for measuring state 
            experimentState = ExperimentStates.Start; // Change experiment state 
        }
        
        // No eye calibration/ validation yet 
        else
        {
            // Change to idle mode 
            experimentState = ExperimentStates.Idle;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Init new subject  
        if (experimentState == ExperimentStates.Init)
        {
            ExperimentStateInit();
        }
        
        // PracticeStart state coroutine 
        else if (experimentState == ExperimentStates.PracticeStart)
        {
            if (!experimentStateCoroutineIsStarted)
            {
                experimentStateCoroutineIsStarted = true;
                StartCoroutine("ExperimentStatePracticeStart");
                
            }
        }
        
        // Practice state coroutine 
        else if (experimentState == ExperimentStates.Practice)
        {
            if (!experimentStateCoroutineIsStarted)
            {
                experimentStateCoroutineIsStarted = true;
                StartCoroutine("ExperimentStatePractice");
                
            }
        }
        
        // PracticeEnd state coroutine 
        else if (experimentState == ExperimentStates.PracticeEnd)
        {
            if (!experimentStateCoroutineIsStarted)
            {
                experimentStateCoroutineIsStarted = true;
                StartCoroutine("ExperimentStatePracticeEnd");
            }
        }
        
        // Start state coroutine 
        else if (experimentState == ExperimentStates.Start)
        {
            // Is coroutine started already? 
            if (!experimentStateCoroutineIsStarted)
            {
                experimentStateCoroutineIsStarted = true; // activate lock 
                StartCoroutine("ExperimentStateStart"); // start coroutine 
            }
        }
        
        // Measuring state coroutine 
        else if (experimentState == ExperimentStates.Measuring)
        {
            if (!experimentStateCoroutineIsStarted)
            {
                experimentStateCoroutineIsStarted = true;
                StartCoroutine("ExperimentStateMeasuring");
                
            }
        }
        
        // BlockPause state coroutine 
        else if (experimentState == ExperimentStates.BlockPause)
        {
            if (!experimentStateCoroutineIsStarted)
            {
                experimentStateCoroutineIsStarted = true;
                StartCoroutine("ExperimentStateBlockPause");
                
            }
        }
        else if (experimentState == ExperimentStates.End)
        {
            if (!experimentStateCoroutineIsStarted)
            {
                experimentStateCoroutineIsStarted = true;
                StartCoroutine("ExperimentStateEnd");
                
            }
        }
        
        else if (experimentState == ExperimentStates.Idle)
        {
           
        }
        
        
        else
        {
            Debug.Log("[ExperimentManager] Specified invalid Experiment State, ignoring!");
        }
        
    }
    
    // Init state 
    private void ExperimentStateInit()
    {
        Debug.Log("[ExperimentManager] Now in state Init.");

        // Reset block number and utcon indices  
        configManager.currentBlock = 1;
        configManager.experimentUtconIdx = 0;
        configManager.practiceUtconIdx = 0;

        // Check if practice is set 
        if (practiceFlowUtcons.Length < 1)
        {
            // No practice
            configManager.isInPractice = false; 
            
            // Change experiment state to start 
            experimentState = ExperimentStates.Start;
        }
        else
        {
            // Practice 
            configManager.isInPractice = true;
            
            // Change experiment state to practice start 
            experimentState = ExperimentStates.PracticeStart;
        }
        
        // Signal to config manager that experiment is running 
        configManager.experimentIsRunning = true;
        
        // Init new subject data in measurement manager
        measurementManager.InitSubjectData();
        
        // Reset trigger input 
        triggerManager.ResetInteractionHappened();
    }
    

    // Coroutine for the practice start state 
    IEnumerator ExperimentStatePracticeStart()
    {
        Debug.Log("[ExperimentManager] Now in state PracticeStart.");
        
        // Update Cue text 
        cueManager.UpdateCueText(CueStates.PracticeStart);
        
        // Display info for a few seconds before continuing 
        yield return new WaitForSeconds(configManager.informationalCuePresentationDuration);
        
        // Go to start state next  
        experimentState = ExperimentStates.Start;
                
        // Reset experiment state settings
        triggerManager.ResetInteractionHappened(); // Reset input to none 
        experimentStateCoroutineIsStarted = false; // disable coroutine lock 
        yield break; // break coroutine 
        
    }
    
    
    // Coroutine for the start state 
    IEnumerator ExperimentStateStart()
    {
        Debug.Log("[ExperimentManager] Now in state Start.");
        
        // Update Cue text 
        cueManager.UpdateCueText(CueStates.Start);
        
        // Reset input to none to make sure spam input is ignored 
        triggerManager.ResetInteractionHappened(); 
        
        // Run until trigger interaction appeared and make sure head is in correct position 
        while (true)
        {
            // Trigger interaction appeared
            if (triggerManager.GetInteractionHappened())
            {
                // Head is in the correct position 
                if (headPositionVolumeManager.GetHmdIsIntersectingHeadPositionVolume())
                {
                    // Empty cue text and wait for delay between trials (and start) 
                    cueManager.UpdateCueText(CueStates.Empty); // Empty cue text 
                    yield return
                        new WaitForSeconds(configManager
                            .delayBetweenTriggerInteractionAndCuePresentation); // Pause before first trial for a certain time 

                    // Change experiment state
                    if (configManager.isInPractice)
                    {
                        experimentState = ExperimentStates.Practice; // practice section 
                    }
                    else
                    {
                        experimentState = ExperimentStates.Measuring; // experiment measuring section
                    }

                    // Reset experiment state settings
                    triggerManager.ResetInteractionHappened(); // Reset input to none 
                    experimentStateCoroutineIsStarted = false; // disable coroutine lock 
                    yield break; // break coroutine 
                }

                // Head is not yet in correct position 
                else
                {
                    Debug.Log("[ExperimentManager] Head is not at the correct position to start measuring.");
                    
                    triggerManager.ResetInteractionHappened(); // Reset the interaction happened 
                    cueManager.UpdateCueText(CueStates.StartMoveHead); // Show info that moving head is necessary

                    // Wait for head to move to correct position 
                    while (!headPositionVolumeManager.GetHmdIsIntersectingHeadPositionVolume())
                    {
                        yield return new WaitForSeconds(.1f);
                    }
                    
                    Debug.Log("[ExperimentManager] Head is now at the correct position to start measuring.");
                    
                    // Update cue when head is at correct position and wait for new trigger interaction 
                    cueManager.UpdateCueText(CueStates.Start);
                    triggerManager.ResetInteractionHappened();
                    
                    yield return new WaitForSeconds(.1f);
                }
            }
            
            // No trigger interaction appeared
            else
            {
                // Keep this coroutine running with a delay to reduce CPU load 
                yield return new WaitForSeconds(.1f);
            }
        }
    }
    
    
    // Coroutine for the practice start state 
    IEnumerator ExperimentStatePracticeEnd()
    {
        Debug.Log("[ExperimentManager] Now in state PracticeEnd.");
        
        // Update Cue text 
        cueManager.UpdateCueText(CueStates.PracticeEnd);
        
        // Display info for a few seconds before continuing 
        yield return new WaitForSeconds(configManager.informationalCuePresentationDuration);
        
        // Go to start state next  
        experimentState = ExperimentStates.Start;
                
        // Reset experiment state settings
        configManager.isInPractice = false; // Not in practice anymore 
        configManager.currentBlock = 1; // Reset current block 
        configManager.currentUtcon = 0; // Reset UTCON 
        triggerManager.ResetInteractionHappened(); // Reset input to none 
                
        // Start eye tracker validation and calibration 
        Debug.Log("[Experiment Manager] Starting Eye Tracker calibration and validation.");
        configManager.resumeExperimentAfterEyeTrackerCalibrationValidation = true;
        SceneManager.LoadScene(configManager.calibrationSceneEyeTrackerName);
                
        yield break; // break coroutine 
        
    }
    
    
    // Coroutine for the Practice state, do not measure in practice 
    IEnumerator ExperimentStatePractice()
    {
        Debug.Log("[ExperimentManager] Now in state Practice.");
        
        // Check if utcon index is valid 
        if (configManager.practiceUtconIdx < 0)
        {
            throw new Exception("UTCON index is negative!");
        }
        
        // Check if utcon index is outside of practice trial number range, in that case, assume practice end is reached
        if (configManager.practiceUtconIdx >= practiceFlowUtcons.Length)
        {
            Debug.Log("[ExperimentManager] UTCON index is greater than the number of practice trials. Assuming practice end is reached. Switching to experiment measuring section.");
            experimentState = ExperimentStates.PracticeEnd; // Go to practice end, reset experiment state variables there 
            experimentStateCoroutineIsStarted = false; // disable coroutine lock 
            yield break; // break coroutine 
        }
        
        // Check if utcon index indicates that block pause is reached 
        if (practiceBlockPausesIdx.Contains(configManager.practiceUtconIdx) && !resumingFromBlockPause)
        {
            Debug.Log("[ExperimentManager] UTCON index is in list of block pause indices, assuming block end is reached and block pause starts.");
            experimentState = ExperimentStates.BlockPause; // BlockPause is reached
            experimentStateCoroutineIsStarted = false; // disable coroutine lock 
            yield break; // break coroutine 
        }
        
        
        // Get current utcon 
        int currentUtcon = practiceFlowUtcons[configManager.practiceUtconIdx];
        
        // Update config manager with utcon 
        configManager.currentUtcon = currentUtcon;
        
        // Update cue text dependent on utcon 
        cueManager.UpdateCueTextFromUtcon(currentUtcon);
        
        // Wait cue displaying time before deactivating cue again 
        yield return new WaitForSeconds(configManager.cuePresentationDuration);
        cueManager.UpdateCueText(CueStates.Empty);
        
        // Wait before displaying tool 
        yield return new WaitForSeconds(configManager.delayBetweenCueAndToolPresentation);
        
        // Display tool on table dependent on utcon 
        toolManager.displayToolOnTable(currentUtcon);
        
        // Wait before playing beep sound 
        yield return new WaitForSeconds(configManager.toolPresentationDurationBeforeBeep);
        
        // Play beep 
        audioManager.PlayBeepSoundImmediately();
        
        // Reset input to none to make sure spam input is ignored 
        triggerManager.ResetInteractionHappened(); 
        
        // Run until trigger interaction appeared 
        while (true)
        {
            // Trigger interaction appeared
            if (triggerManager.GetInteractionHappened())
            {
                // Empty display of cue and tool and wait for a certain time between trials 
                toolManager.DisplayNoTool(); // Deactivate tool display 
                cueManager.UpdateCueText(CueStates.Empty); // Empty cue text
                yield return new WaitForSeconds(configManager.delayBetweenTriggerInteractionAndCuePresentation); // Pause between trials for a certain time 
                
                // Reset experiment states 
                triggerManager.ResetInteractionHappened(); // Reset input to none 
                experimentState = ExperimentStates.Practice; // Keep state in measuring 
                configManager.practiceUtconIdx += 1; // Increase utcon index to display next item in the next coroutine call 
                resumingFromBlockPause = false; // deactivate block pause lock 
                experimentStateCoroutineIsStarted = false; // disable coroutine lock 
                yield break; // break coroutine 
            }
            
            // No trigger interaction appeared
            else
            {
                // Keep this coroutine running with a delay to reduce CPU load 
                yield return new WaitForSeconds(.1f);
            }
        }
    }

    
    // Coroutine for the Measuring state 
    IEnumerator ExperimentStateMeasuring()
    {
        Debug.Log("[ExperimentManager] Now in state Measuring.");
        
        // Check if utcon index is valid 
        if (configManager.experimentUtconIdx < 0)
        {
            throw new Exception("UTCON index is negative!");
        }
        
        // Check if utcon index is outside of experiment trial number range, in that case, assume end is reached
        if (configManager.experimentUtconIdx >= experimentFlowUtcons.Length)
        {
            Debug.Log("[ExperimentManager] UTCON index is greater than the number of experiment trials. Assuming end is reached.");
            experimentState = ExperimentStates.End; // End is reached
            experimentStateCoroutineIsStarted = false; // disable coroutine lock 
            yield break; // break coroutine 
        }
        
        // Check if utcon index indicates that block pause is reached 
        if (experimentBlockPausesIdx.Contains(configManager.experimentUtconIdx) && !resumingFromBlockPause)
        {
            Debug.Log("[ExperimentManager] UTCON index is in list of block pause indices, assuming block end is reached and block pause starts.");
            experimentState = ExperimentStates.BlockPause; // BlockPause is reached
            experimentStateCoroutineIsStarted = false; // disable coroutine lock 
            yield break; // break coroutine 
        }
        
        
        // Get current utcon 
        int currentUtcon = experimentFlowUtcons[configManager.experimentUtconIdx];

        // Update config manager with utcon 
        configManager.currentUtcon = currentUtcon;
        
        // Start measuring right before displaying cue
        measurementManager.StartMeasurement();
        
        // Update cue text dependent on utcon 
        cueManager.UpdateCueTextFromUtcon(currentUtcon);
        
        // Wait cue displaying time before deactivating cue again 
        yield return new WaitForSeconds(configManager.cuePresentationDuration);
        cueManager.UpdateCueText(CueStates.Empty);
        
        // Wait before displaying tool 
        yield return new WaitForSeconds(configManager.delayBetweenCueAndToolPresentation);
        
        // Display tool on table dependent on utcon 
        toolManager.displayToolOnTable(currentUtcon);
        
        // Wait before playing beep sound 
        yield return new WaitForSeconds(configManager.toolPresentationDurationBeforeBeep);
        
        // Play beep 
        audioManager.PlayBeepSoundImmediately();
        
        // Reset input to none to make sure spam input is ignored 
        triggerManager.ResetInteractionHappened(); 
        
        // Run until trigger interaction appeared 
        while (true)
        {
            // Trigger interaction appeared
            if (triggerManager.GetInteractionHappened())
            {
                // Stop measuring right after trigger interaction happened
                measurementManager.StopMeasurement();
                
                // Empty display of cue and tool and wait for a certain time between trials 
                toolManager.DisplayNoTool(); // Deactivate tool display 
                cueManager.UpdateCueText(CueStates.Empty); // Empty cue text
                yield return new WaitForSeconds(configManager.delayBetweenTriggerInteractionAndCuePresentation); // Pause between trials for a certain time 
                
                // Reset experiment states 
                triggerManager.ResetInteractionHappened(); // Reset input to none 
                experimentState = ExperimentStates.Measuring; // Keep state in measuring 
                configManager.experimentUtconIdx += 1; // Increase utcon index to display next item in the next coroutine call 
                resumingFromBlockPause = false; // deactivate block pause lock 
                experimentStateCoroutineIsStarted = false; // disable coroutine lock 
                yield break; // break coroutine 
            }
            
            // No trigger interaction appeared
            else
            {
                // Keep this coroutine running with a delay to reduce CPU load 
                yield return new WaitForSeconds(.1f);
            }
        }
    }
    
    
    // Coroutine for the start state 
    IEnumerator ExperimentStateBlockPause()
    {
        Debug.Log("[ExperimentManager] Now in state BlockPause.");
        
        // Update Cue text 
        cueManager.UpdateCueText(CueStates.Pause);
        
        // Reset input to none to make sure spam input is ignored 
        triggerManager.ResetInteractionHappened(); 
        
        // Run until trigger interaction appeared 
        while (true)
        {
            // Trigger interaction appeared
            if (triggerManager.GetInteractionHappened())
            {
                // Increment current block number 
                configManager.currentBlock += 1;
                
                // Reset experiment states 
                triggerManager.ResetInteractionHappened(); // Reset input to none 
                configManager.currentUtcon = 0; // Reset UTCON 
                
                // Start eye tracker validation and calibration 
                Debug.Log("[Experiment Manager] Starting Eye Tracker calibration and validation.");
                configManager.resumeExperimentAfterEyeTrackerCalibrationValidation = true;
                SceneManager.LoadScene(configManager.calibrationSceneEyeTrackerName);
                
                // Break coroutine 
                yield break;  
            }
            
            // No trigger interaction appeared
            else
            {
                // Keep this coroutine running with a delay to reduce CPU load 
                yield return new WaitForSeconds(.1f);
            }
        }
    }
    
    
    // Coroutine for the end state 
    IEnumerator ExperimentStateEnd()
    {
        Debug.Log("[ExperimentManager] Now in state End.");
        
        // Update Cue text 
        cueManager.UpdateCueText(CueStates.End);
        
        // Write subject data to json 
        //measurementManager.WriteSubjectDataToJson();
        measurementManager.FinishSubjectData();
        
        // Signal to config manager that experiment is stopped 
        configManager.experimentIsRunning = false;
        
        // Reset Subject Data, make configuration of new Subject Data necessary
        configManager.subjectDataIsSet = false;

        // Display end text and wait for a few seconds and return into idle mode 
        yield return new WaitForSeconds(configManager.informationalCuePresentationDuration);
        
        // Reset experiment states 
        triggerManager.ResetInteractionHappened(); // Reset input to none 
        experimentState = ExperimentStates.Idle; // Change experiment state 
        experimentStateCoroutineIsStarted = false; // disable coroutine lock 
        
        // Update Cue text 
        cueManager.UpdateCueText(CueStates.Empty);
        
        // Show main menu in UI 
        uiManager.UpdateAndShowMainMenu();
    }
    
    
    // Update the scene setup 
    public void UpdateSceneConfiguration()
    {
      
        // Transform table and floor 
        AdjustTableAndFloor();
        
        // Adjust Player transform 
        playerManager.UpdatePlayerTransform();
        
        // Transform second view camera
        secondViewCameraManager.UpdateSecondViewCameraTransformAndViewport();
        
        // Transform head volume 
        headPositionVolumeManager.UpdateVolumeTransform();
        
        // Transform trigger 
        triggerManager.UpdateTriggerTransform();
        
        // Setup tools 
        toolManager.UpdateToolData();
        
        // Setup cue 
        cueManager.UpdateCueTransform();
        
        // Load practice flow utcons 
        LoadPracticeFlowUtcons();
        
        // Load experiment flow utcons 
        LoadExperimentFlowUtcons();
        
    }
    
    
    // Start Experiment 
    public void StartPracticeAndExperiment()
    {
        // Set experiment state to init 
        experimentState = ExperimentStates.Init;
    }
    
    
  
    // Load the Experiment Utcons for experiment flow  
    private void LoadExperimentFlowUtcons()
    {
        
        Debug.Log("[ExperimentManager] Loading experiment flow UTCONs from CSV.");
        
        // Get path
        string csvPath = GetExperimentFlowUtconsCsvPath();
        
        // Load experiment flow utcons and experiment block pauses indexes per pass by reference 
        LoadFlowUtcons(csvPath, out experimentFlowUtcons, out experimentBlockPausesIdx);
        
        Debug.Log("[ExperimentManager] Loaded experiment flow UTCONs from CSV.");
    }
    
    
    // Load the Practice Utcons for practice utcon flow  
    private void LoadPracticeFlowUtcons()
    {
        
        Debug.Log("[ExperimentManager] Loading practice flow UTCONs from CSV.");
        
        // Get path
        string csvPath = GetPracticeFlowUtconsCsvPath();
        
        // Load and write practice flow utcons and practice block pauses indexes per pass by reference 
        LoadFlowUtcons(csvPath, out practiceFlowUtcons, out practiceBlockPausesIdx);
        
        Debug.Log("[ExperimentManager] Loaded practice flow UTCONs from CSV.");
    }
    
    
    // Load the utcons from a utcon csv (for practice and experiment csv flow)   
    private void LoadFlowUtcons(string csvPath, out int[] flowUtcons, out int[] blockPausesIdx)
    {
        // Read in the lines 
        List<string> csvLines = GetComponent<CsvIO>().ReadCsvFromPath(csvPath);

        // Make sure that file exists, otherwise close application 
        if (csvLines == null)
        {
            // Quit Experiment through UiManager 
            uiManager.DisplayErrorMessageMenuWithMessage("[ExperimentManager] Could not find UTCON Flow File at \"" +
                                                         csvPath + "\". Exiting.");
            flowUtcons = null;
            blockPausesIdx = null;
            return;
        }
        
        // Store utcons from csv in list first and transform to array later  
        List<int> utcons = new List<int>();
        
        // Store block pause indicator idx in list and transform to array later 
        List<int> pauseIdx = new List<int>();
        
        // Split lines at comma and save name and ids into dictionary
        utconCount = 0;
        foreach (var line in csvLines)
        {
            // Read lines and line elements 
            try
            {
                var lineContents = line.Split(',');
                foreach (var lineElem in lineContents)
                {
                    // Found pause indicator 
                    if (lineElem.ToLower() == configManager.blockPauseIndicator)
                    {
                        // Add Pause idx to list, holds idx of utcon BEFORE which block pause happens 
                        pauseIdx.Add(utconCount);
                    }

                    // Found utcon
                    else
                    {
                        utcons.Add(int.Parse(lineElem));
                        utconCount += 1; // Save total number of utcons in csv file 
                    }
                }
                
            // Handle misformed lines     
            } catch (Exception e)
            {
                Debug.Log("[ExperimentManager] Malformed line " + line + ". Skipping.\n" + e.ToString());
            }
        }
        
        // Transform utcon list to array 
        flowUtcons = utcons.ToArray();
        
        // Transform pause indicator list to array
        blockPausesIdx = pauseIdx.ToArray();
    }
    
    
    
    
    
    
    
    // Get the CalibrationData from the Calibration Scene 
    public void AdjustTableAndFloor()
    {
        Debug.Log("[ExperimentManager] Adjusting table and floor transforms.");
        
        //Debug 
        //Vector3 tablePosition = new Vector3(0.9807393f, -0.04992759f,-0.3992041f);
        //Vector3 tableRotation = new Vector3 (0,-0.21f,0);
        //Vector3 tableRotation = new Vector3 (0,-25,0);
        //Vector3 tableScale = new Vector3(0.5007355f, 1.020325f, 0.3701493f);

        // Find game object of config manager 
        GameObject configManager = GameObject.FindWithTag("ConfigManager");
        
        // Get table and floor calibration from config manager 
        Vector3 tablePosition = configManager.GetComponent<ConfigManager>().tablePosition;
        Vector3 tableRotation = configManager.GetComponent<ConfigManager>().tableRotation;
        Vector3 tableScale = configManager.GetComponent<ConfigManager>().tableScale;
        float floorHeight = configManager.GetComponent<ConfigManager>().floorHeight;
        
        // If transform has not yet been calibrated, fall back to default scale 
        if (tableScale.x < 0.05 | tableScale.y < 0.05 | tableScale.z < 0.05)
        {
            Debug.Log("[ExperimentManager] Table and floor calibration seems to not yet have been set, using default scale.");
            tableScale = new Vector3(1,1,1);
        }
        
        // Adjust the table 
        tableManager.SetTableTransform(tablePosition, tableRotation, tableScale);
        
        // Adjust the floor (and walls)
        room.transform.position = new Vector3(0, floorHeight, 0);
    }

    
    // Get full filepath of tool name and id csv
    public string GetToolNameIdCsvPath()
    {
        return configManager.configFolderPath + "\\" + configManager.filenameToolNameIdCsv;
    }

    
    // Get full filepath of orientation/ cue names and ids csv 
    public string GetToolOrientationCueNamesIdsCsvPath()
    {
        return configManager.configFolderPath + "\\" + configManager.filenameToolOrientationCueNamesIdsCsv;
    }
    
    // Get full filepath of practice flow utcons 
    public string GetPracticeFlowUtconsCsvPath()
    {
        return configManager.configFolderPath + "\\" + configManager.filenamePracticeFlowUtconsCsv;
    }

    // Get full filepath of experiment flow utcons 
    public string GetExperimentFlowUtconsCsvPath()
    {
        return configManager.configFolderPath + "\\" + configManager.filenameExperimentFlowUtconsCsv;
    }
    
    
    // Get Experiment Utcon Info 
    public ExperimentUtconInfo GetExperimentUtconInfo()
    {
        return new ExperimentUtconInfo
        {
            // Get values from current files 
            utconsDuringMeasuringSection = experimentFlowUtcons,
            utconsDuringPracticeSection = practiceFlowUtcons,
            utconIdxBeforeWhichPauseHappensDuringMeasuring = experimentBlockPausesIdx,
            utconIdxBeforeWhichPauseHappensDuringPractice = practiceBlockPausesIdx
        };
    }
  
    // Struct that holds info about the UTCON structure of practice and measurement section 
    [Serializable]
    public struct ExperimentUtconInfo
    {
        public int[] utconsDuringMeasuringSection;
        public int[] utconsDuringPracticeSection;
        public int[] utconIdxBeforeWhichPauseHappensDuringMeasuring;
        public int[] utconIdxBeforeWhichPauseHappensDuringPractice;
    }
    
}



