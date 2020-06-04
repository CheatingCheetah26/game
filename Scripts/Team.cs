using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Team", menuName = "Team")]
public class Team : ScriptableObject
{
    public string name;
    public Color colorPrimary;
    public Color colorSecondary;
    public Sprite logo;
    public Person boss;
    public List<Person> drivers;
}
