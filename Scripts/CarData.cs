using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Car Data",menuName = "Car Data")]
public class CarData : ScriptableObject {
    public Team team;
    public Person driver;
    public int number;
}
