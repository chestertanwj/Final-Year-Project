using System;
using System.Collections;
using UnityEngine;

public class WebSocketManager : MonoBehaviour {

    public GameObject CortexSender;
    public GameObject CortexReceiver;

    public WebSocket ws;
    
    IEnumerator Start ()
    {
        Debug.Log("WebSocketManager start.");

        ws = new WebSocket(new Uri("wss://emotivcortex.com:54321"));

        Debug.Log("Start ws.Connect() coroutine.");
        yield return StartCoroutine(ws.Connect());
        
        Debug.Log("Start CortexSender.Init() coroutine.");
        StartCoroutine(CortexSender.GetComponent<CortexSender>().Init());

        Debug.Log("Start CortexReceiver.Init() coroutine.");
        StartCoroutine(CortexReceiver.GetComponent<CortexReceiver>().Init());
    }
}
