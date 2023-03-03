using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class Episode
{
    protected enum SkillCheckOutcome { CriticalSuccess, Success, Neutral, Fail, CriticalFail };

    [SerializeField]
    List<TraitRange<Traits>> requirements = new List<TraitRange<Traits>>();

    protected abstract SkillCheckOutcome SkillCheck(Person person);
    protected abstract void ModifyPerson(Person person, SkillCheckOutcome outcome);

    protected abstract void ModifyEpisode(SkillCheckOutcome outcome);

    public bool PlayFor(Person person)
    {
        if (requirements.Any(traitRange => !person.personality.FullfillsTraitRange(traitRange))) {
            return false;
        }

        var outcome = SkillCheck(person);

        ModifyPerson(person, outcome);
        ModifyEpisode(outcome);

        return true;
    }
}
