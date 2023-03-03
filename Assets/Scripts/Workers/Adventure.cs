using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventure : MonoBehaviour
{
    Episode[] episodes;

    public void PlayFor(Person person)
    {
        for (int i = 0; i < episodes.Length; i++)
        {
            if (!person.Adventuring) break;

            episodes[i].PlayFor(person);
        }
    }
}
