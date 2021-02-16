/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EyeTrackingUiManager : MonoBehaviour
{
    
    // Config Manager 
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // Eye Tracking Manager
    public EyeTrackingManager eyeTrackingManager;
    
    // Status Overlay
    public GameObject statusOverlay;
    
    // Status Text 
    public Text statusText;
    
    // Buttons
    public Button buttonStartEyeCalibration;
    public Button buttonStartEyeValidation;
    public Button buttonBackToExperiment;
    public Button buttonOverride;
    public Button buttonReload;
    
    // Menus
    public GameObject eyeTrackingMenu;
    public GameObject eyeTrackingOverrideMenu;
    
    
    // Start is called before the first frame update
    void Start()
    {
        // Find config manager 
        configManager = GameObject.FindGameObjectWithTag(configManagerTag).GetComponent<ConfigManager>();

        // Setup listeners for the buttons
        buttonStartEyeCalibration.onClick.AddListener(ClickedButtonStartEyeCalibration);
        buttonStartEyeValidation.onClick.AddListener(ClickedButtonStartEyeValidation);
        buttonBackToExperiment.onClick.AddListener(ClickedButtonBackToExperiment);
        buttonOverride.onClick.AddListener(ClickedButtonOverride);
        buttonReload.onClick.AddListener(ClickedButtonReload);
        
        // Start Eye Tracking Calibration/ Validation status overlay
        StartCoroutine("RefreshEyeCalibrationValidationStatus");
        
        // Start Menu Interactability Check Coroutine 
        StartCoroutine("RefreshInteractability");
        
        // Position Status Overlay and Buttons depending on screen size 
        SetPositionOfStatusOverlay();
        SetPositionOfButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    // Button start Eye Tracking Calibration 
    void ClickedButtonStartEyeCalibration()
    {
        Debug.Log("[EyeTrackingUiManager] Got button click: Start Eye Tracking Calibration.");
        
        // Start Eye Calibration 
        eyeTrackingManager.LaunchEyeCalibration();
    }
    
    // Button start Eye Tracking Validation 
    void ClickedButtonStartEyeValidation()
    {
        Debug.Log("[EyeTrackingUiManager] Got button click: Start Eye Tracking Validation.");
        
        // Start Eye Tracking Validation 
        eyeTrackingManager.LaunchEyeValidation();
    }
    
    // Button go back to Experiment Menu 
    void ClickedButtonBackToExperiment()
    {
        Debug.Log("[EyeTrackingUiManager] Got button click: Back to Experiment.");
        
        // Stop Status Overlay Co-Routine
        StopCoroutine("RefreshEyeCalibrationValidationStatus");
        
        // Change back to experiment scene, destroying Payer game object is toggled in Steam VR_Behaviour script in [SteamVR] child 
        SceneManager.LoadScene(configManager.experimentSceneName);
    }
    
    // Button Override
    void ClickedButtonOverride()
    {
        Debug.Log("[EyeTrackingUiManager] Got button click: Override.");
        
        // Override eye tracking is calibrated and validated
        configManager.eyeTrackingIsCalibrated = true;
        configManager.eyeTrackingIsValidated = true;
        configManager.latestEyeTrackingValidationResults = new Vector3(float.NaN,float.NaN,float.NaN);
    }
    
    // Button Override
    void ClickedButtonReload()
    {
        Debug.Log("[EyeTrackingUiManager] Got button click: Reload.");
        
        // Reload eye tracker scene
        SceneManager.LoadScene(configManager.calibrationSceneEyeTrackerName);
    }
    
    
    // Reposition Status Overlay; position anchor is top right corner and pivot is x=1, y=1
    public void SetPositionOfStatusOverlay()
    {
        Debug.Log("[EyeTrackingUiManager] Adjusting position of eye tracking calibration status overlay.");
        
        // Get padding values 
        float widthPadding = Math.Abs(configManager.paddingForTextOverlayWidth) * -1;
        float heightPadding = Math.Abs(configManager.paddingForTextOverlayHeight) * -1;

        // Reposition 
        statusOverlay.GetComponent<RectTransform>().anchoredPosition = new Vector2(widthPadding,heightPadding);
    }
    
    
    // Reposition buttons (whole menu); anchor is top left and pivot is x=0, y=1
    public void SetPositionOfButtons()
    {
        Debug.Log("[EyeTrackingUiManager] Adjusting position of eye tracking options menu.");
        
        // Get padding values 
        float widthPadding = Math.Abs(configManager.paddingForTextOverlayWidth);
        float heightPadding = Math.Abs(configManager.paddingForTextOverlayHeight);
        
        // Reposition 
        eyeTrackingMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(widthPadding, heightPadding * -1);     
        eyeTrackingOverrideMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(widthPadding, heightPadding);    
        
    }
    
    
    // Activate and deactivate buttons depending on states 
    IEnumerator RefreshInteractability()
    {
        while (true)
        {
            
            
            // Deactivate all buttons, if calibration/ validation are running 
            if (eyeTrackingManager.calibrationIsRunning || eyeTrackingManager.validationIsRunning)
            {
                buttonStartEyeCalibration.interactable = false;
                buttonStartEyeValidation.interactable = false;
                buttonBackToExperiment.interactable = false;
            }

            else
            {
                // Activate Calibration Button 
                buttonStartEyeCalibration.interactable = true;
                
                // Deactivate validation button 
                if (!configManager.eyeTrackingIsCalibrated)
                {
                    buttonStartEyeValidation.interactable = false;
                }
                else
                {
                    buttonStartEyeValidation.interactable = true;
                }
                
                
                // Deactivate back to experiment button 
                if (!configManager.eyeTrackingIsCalibrated || !configManager.eyeTrackingIsValidated || !eyeTrackingManager.ValidationErrorWithinMargin()) 
                {
                    buttonBackToExperiment.interactable = false;
                }
                else
                {
                    buttonBackToExperiment.interactable = true;
                }
                
            }

            // Update menu every 0.8 seconds 
            yield return new WaitForSeconds(0.8f);
            
        }
        

    }
    

    // Update Eye Tracking Status Text 
    IEnumerator RefreshEyeCalibrationValidationStatus()
    {
        while (true)
        {
            // Construct Status text 
            string content = "<size=18><b>Eye-Tracking Calibration Status</b></size>\n\n\n";
            content += "Eye Tracking is calibrated: " + configManager.eyeTrackingIsCalibrated + "\n";
            content += "Eye Tracking is validated: " + configManager.eyeTrackingIsValidated + "\n\n";
            content += "Eye Tracking Calibration is running: " + eyeTrackingManager.calibrationIsRunning + "\n";
            content += "Eye Tracking Validation is running: " + eyeTrackingManager.validationIsRunning + "\n\n";
            content += "Number of calibration attempts: " + eyeTrackingManager.numberOfCalibrationAttempts.ToString() + "\n";
            content += "Number of validation attempts: " + eyeTrackingManager.numberOfValidationAttempts.ToString() + "\n\n";
            
            // Add validation results 
            if (float.IsNaN(configManager.latestEyeTrackingValidationResults.x)) // not yet set
            {
                content += "Latest validation results: - \n";
            }
            else // values set 
            {
                content += "Latest validation results (deviation in degrees): " + configManager.latestEyeTrackingValidationResults + "\n";
            }
            
            // Update status text 
            statusText.text = content; 
            
            // Wait for a short time before updating Status Text 
            yield return new WaitForSeconds(0.7f);
        }
    }
    
    
}
