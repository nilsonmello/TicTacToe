using UnityEngine;

public class DamageEffect : Ability
{
    private int damage;

    public DamageEffect(int damage)
    {
        this.damage = damage;
    }

    public override void ActivateAbility()
    {
        Debug.Log($"Caused {damage} damage");
    }
}
