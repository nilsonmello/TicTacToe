using UnityEngine;

public class Slot 
{
    private Effect slotEffect;

    private Ability ability;

    private int useLimit;
    public void UseSlotEffect()
    {
        slotEffect.ActivateEffect();
    }

    public void SetSlotEffect(Effect effect)
    {
        slotEffect = effect;
    }

    public Effect GetSlotEffect()
    {
        return slotEffect;
    }

    public int GetUseLimit()
    {
        return useLimit;
    }
    public void UseCard(Card card)
    {
        card.UseAbility(this);
    }
}

