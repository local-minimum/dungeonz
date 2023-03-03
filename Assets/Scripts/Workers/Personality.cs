using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Traits
{
    Null,
    Endurance, Inventiveness,
    Stength, Dexterity,
    Organized, Chatoic,
}

[System.Serializable]
public class Personality
{
    public readonly Trait<Traits>[] traits;

    public Personality(int seedTraitsWithDice = 6)
    {
        traits = new Trait<Traits>[]
        {
            new (Traits.Endurance, Traits.Inventiveness, Traits.Null),
            new (Traits.Stength, Traits.Dexterity, Traits.Null),
            new (Traits.Organized, Traits.Chatoic, Traits.Null),
        };

        if (seedTraitsWithDice > 0)
        {
            for (int i = 0; i < traits.Length; i++)
            {
                traits[i].Adjust(Dice.Roll(seedTraitsWithDice).First());
            }
        }
    }

    public override string ToString()
    {
        var traitTexts = string.Join("\n", traits.Select(t => t.ToString()));
        return $"Personality:\n{traitTexts}";
    }

    public bool FullfillsTraitRange(TraitRange<Traits> range) => traits.Any(trait => range.IsFullfilledIn(trait));
}
