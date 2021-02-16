/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Leap.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.VR;

public class UiManager : MonoBehaviour
{
    
    // ***
    // Variables used for all menus 
    [Header("All Menus")]

    // Experiment Manager
    public ExperimentManager experimentManager;
    
    // Config Manager 
    private ConfigManager configManager;
    
    // List of all menus
    private List<GameObject> listOfMenus;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // Game Manager
    public PlayerManager playerManager;

    // Tool Manager 
    public ToolManager toolManager;
    
    // Trigger Manager 
    public TriggerManager triggerManager;
    
    // Head Volume Manager
    public HeadPositionVolumeManager headVolumeManager;
    
   
    // ****
    // Variables related to Status Text 
    
    // Whole Status Overlay
    public GameObject statusOverlay;
    
    // Status Overlay Background
    public GameObject statusOverlayBackground;
    
    // Experiment Status Text 
    public Text experimentStatusText;
    
    // Save last known screen size to keep track of window resizes, x is width, y is height
    private Vector2Int currentWindowSize;

    // Second view camera raw image
    public RawImage secondViewCameraRawImage;
    
    // Second view camera info text
    public Text secondViewCameraInfoText;
    
    
    // ****
    // Variables belonging to Main Menu 
    [Header("Main Menu")] 
    
    // Main Menu 
    public GameObject mainMenu;
   
    // Main Menu Text
    public Text mainMenuText;
    
    // Main Menu Buttons
    public Button buttonCalibrateTableFloor;
    public Button buttonSetSubjectData;
    public Button buttonStartExperiment;
    public Button buttonHelpers;
    public Button buttonQuit;
    
    
    // ****
    // Variables belonging to Experiment is Running Menu 
    [Header("Experiment Is Running Menu")]

    // Experiment is running menu 
    public GameObject experimentIsRunningMenu;
    
    // Experiment is running text
    public Text experimentIsRunningText;
    
    
    // ****
    // Variables belonging to Subject Data Menu 
    [Header("Subject Data Menu")]

    // Subject data menu 
    public GameObject subjectDataMenu;
    
    // Subject ID input field
    public InputField subjectIdInput;
    
    // Subject Age Input field 
    public InputField subjectAgeInput;
    
    // Subject Gender Dropdown field 
    public Dropdown subjectGenderDropdown;
    
    // Subject Gender Dropdown field 
    public Dropdown subjectHandednessDropdown;

    // Save subject data button 
    public Button buttonSaveSubjectData;
    
    // Go back from subject data to main menu button 
    public Button buttonBackFromSubjectDataToMainMenu;
    
    // Set subject data menu text 
    public Text subjectDataMenuText;
    

     // ****
    // Variables belonging to Helpers Menu  
    [Header("Helpers Menu")]
    
    // Helpers Menu 
    public GameObject helpersMenu;

    // Helpers menu main text
    public Text helpersMenuText;

    // Generate UTCONs button 
    public Button buttonGenerateUtcons;

    // Generate Tool Info Files button
    public Button buttonGenerateToolInfos;

    // Calibrate Eye Tracking button
    public Button buttonCalibrateEyeTracking;

    // Switch to LeapMotion button
    public Button buttonSwitchToLeapInput;

    // Switch to SteamVR button 
    public Button buttonSwitchToSteamInput;

    // Toggle HeadVolume Visibility
    public Button buttonToggleHeadVolumeVisibility;
    
    // Display All Tools Sequentially
    public Button buttonDisplayAllToolsSequentially;
    
    // Show about menu 
    public Button buttonAbout;
    
    // Back to main menu button 
    public Button buttonBackFromHelpersMenuToMainMenu;

    
    // ****
    // Variables belonging to Generate Utcon Flow Menu  
    [Header("Generate Utcon Flow Menu")]

    // Generate Utcon Flow Menu  
    public GameObject generateUtconFlowMenu;
    
    // Seed input field
    public InputField utconFlowSeedInput;
    
    // Block number input field 
    public InputField utconFlowBlocksInput;
    
    // Save subject data button 
    public Button buttonSaveUtconFlowToDisk;
    
    // Go back from subject data to helpers menu button 
    public Button buttonBackFromUtconFlowToHelpersMenu;
    
    // Set subject data menu text 
    public Text utconFlowMenuText;
    
    // Utcon flow menu default text 
    public string utconFlowMenuDefaultText;
    
    
    // ****
    // Variables belonging to Display All Tools Menu  
    [Header("Display All Tools Menu")]

    // Display all tools menu 
    public GameObject displayAllToolsMenu;
    
    // Display previous tool
    public Button buttonDisplayPreviousTool;
    
    // Display next tool
    public Button buttonDisplayNextTool;
    
    // Go back from display all tools menu to helpers menu 
    public Button buttonBackDisplayAllToolsToHelpersMenu;
    
    // ****
    // Variables belonging to About Menu 
    [Header("About Menu")]

    // About menu 
    public GameObject aboutMenu;
    
    // Go back from about menu to helpers menu 
    public Button buttonBackAboutToHelpersMenu;
    
    
    // **** 
    // Variable belonging to error message menu 
    [Header("Error Message Menu")]
    
    // Error message Menu  
    public GameObject errorMessageMenu;
    
    // Error message menu text 
    public Text errorMessageMenuText;
    
    // Error message menu button to quit experiment 
    public Button buttonErrorMessageQuit;


    
    // Start is called before the first frame update
    void Start()
    {
        
        // Find config manager 
        configManager = GameObject.FindGameObjectWithTag(configManagerTag).GetComponent<ConfigManager>();
        
        // Get current window resolution 
        currentWindowSize = new Vector2Int(Screen.width, Screen.height);
        
        // Fill list of menus
        listOfMenus = new List<GameObject>();
        listOfMenus.Add(mainMenu);
        listOfMenus.Add(experimentIsRunningMenu);
        listOfMenus.Add(helpersMenu);
        listOfMenus.Add(generateUtconFlowMenu);
        listOfMenus.Add(subjectDataMenu);
        listOfMenus.Add(displayAllToolsMenu);
        listOfMenus.Add(aboutMenu);
        listOfMenus.Add(errorMessageMenu);

        // Setup listeners for the main menu buttons
        buttonCalibrateTableFloor.onClick.AddListener(ClickedButtonCalibrateTableFloor);
        buttonSetSubjectData.onClick.AddListener(ClickedButtonSetSubjectData);
        buttonStartExperiment.onClick.AddListener(ClickedButtonStartExperiment);
        buttonHelpers.onClick.AddListener(ClickedButtonHelpers);
        buttonQuit.onClick.AddListener(ClickedButtonQuit);

        // Setup listeners for set subject data buttons
        buttonSaveSubjectData.onClick.AddListener(ClickedButtonSaveSubjectData);
        buttonBackFromSubjectDataToMainMenu.onClick.AddListener(ClickedButtonBackFromSubjectDataToMainMenu);
        
        // Setup listeners for helpers menu 
        buttonGenerateUtcons.onClick.AddListener(ClickedButtonGenerateUtcons);
        buttonGenerateToolInfos.onClick.AddListener(ClickedButtonGenerateToolInfos);
        buttonCalibrateEyeTracking.onClick.AddListener(ClickedButtonCalibrateEyeTracker);
        buttonSwitchToLeapInput.onClick.AddListener(ClickedButtonSwitchToLeapInput);
        buttonSwitchToSteamInput.onClick.AddListener(ClickedButtonSwitchToSteamInput);
        buttonToggleHeadVolumeVisibility.onClick.AddListener(ClickedButtonToggleHeadVolumeVisibility);
        buttonDisplayAllToolsSequentially.onClick.AddListener(ClickedButtonDisplayAllToolsSequentially);
        buttonAbout.onClick.AddListener(ClickedButtonAbout);
        buttonBackFromHelpersMenuToMainMenu.onClick.AddListener(ClickedButtonBackFromHelpersMenuToMainMenu);

        // Setup listeners for generate utcon flow buttons
        buttonSaveUtconFlowToDisk.onClick.AddListener(ClickedButtonSaveUtconFlowToDisk);
        buttonBackFromUtconFlowToHelpersMenu.onClick.AddListener(ClickedButtonBackFromGenerateUtconsToHelpersMenu);
        
        // Setup listeners for display all tools menu buttons 
        buttonDisplayPreviousTool.onClick.AddListener(ClickedButtonPreviousTool);
        buttonDisplayNextTool.onClick.AddListener(ClickedButtonNextTool);
        buttonBackDisplayAllToolsToHelpersMenu.onClick.AddListener(ClickedButtonBackFromDisplayAllToolsToHelpersMenu);
        
        // Setup listener for about menu 
        buttonBackAboutToHelpersMenu.onClick.AddListener(ClickedButtonBackAboutToHelpersMenu);
        
        // Setup listener for error message menu button 
        buttonErrorMessageQuit.onClick.AddListener(ClickedButtonQuit);
        
        // Start Experiment Status Overlay 
        StartCoroutine("RefreshExperimentStatus");
        
        // Position Status Overlay depending on screen size 
        SetPositionOfStatusOverlay();
        
        
        // Update button clickability
        UpdateAndShowMainMenu();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
    
  
    // ** Main Menu 
    // Listener for button calibrate 
    void ClickedButtonCalibrateTableFloor()
    {
        Debug.Log("[UiManager] Got button click: Calibrate Table & Floor.");
        
        // Stop Experiment Status Overlay 
        StopCoroutine("RefreshExperimentStatus");
        
        // Change Scene; destroying player prefab is done by toggle in SteamVR_Behaviour Script in PlayerPrefab > SteamVRObjects > [SteamVR]  
        Debug.Log("[UiManager] Switching to Floor and Table Calibration Scene.");
        SceneManager.LoadScene(configManager.calibrationSceneTableFloorName);
    }
        
    // ** Main Menu 
    // Listener for button set subject data
    void ClickedButtonSetSubjectData()
    {
        Debug.Log("[UiManager] Got button click: Set Subject Data.");
        
        // Start Coroutine for input field check 
        StartCoroutine("InputCheckSetSubjectData");
        
        // Set gender dropdown entries 
        subjectGenderDropdown.interactable = true;
        subjectGenderDropdown.ClearOptions();
        subjectGenderDropdown.AddOptions(configManager.genders);
        
        // Set handedness dropdown entries
        subjectHandednessDropdown.interactable = true;
        subjectHandednessDropdown.ClearOptions();
        subjectHandednessDropdown.AddOptions(configManager.handednessOptions);
        
        // Change menu 
        ActivateMenu(subjectDataMenu);
    }
    

    // ** Main Menu 
    // Listener for button start experiment 
    void ClickedButtonStartExperiment()
    {
        Debug.Log("[UiManager] Got button click: Start Experiment.");
        
        // Start Experiment 
        experimentManager.StartPracticeAndExperiment(); 
        
        // Change menu 
        ActivateMenu(experimentIsRunningMenu);
    }

    // ** Main Menu 
    // Listener for button go to helpers
    void ClickedButtonHelpers()
    {
        Debug.Log("[UiManager] Got button click: Go to Helpers Menu.");
        
        // Start Coroutine to check which input options are available 
        StartCoroutine("InputCheckStatesForHelpersMenu");
               
        // Change menu 
        ActivateMenu(helpersMenu);
    }

    
    // ** Main Menu and error menu 
    // Listener for button quit 
    void ClickedButtonQuit()
    {
        Debug.Log("[UiManager] Got button click: Quit.");
        
        // Quit application 
        Debug.Log("[UiManager] Quitting.");
        Application.Quit();
    }
    
    // ** Main Menu 
    // Show and update main menu buttons (de)activation  
    public void UpdateAndShowMainMenu()
    {

        Debug.Log("[UiManager] Updating and Showing Experiment Main Menu.");

        // Get calibration states 
        bool tableIsCalibrated = configManager.tableIsCalibrated;
        bool floorIsCalibrated = configManager.floorIsCalibrated;
        bool subjectDataIsSet = configManager.subjectDataIsSet;

        // Set all buttons interactable and selectively deactivate below 
        buttonCalibrateTableFloor.interactable = true;
        buttonSetSubjectData.interactable = true;
        buttonStartExperiment.interactable = true;
        buttonQuit.interactable = true;

        // Table or floor or subject data not set, cannot start experiment 
        if (!tableIsCalibrated | !floorIsCalibrated | !subjectDataIsSet)
        {
            buttonStartExperiment.interactable = false;
        }

        // Make sure overlay is visible 
        statusOverlay.SetActive(true);
        foreach (Transform child in statusOverlay.transform.GetChildren())
        {
            child.gameObject.SetActive(true); // activate background and text and second camera view and info text 
        }

        // Show main menu 
        if (!configManager.experimentIsRunning)
        {
            ActivateMenu(mainMenu);
        }
        // Likely return from eye tracking calibration, displaying experiment menu 
        else
        {
            ActivateMenu(experimentIsRunningMenu);
        }
        
    }

    // ** All menus 
    // Activate specific menu, deactivate all others  
    void ActivateMenu(GameObject activeMenu = null)
    {
        Debug.Log("[UiManager] Activating menu " + activeMenu.ToString() + ".");
        
        // Disable all menus 
        foreach (var menuElem in listOfMenus)
        {
            menuElem.SetActive(false);
        }

        // Except for one
        if (activeMenu != null)
        {
            activeMenu.SetActive(true);
        }
    }
    
    // ** Set subject data menu 
    // Listener for button save subject data 
    void ClickedButtonSaveSubjectData()
    {
        Debug.Log("[UiManager] Updating subject data.");
        
        // Get subject id and age
        int id = 0;
        int age = 0;
        int.TryParse(subjectIdInput.text, out id);
        int.TryParse(subjectAgeInput.text, out age);
        
        // Get subject gender
        string gender = subjectGenderDropdown.options[subjectGenderDropdown.value].text;
        
        // Get subject handedness
        string handedness = subjectHandednessDropdown.options[subjectHandednessDropdown.value].text;
        
        // Update values in config manager 
        configManager.subjectAge = age;
        configManager.subjectId = id;
        configManager.subjectGender = gender;
        configManager.subjectHandedness = handedness;
        
        // Update handedness in multiple formats 
        if (configManager.subjectHandedness.ToLower().Contains("left"))
        {
            // Left Hand
            configManager.subjectHandednessSteamVrFormat = SteamVR_Input_Sources.LeftHand;
            configManager.subjectHandednessLeapFormat = 0;
        }
        else
        {
            // Right Hand 
            configManager.subjectHandednessSteamVrFormat = SteamVR_Input_Sources.RightHand;
            configManager.subjectHandednessLeapFormat = 1;
        }
        
        // Update trigger transform as handedness may have changed 
        triggerManager.UpdateTriggerTransform();
        
        // Update status of subject data set in config manager 
        configManager.subjectDataIsSet = true;
    }

    // ** Set subject data menu 
    // Listener for button back to main menu 
    void ClickedButtonBackFromSubjectDataToMainMenu()
    {
        Debug.Log("[UiManager] Got button click: Back to main menu.");

        // Stop coroutine that checks input fields
        StopCoroutine("InputCheckSetSubjectData");
        
        // Go back to main menu 
        UpdateAndShowMainMenu();
    }


    
    // ** Helpers Menu
    // Listener for button generate utcons
    void ClickedButtonGenerateUtcons()
    {
        Debug.Log("[UiManager] Got button click: Generate UTCON flow.");
        
        // Start Coroutine for input field check 
        StartCoroutine("InputCheckGenerateUtcons");
       
        // Update utcon menu text to default 
        utconFlowMenuText.text = utconFlowMenuDefaultText + "\n\n\n\n\n\n\n\n\n\n\n"; 
        
        // Change menu 
        ActivateMenu(generateUtconFlowMenu);
    }

    // ** Helpers Menu
    // Listener for button generate tool infos 
    void ClickedButtonGenerateToolInfos()
    {
        Debug.Log("[UiManager] Got button click: Generate Tool Info files.");

        // Generate Tool Info Files 
        toolManager.GenerateToolDetailsFile();
    }

    
    // ** Helpers Menu 
    // Listener for button calibrate eye tracker
    void ClickedButtonCalibrateEyeTracker()
    {
        Debug.Log("[UiManager] Got button click: Calibrate Eye-Tracker.");
        
        // Stop Experiment Status Overlay and check of input state 
        StopCoroutine("RefreshExperimentStatus");
        StopCoroutine("InputCheckStatesForHelpersMenu");
        
        // Change Scene; destroying player prefab is done by toggle in SteamVR_Behaviour Script in PlayerPrefab > SteamVRObjects > [SteamVR]  
        Debug.Log("[UiManager] Switching to Eye Tracking Calibration Scene.");
        SceneManager.LoadScene(configManager.calibrationSceneEyeTrackerName);
    }

    // ** Helpers Menu
    // Listener for button switch to leap motion input 
    void ClickedButtonSwitchToLeapInput()
    {
        Debug.Log("[UiManager] Got button click: Switch to Leap Motion Input.");

        //  Switch to Leap Input 
        playerManager.SwitchToLeapMotionInput();
    }

    // ** Helpers Menu
    // Listener for button switch to SteamVR input 
    void ClickedButtonSwitchToSteamInput()
    {
        Debug.Log("[UiManager] Got button click: Switch to SteamVR Input.");

        //  Switch to SteamVR Input  
        playerManager.SwitchToSteamVrInput();
    }
    
    // ** Helpers Menu
    // Listener for button Toggle Head Volume Visibility
    void ClickedButtonToggleHeadVolumeVisibility()
    {
        Debug.Log("[UiManager] Got button click: Toggle Head Volume Visibility.");
        
        // Toggle to opposite of current state 
        headVolumeManager.ToggleHeadVolumeVisibility();
    }

    // ** Helpers Menu
    // Listener for button Display All Tools Sequentially 
    void ClickedButtonDisplayAllToolsSequentially()
    {
        Debug.Log("[UiManager] Got button click: Display All Tools Sequentially.");
        
        // Start Displaying All Tools 
        toolManager.StartDisplayingAllTools();
        
        // Start Coroutine for input field check 
        StartCoroutine("ButtonAvailabilityCheckDisplayAllTools");

        // Change menu 
        ActivateMenu(displayAllToolsMenu);
    }
    
    
    // ** Helpers Menu 
    // Listener for button About 
    void ClickedButtonAbout()
    {
        Debug.Log("[UiManager] Got button click: About.");
        
        // Change menu 
        ActivateMenu(aboutMenu);
    }
    

    // ** Helpers Menu
    // Listener for button back to main menu  
    void ClickedButtonBackFromHelpersMenuToMainMenu()
    {
        Debug.Log("[UiManager] Got button click: Back to main menu.");

        // Stop coroutine that checks input fields
        StopCoroutine("InputCheckStatesForHelpersMenu");
        
        // Make sure head volume is not visible 
        headVolumeManager.SetHeadVolumeVisibility(false);
        
        // Go back to main menu 
        UpdateAndShowMainMenu();
    }


    // ** Generate utcon flow menu 
    // Listener for button save utcon flow to disk 
    void ClickedButtonSaveUtconFlowToDisk()
    {
        Debug.Log("[UiManager] Saving UTCON flow to disk.");
        
        // Get utcon flow seed and number of blocks
        int seed = 0;
        int blocks = 0;
        int.TryParse(utconFlowSeedInput.text, out seed);
        int.TryParse(utconFlowBlocksInput.text, out blocks);
        
        // Create path where to save utcon flow 
        string csvPath = configManager.configFolderPath;
        csvPath += "\\" + Path.GetFileNameWithoutExtension(configManager.filenameExperimentFlowUtconsCsv) + "_" + System.DateTime.Now.ToString("yyyy-MM-dd HH-mm").Replace(" ","_") + ".csv";
        
        // Generate UTCON flow and save to csv
        experimentManager.GetComponent<ExperimentUtconFlowGenerator>().GenerateUtconFlowAndWriteToDisk(blocks,seed,csvPath);

        // Update menu text 
        FileInfo csvFileInfo = new FileInfo(csvPath);
        utconFlowMenuText.text = "\n\n\n\n" + utconFlowMenuDefaultText + "\n\n\n\n\n\n\n\n\n\nSaved UTCON flow at " + csvFileInfo.Name 
            + "\nTo use the new UTCON flow rename the files accordingly and restart the application.";
    }
    
    
    // ** Generate utcon flow menu 
    // Listener for button back to helpers menu 
    void ClickedButtonBackFromGenerateUtconsToHelpersMenu()
    {
        Debug.Log("[UiManager] Got button click: Back to Helpers menu.");

        // Stop coroutine that checks input fields
        StopCoroutine("InputCheckGenerateUtcons");
        
        // Go back to helpers menu 
        ActivateMenu(helpersMenu);
    }
    
    // ** Display all tools menu 
    // Listener for button display previous tool 
    void ClickedButtonPreviousTool()
    {
        Debug.Log("[UiManager] Got button click: Request display of previous tool.");
        
        // Request display of previous tool
        toolManager.DisplayAllToolsRequestPreviousTool(); 
    }
    
    // ** Display all tools menu 
    // Listener for button display next tool 
    void ClickedButtonNextTool()
    {
        Debug.Log("[UiManager] Got button click: Request display of next tool.");
        
        // Request Display of next tool 
        toolManager.DisplayAllToolsRequestNextTool();
    }
    
    // ** Display all tools menu 
    // Listener for button back to helpers menu from display all tools menu 
    void ClickedButtonBackFromDisplayAllToolsToHelpersMenu()
    {
        Debug.Log("[UiManager] Got button click: Back to Helpers menu.");

        // Stop coroutine that checks available buttons
        StopCoroutine("ButtonAvailabilityCheckDisplayAllTools");
        
        // Stop Displaying
        toolManager.StopDisplayingAllTools();
        
        // Go back to helpers menu 
        ActivateMenu(helpersMenu);
    }
    
    // ** About Menu 
    // Listener for button back to helpers menu from about menu 
    void ClickedButtonBackAboutToHelpersMenu()
    {
        Debug.Log("[UiManager] Got button click: Back to Helpers menu.");

        // Go back to helpers menu 
        ActivateMenu(helpersMenu);
    }
    
    
    // ** Error message menu
    // Activate menu with error text
    public void DisplayErrorMessageMenuWithMessage(string message)
    {
        Debug.Log("[UiManager] Displaying error message.");

        // Adapt text of error message menu 
        errorMessageMenuText.text = message;
        
        // Change menu 
        ActivateMenu(errorMessageMenu);
    }
    
    // ** All menus  
    // Reposition Status Overlay; position anchor is top right corner and pivot is x=1, y=1
    public void SetPositionOfStatusOverlay()
    {
        Debug.Log("[UiManager] Adjusting position of experiment status overlay.");
        
        // Get padding values 
        float widthPadding = Math.Abs(configManager.paddingForTextOverlayWidth) * -1;
        float heightPadding = Math.Abs(configManager.paddingForTextOverlayHeight) * -1;

        // Reposition whole overlay
        statusOverlay.GetComponent<RectTransform>().anchoredPosition = new Vector2(widthPadding,heightPadding);
        
        // Determine to-be size, such that white background stretches all the way to the screen bottom with padding according to config manager
        // Use Screen.height for current window resolution 
        float totalHeight = Screen.height - 2 * configManager.paddingForTextOverlayHeight;
        float totalWidth = Screen.width * configManager.textOverlayWidthWindowPercentage / 100.0f;

        // Change size of background (+ padding/2 on each side) , text rect and whole overlay
        statusOverlayBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth + Math.Abs(widthPadding), totalHeight + Math.Abs(heightPadding));
        experimentStatusText.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth, totalHeight);
        statusOverlay.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth,totalHeight);
        
        // Set the second camera view Raw image size to overlay width * overlay width
        // Texture is quadratic, so width and height need to be the same
        // Part of the quadratic texture is not rendered, as the viewport of the camera is nut fully quadratic 
        secondViewCameraRawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth, totalWidth);
       
        // Set the info text position
        // Width is total width of overlay, height height of video + padding  
        secondViewCameraInfoText.GetComponent<RectTransform>().sizeDelta =
           new Vector2(totalWidth, totalWidth * configManager.secondViewCameraViewportHeightPercentage / 100.0f + Math.Abs(heightPadding));
       
        
        // Update current window size
        currentWindowSize.x = Screen.width;
        currentWindowSize.y = Screen.height;
    }
    
   
    // ** Set subject data 
    // Check whether input fields have values that are valid or not and disable save button accordingly 
    IEnumerator InputCheckSetSubjectData()
    {
        int id = 0;
        int age = 0;
        
        while (true)
        {
            // Check whether value in input field for id is valid 
            if (!int.TryParse(subjectIdInput.text, out id))
            {
                //Debug.Log("Input value for Subject ID is not an integer.");
                buttonSaveSubjectData.interactable = false;
            }
            // Check whether value in input field for age is valid 
            else if (!int.TryParse(subjectAgeInput.text, out age))
            {
                //Debug.Log("Input value for Subject age is not an integer.");
                buttonSaveSubjectData.interactable = false;
            }
            // Check whether id and age are non-negative
            else if (id < 0 | age < 0)
            {
                //Debug.Log("Input values for Subject ID and Subject age must be non-negative.");
                buttonSaveSubjectData.interactable = false;
            }
            // Valid inputs
            else
            {
                //Debug.Log("Valid input values for Subject Id and Subject age.");
                buttonSaveSubjectData.interactable = true;
            }
            
            // Pause for a short time and check again 
            yield return new WaitForSeconds(0.1f);
        }
    }

    // ** Helpers Menu 
    // Check whether SteamVR or Leap Motion are available/ activated and enable buttons accordingly
    IEnumerator InputCheckStatesForHelpersMenu()
    {
        // Init button availability 
        bool switchToLeapAvailable;
        bool switchToSteamVrAvailable;
        
        while (true)
        {
            // Init 
            switchToLeapAvailable = true;
            switchToSteamVrAvailable = true;

            //print(playerManager.IsLeapAvailable());
            
            // Leap button is not available if already in Leap Mode or if device is not connected 
            if (configManager.isUsingLeap | !playerManager.IsLeapAvailable())
            {
                switchToLeapAvailable = false;
            }
            
            // Steamvr button not available if already in SteamVR mode 
            if (!configManager.isUsingLeap)
            {
                switchToSteamVrAvailable = false;
            }
            
            // Update button availability
            buttonSwitchToLeapInput.interactable = switchToLeapAvailable;
            buttonSwitchToSteamInput.interactable = switchToSteamVrAvailable; 
            
            // Update menu every 0.5 seconds 
            yield return new WaitForSeconds(0.5f);
            
        } 

    }


    // ** Generate Utcon flow 
    // Check whether input fields have values that are valid or not and disable save button accordingly 
    IEnumerator InputCheckGenerateUtcons()
    {
        int seed = 0;
        int block = 0;
        
        while (true)
        {
            // Check whether value in input field for seed is valid 
            if (!int.TryParse(utconFlowSeedInput.text, out seed))
            {
                //Debug.Log("Input value for Utcon seed is not an integer.");
                buttonSaveUtconFlowToDisk.interactable = false;
            }
            // Check whether value in input field for block number is valid 
            else if (!int.TryParse(utconFlowBlocksInput.text, out block))
            {
                //Debug.Log("Input value for Utcon number of blocks is not an integer.");
                buttonSaveUtconFlowToDisk.interactable = false;
            }
            // Check whether block number is positive
            else if (block < 1)
            {
                //Debug.Log("Input value for Utcon number of blocks is not positive.");
                buttonSaveUtconFlowToDisk.interactable = false;
            }
            // Valid inputs
            else
            {
                //Debug.Log("Valid input values for Utcon seed and number of blocks.");
                buttonSaveUtconFlowToDisk.interactable = true;
            }
            
            // Pause for a short time and check again 
            yield return new WaitForSeconds(0.1f);
        }
        
    }
    
    // ** Display all tools 
    // Check the availability of buttons in display all tools menu 
    IEnumerator ButtonAvailabilityCheckDisplayAllTools()
    {
        while (true)
        {
            // Check if previous is available 
            if (toolManager.DisplayAllToolsIsPreviousAvailable())
            {
                buttonDisplayPreviousTool.interactable = true;
            }
            else
            {
                buttonDisplayPreviousTool.interactable = false;
            }
            
            // Check if next is available 
            if (toolManager.DisplayAllToolsIsNextAvailable())
            {
                buttonDisplayNextTool.interactable = true;
            }
            else
            {
                buttonDisplayNextTool.interactable = false;
            }
            
            // No change, do nothing
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    
    
    
    // ** Experiment Status
    // Update the experiment status overlay text 
    IEnumerator RefreshExperimentStatus()
    {
        while (true)
        {
            // Check whether resolution changed and reset overlay position and size
            if (currentWindowSize.x != Screen.width || currentWindowSize.y != Screen.height)
            {
                SetPositionOfStatusOverlay();
            }
            
            // Init experiment status text 
            string text = "";
            
            // Start 
            text += "<size=18><b>Experiment Status</b></size>\n\n";
            
            // Input Mode 
            if (configManager.isUsingLeap)
            {
                text += "Input Mode: Leap Motion\n\n";
            }
            else 
            {
                text += "Input Mode: Controllers (SteamVR)\n\n";
            }

            // Running & practicing
            text += "Running: " + configManager.experimentIsRunning.ToString() + "\n";
            text += "Practice: " + configManager.isInPractice.ToString() + "\n";
            
            // Infos on block, trial, utcon 
            if (!configManager.experimentIsRunning)
            {
                text += "\n\n\n\n\n\n"; // if not running, just empty lines
            }
            else if (configManager.isInPractice) // is running but in practice
            {
                // Extract tool name and cue orientation name
                string toolName = "";
                string cueOrientationName = "";
                try
                {
                    toolManager.NamesFromUtcon(configManager.currentUtcon, out toolName, out cueOrientationName);
                }
                catch (Exception e)
                {
                    if (configManager.currentUtcon != 0) // Make sure utcon has been set, prohibit spam in the console 
                    {

                        Debug.Log("[UiManager] Cannot extract names from UTCON " +
                                  configManager.currentUtcon.ToString() +
                                  ". Not displaying names in Status Overlay.");
                    }
                }
                // Continue building string
                text += "Block: " + configManager.currentBlock.ToString() + "\n";
                text += "UTCON: " + configManager.currentUtcon.ToString() + "\n";
                text += "Tool: " + toolName + "\n";
                text += "Cue & Orientation: " + cueOrientationName + "\n\n\n";
            }
            else // is running and measuring 
            {
                // Extract tool name and cue orientation name 
                string toolName = "";
                string cueOrientationName = "";
                try
                {
                    toolManager.NamesFromUtcon(configManager.currentUtcon, out toolName, out cueOrientationName);
                }
                catch (Exception e)
                {
                    if (configManager.currentUtcon != 0) // Make sure utcon has been set, prohibit spam in the console 
                    {
                        Debug.Log("[UiManager] Cannot extract names from UTCON " +
                                  configManager.currentUtcon.ToString() +
                                  ". Not displaying names in Status Overlay.");
                    }
                }

                // Continue building string 
                text += "Block: " + configManager.currentBlock.ToString() + "\n"
                    + "Trial (within block): " +  configManager.currentTrial.ToString() + "\n"
                    + "UTCON: " + configManager.currentUtcon.ToString() + "\n";
                text += "Tool: " + toolName + "\n";
                text += "Cue & Orientation: " + cueOrientationName + "\n\n";
            }
            
            // Table and floor calibration 
            text += "Table & Floor Calibrated: " +
                    (configManager.tableIsCalibrated && configManager.floorIsCalibrated).ToString() + "\n\n";

            // Subject ID, age, gender 
            if (configManager.subjectDataIsSet)
            {
                text += "Subject data set:  True\n";
                text += "ID: " + configManager.subjectId.ToString() + "\n"
                        + "Age: " + configManager.subjectAge.ToString() + "\n"
                        + "Gender: " + configManager.subjectGender.ToString() + "\n"
                        + "Handedness: " + configManager.subjectHandedness.ToString() + "\n";

            }
            else
            {
                text += "Subject data set:  False\n";
            }
            
            // FPS Counter
            text += "\nFPS: " + ((int)(1.0f / Time.deltaTime)).ToString() + "\n";

            
            // Update experiment status text
            experimentStatusText.text = text;

            // Wait for a second for reduced CPU load and refresh again 
            yield return new WaitForSeconds(configManager.fpsCounterRefreshRateInSeconds);
        }
    }
    
}
