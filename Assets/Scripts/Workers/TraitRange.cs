using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TraitRange<Traits>
{
    public readonly Traits trait;
    public readonly float minimumValue;
    public readonly float maximumValue;

    public bool IsFullfilledIn(Trait<Traits> trait)
    {
        var value = trait.activeValue;
        return trait.activeTrait.Equals(this.trait) && value >= minimumValue && value <= maximumValue;
    }

    public TraitRange(Traits trait, float minValue, float maxValue = 1f) {
        this.trait = trait;
        minimumValue = minValue;
        maximumValue = maxValue;
    }
}
