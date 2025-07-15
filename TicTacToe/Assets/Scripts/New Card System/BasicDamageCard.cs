using UnityEngine;

public class BasicDamageCard : Card
{
    public override string Name => "Basic Attack";
    public override string Description => "Deals 2 damage.";
    public override int EnergyCost => 1;
    public override Ability Ability => new DamageEffect(2);
    public override UseCase UseCase => UseCase.SLOT;
}