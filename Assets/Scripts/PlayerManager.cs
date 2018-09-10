using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour {

    // Public references.
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
    private float sessionScore;
    
    // Mocap data writing.
    private string mocapFileName;
    private string mocapFileLocation;
    private string mocapFilePath;

    // EEG data writing.
    private string eegFilePath;

    // Debug.
    public Camera mainCamera;

    // Initialisation.
    void Start ()
    {
        // Mocap data writing.
        mocapFileName = "MOCAP_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss_") + PlayerPrefs.GetString("Gender") + PlayerPrefs.GetString("Age") + "_" + PlayerPrefs.GetString("Speed") + ".csv";
        mocapFileLocation = @"C:\Users\nrobinson\Desktop\Chester\";
        mocapFilePath = mocapFileLocation + mocapFileName;
        WriteMocapHeader();

        // Initial rest/loading phase.
        initRestOver = false;
        StartCoroutine(InitRest());

        // Trial management.
        trialIndex = 1;
        trialTotal = 4;
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
        sessionScore = 0;

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
        WriteMocapData();

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
        if (trialIndex > trialTotal)
        {
            scoreText.text = "END: " + sessionScore;
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
        yield return new WaitForSeconds(25);
        scoreText.text = "LOADED";
        yield return new WaitForSeconds(5);
        scoreText.text = "";
        initRestOver = true;

        eegFilePath = CortexReceiver.GetComponent<CortexReceiver>().filePath;
    }

    IEnumerator Trial ()
    {
        // Start trial.
        trialActive = true;

        // Prepare for 5 seconds.
        centre.SetActive(true);
        yield return new WaitForSeconds(5);
        centre.SetActive(false);
        
        // Start LeftMove or RightMove coroutine.
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
        // Let goal appear for 1 second before start left move.
        audioSrc.Play();
        leftGoal.SetActive(true);
        yield return new WaitForSeconds(1);

        // Start left move.
        leftLine.SetActive(true);
        leftMask.SetActive(true);
        moveActive = true;

        // Mark start left move event in mocap data and EEG data.
        StartLeftMoveMarker();

        // Move left mask from -8.5 to -24.5 to reveal left line.
        while (leftMask.transform.position.x > -24.5f)
        {
            if (PlayerPrefs.GetString("Speed") == "FAST")
            {
                leftMask.transform.position += Vector3.left * 0.08f;
            }
            else if (PlayerPrefs.GetString("Speed") == "SLOW")
            {
                leftMask.transform.position += Vector3.left * 0.04f;
            }
                
            yield return null;
        }

        // Reset left mask position.
        leftMask.transform.position = new Vector3(-8.5f, 0.0f, 0.0f);

        // End left move.
        audioSrc.Play();
        leftGoal.SetActive(false);
        leftLine.SetActive(false);
        leftMask.SetActive(false);
        moveActive = false;

        // Mark end left move event in mocap data and EEG data.
        EndLeftMoveMarker();

        // Rest for 6 seconds and display score.
        DisplayScore();
        yield return new WaitForSeconds(6);
        ClearScore();

        // End trial.
        trialIndex++;
        trialActive = false;
    }

    IEnumerator RightMove ()
    {
        // Let goal appear for 1 second before start right move.
        audioSrc.Play();
        rightGoal.SetActive(true);
        yield return new WaitForSeconds(1);

        // Start right move.
        rightLine.SetActive(true);
        rightMask.SetActive(true);
        moveActive = true;

        // Mark start right move event in mocap data and EEG data.
        StartRightMoveMarker();

        // Move right mask from 8.5 to 24.5 to reveal right line.
        while (rightMask.transform.position.x < 24.5f)
        {
            if (PlayerPrefs.GetString("Speed") == "FAST")
            {
                rightMask.transform.position += Vector3.right * 0.08f;
            }
            else if (PlayerPrefs.GetString("Speed") == "SLOW")
            {
                rightMask.transform.position += Vector3.right * 0.04f;
            }
            
            yield return null;
        }

        // Reset right mask position.
        rightMask.transform.position = new Vector3(8.5f, 0.0f, 0.0f);

        // End right move.
        audioSrc.Play();
        rightGoal.SetActive(false);
        rightLine.SetActive(false);
        rightMask.SetActive(false);
        moveActive = false;

        // Mark end right move event in mocap data and EEG data.
        EndRightMoveMarker();

        // Rest for 6 seconds and display score.
        DisplayScore();
        yield return new WaitForSeconds(6);
        ClearScore();

        // End trial.
        trialIndex++;
        trialActive = false;
    }

    void WriteMocapHeader ()
    {
        TextWriter writerMocapHeader = new StreamWriter(@mocapFilePath, false);
        writerMocapHeader.Write("TIME,FRAME,X POS,Y POS,EVENT,");
        writerMocapHeader.Close();
    }

    void WriteMocapData ()
    {
        TextWriter writerMocapData = new StreamWriter(mocapFilePath, true);
        writerMocapData.WriteLine("");
        writerMocapData.Write(Time.time + "," + Time.frameCount + "," + player.transform.position.x.ToString() + "," + player.transform.position.y.ToString() + ",");
        writerMocapData.Close();
    }

    void StartLeftMoveMarker ()
    {
        TextWriter writerMocapRMS = new StreamWriter(mocapFilePath, true);
        writerMocapRMS.Write("TRIAL " + trialIndex.ToString() + " LEFT " + PlayerPrefs.GetString("Speed") + " START");
        writerMocapRMS.Close();
        TextWriter writerEEGRMS = new StreamWriter(eegFilePath, true);
        writerEEGRMS.Write("TRIAL " + trialIndex.ToString() + " LEFT " + PlayerPrefs.GetString("Speed") + " START");
        writerEEGRMS.Close();
    }

    void EndLeftMoveMarker ()
    {
        TextWriter writerMocapRMS = new StreamWriter(mocapFilePath, true);
        writerMocapRMS.Write("TRIAL " + trialIndex.ToString() + " LEFT " + PlayerPrefs.GetString("Speed") + " END");
        writerMocapRMS.Close();
        TextWriter writerEEGRMS = new StreamWriter(eegFilePath, true);
        writerEEGRMS.Write("TRIAL " + trialIndex.ToString() + " LEFT " + PlayerPrefs.GetString("Speed") + " END");
        writerEEGRMS.Close();
    }

    void StartRightMoveMarker ()
    {
        TextWriter writerMocapRMS = new StreamWriter(mocapFilePath, true);
        writerMocapRMS.Write("TRIAL " + trialIndex.ToString() + " RIGHT " + PlayerPrefs.GetString("Speed") + " START");
        writerMocapRMS.Close();
        TextWriter writerEEGRMS = new StreamWriter(eegFilePath, true);
        writerEEGRMS.Write("TRIAL " + trialIndex.ToString() + " RIGHT " + PlayerPrefs.GetString("Speed") + " START");
        writerEEGRMS.Close();
    }

    void EndRightMoveMarker ()
    {
        TextWriter writerMocapRMS = new StreamWriter(mocapFilePath, true);
        writerMocapRMS.Write("TRIAL " + trialIndex.ToString() + " RIGHT " + PlayerPrefs.GetString("Speed") + " END");
        writerMocapRMS.Close();
        TextWriter writerEEGRMS = new StreamWriter(eegFilePath, true);
        writerEEGRMS.Write("TRIAL " + trialIndex.ToString() + " RIGHT " + PlayerPrefs.GetString("Speed") + " END");
        writerEEGRMS.Close();
    }

    void DisplayScore()
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
        else
        {
            score = (float)Math.Round((double)(score / 10), 1);
        }
        sessionScore += score;
        scoreText.text = score.ToString();
    }

    void ClearScore()
    {
        scoreText.text = "";
        score = 0;
        scoreList.Clear();
        idealXList.Clear();
        xPosList.Clear();
        yPosList.Clear();
    }
}
