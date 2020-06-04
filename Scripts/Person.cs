using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Person", menuName = "Person")]
public class Person : ScriptableObject
{
    public string firstName;
    public string lastName;
    public int age;
    public Sprite face;
}
