
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class MirrorToggles : UdonSharpBehaviour
{
    public VRC_MirrorReflection target;
    
    public MeshRenderer meshRenderer;
    public LayerMask HQ;
    public LayerMask LQ;
    public LayerMask CUTOUT;

    private string current;

    public void Toggle_HQ(){
        if(current == "HQ"){ target.gameObject.SetActive(false); current = ""; return;}
        target.gameObject.SetActive(true);
        meshRenderer.material.SetFloat("_HideBackground", 0);
        target.m_ReflectLayers = HQ;
        current = "HQ";
    }
    public void Toggle_LQ(){
        if(current == "LQ"){ target.gameObject.SetActive(false); current = ""; return;}
        target.gameObject.SetActive(true);
        meshRenderer.material.SetFloat("_HideBackground", 0);
        target.m_ReflectLayers = LQ;
        current = "LQ";
    }
    public void Toggle_CUTOUT(){
        if(current == "CUTOUT"){ target.gameObject.SetActive(false); current = ""; return;}
        target.gameObject.SetActive(true);
        meshRenderer.material.SetFloat("_HideBackground", 1);
        target.m_ReflectLayers = CUTOUT;
        current = "CUTOUT";
    }
}
