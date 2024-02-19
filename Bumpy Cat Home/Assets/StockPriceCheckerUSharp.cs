﻿using UdonSharp;
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
using VRC.SDK3.Components;
using VRC.SDKBase.Editor.Source;
using JetBrains.Annotations;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using VRC.Udon.Editor.ProgramSources.UdonGraphProgram.UI;



public class StockPriceCheckerUSharp : UdonSharpBehaviour
{    
    public Text ticker;
    public Text price;
    public Text change;
    public Text tickerInputText;
    public float requestRate = 5f;
    public float requestTime = 0f;
    public VRCUrl inputURL;
    public VRCUrlInputField tickerInput;
    private float lastPrice = 0.00f;
    private float currentPrice = 0.00f;

    private bool dataRecieved = false;
    private float dataAge = 0.00f;
    
    // Start is called before the first frame update

    public void Start()
    {
        price.text = "Loading Data...";
        change.text = "Loading Data...";
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
        string newTicker = GetTickerName(result).ToString();
        string newPrice = GetStockPrice(result).ToString("0.00");
        string newChange = GetStockChange(result).ToString("0.00");
        string newPercent = GetStockPercentChange(newPrice, newChange).ToString("0.00");

        ticker.text = newTicker;
        price.text = "$" + GetStockPrice(result).ToString("0.00");
        change.text = newChange + " (" + newPercent + "%)";


        lastPrice = currentPrice;
        currentPrice = float.Parse(newPrice);
        dataRecieved = true;
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        base.OnStringLoadError(result);
        InputError();
    }

    private void InputError(){
        price.color = Color.red;
        change.color = Color.red;
        price.text = "Loading Failed!";
        change.text = "Ticker Not Found!";
    }

    public void QueryTickerInput(){
        //VRCStringDownloader.LoadUrl(tickerInput.GetUrl(), (IUdonEventReceiver)this);
    }

    private string GetTickerName(string rawData){
        int x = rawData.IndexOf("<title>");
        string query = rawData.Substring(x, 40);
        int vstart = query.IndexOf(">")+1;
        string value = "";
        for(int i = 0; i < query.Length-1; i++){
            if(query[vstart+i] != '<' && query[vstart+i] != '('){
            value += query[vstart+i];
            }
            else{
                break;
            }
        }
        return value;
    }
    private float GetStockPrice(string rawData){
        int x = rawData.IndexOf("data-field=\"regularMarketPrice\" data-trend=\"none\" data-pricehint=\"2\" value");
        string query = rawData.Substring(x, 100);
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
        return float.Parse(value);
    }
    private float GetStockChange(string rawData){
        int x = rawData.IndexOf("data-field=\"regularMarketChange\" data-trend=\"txt\" data-pricehint=\"2\" value");
        string query = rawData.Substring(x, 100);
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

        if(float.Parse(value) <= 0.00f){
            change.color = Color.red;
        }
        else{
            change.color = Color.green;
        }

        return float.Parse(value);
    }

    private float GetStockPercentChange(string price, string change){
        
        float open = float.Parse(price) + Mathf.Abs(float.Parse(change));
        float percent = float.Parse(change)/open * 100;

        return percent;
    }

    void statusColor(){
        if (dataAge < MathF.PI/3){
            float change = currentPrice - lastPrice;
            if(change > 0.00f){
                price.color = Color.Lerp(Color.yellow, Color.green, (Mathf.Cos(6*dataAge+MathF.PI)+1)/2);
            }
            else if(change < 0.00f){
                price.color = Color.Lerp(Color.yellow, Color.red, (Mathf.Cos(6*dataAge+MathF.PI)+1)/2);
            }
        }
        else{
            price.color = Color.yellow;
            dataRecieved = false;
            dataAge = 0;
        }
    }
}
