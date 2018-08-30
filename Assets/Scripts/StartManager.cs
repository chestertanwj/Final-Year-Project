using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour {

    public InputField ageField;
    public Toggle maleToggle;
    public Toggle femaleToggle;
    public GameObject helpMenu;
    public GameObject registrationMenu;

    public CortexManager cm;

    void Start ()
    {
        helpMenu.SetActive(false);
        registrationMenu.SetActive(false);

        cm = new CortexManager();
        // cm.Init();
    }

    public void InputGender()
    {
        if (maleToggle.isOn)
        {
            PlayerPrefs.SetString("Gender", "M");
        }

        if (femaleToggle.isOn)
        {
            PlayerPrefs.SetString("Gender", "F");
        }
    }

    public void InputAge()
    {
        PlayerPrefs.SetString("Age", ageField.text.ToString());
    }

    public void InputDone ()
    {
        SceneManager.LoadScene("Main");
    }

    public void QuitGame ()
    {
        Application.Quit();
    }
}
