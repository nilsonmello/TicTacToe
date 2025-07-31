using System.Collections.Generic;
using UnityEngine;

public class CardLayoutManager : MonoBehaviour
{
    [Header("Cards and list panels")]
    public List<CardInteraction> cards = new List<CardInteraction>();
    public static List<CardLayoutManager> AllPanels = new List<CardLayoutManager>();

    [Header("Layout Settings")]
    public float dynamicSpacing = 150f;
    public float curveHeight = 100f;
    public float selectRaise = 40f;
    private CardInteraction selectedCard;

    [Header("Panel Type")]
    public CardPanel panelData;

    private void OnEnable()
    {
        if (!AllPanels.Contains(this))
            AllPanels.Add(this);
    }

    private void OnDisable()
    {
        if (AllPanels.Contains(this))
            AllPanels.Remove(this);
    }

    public void LayoutCards()
    {
        int count = cards.Count;
        if (count == 0) return;

        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            var card = cards[i];

            if (card.IsDragging)
                continue;

            float xPos = (i - centerIndex) * dynamicSpacing;
            float normalizedX = centerIndex != 0 ? (i - centerIndex) / centerIndex : 0f;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;
            Vector3 targetPos = new Vector3(xPos, yPos, 0);

            if (card == selectedCard)
                targetPos += Vector3.up * selectRaise;

            card.MoveToLocalPosition(targetPos);

            if (card != selectedCard && card.cardVisualInstance != null)
            {
                card.cardVisualInstance.SetSortingOrder(count - i);
            }
        }
    }

    public void SimulateDrag(CardInteraction draggedCard, float dragX)
    {
        if (!cards.Contains(draggedCard)) return;

        List<CardInteraction> tempList = new List<CardInteraction>(cards);
        tempList.Remove(draggedCard);

        float centerIndex = (tempList.Count) / 2f;
        int newIndex = 0;
        float closestDist = float.MaxValue;

        for (int i = 0; i <= tempList.Count; i++)
        {
            float xPos = (i - centerIndex) * dynamicSpacing;
            float dist = Mathf.Abs(dragX - xPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                newIndex = i;
            }
        }

        newIndex = Mathf.Clamp(newIndex, 0, tempList.Count);
        tempList.Insert(newIndex, draggedCard);

        ApplyLayoutFromList(tempList, draggedCard);
    }

    public void ReorderCard(CardInteraction card)
    {
        if (!cards.Contains(card)) return;

        List<CardInteraction> tempList = new List<CardInteraction>(cards);
        tempList.Remove(card);

        float localPosX = transform.InverseTransformPoint(card.transform.position).x;

        float centerIndex = (tempList.Count) / 2f;

        int newIndex = 0;
        float closestDist = float.MaxValue;

        for (int i = 0; i <= tempList.Count; i++)
        {
            float xPos = (i - centerIndex) * dynamicSpacing;
            float dist = Mathf.Abs(localPosX - xPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                newIndex = i;
            }
        }

        newIndex = Mathf.Clamp(newIndex, 0, tempList.Count);
        tempList.Insert(newIndex, card);

        cards = tempList;
        ApplyLayoutFromList(cards);
    }

    public bool IsSelected(CardInteraction card)
    {
        return selectedCard == card;
    }

    public void SelectCard(CardInteraction card)
    {
        if (selectedCard == card)
            return;

        if (selectedCard != null && selectedCard.cardVisualInstance != null)
        {
            int order = cards.IndexOf(selectedCard);
            selectedCard.cardVisualInstance.ResetSortingOrder(order);
            selectedCard.cardVisualInstance.DeselectVisual();
            selectedCard.cardVisualInstance.SetHoverState(false);
        }

        selectedCard = card;

        if (selectedCard != null && selectedCard.cardVisualInstance != null)
        {
            selectedCard.cardVisualInstance.SelectVisual();
        }

        LayoutCards();
    }

    public void DeselectCard()
    {
        if (selectedCard != null && selectedCard.cardVisualInstance != null)
        {
            int order = cards.IndexOf(selectedCard);
            selectedCard.cardVisualInstance.ResetSortingOrder(order);
            selectedCard.cardVisualInstance.DeselectVisual();
        }

        selectedCard = null;
        LayoutCards();
    }

    public void DeselectAllExcept(CardInteraction exceptCard)
    {
        if (selectedCard != null && selectedCard != exceptCard && selectedCard.cardVisualInstance != null)
        {
            selectedCard.cardVisualInstance.DeselectVisual();
            selectedCard.cardVisualInstance.SetSelectedState(false);
            selectedCard.cardVisualInstance.ReturnSortingLayer();
        }

        if (exceptCard == null || selectedCard != exceptCard)
            selectedCard = null;

        LayoutCards();
    }

    private void OnDrawGizmos()
    {
        if (cards == null || cards.Count == 0)
            return;

        float centerIndex = (cards.Count - 1) / 2f;

        foreach (var card in cards)
        {
            if (card.IsDragging)
            {
                float dragX = card.transform.localPosition.x;

                int newIndex = 0;
                float closestDist = float.MaxValue;
                for (int i = 0; i <= cards.Count; i++)
                {
                    float xPos = (i - centerIndex) * dynamicSpacing;
                    float dist = Mathf.Abs(dragX - xPos);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        newIndex = i;
                    }
                }

                float slotXPos = (newIndex - centerIndex) * dynamicSpacing;
                float normalizedX = centerIndex != 0 ? (newIndex - centerIndex) / centerIndex : 0f;
                float slotYPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;
                Vector3 slotLocalPos = new Vector3(slotXPos, slotYPos, 0);

                Vector3 cardWorldPos = card.transform.position;
                Vector3 slotWorldPos = transform.TransformPoint(slotLocalPos);

                Debug.DrawLine(cardWorldPos, slotWorldPos, Color.green);
            }
        }
    }

    private void ApplyLayoutFromList(List<CardInteraction> list, CardInteraction exclude = null)
    {
        int count = list.Count;
        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            var card = list[i];

            if (card == exclude)
                continue;

            float xPos = (i - centerIndex) * dynamicSpacing;
            float normalizedX = centerIndex != 0 ? (i - centerIndex) / centerIndex : 0f;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);

            if (card == selectedCard)
                targetPos += Vector3.up * selectRaise;

            card.SetLocalPositionInstant(targetPos);
        }
    }

    public int GetCardOrder(CardInteraction card)
    {
        return cards.IndexOf(card);
    }
}