using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;

public class MainManager : MonoBehaviour {

    public GameObject player;

    public GameObject centre;
    public GameObject leftGoal;
    public GameObject rightGoal;
    public GameObject leftLine;
    public GameObject rightLine;
    public GameObject leftMask;
    public GameObject rightMask;

    public Text displayText;

    private AudioSource audioSrc;

    // Trial management.
    private bool initRestOver;
    private int trialIndex;
    private int trialTotal;
    private bool trialActive;
    private bool moveActive;
    private List<int> trialList;

    // Score management.
    private float score;
    private List<float> scoreList;
    private List<float> idealXList;
    private List<float> xPosList;
    private List<float> yPosList;

    // Mocap data writing.
    private string fileName;
    private string fileLocation;
    private string filePath;

    // Debug.
    public Camera mainCamera;

    // Initialisation.
    void Start ()
    {
        Debug.Log("PREF" + PlayerPrefs.GetString("Gender"));
        // Mocap data writing.
        fileName = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss_") + PlayerPrefs.GetString("Gender") + PlayerPrefs.GetString("Age");
        // fileLocation = @"C:\Users\nrobinson\Desktop\Chester\"; // Lab use.
        fileLocation = @"C:\Users\Chester\Desktop\"; // Home use.
        filePath = fileLocation + fileName;
        WriteHeader(); // Write mocap data header.

        // Trial management.
        initRestOver = false;
        StartCoroutine(InitRest());
        trialIndex = 1;
        trialTotal = 20;
        trialActive = false;
        moveActive = false;
        trialList = new List<int>();
        for (int i = 1; i <= trialTotal; i++)
        {
            trialList.Add(i);
        }
        for (int j = 0; j < trialTotal; j++)
        {
            int temp = trialList[j];
            int randomIndex = UnityEngine.Random.Range(j, trialTotal);
            trialList[j] = trialList[randomIndex];
            trialList[randomIndex] = temp;
        }
        foreach (int k in trialList)
        {
            Debug.Log(k);
        }

        // Score management.
        scoreList = new List<float>();
        idealXList = new List<float>();
        xPosList = new List<float>();
        yPosList = new List<float>();

        // displayText.text = "";
        centre.SetActive(false);
        leftGoal.SetActive(false);
        rightGoal.SetActive(false);
        leftLine.SetActive(false);
        rightLine.SetActive(false);
        leftMask.SetActive(false);
        rightMask.SetActive(false);
        audioSrc = GetComponent<AudioSource>();
    }
    
    // Game logic.
    void Update ()
    {
        // Start a trial if one is not active.
        if (initRestOver == true && trialIndex <= trialTotal && trialActive == false)
        {
            StartCoroutine(Trial());
        }   

        // Write mocap data at every frame.
        WriteData();

        // Score management.
        if (moveActive == true)
        {
            xPosList.Add(player.transform.position.x);
            yPosList.Add(player.transform.position.y);
        }
        if (moveActive == true && (trialList[trialIndex-1] <= (trialTotal / 2)))
        {
            idealXList.Add(leftMask.transform.position.x + 8.5f);
        }
        else if (moveActive == true && (trialList[trialIndex-1] > (trialTotal / 2)))
        {
            idealXList.Add(rightMask.transform.position.x - 8.5f);
        }

        // End session.
        if (trialIndex == trialTotal)
        {
            displayText.text = "End";
        }

        // Quit application.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Debug.
        player.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 20.0f));
    }

    IEnumerator InitRest ()
    {
        displayText.text = "Rest";
        yield return new WaitForSeconds(10);
        displayText.text = "";
        initRestOver = true;
    }

    IEnumerator Trial ()
    {
        trialActive = true;
        // displayText.text = "Trial " + trialIndex.ToString(); // User is not supposed to know which trial they are at.
        audioSrc.Play();

        // Prepare for 5 seconds.
        centre.SetActive(true);
        yield return new WaitForSeconds(5);
        centre.SetActive(false);

        // displayText.text = ""; // User is not supposed to know which trial they are at.

        if (trialList[trialIndex-1] <= (trialTotal/2))
        {
            StartCoroutine(LeftMove());
        }
        else if (trialList[trialIndex-1] > (trialTotal/2))
        {
            StartCoroutine(RightMove());
        }
    }

    IEnumerator LeftMove ()
    {
        // Start left move.
        leftGoal.SetActive(true);
        leftLine.SetActive(true);
        leftMask.SetActive(true);
        moveActive = true;
        TextWriter writerLMS = new StreamWriter(filePath, true);
        writerLMS.WriteLine("Trial " + trialIndex.ToString() + ": Left move started.");
        writerLMS.Close();

        // Move left mask to reveal left line for ~5.333... seconds.
        while (leftMask.transform.position.x > -24.5f)
        {
            leftMask.transform.position += Vector3.left * 0.1f;
            yield return new WaitForSeconds(0.02f);
        }

        // Reset left mask position.
        leftMask.transform.position = new Vector3(-8.5f, 0.0f, 0.0f);

        // End left move.
        leftGoal.SetActive(false);
        leftLine.SetActive(false);
        leftMask.SetActive(false);
        moveActive = false;
        TextWriter writerLME = new StreamWriter(filePath, true);
        writerLME.WriteLine("Trial " + trialIndex.ToString() + ": Left move ended.");
        writerLME.Close();

        // Rest for 6 seconds.
        for (int i = 0; i < idealXList.Count; i++)
        {
             scoreList.Add(Mathf.Sqrt(Mathf.Pow((Mathf.Abs(idealXList[i] - xPosList[i])), 2) + Mathf.Pow(Mathf.Abs(yPosList[i]), 2)));
        }
        score = scoreList.Sum();
        displayText.text = score.ToString();
        yield return new WaitForSeconds(6);
        displayText.text = "";
        score = 0;
        scoreList.Clear();
        idealXList.Clear();
        xPosList.Clear();
        yPosList.Clear();

        // End of trial.
        trialIndex++;
        trialActive = false;
    }

    IEnumerator RightMove ()
    {
        // Start right move.
        rightGoal.SetActive(true);
        rightLine.SetActive(true);
        rightMask.SetActive(true);
        moveActive = true;
        TextWriter writerRMS = new StreamWriter(filePath, true);
        writerRMS.WriteLine("Trial " + trialIndex.ToString() + ": Right move started.");
        writerRMS.Close();

        // Move right mask to reveal right line for ~5.333... seconds.
        while (rightMask.transform.position.x < 24.5f)
        {
            rightMask.transform.position += Vector3.right * 0.1f;
            yield return new WaitForSeconds(0.02f);
        }

        // Reset right mask position.
        rightMask.transform.position = new Vector3(8.5f, 0.0f, 0.0f);

        // End right move.
        rightGoal.SetActive(false);
        rightLine.SetActive(false);
        rightMask.SetActive(false);
        moveActive = false;
        TextWriter writerRME = new StreamWriter(filePath, true);
        writerRME.WriteLine("Trial " + trialIndex.ToString() + ": Right move ended.");
        writerRME.Close();

        // Rest for 6 seconds.
        for (int i = 0; i < idealXList.Count; i++)
        {
            scoreList.Add(Mathf.Sqrt(Mathf.Pow((Mathf.Abs(idealXList[i] - xPosList[i])), 2) + Mathf.Pow(Mathf.Abs(yPosList[i]), 2)));
        }
        score = scoreList.Sum();
        displayText.text = score.ToString();
        yield return new WaitForSeconds(6);
        displayText.text = "";
        score = 0;
        scoreList.Clear();
        idealXList.Clear();
        xPosList.Clear();
        yPosList.Clear();

        // End of trial.
        trialIndex++;
        trialActive = false;
    }

    void WriteHeader ()
    {
        TextWriter writerH = new StreamWriter(@filePath, false);
        writerH.WriteLine("Time,Frame,X Pos,Y Pos,Event");
        writerH.Close();
    }

    void WriteData ()
    {
        TextWriter writerD = new StreamWriter(filePath, true);
        writerD.WriteLine(Time.time + "," + Time.frameCount + "," + player.transform.position.x.ToString() + "," + player.transform.position.y.ToString());
        writerD.Close();
    }
}
