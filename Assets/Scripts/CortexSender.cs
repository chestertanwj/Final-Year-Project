﻿using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// Example Cortex Workflow: login, authenticate/authorize, query headset, create session, subscribe.
// Minimal Cortex Workflow: logout, login, authenticate/authorize, create session, subscribe.

// Request IDs
// Login:           1
// Logout:          2
// Authorize:       3
// QueryHeadsets:   4
// CreateSession:   5
// Subscribe:       6
// QuerySessions:   7

public class CortexSender : MonoBehaviour {

    public GameObject WebSocketManager;
    public GameObject CortexReceiver;

    WebSocket ws;

    string username;
    string password;
    string license;
    string client_id;
    string client_secret;

    string token;

    public IEnumerator Init()
    {
        Debug.Log("CortexSender start.");

        username = "asntchua";
        password = "Hesl1234";
        license = "a69442ad-6760-4d6d-9d36-0ad59bd45750";
        client_id = "sw37ALQ0sEXese8dA723pqracKIy49OSsYpCWezQ";
        client_secret = "Yt3BC6hXbJTwDneJOEjaXu2Q8rBEtjZsKG6Ria2E2rkTUUq795Uk0vroszf54J6OFoVHWfio8q8wJEUfyhaUoujCDDkrUHBjvW7xpEg5krg7wXofxu4Nxm5ouQGXdo6Y";

        Debug.Log("Fetching websocket from WebSocketManager.");
        ws = WebSocketManager.GetComponent<WebSocketManager>().ws;
        Debug.Log("Websocket from WebSocketManager fetched.");

        Debug.Log("Calling CortexReceiver.Init() coroutine.");
        StartCoroutine(CortexReceiver.GetComponent<CortexReceiver>().Init());
        Debug.Log("CortexReceiver.Init() coroutine called.");

        Logout();
        yield return new WaitForSeconds(5);
        Login();
        yield return new WaitForSeconds(5);
        Authorize();
        token = CortexReceiver.GetComponent<CortexReceiver>().token;
        yield return new WaitForSeconds(5);
        CreateSession();
        yield return new WaitForSeconds(5);
        Subscribe();
    }

    void Login()
    {
        Debug.Log("Login() called.");

        JObject request = new JObject();
        JProperty jsonrpc = new JProperty("jsonrpc", "2.0");
        request.Add(jsonrpc);
        JProperty method = new JProperty("method", "login");
        request.Add(method);

        JObject param = new JObject();
        JProperty username = new JProperty("username", this.username);
        param.Add(username);
        JProperty password = new JProperty("password", this.password);
        param.Add(password);
        JProperty client_id = new JProperty("client_id", this.client_id);
        param.Add(client_id);
        JProperty client_secret = new JProperty("client_secret", this.client_secret);
        param.Add(client_secret);
        request.Add("params", param);

        JProperty id = new JProperty("id", 1);
        request.Add(id);

        Debug.Log(request.ToString(Formatting.None));
        ws.SendString(request.ToString());
    }

    void Logout()
    {
        Debug.Log("Logout() called.");

        JObject request = new JObject();
        JProperty jsonrpc = new JProperty("jsonrpc", "2.0");
        request.Add(jsonrpc);
        JProperty method = new JProperty("method", "logout");
        request.Add(method);

        JObject param = new JObject();
        JProperty username = new JProperty("username", this.username);
        param.Add(username);
        request.Add("params", param);

        JProperty id = new JProperty("id", 2);
        request.Add(id);

        Debug.Log(request.ToString(Formatting.None));
        ws.SendString(request.ToString());
    }

    // Get authentication token.
    void Authorize()
    {
        Debug.Log("Authorize() called.");

        JObject request = new JObject();
        JProperty jsonrpc = new JProperty("jsonrpc", "2.0");
        request.Add(jsonrpc);
        JProperty method = new JProperty("method", "authorize");
        request.Add(method);

        JObject param = new JObject();
        JProperty username = new JProperty("client_id", this.client_id);
        param.Add(username);
        JProperty password = new JProperty("client_secret", this.client_secret);
        param.Add(password);
        JProperty client_id = new JProperty("license", this.license);
        param.Add(client_id);
        request.Add("params", param);

        JProperty id = new JProperty("id", 3);
        request.Add(id);

        Debug.Log(request.ToString(Formatting.None));
        ws.SendString(request.ToString());
    }

    // Create active session on default headset.
    void CreateSession()
    {
        Debug.Log("CreateSession() called.");

        JObject request = new JObject();
        JProperty jsonrpc = new JProperty("jsonrpc", "2.0");
        request.Add(jsonrpc);
        JProperty method = new JProperty("method", "createSession");
        request.Add(method);

        JObject param = new JObject();
        JProperty _auth = new JProperty("_auth", this.token);
        param.Add(_auth);
        JProperty status = new JProperty("status", "active");
        param.Add(status);
        request.Add("params", param);

        JProperty id = new JProperty("id", 5);
        request.Add(id);

        Debug.Log(request.ToString(Formatting.None));
        ws.SendString(request.ToString());
    }

    // Subscribe only EEG.
    public void Subscribe()
    {
        Debug.Log("Subscribe() called.");

        JObject request = new JObject();
        JProperty jsonrpc = new JProperty("jsonrpc", "2.0");
        request.Add(jsonrpc);
        JProperty method = new JProperty("method", "subscribe");
        request.Add(method);

        JObject param = new JObject();
        JProperty _auth = new JProperty("_auth", this.token);
        param.Add(_auth);
        string[] array = new string[1];
        array[0] = "eeg";
        JProperty streams = new JProperty("streams", array);
        param.Add(streams);
        request.Add("params", param);

        JProperty id = new JProperty("id", 6);
        request.Add(id);

        Debug.Log(request.ToString(Formatting.None));
        ws.SendString(request.ToString());
    }
}
