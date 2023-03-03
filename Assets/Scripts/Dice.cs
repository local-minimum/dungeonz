using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Dice
{
    public static IEnumerable<int> Roll(params int[] sides) => sides.Select(sides => Random.Range(1, sides + 1));



}
