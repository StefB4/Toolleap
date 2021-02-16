/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Valve.VR.InteractionSystem;

public class PlayerManager : MonoBehaviour
{
    // Config Manager
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // Leap XR Service Provider 
    public LeapXRServiceProvider leapServiceProvider;
    
    // Leap Controller
    private Leap.Controller leapController;

    // SteamVR Player
    public GameObject steamVrPlayer;

    // LeapMotion Player
    public GameObject leapMotionPlayer;
    
    // SteamVR right hand 
    public Hand steamVrRightHand;
    
    // SteamVR left hand
    public Hand steamVrLeftHand;
    
    // Lock for Hover Distance Update
    private bool lockedHoverDistanceUpdate;


    // Start is called before the first frame update
    void Start()
    {
        // Find config manager
        configManager = GameObject.FindGameObjectWithTag(configManagerTag).GetComponent<ConfigManager>();

        // Get Leap Controller 
        leapController = leapServiceProvider.GetLeapController();
        
        // Update hover distances lock
        lockedHoverDistanceUpdate = false;
        
        // Set input depending on configuration 
        if (configManager.isUsingLeap)
        {
            SwitchToLeapMotionInput();
        }
        else
        {
            SwitchToSteamVrInput();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update Hover Distances
        // Do in Update() to guarantee hands were loaded
        if (!lockedHoverDistanceUpdate)
        {
            UpdateHoverDistances();
            lockedHoverDistanceUpdate = true;
        }

    }
    
    // Update Hover Distances
    public void UpdateHoverDistances()
    {
        Debug.Log("[PlayerManager] Updating hand hover distances.");
        
        // Update SteamVR hands hover distances 
        steamVrRightHand.fingerJointHoverRadius = configManager.fingerJointHoverRadiusSteamVr;
        steamVrLeftHand.fingerJointHoverRadius = configManager.fingerJointHoverRadiusSteamVr;
    }
    
    // Update the player transform 
    public void UpdatePlayerTransform()
    {
        Debug.Log("[PlayerManager] Updating Player Transform.");
        
        // Update player rotation as set in config manager
        Vector3 currentPlayerRotation = transform.rotation.eulerAngles;
        currentPlayerRotation.y = configManager.playerRotationDegrees;
        transform.rotation = Quaternion.Euler(currentPlayerRotation);
    }

    // Switch to LeapMotion input 
    public void SwitchToLeapMotionInput()
    {
        Debug.Log("[PlayerManager] Switching to Leap Motion Input.");
        
         // Update status 
         configManager.isUsingLeap = true;

         // Switch Player prefab 
         steamVrPlayer.SetActive(false);
         leapMotionPlayer.SetActive(true);

    }

    // Switch to SteamVR input 
    public void SwitchToSteamVrInput()
    {
        Debug.Log("[PlayerManager] Switching to SteamVR Input.");
        
        // Update status
        configManager.isUsingLeap = false;

        // Switch Player prefab 
        leapMotionPlayer.SetActive(false);
        steamVrPlayer.SetActive(true);
    }

    // Is Leap Motion available ?
    public bool IsLeapAvailable()
    {
        return leapController.IsConnected;
    }
    
}
