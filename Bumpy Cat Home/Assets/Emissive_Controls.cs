
using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Emissive_Controls : UdonSharpBehaviour
{
    public Material material;
    public Renderer referenceRenderer;

    public Slider r;
    public Slider g;
    public Slider b;
    public Slider intesity;
    public void Emission_Toggle(){
       if(material.IsKeywordEnabled("_EMISSION")){
            material.DisableKeyword("_EMISSION");
        }
        else{
            material.EnableKeyword("_EMISSION");
        }
        RendererExtensions.UpdateGIMaterials(referenceRenderer);
    }

    public void Update_Color(){
        material.SetColor("_EmissionColor", new Color(r.value,g.value,b.value, 1) * intesity.value);
        RendererExtensions.UpdateGIMaterials(referenceRenderer);
    }
}
