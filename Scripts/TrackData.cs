using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New track data",menuName = "Track Data"), System.Serializable]
public class TrackData : ScriptableObject{
    public string name;
    public string ident;
    public List<WayPoint> wayPoints;

    public int lapCount;

    public Pit pit;
    public WayPoint startFinishLine;

    public TilemapData trackTiles;
    public TilemapData gripTiles;
}

[System.Serializable]
public class WayPoint{
    public Vector2 pos;
    public float width;
    public float rotation;
    public float idealSpeed;
    public bool canOvertake;

    public WayPoint(Vector2 posC = new Vector2(),float widthC = 10f, float rotationC = 0f, float idealSpeedC = 1f, bool canOvertakeC = true){
        pos = posC;
        width = widthC;
        rotation = rotationC;
        idealSpeed = idealSpeedC;
        canOvertake = canOvertakeC;
    }
}

[System.Serializable]
public struct Pit
{
    public Vector2 pos;
    public float width;
    public float length;
    public float angle;
    public int indexBeforePit;
    public int indexAfterPit;
    public WayPoint entryWayPoint;
    public WayPoint exitWayPoint;
}