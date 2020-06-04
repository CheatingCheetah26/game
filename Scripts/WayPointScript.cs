using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointScript : MonoBehaviour
{
    public int index;
    public int gotoState;
    public SessionManager sessionManager;
    public TrackDataHolder TrackDataHolder;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.CompareTag("AI"))
        {
            AIController aIController = collision.GetComponent<AIController>();
            if (aIController != null){
                if (aIController.state == 0)
                {
                    aIController.IncrementWayPoint(index);
                }
                aIController.state = gotoState;
            }
            else{
                Debug.LogWarning("An AI touched entered a trigger, but no AIController was found !");
            }
        }
        if (gotoState == 2)
        {
            if(collision.CompareTag("Player"))
            {
                collision.GetComponent<CarController>().StartPitStop(3,0.4f);
            }
        }
        if(gotoState == 0)
        {
            if (collision.CompareTag("Player") && collision.GetComponent<CarController>().isInPit)
            {
                collision.GetComponent<CarController>().isInPit = false;
            }
        }
    }

    private void Start()
    {
        sessionManager = GameObject.FindWithTag("SessionManager").GetComponent<SessionManager>();
    }
}
