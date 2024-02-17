using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections;
using VRC.SDK3.Network;
using VRC.SDK3.StringLoading;
using System.Security.Policy;
using VRC.Udon.Common.Interfaces;
using System;
using UnityEngine.UI;
using Unity.Mathematics;



public class StockPriceCheckerUSharp : UdonSharpBehaviour
{    
    public Text text;
    public float requestRate = 5f;
    public float requestTime = 0f;
    public VRCUrl inputURL;

    private float lastPrice = 0.00f;
    private float currentPrice = 0.00f;

    private bool dataRecieved = false;
    private float dataAge = 0.00f;
    
    // Start is called before the first frame update

    public void Start()
    {
        VRCStringDownloader.LoadUrl(inputURL, (IUdonEventReceiver)this);
    }

    public void Update(){
        if(requestTime < requestRate){
            requestTime += Time.deltaTime;
        }
        else{
            requestTime = 0;
            VRCStringDownloader.LoadUrl(inputURL, (IUdonEventReceiver)this);
        }

        if(dataRecieved){
            dataAge += Time.deltaTime;
            statusColor();
        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload data){
        string result = data.Result;
        int x = result.IndexOf("data-pricehint=" + '"' + "2" + '"');
        string query = result.Substring(x, 50);
        int vstart = query.IndexOf("value")+7;
        string value = "";
        for(int i = 0; i < query.Length-1; i++){
            if(query[vstart+i] != '"'){
            value += query[vstart+i];
            }
            else{
                break;
            }
        }
        text.text = "$" + float.Parse(value).ToString("0.00");
        lastPrice = currentPrice;
        currentPrice = float.Parse(value);
        dataRecieved = true;

    }


    void statusColor(){
        if (dataAge < MathF.PI/3){
            float change = currentPrice - lastPrice;
            if(change > 0.00f){
                text.color = Color.Lerp(Color.yellow, Color.green, (Mathf.Cos(6*dataAge+MathF.PI)+1)/2);
            }
            else if(change < 0.00f){
                text.color = Color.Lerp(Color.yellow, Color.red, (Mathf.Cos(6*dataAge+MathF.PI)+1)/2);
            }
        }
        else{
            text.color = Color.yellow;
            dataRecieved = false;
            dataAge = 0;
        }
    }
}
