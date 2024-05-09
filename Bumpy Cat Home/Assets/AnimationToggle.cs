using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class AnimationToggle : UdonSharpBehaviour
{
    public Animator target;
    public string boolID;
    
    public void ToggleBoolValue(){
        bool x = target.GetBool(boolID);
        target.SetBool(boolID, !x);
    }
}
