
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;


public class Local_Benefits : UdonSharpBehaviour
{

    public VRCUrl VRPERKS_URI;

    void Start()
    {
        VRCStringDownloader.LoadUrl(VRPERKS_URI, (IUdonEventReceiver)this);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload data){
        if(VRCJson.TryDeserializeFromJson(data.Result, out DataToken jsonData)){
            Debug.Log(jsonData.String.ToString());
        }
        Debug.Log("we downloaded " + data.Result.ToString());
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.Log($"[{gameObject.name}] URL Could Not Be Loaded!");
    }
}
