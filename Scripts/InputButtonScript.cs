using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputButtonScript : MonoBehaviour
{
    public string input;

    public void OnMouseDown()
    {
        Debug.Log("Sending down");
        CommonReferences.carController.GetDigitalInput(input, true);
    }

    

    public void OnMouseUp()
    {
        Debug.Log("Sending up");
        CommonReferences.carController.GetDigitalInput(input, false);
    }

}
