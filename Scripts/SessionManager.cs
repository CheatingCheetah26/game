using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public List<CarData> cars;

    public List<LapTime> times = new List<LapTime>();

    public int sessionType;//0: practice  1: quali  2: race

    public TrackDataHolder TrackDataHolder;

    public GameObject AIPrefab;

    public Standings standings = new Standings();

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
        GameObject newCar = Instantiate(AIPrefab, TrackDataHolder.GetPitPosParked(3),Quaternion.Euler(new Vector3(0,0,TrackDataHolder.trackData.pit.angle+90f)));
        AIController ai = newCar.GetComponent<AIController>();
        
    }

    private void Start()
    {

    }

    public void StartRace()
    {
        
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