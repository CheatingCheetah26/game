using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject mainPause;
    public GameObject optionsMenu;
    public GameObject inputs;
    public GameObject steering;
    public GameObject autoBrake;
    public GameObject brakePedal;
    public GameObject hiddenInPause;
    public TMP_Text controllerName;
    public bool connected = false;

    public AudioMixer mixer;

    // Start is called before the first frame update
    void Start()
    {
        Resume();
        
        GamePreferences.difficulty = 1f;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        mixer.SetFloat("Volume", -80f);
        optionsMenu.SetActive(false);
        mainPause.SetActive(true);
        hiddenInPause.SetActive(false);
    }

    public void Resume()
    {
        StartCoroutine(RestartCountdown());
        pauseMenu.SetActive(false);
        mixer.SetFloat("Volume", 0f);
        if(GamePreferences.controlScheme != 0)
        {
            inputs.SetActive(true);
            if(GamePreferences.controlScheme == 1)
            {
                steering.SetActive(false);
            }
            else
            {
                steering.SetActive(true);
            }
        }
        hiddenInPause.SetActive(true);
    }

    public void Options()
    {
        optionsMenu.SetActive(true);
        mainPause.SetActive(false);
    }

    public void ScanControllers()
    {
        GamePreferences.usingController = true;
        StartCoroutine(CheckForControllers());
    }

    public void ChangeControls(int controls)
    {
        GamePreferences.controlScheme = controls;
        Debug.Log("The control scheme is now :" + GamePreferences.controlScheme);
        if(GamePreferences.controlScheme != 0)
        {
            autoBrake.SetActive(true);
        }
        else
        {
            autoBrake.SetActive(false);
        }
    }

    public void SetAutoBrake(bool trueOrFalse)
    {
        GamePreferences.autoBrake = trueOrFalse;
        brakePedal.SetActive(!trueOrFalse);
    }

    public void SetDifficulty(float difficulty)
    {
        GamePreferences.difficulty = difficulty;
        Debug.Log("Difficulty set to " + GamePreferences.difficulty);
    }

    public void SetAdaptativeAI(bool trueOrFalse)
    {
        GamePreferences.adaptativeAI = trueOrFalse;
    }

    public void CallStart()
    {
        CommonReferences.sessionManager.UserPressedStart();
    }

    IEnumerator CheckForControllers()
    {
        int i = 0;
        while (i<5)
        {
            var controllers = Input.GetJoystickNames();
            if (!connected && controllers.Length > 0)
            {
                connected = true;
                Debug.Log("Connected");
                controllerName.text = controllers[0];
            }
            else if (connected && controllers.Length == 0)
            {
                connected = false;
                Debug.Log("Disconnected");
                controllerName.text = "no controller attached";
            }
            i++;
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator RestartCountdown()
    {
        yield return new WaitForEndOfFrame();
        Time.timeScale = 1f;
    }
}
