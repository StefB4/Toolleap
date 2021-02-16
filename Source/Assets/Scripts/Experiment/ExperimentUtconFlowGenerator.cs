/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ExperimentUtconFlowGenerator : MonoBehaviour
{

    /*
     * UTCON structure: YYZ
     * YY is the tool id, Z is the cue orientation ID   
    */ 

    // List of tool and cue orientation ids to create utcons 
    private List<int> toolIds = new List<int>();
    private List<int> cueOrientationIds = new List<int>();

    // List of all possible utcons 
    private List<int> possibleUtcons = new List<int>();

    // Experiment Flow, list of strings of utcons and pause elements
    private List<string> fullExperimentUtconsFlow = new List<string>();

    // Last used seed for random 
    private int lastRandomSeed;
    
    // Tool Manager 
    private ToolManager toolManager;
    
    // Config Manager
    private ConfigManager configManager; 
    
    
    // Start is called before the first frame update
    void Start()
    {
        // Get Tool Manager from Experiment Manager 
        toolManager = GetComponent<ExperimentManager>().toolManager;
        
        // Find config manager
        configManager = GameObject.FindWithTag(GetComponent<ExperimentManager>().configManagerTag).GetComponent<ConfigManager>();
        
    }

    // Update is called once per frame
    void Update()
    {        
    }

    // Initialize Experiment Utcon Flow generator
    private void InitUtconFlowGenerator()
    {
        GetIds();
        GeneratePossibleUtcons();
    }


    // Process Tool IDs and Orientation Cue Ids
    private void GetIds()
    {
        // Get ToolIds from Tool Manager and make sure to exclude those which are marked to be excluded
        IEnumerable<int> toolsIdsEnumerable = from id in toolManager.GetToolIds().ToList()
            where !configManager.excludeToolIdsForUtconFlowCreation.Contains(id) select id;
        toolIds = toolsIdsEnumerable.ToList();

        foreach (var id in toolIds)
        {
            print(id);
        }
        
        // Get Cue Orientation Ids from Tool Manager 
        cueOrientationIds = toolManager.GetCueOrientationIds().ToList();

        /*
         // Debug 
        toolIds = new List<int>();
        toolIds.Add(10);
        toolIds.Add(11);
        toolIds.Add(12);
        toolIds.Add(13);
        toolIds.Add(14); 
        toolIds.Add(15);
        toolIds.Add(16);
        toolIds.Add(17);
        toolIds.Add(18);
        toolIds.Add(19);
        toolIds.Add(20);
        toolIds.Add(21);

        cueOrientationIds = new List<int>();
        cueOrientationIds.Add(1);
        cueOrientationIds.Add(2);
        cueOrientationIds.Add(3);
        cueOrientationIds.Add(4);
        */

    }
    
    
    // Generate Utcon Flow with specified number of blocks and a seed and write to disk 
    public void GenerateUtconFlowAndWriteToDisk(int numberOfBlocks, int seed, string csvPath)
    {
        // Init 
        InitUtconFlowGenerator();
        
        // Generate 
        GenerateExperimentFlow(numberOfBlocks, seed);
        
        // Write To Disk 
        WriteExperimentFlowToDisk(csvPath);
    }


    // Generate all possible UTCONs
    private void GeneratePossibleUtcons()
    {
        Debug.Log("[UtconFlowGenerator] Generating possible UTCONs.");

        // Calculate cartesian join of tool ids and cue orientation ids 
        var cartesianJoin = from tool in toolIds
                        from cueOrient in cueOrientationIds
                        select new {tool, cueOrient};

        // Init list of possible utcons 
        possibleUtcons = new List<int>();

        // Create utcon integer from cartesian join 
        foreach (var elem in cartesianJoin)
        {
            possibleUtcons.Add(elem.tool * 10 + elem.cueOrient);
        }
        Debug.Log("[UtconFlowGenerator] Created " + possibleUtcons.Count.ToString() + " possible UTCONs from " + toolIds.Count.ToString() + " tool IDs and " + cueOrientationIds.Count.ToString() + " cue and orientation IDs.");
    }


    // Generate experiment utcon flow with specified number of blocks and a seed 
    private void GenerateExperimentFlow(int numberOfBlocks, int seed)
    {
        Debug.Log("[UtconFlowGenerator] Generating experiment UTCON flow with " + numberOfBlocks.ToString() + " blocks and seed: " + seed.ToString());

        // Make sure numberOfBlocks is > 0 
        if (numberOfBlocks <= 0)
        {
            Debug.Log("[UtconFlowGenerator] Provided number of blocks " + numberOfBlocks.ToString() + " is not a positive integer, not generating experiment flow file!");
        }

        // Init experiment flow list 
        fullExperimentUtconsFlow = new List<string>();

        // Update last used seed 
        lastRandomSeed = seed;

        // Generate new random object 
        System.Random rnd = new System.Random(lastRandomSeed);

        // Generate shuffled trials for each block, init temporary shuffled utcons list and create full experiment flow list 
        int blockCnt = 1;
        List<int> shuffledUtcons;
        while (blockCnt <= numberOfBlocks)
        {
            // Shuffle utcons
            shuffledUtcons = new List<int>(possibleUtcons);
            Shuffle<int>(shuffledUtcons, rnd);

            // Add shuffled utcons to experiment flow list 
            foreach(int utcon in shuffledUtcons)
            {
                fullExperimentUtconsFlow.Add(utcon.ToString());
            }

            // Add pause indicator, except for after last block
            if (blockCnt < numberOfBlocks)
            {
                fullExperimentUtconsFlow.Add(configManager.blockPauseIndicator);
            }

            // Increment block counter 
            blockCnt += 1;
        }
        
        Debug.Log("[UtconFlowGenerator] Finished generating experiment UTCON flow.");
    }

    
    // Fisher-Yates Shuffle 
    // Shuffle list using instantiated random object 
    private void Shuffle<T>(List<T> list, System.Random rnd)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


    // Write experiment utcon flow to disk at specified path 
    private void WriteExperimentFlowToDisk(string csvPath)
    {
        Debug.Log("[UtconFlowGenerator] Saving experiment UTCON flow to disk.");

        // Add new lines to temporary before writing to disk for better readability 
        List<String> utconsWithNewlines = new List<string>();
        foreach (string elem in fullExperimentUtconsFlow)
        {
            utconsWithNewlines.Add(elem);
            if (elem == configManager.blockPauseIndicator)
            {
                utconsWithNewlines.Add("\n"); // after block pause indicators
            }
        }

        // Write CSV 
        GetComponent<CsvIO>().WriteCsvFromElementListToPath(utconsWithNewlines, csvPath);
    }
}
