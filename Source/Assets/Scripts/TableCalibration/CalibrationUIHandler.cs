/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CalibrationUIHandler : MonoBehaviour
{
    
    // ***
    // Variables used for all menus 
    [Header("All Menus")]
    
    // Canvas
    public GameObject canvas; 
    
    // CalibrationManager game object to access states 
    public GameObject calibrationManager;
    
    // CalibrationManager script
    private CalibrationManager calibrationManagerScript;
    
    // Config Manager
    private ConfigManager configManager;

    // List of all menus
    private List<GameObject> listOfMenus;
    
    
    // ****
    // Variables belonging to Main Menu 
    [Header("Main Menu")] 
    
    // Main Menu 
    public GameObject mainMenu;
    
    // Main Menu Text 
    public Text mainMenuText;
   
    // Main Menu Buttons
    public Button calibrateTableButton;
    public Button calibrateFloorButton;
    public Button saveCalibrationsButton;
    public Button loadCalibrationsButton;
    public Button backToExperimentButton;
    
    // Main menu default text 
    private string mainMenuDefaultText = "Table and Floor Calibration\n\nWhat do you wish to do?";
    
    
    // ****
    // Variables belonging to Table Calibration Explanation
    [Header("Table Calibration Info")]
    
    // TCE 
    public GameObject tableCalibrationExplanation;
    
    // Turn left button
    public Button turnPlayerLeftButton;
    
    // Turn right button
    public Button turnPlayerRightButton;
    
    
    // ****
    // Variables belonging to Floor Calibration Explanation 
    [Header("Floor Calibration Info")]
    
    // FCE
    public GameObject floorCalibrationExplanation; 
    
    
    // ****
    // Variables belonging to Save Calibrations Menu 
    [Header("Save Calibrations Menu")]

    // Save calibration menu 
    public GameObject saveCalibrationMenu;
    
    // Save calibrations input field
    public InputField saveCalibrationInput;
    
    // Save calibration Buttons 
    public Button saveButton;
    public Button backFromSaveToMainMenuButton;
    
    // Save calibration Menu text 
    public Text saveCalibMenuText;
    
    // Save calibration menu default text 
    private string saveCalibMenuDefaultText = "Please specify the name you wish to save the calibration under.";

    
    // ****
    // Variables belonging to Load Calibration Menu 
    [Header("Load Calibrations Menu")]
    
    // Load Calibration Menu 
    public GameObject loadCalibrationMenu; 
    
    // Load Calibrations DropDown
    public Dropdown loadCalibrationDropDown;
    
    // Load Calibration Buttons 
    public Button loadButton;
    public Button backFromLoadToMainMenuButton;
    
    // Load calibration menu text 
    public Text loadCalibMenuText;
    
    // Load calibration menu default text 
    private string loadCalibMenuDefaultText = "Please select the calibration you wish to load.";


    // Start is called before the first frame update
    void Start()
    {
        // Get CalibrationManagerScript
        calibrationManagerScript = calibrationManager.GetComponent<CalibrationManager>();
        
        // Find Config Manager 
        configManager = GameObject.FindWithTag(calibrationManagerScript.configManagerTag).GetComponent<ConfigManager>();
        
        // Fill list of menus
        listOfMenus = new List<GameObject>();
        listOfMenus.Add(mainMenu);
        listOfMenus.Add(tableCalibrationExplanation);
        listOfMenus.Add(floorCalibrationExplanation);
        listOfMenus.Add(saveCalibrationMenu);
        listOfMenus.Add(loadCalibrationMenu);
        
        // Setup listeners for the main menu buttons
        calibrateTableButton.onClick.AddListener(InitCalibrateTable);
        calibrateFloorButton.onClick.AddListener(InitCalibrateFloor);
        saveCalibrationsButton.onClick.AddListener(InitCalibrationSaving);
        loadCalibrationsButton.onClick.AddListener(InitCalibrationLoading);
        backToExperimentButton.onClick.AddListener(BackToExperiment);
        
        // Setup listener for the Table Calibration Info Buttons
        turnPlayerLeftButton.onClick.AddListener(GotClickTurnPlayerLeft);
        turnPlayerRightButton.onClick.AddListener(GotClickTurnPlayerRight);
        
        // Setup listeners for the save menu buttons and the input field
        saveButton.onClick.AddListener(SaveSpecifiedCalibration);
        backFromSaveToMainMenuButton.onClick.AddListener(BackToMainMenu);
        saveCalibrationInput.onValueChanged.AddListener(SaveCalibrationInputCheck);
        
        // Setup listeners for the load menu buttons 
        loadButton.onClick.AddListener(LoadSelectedCalibration);
        backFromLoadToMainMenuButton.onClick.AddListener(BackToMainMenu);
        
        // Update canvas texts
        UpdateMainMenu();
    }
    
    
    // ** Main Menu 
    // Initiate Table Calibration
    void InitCalibrateTable()
    {
        Debug.Log("Button pressed: Starting table calibration.");
        ActivateMenu(tableCalibrationExplanation);
        calibrationManagerScript.StartCalibration("table");
    }
    
    
    // ** Main Menu 
    // Initiate Floor Calibration
    void InitCalibrateFloor()
    {
        Debug.Log("Button pressed: Starting floor calibration.");
        ActivateMenu(floorCalibrationExplanation);
        calibrationManagerScript.StartCalibration("floor");
    }
    
    
    // ** Main Menu 
    // Show calibration saving options
    void InitCalibrationSaving()
    {
        Debug.Log("Button pressed: Show calibration saving menu.");
        
        // Make save button non-interactable at beginning 
        saveButton.interactable = false;
        
        // Activate menu 
        saveCalibMenuText.text = saveCalibMenuDefaultText; // default text 
        ActivateMenu(saveCalibrationMenu);
        
    }
    
    
    // ** Main Menu 
    // Show calibration loading options
    void InitCalibrationLoading()
    {
        Debug.Log("Button pressed: Show calibration loading menu.");
        
        // Load list of possible calibrations 
        string[] availableCalibs = calibrationManagerScript.ListAvailableCalibrationConfigurations();
        
        // Set dropdown entries 
        loadCalibrationDropDown.interactable = true;
        loadCalibrationDropDown.ClearOptions();
        List<string> availableCalibsList = availableCalibs.ToList();
        loadCalibrationDropDown.AddOptions(availableCalibsList);
        loadButton.interactable = true; // activate load button
        
        // Text to display in menu 
        string loadMenuTextDisplay = loadCalibMenuDefaultText;
        
        // No calibrations were found 
        if (availableCalibs.Length < 1)
        {
            loadCalibrationDropDown.interactable = false;
            loadButton.interactable = false;
            loadMenuTextDisplay += "\n\n\n\n\n\n\n\nNo calibrations found.";
        }
        
        // Activate menu 
        loadCalibMenuText.text = loadMenuTextDisplay; // show text 
        ActivateMenu(loadCalibrationMenu);
    }


    // ** Main Menu 
    // Go back to experiment
    void BackToExperiment()
    {
        Debug.Log("Button pressed: Back to Experiment.");
        
        // Save calibration to config manager 
        calibrationManagerScript.SaveCalibrationToConfigManager();
        
        // Change back to experiment scene, destroying camera rig is toggled in Steam VR_Behaviour script in [Camera Rig] Prefab
        SceneManager.LoadScene(configManager.experimentSceneName);

    }
    
    // ** Table Calibration Info 
    // Got button click to turn player left
    void GotClickTurnPlayerLeft()
    {
        Debug.Log("Button pressed: Turn Player Left.");
        calibrationManagerScript.TurnPlayerLeft();
    }
    
    // ** Table Calibration Info 
    // Got button click to turn player right
    void GotClickTurnPlayerRight()
    {
        Debug.Log("Button pressed: Turn Player Right.");
        calibrationManagerScript.TurnPlayerRight();
    }

    // ** Load Calibration Menu  
    // Load the calibration selected in drop down menu
    void LoadSelectedCalibration()
    {
        // Get selected calibration
        string selectedCalib = loadCalibrationDropDown.options[loadCalibrationDropDown.value].text;
        
        // Load calibration from disk
        bool loadSucceeded = calibrationManagerScript.LoadCalibrationFromDisk(selectedCalib);
        
        // Update load calibration menu text 
        if (loadSucceeded)
        {
            loadCalibMenuText.text =
                loadCalibMenuDefaultText + "\n\n\n\n\n\n\n\nLoaded calibration:\n" + selectedCalib; // load successfull
        }
        else // loading not successful 
        {
            loadCalibMenuText.text =
                loadCalibMenuDefaultText + "\n\n\n\n\n\n\n\nCould not load calibration from specified file!";
        }

    }


    // ** Load Calibration Menu and Save Calibration Menu 
    // Go back to main menu 
    void BackToMainMenu()
    {
        UpdateMainMenu();
    }
    
    
    // ** Save calibration Menu 
    // Save the calibration with specified calibration name 
    void SaveSpecifiedCalibration()
    {
        // Get the name from input field 
        string calibName = saveCalibrationInput.text;
        
        // Save calibration to disk
        string fullCalibFilePath = calibrationManagerScript.SaveCalibrationToDisk(calibName);
        
        // Update save calibration menu text 
        saveCalibMenuText.text =
            saveCalibMenuDefaultText + "\n\n\n\n\n\n\nSaved calibration with name:\n" 
                                     + Path.GetFileName(fullCalibFilePath);

    }
    
    // ** Save calibration menu 
    // Check whether input field has text or not and disable save button accordingly
    void SaveCalibrationInputCheck(string s)
    {
        if (string.IsNullOrEmpty(saveCalibrationInput.text) | string.IsNullOrWhiteSpace(saveCalibrationInput.text))
        {
            saveButton.interactable = false;
        }
        else
        {
            saveButton.interactable = true;
        }
    }
    
    
    
    // ** All 
    // Activate specific menu, deactivate all others  
    void ActivateMenu(GameObject activeMenu = null)
    {
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
    
       
    // ** All 
    // Update canvas texts 
    public void UpdateMainMenu()
    {
        // Get calibration states 
        bool tableWasCalibrated = calibrationManagerScript.GetTableCalibrationState();
        bool floorWasCalibrated = calibrationManagerScript.GetFloorCalibrationState();
        
        // Update canvas text 
        string menuText = mainMenuDefaultText + "\n\n\n\n\n\n\n\n\nCalibration States\nFloor calibrated: " 
                          + floorWasCalibrated.ToString()
                          + "\nTable calibrated: " + tableWasCalibrated.ToString();
        mainMenuText.text = menuText;
        
        // Set all buttons interactable and selectively deactivate below 
        calibrateTableButton.interactable = true;
        calibrateFloorButton.interactable = true;
        saveCalibrationsButton.interactable = true;
        loadCalibrationsButton.interactable = true;
        backToExperimentButton.interactable = true;
      
        // Floor was set, table was not yet set 
        // Disable saving
        if (floorWasCalibrated && !tableWasCalibrated)
        {
            saveCalibrationsButton.interactable = false;
            backToExperimentButton.interactable = false;
        }
        
        // Table was set and floor was not set (should not happen) 
        // Disable saving
        // Disable floor calibration, making it necessary to start at floor calibration 
        else if (tableWasCalibrated && !floorWasCalibrated)
        {
            saveCalibrationsButton.interactable = false;
            calibrateTableButton.interactable = false;
            backToExperimentButton.interactable = false;
        }
        
        // Table and floor were not set 
        // Disable table calibration to guarantee first calibrating floor
        // Disable saving 
        else if (!tableWasCalibrated && !floorWasCalibrated)
        {
            calibrateTableButton.interactable = false;
            saveCalibrationsButton.interactable = false;
            backToExperimentButton.interactable = false;
        }
        
        // Show main menu 
        ActivateMenu(mainMenu);
        
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
