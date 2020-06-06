using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{
    public List<CarData> cars;

    public List<GameObject> carsGO;

    public List<LapTime> times = new List<LapTime>();

    public int sessionType;//0: practice  1: quali  2: race

    public TrackDataHolder trackDataHolder;

    public GameObject AIPrefab;

    public Standings standings = new Standings();

    public GameObject lightsGO;

    public Sprite[] lightSprites = new Sprite[6];

    public bool hasRaceStarted = false;

    public int AddTime(LapTime lapTime)
    {
        Debug.Log("adding the time: " + lapTime.time + "...");
        if(times.Count == 0)
        {
            times.Add(lapTime);
            return 1;
        }
        else
        {
            for(int i = 0 ; i<times.Count ; i++)
            {
                if(times[i].carData == lapTime.carData)
                {
                    times.Remove(times[i]);
                    Debug.Log("A time was removed as it will be replaced");
                }
            }
            for(int i = 0; i < times.Count; i++)
            {
                if (times[i].time > lapTime.time)
                {
                    times.Insert(i, lapTime);
                    return i+1;
                }
            }
            times.Add(lapTime);
            return times.Count;
        }
    }

    public void SpawnCar(CarData carData)
    {
        GameObject newCar = Instantiate(AIPrefab, trackDataHolder.GetPitPosParked(3),Quaternion.Euler(new Vector3(0,0,trackDataHolder.trackData.pit.angle+90f)));
        AIController ai = newCar.GetComponent<AIController>();
    }

    private void Start()
    {
        lightsGO = GameObject.FindWithTag("UI_Lights");

        StartRace(cars);
    }

    public void StartRace(List<CarData> cars)
    {
        //spawn cars on the grid
        SpawnGrid(cars);
        //user presses start

        //initiate countdown
    }

    public void SpawnGrid(List<CarData> cars)
    {
        for(int i = 0; i<cars.Count; i++)
        {
            GameObject newCar = Instantiate(AIPrefab, GetGridPos(i), Quaternion.Euler(new Vector3(0, 0, trackDataHolder.trackData.startFinishLine.rotation+90f)));
            CarMainScript script = newCar.GetComponent<CarMainScript>();
            script.carData = cars[i];
            newCar.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            carsGO.Add(newCar);
        }
    }

    public Vector2 GetGridPos(int place)
    {
        float x;
        if(place%2 == 1)
        {
            x = 0.4f;
        }
        else
        {
            x = -0.4f;
        }
        WayPoint line = trackDataHolder.trackData.startFinishLine;
        Vector2 linepos = line.pos;
        float angle = line.rotation-90f;
        place = place + 2;
        Vector2 offset = new Vector2(place * Mathf.Cos(angle * Mathf.Deg2Rad) + x * Mathf.Sin(angle * Mathf.Deg2Rad),
                                     place * Mathf.Sin(angle * Mathf.Deg2Rad) + x * Mathf.Cos(angle * Mathf.Deg2Rad))/3;

        return linepos + offset;
    }

    public void UserPressedStart()//is called by PauseMenu.cs
    {
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        Image image = lightsGO.GetComponent<Image>();
        image.sprite = lightSprites[0];
        yield return new WaitForSeconds(1);
        for(int i = 1; i < 6; i++)
        {
            yield return new WaitForSeconds(1.2f);
            image.sprite = lightSprites[i];
        }
        yield return new WaitForSeconds(Random.Range(0.6f, 3f));
        image.sprite = lightSprites[0];
        Go();
        yield return new WaitForSeconds(1f);
        lightsGO.SetActive(false);
    }

    public void Go()
    {
        hasRaceStarted = true;
        foreach(GameObject car in carsGO)
        {
            car.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
    }
}

public class LapTime
{
    public float time;
    public CarData carData;

    public LapTime(float timeC, CarData carDataC)
    {
        time = timeC;
        carData = carDataC;
    }
}

public class Standings
{
    public Dictionary<CarData, int> laps = new Dictionary<CarData, int>();

    public List<CarData> standings = new List<CarData>();

    public List<CarData> AddLap(CarData car)
    {
        if (laps.ContainsKey(car))
        {
            laps[car] += 1;
        }
        else
        {
            laps.Add(car, 1);
            standings.Add(car);
            Debug.Log("The " + car.number + " car was just added to the standings, so it's put at the end.");
            return standings;
        }
        standings.Remove(car);
        for (int i = 0; i < standings.Count; i++)
        {
            if (laps[standings[i]] < laps[car])
            {
                Debug.Log(car.number + " has more laps than car number " + standings[i].number);
                
                standings.Insert(i,car);
                Debug.Log("The " + car.number + " is ahead of the "+standings[i+1].number+" car. It's put ahead of it.");
                return standings;
            }
        }
        standings.Add(car);
        Debug.Log("The " + car.number + " is last.");
        return standings;
    }

    
}