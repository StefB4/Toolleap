/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    // Config Manager
    private ConfigManager configManager;
    
    // Config Manager Tag
    public string configManagerTag;
    
    // Beep Audio Source
    public AudioSource beepAudioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        // Find config manager
        configManager = GameObject.FindGameObjectWithTag(configManagerTag).GetComponent<ConfigManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug 
        /*
        if (Input.GetKeyDown("space"))
        {
            print("Space");
            PlayBeepSoundImmediately();
        }
        */
        
    }
    
    // Play Beep Sound without delay
    public void PlayBeepSoundImmediately()
    {
        Debug.Log("[AudioManager] Playing Beep Audio Clip.");
        beepAudioSource.PlayOneShot(beepAudioSource.clip,1.0f);
    }


}
