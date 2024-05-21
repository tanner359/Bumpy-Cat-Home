
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using System;
using VRC.Udon.Common;
using UnityEngine.UI;
using VRC.SDK3.Components;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using VRC.SDK3.Data;
using VRC.Core;
using VRC.Udon.Wrapper.Modules;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;
using Unity.Collections;
using System.ComponentModel;
using VRC.Udon;
using UdonSharp.Internal;
using HarmonyLib;

namespace BumpyCat{
public class Supporter_Benefits : UdonSharpBehaviour
{
    //SUPPORT BENEFITS WORK BY: CROSS REFERENCING YOUR VRC+DIS ACCOUNT -> CHECKS FOR TYPE OF SUPPORTER ROLE -> PERKS ACTIVATE.

    public VRCUrl DISCORD_MEMBERS_URL; //https://discord.com/api/v10/guilds/1235352747923210331/members/search?query=bumpy_cat
    [Header("VRC INFO")]
    public string VRC_NAME;
    [Header("DISCORD INFO")]
    public string DISCORD_NAME;
    public string DISCORD_USER_NAME;
    public string[] DISCORD_ROLES;

    public bool SERVER_BOOSTER;

    [Header("ROLE ID'S")]
    public long SUPPORTER_ID = 1240128353604730930;
    public long SUPPORTER_PLUS_ID = 1240128353604730930;
    public long SUPPORTER_ULTIMATE_ID = 1240128353604730930;

    public string[] DISCORD_MEMBERS_NAMES = new string[0];

    // Start is called before the first frame update
    public void Start()
    {
        VRCStringDownloader.LoadUrl(DISCORD_MEMBERS_URL, (IUdonEventReceiver)this);
        Debug.Log($"[{gameObject.name}] Fetching Discord Members...");

        VRC_NAME = Networking.LocalPlayer.displayName;
    }

    public void Update(){
        
    }

    public override void OnStringLoadSuccess(IVRCStringDownload data){
        if(VRCJson.TryDeserializeFromJson(data.Result, out DataToken jsonData)){
            if(jsonData.TokenType == TokenType.DataDictionary){
                jsonData.DataDictionary.TryGetValue(1, out DataToken test);
                Debug.Log("JSON is type Dictionary: " + test);
            }
            else if(jsonData.TokenType == TokenType.DataList){ //datalist-> dictionaries -> datalist -> tokens -> data?
                //DISCORD_MEMBERS_NAMES = new string[jsonData.DataList.Count];
                for (int i = 0; i < jsonData.DataList.Count;){
                    DataToken a = jsonData.DataList[i];
                    if(a.TokenType == TokenType.DataDictionary){
                        if(a.DataDictionary.TryGetValue("user", TokenType.DataDictionary, out DataToken userData)){
                            DataToken GLOBAL_NAME = userData.DataDictionary["global_name"];
                            if(!GLOBAL_NAME.IsNull){
                                DISCORD_MEMBERS_NAMES = DISCORD_MEMBERS_NAMES.Array_Add_Item(GLOBAL_NAME.String);
                            }
                        }
                    }
                    i++;
                }
            }
        }
        else{
                Debug.Log($"Failed to Deserialize json {data.Result} - {jsonData.ToString()}");
        }
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.Log($"[{gameObject.name}] URL Could Not Be Loaded!");
    }

    /// <summary> Parses the rawData and returns a DataList containing all matching key values</summary>
    public DataList PARSE_JSON(DataToken rawData, string key){

        if (rawData.TokenType == TokenType.DataDictionary){
            
        }
        else if(rawData.TokenType == TokenType.DataList){

        }

        return null;
    }

}
}