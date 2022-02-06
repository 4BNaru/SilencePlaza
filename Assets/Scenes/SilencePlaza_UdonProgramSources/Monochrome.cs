
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Monochrome : UdonSharpBehaviour
{
    public GameObject MC; 
    void Start()
    {
        
    }
    public void MonochromeIO()
    {
        if (MC.activeInHierarchy)
        {
            MC.SetActive(false);
        }
        else
        {
            MC.SetActive(true);
        }
    }
}

