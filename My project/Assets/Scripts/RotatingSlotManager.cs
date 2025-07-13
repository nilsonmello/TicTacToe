using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotatingSlotManager : MonoBehaviour
{
    [Header("Config")]
    public int maxSlots = 7;
    public float slotSpacing = 150f;
    public float curveHeight = 50f;
    public float selectionOffset = 60f;

    [Header("Refs")]
    public List<Transform> slots = new List<Transform>();
    public List<Card> cards = new List<Card>();

    [Header("State")]
    public Card selectedCard = null;

    [Header("Deck Secundário")]
    public RotatingSlotManager otherManager;

    private void Start()
    {
        CreateSlots();
        UpdateCardParents();
        UpdateSlotsPosition();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIElement())
            {
                DeselectCard();
            }
        }
    }

    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    private void CreateSlots()
    {
        foreach (Transform t in slots)
        {
            if (Application.isPlaying)
                Destroy(t.gameObject);
        }
        slots.Clear();

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotGO = new GameObject("Slot_" + i, typeof(RectTransform));
            slotGO.transform.SetParent(transform, false);
            slotGO.transform.localPosition = Vector3.zero;
            slots.Add(slotGO.transform);
        }
    }

    public void UpdateSlotsPosition()
    {
        int count = cards.Count;
        if (count == 0) return;

        for (int i = 0; i < slots.Count; i++)
            slots[i].gameObject.SetActive(i < count); // ← só ativa os slots que serão usados

        for (int i = 0; i < count; i++)
        {
            float x = (i - (count - 1) / 2f) * slotSpacing;
            slots[i].localPosition = new Vector3(x, 0, 0);
        }
    }

    public void UpdateCardParents()
    {
        int count = cards.Count;
        for (int i = 0; i < count; i++)
        {
            if (cards[i] == null) continue;

            cards[i].transform.SetParent(slots[i], false);

            float yOffset = cards[i].IsLifted ? cards[i].hoverLift : 0f;
            cards[i].transform.localPosition = new Vector3(0, yOffset, 0);
        }
    }

    public void RotateLeft()
    {
        if (cards.Count == 0) return;
        Card first = cards[0];
        cards.RemoveAt(0);
        cards.Add(first);
        UpdateCardParents();
        UpdateSlotsPosition();
    }

    public void RotateRight()
    {
        if (cards.Count == 0) return;
        Card last = cards[cards.Count - 1];
        cards.RemoveAt(cards.Count - 1);
        cards.Insert(0, last);
        UpdateCardParents();
        UpdateSlotsPosition();
    }

    public void SwapCards(int i, int j)
    {
        if (i == j) return;

        if (i < 0 || i >= cards.Count) return;
        if (j < 0 || j >= cards.Count) return;

        Card temp = cards[i];
        cards[i] = cards[j];
        cards[j] = temp;
        UpdateCardParents();
        UpdateSlotsPosition();
    }

    public void SelectCard(Card card)
    {
        foreach (var c in cards)
        {
            if (c != null)
                c.SetHoverScale(false);
        }

        if (selectedCard == card)
        {
            if (selectedCard != null)
                selectedCard.ResetVisualLift(false);
            selectedCard = null;
        }
        else
        {
            if (selectedCard != null)
                selectedCard.ResetVisualLift(false);

            selectedCard = card;
            selectedCard.ResetVisualLift(true);
            HighlightCardsOfSameType(selectedCard.cardType, true);
        }

        UpdateCardParents();
        UpdateSlotsPosition();
    }

    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard.ResetVisualLift(false);
            selectedCard = null;

            UpdateCardParents();
            UpdateSlotsPosition();

            foreach (var card in cards)
            {
                if (card != null)
                    card.SetHoverScale(false);
            }
        }
    }

    public void HighlightCardsOfSameType(CardType type, bool highlight)
    {
        if (cards == null) return;

        foreach (var card in cards)
        {
            if (card == null) continue;

            if (card.cardType == type && !card.IsLifted)
            {
                card.SetHoverScale(highlight);
            }
        }
    }

    public void TransferCardToOtherManager(Card card, RotatingSlotManager targetManager)
    {
        if (targetManager == null) return;
        if (targetManager.cards.Count >= targetManager.maxSlots) return;

        bool removed = cards.Remove(card);
        if (!removed)
        {
            Debug.LogWarning("Carta não estava na lista do deck atual.");
        }

        card.slotManager = targetManager;
        targetManager.cards.Add(card);
        card.ResetVisualLift(false);

        // Remove bloco antigo de SetParent manual

        UpdateCardParents();
        UpdateSlotsPosition();

        targetManager.UpdateCardParents();
        targetManager.UpdateSlotsPosition();
    }
}
