using UnityEditor.Localization.Editor;

public class DefaultSlot : Slot
{
    public override string GetName()
    {
        return LocalizationManager.Instance.Get(LocalizationKeys.Gameplay.Slots.DefaultSlot.Name);
    }
    // DefaultSlot does not have any special effects or behaviors.
}