public class DefaultSlot : Slot
{
    public override string GetName()
    {
        return LocalizationManager.Instance.Get("Gameplay.Slots.DefaultSlot.Name");
    }
    // DefaultSlot does not have any special effects or behaviors.
}