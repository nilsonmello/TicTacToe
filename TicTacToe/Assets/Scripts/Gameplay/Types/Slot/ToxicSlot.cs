public class ToxicSlot : Slot
{
    int useLimit = 5;
    public override int GetUseLimit()
    {
        return useLimit;
    }
   public override string GetName()
   {
       return LocalizationManager.Instance.Get(LocalizationKeys.Gameplay.Slots.ToxicSlot.Name);
   }

}
// ToxicSlot is a placeholder for a slot type that could have specific effects or behaviors in the game.
