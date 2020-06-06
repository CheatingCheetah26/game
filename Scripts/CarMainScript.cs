using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMainScript : MonoBehaviour
{
    public CarData carData;

    public bool isPlayer;

    public TMPro.TMP_Text posText;

    LapTime myTime;

    public SpriteRenderer sr_primary;
    public SpriteRenderer sr_secondary;


    // Start is called before the first frame update
    void Start()
    {

        if (CompareTag("Player"))
        {
            isPlayer = true;
            posText = GameObject.FindWithTag("UI_Position").GetComponent<TMPro.TMP_Text>();
        }
        else
        {
            isPlayer = false;
        }

        name = carData.name + " " + carData.number;

        if (isPlayer)
        {
            name.Insert(0, "(Player) ");
        }

        sr_primary.color = carData.team.colorPrimary;
        sr_secondary.color = carData.team.colorSecondary;

        GetComponent<Rigidbody2D>().centerOfMass = new Vector2(-0.075f, 0);

    }

    // Update is called once per frame
    void Update()
    {
        if(myTime != null)
        {
            myTime.time += Time.deltaTime;
        }
    }

    public void LineCrossed()
    {
        if (CommonReferences.sessionManager.times.Count != 0)
        {
            GameObject.FindWithTag("UI_Time").GetComponent<TMPro.TMP_Text>().text = CommonReferences.sessionManager.times[0].time.ToString();
        }
        if (myTime != null)
        {
            CommonReferences.sessionManager.AddTime(myTime);
            if (isPlayer && GamePreferences.adaptativeAI)
            {
                GamePreferences.difficulty = (myTime.time - 39f) / -6.5f;
                Debug.Log("difficulty set to " + GamePreferences.difficulty);
            }
            myTime.time = 0f;

        }
        if (myTime == null)
        {
            myTime = new LapTime(0f, carData);
        }
        

        if (isPlayer)
        {
            List<CarData> positions = CommonReferences.sessionManager.standings.AddLap(carData);
            posText.text = "Pos: " + (positions.IndexOf(carData)+1) + "/" + positions.Count;
        }
        else
        {
            CommonReferences.sessionManager.standings.AddLap(carData);
        }
    }
}
