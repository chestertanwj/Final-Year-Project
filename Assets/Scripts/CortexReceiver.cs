using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// Polling to receive only responses from authorization, subscription and streaming.

public class CortexReceiver : MonoBehaviour {

    public GameObject WebSocketManager;
    public GameObject CortexSender;

    WebSocket ws;

    string response;
    JObject Jresponse;

    string fileName;
    string fileLocation;
    public string filePath;

    public string token;

    public IEnumerator Init ()
    {
        Debug.Log("CortexReceiver start.");

        // EEG data writing.
        fileName = "Emotiv_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss_") + PlayerPrefs.GetString("Gender") + PlayerPrefs.GetString("Age") + ".csv";
        fileLocation = @"C:\Users\nrobinson\Desktop\Chester\";
        filePath = fileLocation + fileName;

        Debug.Log("Fetch websocket from WebSocketManager to CortexReceiver.");
        ws = WebSocketManager.GetComponent<WebSocketManager>().ws;

        while (true)
        {
            response = ws.RecvString();

            if (response != null)
            {
                Jresponse = JObject.Parse(response);
                Debug.Log("Response: " + Jresponse.ToString(Formatting.None));

                // Response from Authorize() request.
                // Get authentication token.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 2)
                {
                    token = Jresponse["result"]["_auth"].ToString();
                }

                // Response from Subscribe() request.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 4)
                {
                    WriteHeader();
                }

                // Handle streaming EEG data.
                if (!Jresponse.ToString().Contains("result") && Jresponse.ToString().Contains("eeg"))
                {
                    WriteData();
                }
            }
            yield return null;
        }
    }

    void WriteHeader ()
    {
        TextWriter writer = new StreamWriter(filePath, false);
        writer.Write("TIME,FRAME,");
        for (int i = 4; i < 17; i++)
        {
            writer.Write(Jresponse["result"][0]["eeg"]["cols"][i].ToString() + ",");
        }
        writer.Write("EVENT,");
        writer.Close();
    }

    void WriteData ()
    {
        TextWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine("");
        writer.Write(Time.time + "," + Time.frameCount + ",");
        for (int i = 4; i < 17; i++)
        {
            writer.Write(Jresponse["eeg"][i].ToString() + ",");
        }
        writer.Close();
    }
}
