/*
 * https://github.com/StefB4/Toolleap
 * 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTransformHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Calculate the axis aligned bounding box (aabb) of an object 
    // Extracts from the (mesh-)renderers of the object
    // Takes into account potential multiple meshes per object
    public Bounds GetBoundingBox(GameObject gameObject)
    {
        
        Debug.Log("[ObjectTransformTools] Calculating axis-aligned bounding box of game object \"" + gameObject.name + "\".");
        
        // Init bounds and get child renderers 
        Bounds bounds; // DO NOT INIT WITH NEW BOUNDS(), otherwise bounds will always include (0,0,0)
        Renderer[] childRenderers = gameObject.GetComponentsInChildren<Renderer>(); // all renderers in all (grand)children
        
        // Make sure renderers exist and calculate bounding box 
        if (childRenderers.Length > 0)
        {
            // Make bounds be first child's bounds 
            bounds = childRenderers[0].bounds; 
            
            // Extend first child's bounds
            foreach (Renderer childRenderer in childRenderers) 
            {
                bounds.Encapsulate(childRenderer.bounds);
            }
        }
        
        // Renderers do not exist in children 
        else
        {
            Debug.Log("[ObjectTransformTools] There are no children with renderers. Falling back to default new bounds.");
            bounds = new Bounds();
        }
        
        // Return 
        return bounds;
    }
    
    
    
    
    
    
}
