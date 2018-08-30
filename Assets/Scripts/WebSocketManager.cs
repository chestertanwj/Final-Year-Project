using System;
using System.Collections;
using UnityEngine;

public class WebSocketManager : MonoBehaviour {

    public GameObject CortexSender;

    public WebSocket ws;
    
    IEnumerator Start ()
    {
        Debug.Log("WebSocketManager start.");

        ws = new WebSocket(new Uri("wss://emotivcortex.com:54321"));

        Debug.Log("Calling ws.Connect() coroutine.");
        yield return StartCoroutine(ws.Connect());
        Debug.Log("ws.Connect() coroutine called.");

        Debug.Log("Calling CortexSender.Init() coroutine.");
        StartCoroutine(CortexSender.GetComponent<CortexSender>().Init());
        Debug.Log("CortexSender.Init() coroutine called.");

        Debug.Log("WebSocketManager end.");
    }
}
