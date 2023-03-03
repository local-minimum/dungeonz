using UnityEngine;

[System.Serializable]
public class Trait<Traits>
{
    public readonly Traits positiveTrait;
    public readonly Traits negativeTrait;
    public readonly Traits zeroTrait;

    [SerializeField, HideInInspector]
    private float _value;

    public float traitValue
    {
        get { return _value; }
        set { _value = Mathf.Clamp(value, -1, 1); }
    }

    public float PullTowards(Traits trait, float amount)
    {
        if (trait.Equals(positiveTrait))
        {
            traitValue += amount;
            return traitValue;
        }
        if (trait.Equals(negativeTrait))
        {
            traitValue -= amount;
            return traitValue;
        }
        if (trait.Equals(zeroTrait))
        {
            if (traitValue == 0) return 0;
            if (traitValue < 0)
            {
                traitValue = Mathf.Min(0, traitValue + amount);
            }
            else
            {
                traitValue = Mathf.Max(0, traitValue - amount);
            }
            return traitValue;
        }
        throw new System.ArgumentException($"The trait '{trait}' is neither '{positiveTrait}' nor '{negativeTrait}'");
    }

    public float PullTowards(Traits trait, int points)
    {
        return PullTowards(trait, (float)points / 100f);
    }

    public float Adjust(float points)
    {
        if (Random.value < 0.5f)
        {
            return PullTowards(positiveTrait, points);
        } 
        return PullTowards(negativeTrait, points);
    }

    public float Adjust(int points)
    {
        return Adjust((float)points / 100f);
    }

    public Traits activeTrait
    {
        get
        {
            if (traitValue == 0) return zeroTrait;
            return traitValue < 0 ? negativeTrait : positiveTrait;
        }
    }

    public float activeValue
    {
        get
        {
            return Mathf.Abs(traitValue);
        }
    }

    public Trait(Traits positive, Traits negative, Traits zero, float initialValue = 0f)
    {
        positiveTrait = positive;
        negativeTrait = negative;
        zeroTrait = zero;
        traitValue = initialValue;
    }

    public override string ToString()
    {
        return $"{activeValue:0.00} {activeTrait} ({positiveTrait} <- {zeroTrait} -> {negativeTrait})";
    }
}
