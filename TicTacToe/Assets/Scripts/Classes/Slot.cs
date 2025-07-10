using UnityEditor;
using UnityEngine;

public abstract class Slot
{
    private string name;
    private Effect slotEffect;
    private int useLimit;

    private SlotStates state = SlotStates.EMPTY;

    public virtual void UseSlotEffect()
    {
        slotEffect.ActivateEffect();
    }

    public virtual void SetSlotEffect(Effect effect)
    {
        slotEffect = effect;
    }

    public virtual Effect GetSlotEffect()
    {
        return slotEffect;
    }

    public virtual int GetUseLimit()
    {
        return useLimit;
    }
    public virtual void UseCard(Card card)
    {
        card.UseAbility(this);
    }
    public virtual string GetName()
    {
        return name;
    }
    public SlotStates GetState()
    {
        return state;
    }
}

