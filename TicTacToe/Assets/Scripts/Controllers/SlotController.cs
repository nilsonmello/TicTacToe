using UnityEngine;
using UnityEngine.EventSystems;

public class SlotController : MonoBehaviour, IPointerClickHandler
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

        ownerName = "";
        slotTypeInfo = "";
        slotStateInfo = "";

        slot.OnOwnerChanged += SpawnOwnerIcon;
        slot.OnOwnerChanged += UpdateSlotInfo;
        UpdateSlotInfo();
        SpawnOwnerIcon();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slot != null
        && slot.GetState() == SlotStates.EMPTY
        && GameManager.Instance.GetCurrentPlayer() != null)
        {
            slot.SetOwner(GameManager.Instance.GetCurrentPlayer());
            slot.SetState(SlotStates.OCCUPIED);
            GameManager.Instance.ChangeTurn();
            // Debug.Log($"Slot {slot.GetName()} used by {slot.GetOwner()?.GetName() ?? "No Owner"}");
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
