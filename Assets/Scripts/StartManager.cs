using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour {

    // Public references.
    public InputField ageField;
    public Toggle maleToggle;
    public Toggle femaleToggle;
    public Toggle fastToggle;
    public Toggle slowToggle;
    public GameObject helpMenu;
    public GameObject registrationMenu;

    // Initialisation.
    void Start ()
    {
        // Game object setting.
        helpMenu.SetActive(false);
        registrationMenu.SetActive(false);
    }

    // Gender toggle group.
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

    // Age field.
    public void InputAge()
    {
        PlayerPrefs.SetString("Age", ageField.text.ToString());
    }

    // Game speed toggle group.
    public void InputSpeed()
    {
        if (fastToggle.isOn)
        {
            PlayerPrefs.SetString("Speed", "FAST");
        }
        
        if (slowToggle.isOn)
        {
            PlayerPrefs.SetString("Speed", "SLOW");
        }
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
