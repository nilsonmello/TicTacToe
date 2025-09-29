using UnityEngine;

public class SupportEffect : Ability
{
    private int supportAmount;

    // Constructor to initialize the damage amount
    public SupportEffect(int hp)
    {
        this.supportAmount = hp;
    }

    // Override the ActivateAbility method to apply support
    public override void ActivateAbility()
    {
        Debug.Log($"Applied {supportAmount} of recovery");
    }
}