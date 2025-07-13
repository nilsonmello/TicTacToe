using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotatingCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RotatingSlotManager slotManager;
    private RectTransform rectTransform;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private Transform originalParent;

    private Card card;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        card = GetComponent<Card>();
        slotManager = GetComponentInParent<RotatingSlotManager>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Atualiza dinamicamente as referências
        slotManager = GetComponentInParent<RotatingSlotManager>();
        card = GetComponent<Card>();

        if (slotManager == null || card == null || card.IsLifted)
            return;

        isDragging = true;
        originalParent = transform.parent;

        // Desacopla para permitir movimento livre
        transform.SetParent(slotManager.transform, true);

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector3 globalMousePos))
        {
            dragOffset = transform.position - globalMousePos;
        }
        else
        {
            dragOffset = Vector3.zero;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || rectTransform == null) return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector3 globalMousePos))
        {
            transform.position = globalMousePos + dragOffset;
            CheckSwap();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || slotManager == null || card == null) return;

        isDragging = false;

        // Detecta se foi solto sobre outro deck
        RotatingSlotManager targetManager = GetManagerUnderMouse(eventData);

        if (targetManager != null && targetManager != slotManager && targetManager.cards.Count < targetManager.maxSlots)
        {
            slotManager.TransferCardToOtherManager(card, targetManager);

            // Atualiza referências depois da transferência
            card.slotManager = targetManager;
            slotManager = targetManager;

            return;
        }

        // Verifica se está fora da borda para rotação
        Vector3 localPos = slotManager.transform.InverseTransformPoint(transform.position);
        float halfWidth = (slotManager.maxSlots - 1) * slotManager.slotSpacing / 2f;

        if (localPos.x < -halfWidth - slotManager.slotSpacing / 2f)
        {
            slotManager.RotateLeft();
        }
        else if (localPos.x > halfWidth + slotManager.slotSpacing / 2f)
        {
            slotManager.RotateRight();
        }

        // Reacopla a carta ao slot original
        transform.SetParent(originalParent, false);
        slotManager.UpdateCardParents();
        slotManager.UpdateSlotsPosition();
    }

    private RotatingSlotManager GetManagerUnderMouse(PointerEventData eventData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            RotatingSlotManager manager = result.gameObject.GetComponentInParent<RotatingSlotManager>();
            if (manager != null)
                return manager;
        }

        return null;
    }

    private void CheckSwap()
    {
        if (slotManager == null || card == null) return;

        float minDistance = slotManager.slotSpacing / 2f;
        int swapIndex = -1;

        Vector3 localPos = slotManager.transform.InverseTransformPoint(transform.position);

        for (int i = 0; i < slotManager.slots.Count; i++)
        {
            Vector3 slotLocalPos = slotManager.slots[i].localPosition;
            if (Mathf.Abs(localPos.x - slotLocalPos.x) < minDistance)
            {
                swapIndex = i;
                break;
            }
        }

        if (swapIndex >= 0)
        {
            int currentIndex = slotManager.cards.IndexOf(card);
            if (swapIndex != currentIndex && currentIndex >= 0)
            {
                slotManager.SwapCards(currentIndex, swapIndex);
            }
        }
    }
}
