using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{
    
    public Personality personality;

    private void Awake()
    {
        personality = new Personality();
        Debug.Log(personality);
    }

    public bool Adventuring
    {
        get { return true;  }
    }
}
