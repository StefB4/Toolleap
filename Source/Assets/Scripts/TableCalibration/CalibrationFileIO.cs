/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CalibrationFileIO : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }

    
    // Write serialized data to disk (struct definition in CalibrationManager)
    // Returns the full file path 
    public string WriteJson(CalibrationData calibData)
    {
        // Construct an individual filename for the calibration data file
        // No ":" in windows filenames, date formatted accordingly 
        // Replace spaces with underscores 
        string fileName = "\\" + calibData.calibrationName + " " + calibData.date + ".json";
        fileName = fileName.Replace(" ", "_");
        
        // Extract the base filepath and append calibration specific name
        string filePath = calibData.directoryPath + fileName;
        
        // Make sure directory for file exists 
        FileInfo fileInfo = new FileInfo(filePath);
        fileInfo.Directory.Create();
        
        // Serialize calibration data 
        string serialized = JsonUtility.ToJson(calibData);
        
        // Write calibration data 
        Debug.Log("Writing calibration data at " + filePath + " to disk.");
        File.WriteAllText(filePath,serialized);
        Debug.Log("Writing calibration data to disk finished.");
        
        // Return full filepath 
        return filePath;
    }


    // List all available jsons at directory  
    public string[] ListAvailableCalibrationConfigurations(string directoryPath)
    {
        // Make sure directory exists
        FileInfo fileInfo = new FileInfo(directoryPath + "\\empty.ini");
        fileInfo.Directory.Create();
        
        // Find all jsons in directory 
        string[] jsonsLong = Directory.GetFiles(directoryPath, "*.json");
        string[] jsons = new String[jsonsLong.Length];

        // Keep only filenames of the jsons
        int cnt = 0;
        foreach (var jsonLong in jsonsLong)
        {
            jsons[cnt] = Path.GetFileName(jsonLong);
            cnt += 1;
        }
        
        // Return the json filenames 
        return jsons;
    }
    
    
    // Read serialized data from disk (struct definition in CalibrationManager)
    public CalibrationData ReadJson(string fullFilePath)
    {
        // Read from disk
        Debug.Log("Reading calibration data " + fullFilePath + " from disk.");
        string serialized = File.ReadAllText(fullFilePath);
        
        // Deserialize
        CalibrationData calibData = JsonUtility.FromJson<CalibrationData>(serialized);
        Debug.Log("Reading calibration data from disk finished.");

        // Return calibration data
        return calibData;
    }
}
