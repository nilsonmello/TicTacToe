using UnityEngine;

public class SlotController : MonoBehaviour
{

    [SerializeField] private string slotDebugInfo;
    public Slot slot;

    public Slot GetSlot()
    {
        return slot;
    }
    public void SetSlot(Slot newSlot)
    {
        slot = newSlot;
        slotDebugInfo = $"SlotController on {gameObject.name} set to slot type: {slot.GetName()}";
    }
        
}
