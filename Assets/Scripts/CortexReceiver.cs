using System.IO;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

public class CortexReceiver : MonoBehaviour {

    public GameObject WebSocketManager;
    public GameObject CortexSender;

    WebSocket ws;

    string response;
    JObject Jresponse;

    string fileName;
    string fileLocation;
    string filePath;

    public string token;
    public string headset_id;

    public IEnumerator Init ()
    {
        Debug.Log("CortexReceiver start.");

        // EEG data writing.
        fileName = "Emotiv_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss_") + PlayerPrefs.GetString("Gender") + PlayerPrefs.GetString("Age") + ".csv";
        // fileLocation = @"C:\Users\Chester\Desktop\";
        fileLocation = @"C:\Users\nrobinson\Desktop\Chester\";
        filePath = fileLocation + fileName;
        
        // yield return new WaitForSeconds(0.2f);
        ws = WebSocketManager.GetComponent<WebSocketManager>().ws;

        while (true)
        {
            // Debug.Log("CortexReceiver loop start.");
            response = ws.RecvString();
            if (response != null)
            {
                Debug.Log("There is a response.");
                Jresponse = JObject.Parse(response);
                Debug.Log("Jresponse: " + Jresponse.ToString(Formatting.None));

                // Response from Authorize() request.
                // Getting authentication token.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 3)
                {
                    token = Jresponse["result"]["_auth"].ToString();
                }

                // Response from QueryHeadsets() request.
                // Getting headset id.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 4)
                {
                    headset_id = Jresponse["result"][0]["id"].ToString();
                }

                // Response from Subscribe() request.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 6)
                {
                    TextWriter writer = new StreamWriter(filePath, false);
                    for (int i = 0; i < 18; i++)
                    {
                        writer.Write(Jresponse["result"][0]["eeg"]["cols"][i].ToString() + ",");
                    }
                    writer.WriteLine();
                    writer.Close();
                }

                // Handling streaming EEG data.
                if (!Jresponse.ToString().Contains("result") && Jresponse.ToString().Contains("eeg"))
                {
                    TextWriter writer = new StreamWriter(filePath, true);
                    for (int i = 0; i < 18; i++)
                    {
                        writer.Write(Jresponse["eeg"][i].ToString() + ",");
                    }
                    writer.WriteLine();
                    writer.Close();
                }

                // Response from QuerySessions() request.
                if (Jresponse.ToString().Contains("result") && Int32.Parse(Jresponse["id"].ToString()) == 7)
                {
                    string file = @"C:\Users\nrobinson\Desktop\Chester\QuerySessions.txt";

                    TextWriter writer = new StreamWriter(file, false);
                    writer.WriteLine(response.ToString());
                    writer.Close();
                }
            }
            
            // Debug.Log("CortexReceiver loop end");
            yield return null;
        }
    }
}
