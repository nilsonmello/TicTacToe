using UnityEngine;

public abstract class Card
{
    private string cardName;
    private string description;
    private int energyCost;
    private Ability ability;
    private UseCase useCase;

    public Card(string name, string description, int energyCost, Ability ability, UseCase usecase)
    {
        this.cardName = name;
        this.description = description;
        this.energyCost = energyCost;
        this.ability = ability;
        this.useCase = usecase;
    }

    public string getName()
    {
        return cardName;
    }

    public string getDescription()
    {
        return description;
    }

    public int getEnergyCost()
    {
        return energyCost;
    }

    public Ability getAbility()
    {
        return ability;
    }

    public UseCase getUseCase()
    {
        return useCase;
    }

    public void UseAbility(Slot slot = null)
    {
        if (useCase == UseCase.SLOT && slot != null)
        {
            ability.ActivateAbility();
        }
        else if (useCase == UseCase.PLAYER || useCase == UseCase.GLOBAL)
        {
            ability.ActivateAbility();
        }
        else
        {
            Debug.Log("Card cannot be used in this context.");
        }
    }
}
