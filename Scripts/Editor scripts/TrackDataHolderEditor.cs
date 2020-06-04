using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackDataHolder))]
public class TrackDataHolderEditor : Editor
{
    TrackDataHolder trackDataHolder;
    List<WayPoint> wayPoints;

    private GUIStyle speedLabel = new GUIStyle();


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Append WayPoint before number " + trackDataHolder.Index)){
            trackDataHolder.trackData.wayPoints.Insert(trackDataHolder.Index, new WayPoint());
        }
        if (GUILayout.Button("Delete WayPoint at index " + trackDataHolder.Index))
        {
            trackDataHolder.trackData.wayPoints.RemoveAt(trackDataHolder.Index);
        }
        if(GUILayout.Button("Add new WayPoint")){
            trackDataHolder.trackData.wayPoints.Add(new WayPoint());
        }
        if (GUILayout.Button("Save Track tiles"))
        {
            trackDataHolder.trackData.trackTiles = TilemapData.Convert(trackDataHolder.trackTilemap);
            trackDataHolder.trackData.gripTiles = TilemapData.Convert(trackDataHolder.gripTilemap);
        }
        if (GUILayout.Button("Save Changes"))
        {
            EditorUtility.SetDirty(trackDataHolder.trackData);
        }
    }

    private void OnSceneGUI()
    {
        for (int i = 0; i < wayPoints.Count; i++){
            Vector2 pos = new Vector2(wayPoints[i].pos.x, wayPoints[i].pos.y);
            Vector2 secondPos = new Vector2(wayPoints[i].pos.x + Mathf.Cos(wayPoints[i].rotation*Mathf.Deg2Rad) * wayPoints[i].width / 15,
                                            wayPoints[i].pos.y + Mathf.Sin(wayPoints[i].rotation*Mathf.Deg2Rad) * wayPoints[i].width / 15);
            Vector2 speedPos = new Vector2(wayPoints[i].pos.x + Mathf.Cos(wayPoints[i].rotation*Mathf.Deg2Rad) * wayPoints[i].width / 15 * -wayPoints[i].idealSpeed,
                                           wayPoints[i].pos.y + Mathf.Sin(wayPoints[i].rotation*Mathf.Deg2Rad) * wayPoints[i].width / 15 * -wayPoints[i].idealSpeed);

            Vector2 overPos = new Vector2(wayPoints[i].pos.x + (Mathf.Cos(wayPoints[i].rotation * Mathf.Deg2Rad) * wayPoints[i].width / 15/2),
                                          wayPoints[i].pos.y + (Mathf.Sin(wayPoints[i].rotation * Mathf.Deg2Rad) * wayPoints[i].width / 15/2));

            Handles.color = Color.white;
            Vector2 newPos = Handles.FreeMoveHandle(pos, Quaternion.identity, 0.1f, Vector2.zero, Handles.CylinderHandleCap);
            Handles.color = Color.blue;
            Vector2 newSec = Handles.FreeMoveHandle(secondPos, Quaternion.identity, 0.1f, Vector2.zero, Handles.CylinderHandleCap);
            Handles.color = Color.red;
            Vector2 newSpeed = Handles.FreeMoveHandle(speedPos, Quaternion.identity, 0.05f, Vector2.zero, Handles.CylinderHandleCap);

            if (wayPoints[i].canOvertake)
            {
                Handles.color = Color.green;
            }
            else
            {
                Handles.color = Color.red;
            }

            if (Handles.Button(overPos, Quaternion.identity, 0.05f,0f, Handles.CubeHandleCap))
            {
                if (wayPoints[i].canOvertake)
                {
                    wayPoints[i].canOvertake = false;
                }
                else
                {
                    wayPoints[i].canOvertake = true;
                }
            }

            Handles.Label(pos, i.ToString());

            speedLabel.fontSize = 10;
            Handles.Label(newSpeed, wayPoints[i].idealSpeed.ToString(),speedLabel);

            if (newPos != pos){
                wayPoints[i].pos.x = newPos.x;
                wayPoints[i].pos.y = newPos.y;
            }
            if (newSec != secondPos){
                wayPoints[i].rotation = Mathf.Atan2(newPos.y-newSec.y, newPos.x - newSec.x) * Mathf.Rad2Deg+180f;
                wayPoints[i].width = Vector2.Distance(newPos, newSec) * 15f;
            }
            if(newSpeed != speedPos){
                wayPoints[i].idealSpeed = Mathf.Round((Mathf.Min(Vector2.Distance(newPos, newSpeed)/wayPoints[i].width*15,1f))*10f)/10f;
            }
        }
    }

    private void OnEnable()
    {
        trackDataHolder = (TrackDataHolder)target;
        wayPoints = trackDataHolder.trackData.wayPoints;
        if(wayPoints == null){
            wayPoints = new List<WayPoint>();
        }
    }
}
