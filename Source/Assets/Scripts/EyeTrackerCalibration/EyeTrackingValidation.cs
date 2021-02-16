/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Valve.VR.InteractionSystem;
using ViveSR.anipal.Eye;

public class EyeTrackingValidation : MonoBehaviour
{
    
    // Config Manager Tag
    public string configManagerTag;
    
    // Config Manager
    private ConfigManager configManager;
    
    // Eye Tracking Manager
    public EyeTrackingManager eyeTrackingManager; 
    
    
    // Positions of the validation balls
    public List<Vector3> keyPositions;

    // Delay before starting the validation
    public float delayBeforeValidationStart; 
    
    // Keeping track of Validation 
    private int validationPointIdx;
    private int validationTrial;
    
    // Keep track of when validation is finished 
    public bool validationFinished;
    
    // Store validation data before writing to disk 
    private EyeTrackingValidationData eyeTrackingValidationData;
    

    void Start()
    {
        // Find ConfigManager
        configManager = GameObject.FindGameObjectWithTag(configManagerTag).GetComponent<ConfigManager>();
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
        }
        yield break;
    }
    
    
    // Start validation
    public void StartValidation()
    {
        Debug.Log("[EyeTrackingValidation] Starting Validation.");
        gameObject.SetActive(true);
        validationFinished = false;
        StartCoroutine(Validate());
    }
    
    // Update config values after validation finished 
    public void UpdateValuesAfterValidationFinished()
    {
        gameObject.SetActive(false);
        validationFinished = true;
        Debug.Log("[EyeTrackingValidation] Validation finished.");
    }
    

    // Validation main loop 
    private IEnumerator Validate()
    {
        
        // Start, wait and setup 
        yield return new WaitForSeconds(delayBeforeValidationStart);
        List<float> anglesX = new List<float>();
        List<float> anglesY = new List<float>();
        List<float> anglesZ = new List<float>();
        validationTrial += 1;
        float startTime = Time.time;
        
        // Store all validation samples before writing to disk 
        eyeTrackingValidationData = new EyeTrackingValidationData();
        List<EyeTrackingValidationDataSample> validationDataPoints = new List<EyeTrackingValidationDataSample>();
        eyeTrackingValidationData.eyeTrackingValidationDataSamples = validationDataPoints;
        
        // Set meta data 
        eyeTrackingValidationData.subjectId = configManager.subjectId;
        eyeTrackingValidationData.blockNumber = configManager.currentBlock;
        eyeTrackingValidationData.validationAttemptNumber = eyeTrackingManager.numberOfValidationAttempts;
        eyeTrackingValidationData.dateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH-mm");
        
        // Move validation ball to first position, to make sure ball moves properly within loop 
        transform.position = Player.instance.hmdTransform.position + Player.instance.hmdTransform.rotation * keyPositions[0];
        transform.LookAt(Player.instance.hmdTransform);
        
        // Go through all validation ball positions and measure
        for (int i = 1; i < keyPositions.Count; i++)
        {
            
            // Within 1 second, move validation ball to next position 
            startTime = Time.time; // Time in seconds
            float timeDiff = 0;
            while (timeDiff < 1f)
            {
                transform.position = Player.instance.hmdTransform.position + Player.instance.hmdTransform.rotation * Vector3.Lerp(keyPositions[i-1], keyPositions[i], timeDiff / 1f); // move smoothly depending on time since start from previous to next position
                transform.LookAt(Player.instance.hmdTransform);
                yield return new WaitForEndOfFrame();
                timeDiff = Time.time - startTime;
            }
            
           
            // Measure Validation Samples for 3 seconds
            validationPointIdx = i;
            startTime = Time.time;
            timeDiff = 0;
            while (timeDiff < 3f)
            {
                // Move validation ball to designated position 
                transform.position = Player.instance.hmdTransform.position + Player.instance.hmdTransform.rotation * keyPositions[i] ;
                transform.LookAt(Player.instance.hmdTransform);
                
                // Get measurements and save to ValidationSample
                EyeTrackingValidationDataSample validationSample = GetValidationSample();
                anglesX.Add(validationSample.combinedEyeAngleOffset.x);
                anglesY.Add(validationSample.combinedEyeAngleOffset.y);
                anglesZ.Add(validationSample.combinedEyeAngleOffset.z);
                validationSample.validationResults.x = CalculateValidationError(anglesX);
                validationSample.validationResults.y = CalculateValidationError(anglesY);
                validationSample.validationResults.z = CalculateValidationError(anglesZ);
                    
                
                // Add sample to list of all samples
                validationDataPoints.Add(validationSample);
                
                yield return new WaitForEndOfFrame();
                timeDiff = Time.time - startTime;
            }
        }
        
        // Log validation Result overall  
        string validationResult = "(" + CalculateValidationError(anglesX).ToString("0.00") +
                                    ", " +
                                    CalculateValidationError(anglesY).ToString("0.00") +
                                    ", " +
                                    CalculateValidationError(anglesZ).ToString("0.00") + ")";
        Debug.Log(validationResult);
        
        // Save overall validation result 
        eyeTrackingValidationData.combinedEyeAngleOffsetValidationResultX = CalculateValidationError(anglesX);
        eyeTrackingValidationData.combinedEyeAngleOffsetValidationResultY = CalculateValidationError(anglesY);
        eyeTrackingValidationData.combinedEyeAngleOffsetValidationResultZ = CalculateValidationError(anglesZ);
        
        // Write resulting data to disk 
        WriteValidationDataToJson();
        
        // Update latest Validation Result in Config manager
        configManager.latestEyeTrackingValidationResults = new Vector3(eyeTrackingValidationData.combinedEyeAngleOffsetValidationResultX, eyeTrackingValidationData.combinedEyeAngleOffsetValidationResultY, eyeTrackingValidationData.combinedEyeAngleOffsetValidationResultZ);
        
        // Update config values after validation 
        UpdateValuesAfterValidationFinished();


    }


    // Calculate the validation error per list of angles as average of all measured angles in that list 
    private float CalculateValidationError(List<float> angles)
    {
        return angles.Select(f => f > 180 ? Mathf.Abs(f - 360) : Mathf.Abs(f)).Sum() / angles.Count; // orig 
    }

    
    // Generate an EyeTrackingValidationDataSample 
    private EyeTrackingValidationDataSample GetValidationSample()
    {
        Ray ray;

        EyeTrackingValidationDataSample sample = new EyeTrackingValidationDataSample();
        
        sample.validationPointIdx = validationPointIdx;
        
        var debText = "";

        sample.unixTimestamp = GetCurrentTimestamp(); //new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        sample.timestamp = Time.realtimeSinceStartup;

        var hmdTransform = Player.instance.hmdTransform;

        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out ray))
        {
            var angles = Quaternion.FromToRotation((transform.position - hmdTransform.position).normalized, hmdTransform.rotation * ray.direction)
                .eulerAngles;

            debText += "\nLeft Eye: " + angles + "\n";
            sample.leftEyeAngleOffset = angles;
        }

        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out ray))
        {
            var angles = Quaternion.FromToRotation((transform.position - hmdTransform.position).normalized, hmdTransform.rotation * ray.direction)
                .eulerAngles;
            debText += "Right Eye: " + angles + "\n";
            sample.rightEyeAngleOffset = angles;
        }

        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out ray))
        {
            var angles = Quaternion.FromToRotation((transform.position - hmdTransform.position).normalized, hmdTransform.rotation * ray.direction)
                .eulerAngles;
            debText += "Combined Eye: " + angles + "\n";
            sample.combinedEyeAngleOffset = angles;
        }
        
        
        // Update fields 
        sample.headTransform = Player.instance.hmdTransform;
        sample.pointToFocus = transform.position;

        
        return sample;

    }

    
    // Get current Timestamp 
    public double GetCurrentTimestamp()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (System.DateTime.UtcNow - epochStart).TotalSeconds;
    }


    // Write subject data to disk at specified path 
    public void WriteValidationDataToJson()
    {
        // Get the directory for the subject data from the config manager 
        string jsonDirectoryPath = configManager.subjectDataFolderPath;
        
        // Construct an individual filename for the eye tracking calibration data with subject id and block number 
        // Replace any empty spaces with underscores
        string fileName = "/EyeTrackingValidation/SubjectID_" + eyeTrackingValidationData.subjectId + "_BlockNumber_" + eyeTrackingValidationData.blockNumber + "_ValidationAttemptNumber_" + eyeTrackingValidationData.validationAttemptNumber + "_DateTime_" + eyeTrackingValidationData.dateTime + ".json";
        fileName = fileName.Replace(" ", "_");
        
        // Full filepath 
        string filePath = jsonDirectoryPath + fileName;
        
        // Make sure directory for file exists 
        FileInfo fileInfo = new FileInfo(filePath);
        fileInfo.Directory.Create();

        // Serialize subject data  
        string serialized = JsonUtility.ToJson(eyeTrackingValidationData,true);
        
        // Write subject data 
        Debug.Log("[EyeValidationSample] Writing eye tracking validation data at " + filePath + " to disk.");
        File.WriteAllText(filePath,serialized);
        Debug.Log("[EyeValidationSample] Writing eye tracking validation data to disk finished.");
    }
    
    
    // Holds data of a single eye tracking validation measurement 
    [Serializable]
    public struct EyeTrackingValidationDataSample
    {
        public int validationPointIdx;
        public double unixTimestamp;
        public float timestamp;
        public Transform headTransform;
        public Vector3 pointToFocus;
        public Vector3 leftEyeAngleOffset;
        public Vector3 rightEyeAngleOffset;
        public Vector3 combinedEyeAngleOffset;
        public Vector3 validationResults;
    }


    // Holds combined eye tracking validation data of an entire validation  
    [Serializable] 
    public struct EyeTrackingValidationData
    {
        public int subjectId;
        public int validationAttemptNumber;
        public int blockNumber;
        public string dateTime;
        public float combinedEyeAngleOffsetValidationResultX;
        public float combinedEyeAngleOffsetValidationResultY;
        public float combinedEyeAngleOffsetValidationResultZ;
        public List<EyeTrackingValidationDataSample> eyeTrackingValidationDataSamples;
    }
}

