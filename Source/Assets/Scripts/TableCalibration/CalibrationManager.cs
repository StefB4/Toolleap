/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class CalibrationManager : MonoBehaviour
{
    /*
     * Controls calibration process for floor and table. 
     */

    // ****
    // Variables related to input
    [Header("Controller input")]

    // action (Set to GrabPinch from default config)
    public SteamVR_Action_Boolean ButtonPressAction;

    // controllers 
    public GameObject leftController;
    public GameObject rightController;

    // name of the controller basepart/ very bottom tip (HTC Vive default is "base")
    public string controllerBasepartName = "base";

    // inputs (get from controllers)
    private SteamVR_Input_Sources leftHand;
    private SteamVR_Input_Sources rightHand;

    // render models 
    private GameObject leftControllerModel;
    private GameObject rightControllerModel;

    // controllerpositions (get from controllers Transform)
    private Vector3 leftControllerPosition;
    private Vector3 rightControllerPosition;


    // ****
    // Variables related to game objects 
    [Header("Game Objects")]

    // Spheres to attach to controllers
    public GameObject leftSphereAttach;
    public GameObject rightSphereAttach;

    // Text objects indicating left and right controller
    public GameObject leftTextAttach;
    public GameObject rightTextAttach;

    // Table
    public GameObject table;

    // CameraRig 
    public GameObject cameraRig;
    
    // Table Manipulation Script
    private CalibrationTableManipulator tableManipulatorScript;

    // Room
    public GameObject room;
    
    // UIHandler game object 
    public GameObject uiHandler; 
    
    // UIHandler script
    private CalibrationUIHandler uiHandlerScript;
    
    // Config Manager
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    

    // **** 
    // Variables determining the state of the calibration
    
    // Which state is the calibration in?
    private enum CalibrationStates { Waiting, Running, Finished };
    private CalibrationStates calibrationState = CalibrationStates.Finished;
    
    // Buttons down? 
    private bool leftIsReady = false;
    private bool rightIsReady = false;
    
    // Measuring floor or table? 
    private string measureMode; // "floor" or "table"
    
    
    // ****
    // Variables to measure the positions of the controllers
    [Header("Measuring")]

    // For a time window how long should calibration values be regarded (in seconds)?  --- from config manager
    private float calibrationTimeWindow;
    
    // Height offset to account for tip coordinates not being exactly flush with controller bottom (found -0.01f)  --- from config manager
    private float heightOffsetY;
    
    // Depth of the table  --- from config manager
    private float tableDepth;
    
    // How much time passed? 
    private float timer = 0.0f;
    
    // Lists to store measured positions
    private List<Vector3> leftControllerMeasurements = new List<Vector3>();
    private List<Vector3> rightControllerMeasurements = new List<Vector3>();
    
    // Calibration Results
    private Vector3 leftControllerPositionAverage;
    private Vector3 rightControllerPositionAverage;
    
    // Final corner values with added offset 
    private Vector3 leftCornerPosition;
    private Vector3 rightCornerPosition;
    
    // Final positions of floor points
    private Vector3 leftFloorPosition;
    private Vector3 rightFloorPosition;
    
    // Final position of floor
    private Vector3 floorPositionUpdated; 
    
    // Player Rotation 
    private float playerRotationDegrees;
    
    
    // ***
    // Variables related to history of calibration 
   
    // Remember whether table and floor were calibrated 
    private bool tableWasCalibrated = false;
    private bool floorWasCalibrated = false; 
    
    // Last calibration date
    private string lastCalibrationDate = ""; 
    
    // Where to store calibration data (for read and write) --- from config manager
    private string calibrationDataDirectoryPath;
    
    
    // ***
    // Methods 

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting Calibration Scene.");
        
        // Find Config Manager 
        configManager = GameObject.FindWithTag(configManagerTag).GetComponent<ConfigManager>();
        
        // Set input sources from controllers
        leftHand = leftController.GetComponent<SteamVR_Behaviour_Pose>().inputSource;
        rightHand = rightController.GetComponent<SteamVR_Behaviour_Pose>().inputSource;

        // Get controller render models
        leftControllerModel = leftController.transform.GetChild(0).gameObject;
        rightControllerModel = rightController.transform.GetChild(0).gameObject;
    
        // Get table manipulator script
        tableManipulatorScript = table.GetComponent<CalibrationTableManipulator>();
        
        // Get UIHandler script
        uiHandlerScript = uiHandler.GetComponent<CalibrationUIHandler>();
        
        // Update config from config manager 
        UpdateConfig();
        
        
        // ** Debug 
        //SetTestValues();
        //SaveCalibrationToDisk("test save");
        //LoadCalibrationFromDisk("test save 2020-04-01 20-31.json");
        //ListAvailableCalibrationConfigurations();

    }
    
    
    // Update configuration from config manager 
    void UpdateConfig()
    {
        Debug.Log("Updating Calibration Manager configuration from Config Manager.");
        
        // Update configs
        calibrationTimeWindow = configManager.calibrationTimeWindow;
        heightOffsetY = configManager.calibrationHeightOffset;
        tableDepth = configManager.calibrationTableDepth;
        calibrationDataDirectoryPath = configManager.calibrationDataFolderPath;
    }

    // Test calibration 
    void SetTestValues()
    {
        tableWasCalibrated = floorWasCalibrated = true;
        leftCornerPosition = new Vector3(1.484665f, 1.145234f, -0.179664f);
        rightCornerPosition = new Vector3(1.478626f, 1.139808f, 0.446821f);
        floorPositionUpdated = new Vector3(0, 0.1f, 0);
        lastCalibrationDate = System.DateTime.Now.ToString("yyyy-MM-dd HH-mm"); 
    }
    
    // Start Calibration in table or floor mode 
    public void StartCalibration(string measureModeArg)
    {
        // Check for valid passed measureMode
        if (measureModeArg != "floor" && measureModeArg != "table")
        {
            Debug.Log("Could not start calibration! Unknown measure mode.");
            return;
        }
        
        // Start calibration (queues and indicators are cleaned within calibration) 
        measureMode = measureModeArg;
        Debug.Log("Starting " + measureMode + " calibration.");
        calibrationState = CalibrationStates.Waiting;
    }
    
    
    // Update is called once per frame
    void Update()
    {
      
        // Update controller base (bottom tip) positions
        // Get controller-component transform (of base) from render model
        // Make sure controllers are present, try individually to display spheres and texts at least for one
        // Both controllers are need for calibration 
        
        // Left controller
        try
        {
            // Deactivate sphere and text first, in case controller is not present
            leftSphereAttach.SetActive(false);
            leftTextAttach.SetActive(false);
            
            // Get controller position 
            leftControllerPosition = leftControllerModel.GetComponent<SteamVR_RenderModel>()
                .GetComponentTransform(controllerBasepartName).position;
            
            // Attach sphere to controller and reactivate
            leftSphereAttach.transform.position = leftControllerPosition;
            leftSphereAttach.SetActive(true);
            
            // Attach text (L) to controller and reactivate
            leftTextAttach.transform.position = leftControllerPosition;
            leftTextAttach.transform.rotation = leftController.transform.rotation;
            leftTextAttach.SetActive(true);
            
        }
        catch (Exception e)
        {
            //Debug.Log("Left controller seems not to be connected!\n" + e.ToString());
        }
        
        // Right controller
        try
        {
            // Deactivate sphere and text first, in case controller is not present
            rightSphereAttach.SetActive(false);
            rightTextAttach.SetActive(false);
            
            // Get controller position 
            rightControllerPosition = rightControllerModel.GetComponent<SteamVR_RenderModel>()
                .GetComponentTransform(controllerBasepartName).position;
            
            // Attach sphere to controller and reactivate
            rightSphereAttach.transform.position = rightControllerPosition;
            rightSphereAttach.SetActive(true);
            
            // Attach text (R) to controller and reactivate 
            rightTextAttach.transform.position = rightControllerPosition;
            rightTextAttach.transform.rotation = rightController.transform.rotation;
            rightTextAttach.SetActive(true);
        }
        catch (Exception e)
        {
            //Debug.Log("Right controller seems not to be connected!\n" + e.ToString());
        }

        
        //
        // Calibration
        // States
        //
        
        // Do not do anything 
        if (calibrationState == CalibrationStates.Finished)
        { }

        // Recognizing buttons down for calibration to start
        else if (calibrationState == CalibrationStates.Waiting)
        {
            // Check state of buttons 
            if (ButtonPressAction.GetStateDown(rightHand)) //right pressed
            {
                Debug.Log("Right controller ready.");
                rightIsReady = true;
            }
            else if (ButtonPressAction.GetStateUp(rightHand)) // right released
            {
                Debug.Log("Right controller unready.");
                rightIsReady = false;
            }
            if (ButtonPressAction.GetStateDown(leftHand)) // left pressed 
            { 
                Debug.Log("Left controller ready.");
                leftIsReady = true;
            }
            else if (ButtonPressAction.GetStateUp(leftHand)) // left released 
            {
                 Debug.Log("Left controller unready.");
                 leftIsReady = false;
            }
            
            // Check whether both are down 
            if (rightIsReady && leftIsReady)
            {
                // Reset ready indicators for next run
                rightIsReady = leftIsReady = false;
                
                // Start calibration
                Debug.Log("Both controllers ready, starting calibration.");
                calibrationState = CalibrationStates.Running; // Change state
            }
        }

        
        // Run calibration, both buttons were pressed 
        else if (calibrationState == CalibrationStates.Running)
        {
            // Store controller positions
            leftControllerMeasurements.Add(leftControllerPosition);
            rightControllerMeasurements.Add(rightControllerPosition);
            
            // Update elapsed time 
            timer += Time.deltaTime; // in seconds

            // Measurement finished 
            if (timer > calibrationTimeWindow)
            {
                // Calculate average position of left controller 
                leftControllerPositionAverage = new Vector3();
                int amountLeft = leftControllerMeasurements.Count;
                foreach (Vector3 pos in leftControllerMeasurements)
                {
                    leftControllerPositionAverage += pos;
                }
                leftControllerPositionAverage = leftControllerPositionAverage / amountLeft;
                
                // Calculate average position of right controller 
                rightControllerPositionAverage = new Vector3();
                int amountRight = rightControllerMeasurements.Count;
                foreach (Vector3 pos in rightControllerMeasurements)
                {
                    rightControllerPositionAverage += pos;
                }
                rightControllerPositionAverage = rightControllerPositionAverage / amountRight;

                
                // Reposition table with the acquired values 
                if (measureMode == "table")
                {
                    // Calculate final corner positions including offset 
                    leftCornerPosition = leftControllerPositionAverage + new Vector3(0f, heightOffsetY, 0f);
                    rightCornerPosition = rightControllerPositionAverage + new Vector3(0f, heightOffsetY, 0f);

                    // Update table position
                    tableManipulatorScript.FitToFrontCornerPositions(leftCornerPosition, rightCornerPosition, tableDepth);

                    // Keep track of calibration
                    tableWasCalibrated = true;
                }
                
                // Adjust height of floor
                else if (measureMode == "floor")
                {
                    // Calculate final floor positions
                    leftFloorPosition = leftControllerPositionAverage + new Vector3(0f, heightOffsetY, 0f);
                    rightFloorPosition = rightControllerPositionAverage + new Vector3(0f, heightOffsetY, 0f);

                    // Update floor position
                    floorPositionUpdated = room.GetComponent<Transform>().position;
                    floorPositionUpdated.y = (leftFloorPosition.y + rightFloorPosition.y) / 2;
                    room.GetComponent<Transform>().position = floorPositionUpdated;

                    // Keep track of calibration; adjusting floor makes recalibrating table necessary
                    floorWasCalibrated = true;
                    tableWasCalibrated = false;
                }
                
                // Save last calibration date
                lastCalibrationDate = System.DateTime.Now.ToString("yyyy-MM-dd HH-mm"); 
                
                // Reset timer
                timer = 0.0f;
                
                // Reset controller position queues for next run
                leftControllerMeasurements.Clear();
                rightControllerMeasurements.Clear();
                
                // Update and show UI again
                uiHandlerScript.UpdateMainMenu();
                
                // Change state to idle 
                Debug.Log("Finished calibration.");
                calibrationState = CalibrationStates.Finished;
            }
        }
    }

    
    // Save calibration to disk with specific name  
    // Returns full filepath of saved calibration 
    public string SaveCalibrationToDisk(string calibrationName)
    {
        // Check whether calibration took place
        if (!(floorWasCalibrated && tableWasCalibrated))
        {
            Debug.Log("Cannot save data to disk, need to calibrate first.");
        }
        
        // Check that calibrationName does not have characters illegal for windows file names
        Regex illegalChars = new Regex(@"[\\/:*?""<>|]");
        calibrationName = illegalChars.Replace(calibrationName, "");    
        
        // Create new serializable struct of calibration data 
        CalibrationData calibData = new CalibrationData();
        calibData.calibrationName = calibrationName;
        calibData.date = lastCalibrationDate;
        calibData.directoryPath = calibrationDataDirectoryPath;
        calibData.leftCorner = leftCornerPosition;
        calibData.rightCorner = rightCornerPosition;
        calibData.floor = floorPositionUpdated;
        calibData.playerRotation = playerRotationDegrees;

        // Write to disk and get full file path 
        string fullFilePath = GetComponent<CalibrationFileIO>().WriteJson(calibData);
        
        // Return full filepath of calibration file
        return fullFilePath;
    }
    
    
    // Fetch list of available calibration config files at saving directory 
    public string[] ListAvailableCalibrationConfigurations()
    {
        // Return list of jsons available at configuration data path 
        return GetComponent<CalibrationFileIO>().ListAvailableCalibrationConfigurations(calibrationDataDirectoryPath);
    }
    

    // Load calibration from disk 
    // Returns true if successful, otherwise false 
    public bool LoadCalibrationFromDisk(string fileName)
    {
        // Combine file path of calibration file from directory and filename 
        string fullFilePath = calibrationDataDirectoryPath + "\\" + fileName;

        
        // Read from disk 
        CalibrationData calibData = GetComponent<CalibrationFileIO>().ReadJson(fullFilePath);
        
        // Unpack data 
        lastCalibrationDate = calibData.date;
        leftCornerPosition = calibData.leftCorner;
        rightCornerPosition = calibData.rightCorner;
        floorPositionUpdated = calibData.floor;
        playerRotationDegrees = calibData.playerRotation;

        // Do a sanity check on data to check whether provided file is valid (date null or empty or left and right corner are the same, for example 0,0,0)
        if (String.IsNullOrEmpty(lastCalibrationDate) | leftCornerPosition == rightCornerPosition)
        {
            // Loading was not successful
            Debug.Log("File does not hold calibration data, did not load calibration!");
            return false;
        }
        
        // Update player rotation 
        Vector3 currentPlayerRotation = cameraRig.transform.rotation.eulerAngles;
        currentPlayerRotation.y = playerRotationDegrees;
        cameraRig.transform.rotation = Quaternion.Euler(currentPlayerRotation);

        // Calibrate floor and table 
        room.GetComponent<Transform>().position = floorPositionUpdated;
        tableManipulatorScript.FitToFrontCornerPositions(leftCornerPosition, rightCornerPosition, tableDepth);
        floorWasCalibrated = true;
        tableWasCalibrated = true;

        // Loading successful
        return true;
    }

    
    // Getter for calibration state of floor
    public bool GetFloorCalibrationState()
    {
        return floorWasCalibrated;
    }
    
    // Getter for calibration state of table
    public bool GetTableCalibrationState()
    {
        return tableWasCalibrated;
    }
    
    // Turn player left
    public void TurnPlayerLeft()
    {
        Debug.Log("Turning Player Left.");
        
        // Change degree count and update rotation of camera prefab 
        playerRotationDegrees -= 90;
        playerRotationDegrees = playerRotationDegrees % 360;
        Vector3 rotation = cameraRig.transform.rotation.eulerAngles;
        rotation.y = playerRotationDegrees;
        cameraRig.transform.rotation = Quaternion.Euler(rotation);
    }
    
    // Turn player right
    public void TurnPlayerRight()
    {
        Debug.Log("Turning Player Right.");
        
        // Change degree count and update rotation of camera prefab 
        playerRotationDegrees += 90;
        playerRotationDegrees = playerRotationDegrees % 360;
        Vector3 rotation = cameraRig.transform.rotation.eulerAngles;
        rotation.y = playerRotationDegrees;
        cameraRig.transform.rotation = Quaternion.Euler(rotation);
    }

    // Save calibration data to config manager 
    public void SaveCalibrationToConfigManager()
    {
        Debug.Log("Saving table and floor calibration to Config Manager.");
        
        // Find game object of config manager 
        GameObject configManager = GameObject.FindWithTag("ConfigManager");

        // Get current table and floor calibration 
        Vector3 tablePosition = table.GetComponent<Transform>().position;
        Vector3 tableRotation = table.GetComponent<Transform>().rotation.eulerAngles;
        Vector3 tableScale = table.GetComponent<Transform>().localScale;
        float floorHeight = room.GetComponent<Transform>().position.y;
        
        // Update calibration data 
        configManager.GetComponent<ConfigManager>().tablePosition = tablePosition;
        configManager.GetComponent<ConfigManager>().tableRotation = tableRotation;
        configManager.GetComponent<ConfigManager>().tableScale = tableScale;
        configManager.GetComponent<ConfigManager>().floorHeight = floorHeight;
        configManager.GetComponent<ConfigManager>().floorIsCalibrated = floorWasCalibrated;
        configManager.GetComponent<ConfigManager>().tableIsCalibrated = tableWasCalibrated;
        configManager.GetComponent<ConfigManager>().playerRotationDegrees = playerRotationDegrees;
    }
    
    
}

// Struct to hold calibration data for easy jsonification
[Serializable]
public struct CalibrationData
{
    public string calibrationName;
    public string date;
    public string directoryPath;
    public Vector3 leftCorner;
    public Vector3 rightCorner;
    public Vector3 floor;
    public float playerRotation;
}