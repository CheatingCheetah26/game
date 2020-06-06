using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrackDataHolder : MonoBehaviour
{
    public GameObject wayPointPrefab;
    public GameObject linePrefab;
    public TrackData trackData;

    public int Index;

    public GameObject player;

    public Tilemap trackTilemap;
    public Tilemap gripTilemap;

    private void OnDrawGizmos()
    {
        //drawing the waypoints
        foreach (WayPoint wp in trackData.wayPoints)
        {
            Vector3 center = new Vector3(wp.pos.x, wp.pos.y);
            Gizmos.color = new Color(1, wp.idealSpeed, wp.idealSpeed);
            Gizmos.DrawLine(center - (new Vector3(Mathf.Cos(wp.rotation * Mathf.PI / 180f) * wp.width, Mathf.Sin(wp.rotation * Mathf.PI / 180f) * wp.width) / 15),
                            center + (new Vector3(Mathf.Cos(wp.rotation * Mathf.PI / 180f) * wp.width, Mathf.Sin(wp.rotation * Mathf.PI / 180f) * wp.width) / 15));
        }
        //drawing the pits
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(trackData.pit.pos.x + Mathf.Cos(trackData.pit.angle*Mathf.Deg2Rad) * trackData.pit.length / 2, trackData.pit.pos.y + Mathf.Sin(trackData.pit.angle * Mathf.Deg2Rad) *trackData.pit.length/2),
                        new Vector3(trackData.pit.pos.x - Mathf.Cos(trackData.pit.angle * Mathf.Deg2Rad) * trackData.pit.length / 2, trackData.pit.pos.y - Mathf.Sin(trackData.pit.angle * Mathf.Deg2Rad) * trackData.pit.length/2));
        //drawing the pit entry waypoint
        WayPoint pwp = trackData.pit.entryWayPoint;
        Vector3 pcenter = pwp.pos;
        Gizmos.color = new Color(1, pwp.idealSpeed, pwp.idealSpeed);
        Gizmos.DrawLine(pcenter - (new Vector3(Mathf.Cos(pwp.rotation * Mathf.PI / 180f) * pwp.width, Mathf.Sin(pwp.rotation * Mathf.PI / 180f) * pwp.width) / 15),
                        pcenter + (new Vector3(Mathf.Cos(pwp.rotation * Mathf.PI / 180f) * pwp.width, Mathf.Sin(pwp.rotation * Mathf.PI / 180f) * pwp.width) / 15));

        //drawing the start finish line
        WayPoint lwp = trackData.startFinishLine;
        pcenter = lwp.pos;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pcenter - (new Vector3(Mathf.Cos(lwp.rotation * Mathf.PI / 180f) * lwp.width, Mathf.Sin(lwp.rotation * Mathf.PI / 180f) * lwp.width) / 15),
                        pcenter + (new Vector3(Mathf.Cos(lwp.rotation * Mathf.PI / 180f) * lwp.width, Mathf.Sin(lwp.rotation * Mathf.PI / 180f) * lwp.width) / 15));

    }

    private void Start()
    {
        for (int i = 0; i < trackData.wayPoints.Count;i++){
            GameObject go = Instantiate(wayPointPrefab, new Vector3(trackData.wayPoints[i].pos.x, trackData.wayPoints[i].pos.y), Quaternion.Euler(0, 0, trackData.wayPoints[i].rotation));
            go.transform.localScale = new Vector3(trackData.wayPoints[i].width, 0.3f, 1f);
            go.GetComponent<WayPointScript>().index = i;
            if(i == trackData.pit.indexBeforePit)
            {
                go.GetComponent<WayPointScript>().gotoState = 1;
            }
            go.name = "waypoint " + i;
        }

        GameObject entry = Instantiate(wayPointPrefab, trackData.pit.entryWayPoint.pos, Quaternion.Euler(0, 0, trackData.pit.angle + 90));
        entry.transform.localScale = new Vector3(trackData.pit.width, 0.3f, 1f);
        entry.GetComponent<WayPointScript>().gotoState = 2;
        entry.name = "pit entry";

        GameObject exit = Instantiate(wayPointPrefab, trackData.pit.exitWayPoint.pos, Quaternion.Euler(0, 0, trackData.pit.angle + 90));
        exit.transform.localScale = new Vector3(trackData.pit.width, 0.3f, 1f);
        exit.GetComponent<WayPointScript>().gotoState = 0;
        exit.name = "pit exit";

        GameObject line = Instantiate(linePrefab, trackData.startFinishLine.pos, Quaternion.Euler(0, 0, trackData.startFinishLine.rotation));
        line.transform.localScale = new Vector3(trackData.startFinishLine.width/7, 1, 1);
        line.name = "start finish line";

        player = GameObject.FindWithTag("Player");

        trackTilemap = trackData.trackTiles.Create(trackTilemap);
        gripTilemap = trackData.gripTiles.Create(gripTilemap);
    }

    public float DistanceToPlayer(Vector2 position){
        return Vector2.Distance(player.transform.position, position);
    }

    public Vector2 GetPitPos(int carIndex)
    {
        Pit pit = trackData.pit;
        Vector2 pos = new Vector2();
        float correctedIndex = Map(carIndex, 0,5 , -1,1);
        pos = pit.pos + new Vector2(Mathf.Cos(pit.angle * Mathf.Deg2Rad)*pit.length/2, Mathf.Sin(pit.angle * Mathf.Deg2Rad) * pit.length/2) * correctedIndex;
        return pos;
    }

    public Vector2 GetPitPosParked(int carIndex)
    {
        Pit pit = trackData.pit;
        return GetPitPos(carIndex) + new Vector2(Mathf.Cos((pit.angle-90)*Mathf.Deg2Rad)*pit.width/10, Mathf.Sin((pit.angle-90)*Mathf.Deg2Rad)*pit.width/10);
    }

    float Map(float original, float baseMin, float baseMax, float newMin, float newMax)
    {
        return newMin + (original - baseMin) * (newMax - newMin) / (baseMax - baseMin);
    }

}
