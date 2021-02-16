/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolAttachmentPointInfo : MonoBehaviour
{
    
    // Attachment Point Pose changes depending on side of handle/ effector 
    public bool poseIsDependentOnPointOfInterestSide;
    
    // Attachment Point ID POI left
    public int attachmentPointIdPointOfInterestLeft;
    
    // Attachment Point ID POI right 
    public int attachmentPointIdPointOfInterestRight;

    // Attachment Point ID independent of POI 
    public int attachmentPointIdPointOfInterestIndependent;
    
    // Point of interest is Handle
    public bool pointOfInterestIsHandle;
    
    // Point of interest is Effector
    public bool pointOfInterestIsEffector;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
