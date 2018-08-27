using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour {

    public InputField ageField;
    public Toggle maleToggle;
    public Toggle femaleToggle;

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

        Debug.Log(PlayerPrefs.GetString("Gender"));
    }

    public void InputMaleGender()
    {

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
        Debug.Log("Quit.");
        Application.Quit();
    }
}
