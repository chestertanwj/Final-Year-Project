using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

// Polling to receive only responses from authorization, subscription and streaming.

public class CortexReceiver : MonoBehaviour {

    public GameObject WebSocketManager;
    public GameObject CortexSender;

    public Text scoreText;

    WebSocket ws;

    string response;
    JObject Jresponse;

    string eegFileName;
    string eegFileLocation;
    public string eegFilePath;

    public string token;

    public IEnumerator Init ()
    {
        Debug.Log("CortexReceiver start.");

        // EEG data writing.
        eegFileName = "EEG_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss_") + PlayerPrefs.GetString("Gender") + PlayerPrefs.GetString("Age") + "_" + PlayerPrefs.GetString("Speed") + ".csv";
        eegFileLocation = @"C:\Users\nrobinson\Desktop\Chester\";
        eegFilePath = eegFileLocation + eegFileName;

        Debug.Log("Fetch websocket from WebSocketManager to CortexReceiver.");
        ws = WebSocketManager.GetComponent<WebSocketManager>().ws;

        while (true)
        {
            response = ws.RecvString();

            if (response != null)
            {
                Jresponse = JObject.Parse(response);
                Debug.Log("Response: " + Jresponse.ToString(Formatting.None));

                // Response from Logout() request.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 0)
                {
                    if (Jresponse.ToString().Contains("result"))
                    {
                        scoreText.text = "LOGGED OUT";
                    }
                    else if (Jresponse.ToString().Contains("error"))
                    {
                        scoreText.text = "ERROR";
                    }
                    
                }

                // Response from Login() request.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 1)
                {
                    if (Jresponse.ToString().Contains("result"))
                    {
                        scoreText.text = "LOGGED IN";
                    }
                    else if (Jresponse.ToString().Contains("error"))
                    {
                        scoreText.text = "ERROR";
                    }
                }

                // Response from Authorize() request.
                // Get authentication token.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 2)
                {
                    if (Jresponse.ToString().Contains("result"))
                    {
                        token = Jresponse["result"]["_auth"].ToString();
                        scoreText.text = "AUTHORIZED";
                    }
                    else if (Jresponse.ToString().Contains("error"))
                    {
                        scoreText.text = "ERROR";
                    }
                }

                // Response from CreateSession() request.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 3)
                {
                    if (Jresponse.ToString().Contains("result"))
                    {
                        scoreText.text = "SESSION CREATED";
                    }
                    else if (Jresponse.ToString().Contains("error"))
                    {
                        scoreText.text = "ERROR";
                    }
                }

                // Response from Subscribe() request.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 4)
                {
                    if (Jresponse.ToString().Contains("result"))
                    {
                        WriteEEGHeader();

                        scoreText.text = "SUBSCRIBED";
                    }
                    else if (Jresponse.ToString().Contains("error"))
                    {
                        scoreText.text = "ERROR";
                    }
                }

                // Handle streaming EEG data.
                if (!Jresponse.ToString().Contains("result") && Jresponse.ToString().Contains("eeg"))
                {
                    WriteEEGData();
                }
            }
            yield return null;
        }
    }

    void WriteEEGHeader ()
    {
        TextWriter writerEEGHeader = new StreamWriter(eegFilePath, false);
        writerEEGHeader.Write("TIME,FRAME,");
        for (int i = 3; i < 17; i++)
        {
            writerEEGHeader.Write(Jresponse["result"][0]["eeg"]["cols"][i].ToString() + ",");
        }
        writerEEGHeader.Write("EVENT,");
        writerEEGHeader.Close();
    }

    void WriteEEGData ()
    {
        TextWriter writerEEGdata = new StreamWriter(eegFilePath, true);
        writerEEGdata.WriteLine("");
        writerEEGdata.Write(Time.time + "," + Time.frameCount + ",");
        for (int i = 3; i < 17; i++)
        {
            writerEEGdata.Write(Jresponse["eeg"][i].ToString() + ",");
        }
        writerEEGdata.Close();
    }
}
