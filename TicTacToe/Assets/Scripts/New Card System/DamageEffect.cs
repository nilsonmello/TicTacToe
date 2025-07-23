using UnityEngine;

public class DamageEffect : Ability
{
    private int damage;

    // Constructor to initialize the damage amount
    public DamageEffect(int damage)
    {
        this.damage = damage;
    }

    // Override the ActivateAbility method to apply damage
    public override void ActivateAbility()
    {
        Debug.Log($"Caused {damage} damage");
    }
}
