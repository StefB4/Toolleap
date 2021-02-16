/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroyKeepOnlyOneInstanceByTag : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        // Check if GameObject with this tag already exists 
        GameObject[] objs = GameObject.FindGameObjectsWithTag(this.gameObject.tag);
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else // First instance of GameObject 
        {
            // Make sure GameObject does not get destroied between scene loads 
            DontDestroyOnLoad(this.gameObject);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
