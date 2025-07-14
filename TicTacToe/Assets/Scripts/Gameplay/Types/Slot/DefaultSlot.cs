using UnityEngine;

public class DefaultSlot : Slot
{
    public override void UseSlotEffect()
    {
        Debug.Log("DefaultSlot effect used. No special effect defined.");
    }

    public override string GetName()
    {
        return LocalizationManager.Instance.Get(LocalizationKeys.Gameplay.Slots.DefaultSlot.Name);
    }
    // DefaultSlot does not have any special effects or behaviors.
}