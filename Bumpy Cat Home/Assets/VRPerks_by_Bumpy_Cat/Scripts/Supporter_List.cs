
using BumpyCat;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using VRC.SDK3.Data;
public class Supporter_List : UdonSharpBehaviour
{   
    public TMP_Text body;
    public long roleID;

    public void Initialize(Supporter_Benefits main){
        body.text = main.Get_Supporters(roleID);
    }
}
