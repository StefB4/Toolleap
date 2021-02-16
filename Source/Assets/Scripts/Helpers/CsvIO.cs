/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CsvIO : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    // Read a CSV file from supplied path 
    // Returns individual lines 
    public List<string> ReadCsvFromPath(string csvPath)
    {
        
        // Check whether file at csvPath exists
        if (!File.Exists(csvPath))
        {
            Debug.Log("[CsvIO] File at " + csvPath.ToString() + " does not exist, not reading!");
            return null;
        }
        
        // Read csv at path using StreamReader
        List<string> lines = new List<string>();
        using (var streamReader = new StreamReader(csvPath))
        {
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();

                // Prevent reading of empty lines 
                if (!string.IsNullOrWhiteSpace(line) && !string.IsNullOrEmpty(line))
                {
                    lines.Add(line);
                }
            }
        }
    
        // Return list of lines 
        Debug.Log("[CsvIO] Read CSV at " + csvPath.ToString() + ".");
        return lines;
    }
    
    
    // Write a CSV file to supplied path 
    // Write string list elements seperated by specified seperator 
    // Write a new line, when "\n" appears
    public void WriteCsvFromElementListToPath(List<string> elementsList, string csvPath, string seperator = ",")
    {

        Debug.Log("[CsvIO] Writing CSV data at " + csvPath + " to disk.");

        // Make sure there are elements to be written 
        if (elementsList.Count < 1)
        {
            Debug.Log("[CsvIO] No content to write to disk, not writing!");
            return;
        }

        // Make sure folder exists 
        FileInfo fileInfo = new FileInfo(csvPath);
        fileInfo.Directory.Create(); // does nothing if already exists

        // Construct the full string to write to disk 
        string fullCsvContent = "";
        int elemCnt = 1;
        foreach (string element in elementsList)
        {
            // Add new line 
            if (element == "\n")
            {
                fullCsvContent += "\n";
            }

            // Add element and seperator
            else if (elemCnt < elementsList.Count)
            {
                fullCsvContent += element + seperator;
            }

            // Add only element for last element of list 
            else
            {
                fullCsvContent += element;
            }

            // Incremenent element count 
            elemCnt += 1; 
        }

        // Write csv content to disk  
        File.WriteAllText(csvPath, fullCsvContent);
        Debug.Log("[CsvIO] Writing CSV data to disk finished.");
    }
    
    
    // Write arbitrary text string to disk at path 
    // Creates directory of path if not existent 
    public void WriteArbitraryTextToDisk(string text, string path)
    {
        Debug.Log("[CsvIO] Writing arbitrary text to disk at " + path + ".");

        // Nothing to write
        if (String.IsNullOrEmpty(text) || String.IsNullOrWhiteSpace(text))
        {
            Debug.Log("[CsvIO] Text to write is empty, not writing.");
        }
        
        // Check if path is valid
        try
        {
            Path.GetFullPath(path);
        }
        catch (Exception e)
        {
            Debug.Log("[CsvIO] Path is not valid, not writing.\n" + e.ToString());
        }
        
        // Make sure directory exists 
        FileInfo fileInfo = new FileInfo(path);
        fileInfo.Directory.Create(); // does nothing if already exists
        
        // Write csv content to disk  
        File.WriteAllText(path, text);
        Debug.Log("[CsvIO] Writing arbitrary text to disk finished.");
    }
    
    
}
