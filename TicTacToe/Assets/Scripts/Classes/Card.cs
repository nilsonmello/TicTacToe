using UnityEngine;

public abstract class Card
{
    //card name, description, energy cost, ability, use case, and artwork
    //these properties must be implemented by derived classes
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract int EnergyCost { get; }
    public abstract Ability Ability { get; }
    public abstract UseCase UseCase { get; }
    public abstract Sprite Artwork { get; }

    //method to use the card's ability
    //this method checks the use case and activates the ability accordingly
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