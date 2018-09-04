using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour {

    public GameObject player;
    public GameObject CortexReceiver;

    public GameObject centre;
    public GameObject leftGoal;
    public GameObject rightGoal;
    public GameObject leftLine;
    public GameObject rightLine;
    public GameObject leftMask;
    public GameObject rightMask;

    public Text scoreText;

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

    // EEG data writing.
    string eegfilePath;

    // Debug.
    public Camera mainCamera;

    // Initialisation.
    void Start ()
    {
        // Mocap data writing.
        fileName = "OptiTrack_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss_") + PlayerPrefs.GetString("Gender") + PlayerPrefs.GetString("Age") + ".csv";
        fileLocation = @"C:\Users\nrobinson\Desktop\Chester\";
        filePath = fileLocation + fileName;
        WriteHeader();

        // Initial rest/loading phase.
        initRestOver = false;
        StartCoroutine(InitRest());

        // Trial management.
        trialIndex = 1;
        trialTotal = 10;
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

        // Score management.
        scoreList = new List<float>();
        idealXList = new List<float>();
        xPosList = new List<float>();
        yPosList = new List<float>();

        // Game object setting.
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
            scoreText.text = "END THANK YOU";
        }

        // Return to start.
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene("Start");
        }

        // Debug.
        // player.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 20.0f));
    }

    IEnumerator InitRest ()
    {
        scoreText.text = "LOADING";
        yield return new WaitForSeconds(20);
        scoreText.text = "LOADED";
        yield return new WaitForSeconds(5);
        scoreText.text = "";
        initRestOver = true;

        eegfilePath = CortexReceiver.GetComponent<CortexReceiver>().filePath;
    }

    IEnumerator Trial ()
    {
        trialActive = true;
        audioSrc.Play();

        // Prepare for 5 seconds.
        centre.SetActive(true);
        yield return new WaitForSeconds(5);
        centre.SetActive(false);
        
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
        // Start left move. Mark event in mocap data and EEG data.
        leftGoal.SetActive(true);
        leftLine.SetActive(true);
        leftMask.SetActive(true);
        moveActive = true;
        TextWriter writerLMS = new StreamWriter(filePath, true);
        writerLMS.Write("TRIAL " + trialIndex.ToString() + " LEFT START");
        writerLMS.Close();
        TextWriter writerEEGLMS = new StreamWriter(eegfilePath, true);
        writerEEGLMS.Write("TRIAL " + trialIndex.ToString() + " LEFT START");
        writerEEGLMS.Close();

        // Move left mask to reveal left line for ~5.333... seconds.
        while (leftMask.transform.position.x > -24.5f)
        {
            leftMask.transform.position += Vector3.left * 0.1f;
            yield return new WaitForSeconds(0.02f);
        }

        // Reset left mask position.
        leftMask.transform.position = new Vector3(-8.5f, 0.0f, 0.0f);

        // End left move. Mark event in mocap data and EEG data.
        leftGoal.SetActive(false);
        leftLine.SetActive(false);
        leftMask.SetActive(false);
        moveActive = false;
        TextWriter writerLME = new StreamWriter(filePath, true);
        writerLME.Write("TRIAL " + trialIndex.ToString() + " LEFT END");
        writerLME.Close();
        TextWriter writerEEGLME = new StreamWriter(eegfilePath, true);
        writerEEGLME.Write("TRIAL " + trialIndex.ToString() + " LEFT END");
        writerEEGLME.Close();

        // Rest for 6 seconds.
        DisplayScore();
        yield return new WaitForSeconds(6);
        ClearScore();

        // End of trial.
        trialIndex++;
        trialActive = false;
    }

    IEnumerator RightMove ()
    {
        // Start right move. Mark event in mocap data and EEG data.
        rightGoal.SetActive(true);
        rightLine.SetActive(true);
        rightMask.SetActive(true);
        moveActive = true;
        TextWriter writerRMS = new StreamWriter(filePath, true);
        writerRMS.Write("TRIAL " + trialIndex.ToString() + " RIGHT START");
        writerRMS.Close();
        TextWriter writerEEGRMS = new StreamWriter(eegfilePath, true);
        writerEEGRMS.Write("TRIAL " + trialIndex.ToString() + " RIGHT START");
        writerEEGRMS.Close();

        // Move right mask to reveal right line for ~5.333... seconds.
        while (rightMask.transform.position.x < 24.5f)
        {
            rightMask.transform.position += Vector3.right * 0.1f;
            yield return new WaitForSeconds(0.02f);
        }

        // Reset right mask position.
        rightMask.transform.position = new Vector3(8.5f, 0.0f, 0.0f);

        // End right move. Mark event in mocap data and EEG data.
        rightGoal.SetActive(false);
        rightLine.SetActive(false);
        rightMask.SetActive(false);
        moveActive = false;
        TextWriter writerRME = new StreamWriter(filePath, true);
        writerRME.Write("TRIAL " + trialIndex.ToString() + " RIGHT END");
        writerRME.Close();
        TextWriter writerEEGRME = new StreamWriter(eegfilePath, true);
        writerEEGRME.Write("TRIAL " + trialIndex.ToString() + " RIGHT END");
        writerEEGRME.Close();

        // Rest for 6 seconds.
        DisplayScore();
        yield return new WaitForSeconds(6);
        ClearScore();

        // End of trial.
        trialIndex++;
        trialActive = false;
    }

    void WriteHeader ()
    {
        TextWriter writer = new StreamWriter(@filePath, false);
        writer.Write("TIME,FRAME,X POS,Y POS,EVENT,");
        writer.Close();
    }

    void WriteData ()
    {
        TextWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine("");
        writer.Write(Time.time + "," + Time.frameCount + "," + player.transform.position.x.ToString() + "," + player.transform.position.y.ToString() + ",");
        writer.Close();
    }

    void DisplayScore ()
    {
        for (int i = 0; i < idealXList.Count; i++)
        {
            scoreList.Add(Mathf.Sqrt(Mathf.Pow((Mathf.Abs(idealXList[i] - xPosList[i])), 2) + Mathf.Pow(Mathf.Abs(yPosList[i]), 2)));
        }
        score = 1000 - scoreList.Sum();
        if (score < 0.0f)
        {
            score = 0.0f;
        }
        scoreText.text = score.ToString();
    }

    void ClearScore ()
    {
        scoreText.text = "";
        score = 0;
        scoreList.Clear();
        idealXList.Clear();
        xPosList.Clear();
        yPosList.Clear();
    }
}
