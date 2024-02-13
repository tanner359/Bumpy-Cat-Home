using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using System.Net;
using UnityEngine.Networking;
using System.Linq;
using BestHTTP.Extensions;

public class StockPriceChecker : MonoBehaviour
{
    public TMP_Text text;
    public float requestRate = 1.5f;
    public float requestTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RequestData());
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(requestTime > 0){
            requestTime -= Time.deltaTime;
            StartCoroutine(RequestData());
            return;
        }
        requestTime = requestRate;
        */
    }

    IEnumerator RequestData()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://finance.yahoo.com/webservice/v1/symbols/AAPL/quote?format=json&view=detail"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string result = www.downloadHandler.text;
                text.text = "SPDR S&P 500 ETF\n" + "$" + result;
                Debug.Log("Server Connection Success!");
            }
        }
    }
}
