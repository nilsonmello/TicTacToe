using UnityEngine;

public class SlotController : MonoBehaviour
{

    [SerializeField] private string slotTypeInfo;
    [SerializeField] private string slotStateInfo;
    [SerializeField] private string ownerName;

    public Slot slot;

    public Slot GetSlot()
    {
        return slot;
    }
    public void SetSlot(Slot newSlot)
    {
        if (slot != null)
        {
            slot.OnOwnerChanged -= SpawnOwnerIcon;
            slot.OnOwnerChanged -= UpdateSlotInfo;
        }
        slot = newSlot;
    if (slot != null)
    {
        slot.OnOwnerChanged -= SpawnOwnerIcon;    // Defensive: ensure no duplicates
        slot.OnOwnerChanged -= UpdateSlotInfo;
        slot.OnOwnerChanged += SpawnOwnerIcon;
        slot.OnOwnerChanged += UpdateSlotInfo;
    }
    }


    public void SpawnOwnerIcon()
    {
        Transform iconTransform = transform.Find("OwnerIcon");
        if (iconTransform != null)
        {
            Destroy(iconTransform.gameObject);
        }
        if (slot != null && slot.GetOwner() != null && slot.GetOwner().GetIcon() != null)
        {
            GameObject iconObj = new GameObject("OwnerIcon");
            iconObj.transform.SetParent(transform);

            iconObj.transform.localPosition = new Vector3(0, 0, 0);

            iconObj.transform.localScale = new Vector3(1, 1, 1);

            var sr = iconObj.AddComponent<SpriteRenderer>();
            sr.sprite = slot.GetOwner().GetIcon();
            sr.sortingOrder = 10;
        }
    }
    void UpdateSlotInfo()
    {
        if (slot != null)
        {
            slotTypeInfo = $"{slot.GetName()}";
            slotStateInfo = $"{slot.GetState()}";
            if (slot.GetOwner() != null)
            {
                ownerName = slot.GetOwner().GetName();
            }
            else
            {
                ownerName = "No Owner";
            }
        }
        else
        {
            slotTypeInfo = "No Slot Assigned";
        }
    }
}
