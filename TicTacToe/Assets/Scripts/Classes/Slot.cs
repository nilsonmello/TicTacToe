using System;
using UnityEditor;
using UnityEngine;

public abstract class Slot
{
    private string name;
    private Effect slotEffect;
    private int useLimit;

    public SlotStates slotState = SlotStates.EMPTY;

    public Player owner = null;


    public event Action OnOwnerChanged;
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
    public virtual SlotStates GetState()
    {
        return slotState;
    }
    public virtual Player GetOwner()
    {
        return owner;
    }
    public virtual void SetOwner(Player newOwner)
    {
        owner = newOwner;
        OnOwnerChanged?.Invoke();
    }
    public virtual void SetState(SlotStates newState)
    {
        slotState = newState;
    }
}

