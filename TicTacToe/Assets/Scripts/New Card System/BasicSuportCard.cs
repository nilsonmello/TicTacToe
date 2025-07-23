using UnityEngine;

public class BasicSuportCard : Card
{
    // Load the sprite for the card from Resources, change the path as necessary
    private static Sprite sprite = Resources.Load<Sprite>("Cards/BasicSupport");

    // Implement the abstract properties from the Card class
    public override string Name => "Support";
    public override string Description => "Causes 5 damage and provides 3 energy.";
    public override int EnergyCost => 1;
    public override Ability Ability => new DamageEffect(5);
    public override UseCase UseCase => UseCase.SLOT;
    public override Sprite Artwork => sprite;
}