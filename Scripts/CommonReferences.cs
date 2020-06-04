using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonReferences : MonoBehaviour
{

    public static GameObject playerGO;

    public static CarController carController;

    public static TrackData trackData;

    public static SessionManager sessionManager;

    // Start is called before the first frame update
    void Start()
    {
        playerGO = GameObject.FindWithTag("Player");
        carController = playerGO.GetComponent<CarController>();
        trackData = GameObject.FindWithTag("TrackData").GetComponent<TrackDataHolder>().trackData;
        Debug.Log(trackData.name);
        sessionManager = GameObject.FindWithTag("SessionManager").GetComponent<SessionManager>();
    }
}
