using UnityEngine;

public class BasicDamageCard : Card
{
    // Load the sprite for the card from Resources, change the path as necessary
    // Ensure the sprite exists in the specified path
    private static Sprite sprite = Resources.Load<Sprite>("Cards/BasicDamage");

    // Implement the abstract properties from the Card class
    // These properties define the card's name, description, energy cost, ability, use case
    public override string Name => "Attack";
    public override string Description => "Causes 5 damage.";
    public override int EnergyCost => 1;
    public override Ability Ability => new DamageEffect(5);
    public override UseCase UseCase => UseCase.SLOT;
    public override Sprite Artwork => sprite;
}
