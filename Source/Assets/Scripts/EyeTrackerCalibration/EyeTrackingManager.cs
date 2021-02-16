/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */
 
using System.Collections;
using UnityEngine;
using ViveSR.anipal.Eye;


public class EyeTrackingManager : MonoBehaviour
{

    // Tag of the ConfigManager 
    public string configManagerTag;
    
    // Config manager
    private ConfigManager configManager;
   
    // Eye Tracking Validation 
    public EyeTrackingValidation eyeTrackingValidation;

    // Keep track of number of calibrations and validations per scene call
    public int numberOfCalibrationAttempts;
    public int numberOfValidationAttempts;
    
    // Keep track of whether calibration/ validation is running 
    public bool calibrationIsRunning;
    public bool validationIsRunning;

    // Validation max error for validation to be accepted
    public float validationErrorMarginDegrees;
    

    private void Start()
    {
        // Find ConfigManager
        configManager = GameObject.FindGameObjectWithTag(configManagerTag).GetComponent<ConfigManager>();
        
        // Reset number of calibration/ validation attempts, count anew for each scene load  
        numberOfCalibrationAttempts = numberOfValidationAttempts = 0; 
        
        // Reset is calibrated/ validated
        configManager.eyeTrackingIsCalibrated = configManager.eyeTrackingIsValidated = false;
        
        // Reset latest validation result
        configManager.latestEyeTrackingValidationResults = new Vector3(float.NaN, float.NaN, float.NaN);
    }
    
    
    // Is validation error small enough?
    public bool ValidationErrorWithinMargin()
    {
        // Check whether calculated validation values are within error margin 
        if (configManager.latestEyeTrackingValidationResults.x > validationErrorMarginDegrees || configManager.latestEyeTrackingValidationResults.y > validationErrorMarginDegrees ||
            configManager.latestEyeTrackingValidationResults.z > validationErrorMarginDegrees)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    
    // Launch eye tracker calibration
    public void LaunchEyeCalibration()
    {
        Debug.Log("[EyeTrackingManager] Starting Eye Tracker Calibration.");
        
        // Increment number of calibration attempts 
        numberOfCalibrationAttempts += 1;
        
        // Keep track of calibration is running 
        calibrationIsRunning = true;
        
        // When running calibration, validation becomes necessary
        configManager.eyeTrackingIsValidated = false;
        
        // Launch 
        StartCoroutine("LaunchSRanipalCalibration");
    }
    
    
    // Coroutine of Launching Eye Calibration to prevent busy waiting
    IEnumerator LaunchSRanipalCalibration()
    {
        
        // Calibration successful 
        if (SRanipal_Eye_v2.LaunchEyeCalibration())
        {
            Debug.Log("[EyeTrackingManager] Eye Tracker Calibration was successful.");
            
            // Update config manager and calibration is running 
            configManager.eyeTrackingIsCalibrated = true;
        }
        
        // Calibration did not succeed
        else
        {
            Debug.Log("[EyeTrackingManager] Eye Tracker Calibration did not succeed.");
            
            // Update config manager and calibration is running 
            configManager.eyeTrackingIsCalibrated = false;
        }
        
        // Keep track of calibration is running 
        calibrationIsRunning = false;

        yield break;
    }
    
    
    // Launch eye tracker validation 
    public void LaunchEyeValidation()
    {
        Debug.Log("[EyeTrackingManager] Starting Eye Tracker Validation.");
        
        // Increment number of validation attempts 
        numberOfValidationAttempts += 1;
        
        // Keep track of running state
        validationIsRunning = true;
        
        // Reset latest validation results
        configManager.latestEyeTrackingValidationResults = new Vector3(float.NaN, float.NaN, float.NaN);
        
        // Start validation
        StartCoroutine("LaunchValidation");
    }

    
    // Coroutine to start the eye tracking validation 
    IEnumerator LaunchValidation()
    {
        // Start validation 
        eyeTrackingValidation.StartValidation();

        // Check whether validation finished 
        yield return new WaitForSeconds(1f);
        while (!eyeTrackingValidation.validationFinished)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        // Validation finished 
        validationIsRunning = false;
        configManager.eyeTrackingIsValidated = true;
        Debug.Log("[EyeTrackingManager] Finished Eye Tracker Validation.");

        yield break;
    }
}
