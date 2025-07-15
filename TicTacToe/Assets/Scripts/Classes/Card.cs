using UnityEngine;

public abstract class Card
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract int EnergyCost { get; }
    public abstract Ability Ability { get; }
    public abstract UseCase UseCase { get; }

    public void UseAbility(Slot slot = null)
    {
        if (UseCase == UseCase.SLOT && slot != null)
        {
            Ability.ActivateAbility();
        }
        else if (UseCase == UseCase.PLAYER || UseCase == UseCase.GLOBAL)
        {
            Ability.ActivateAbility();
        }
        else
        {
            Debug.Log("Card cannot be used in this context.");
        }
    }
}
