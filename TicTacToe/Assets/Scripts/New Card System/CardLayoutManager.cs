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

    private List<CardInteraction> selectedCards = new List<CardInteraction>();
    private const int maxSelected = 3;

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

        float maxWidth = 1000f;
        float effectiveSpacing = Mathf.Min(dynamicSpacing, maxWidth / Mathf.Max(1, count - 1));
        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            var card = cards[i];
            if (card.IsDragging) continue;

            float xPos = (i - centerIndex) * effectiveSpacing;
            float normalizedX = centerIndex != 0 ? (i - centerIndex) / centerIndex : 0f;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);
            if (selectedCards.Contains(card))
                targetPos += Vector3.up * selectRaise;

            card.MoveToLocalPosition(targetPos);

            if (!selectedCards.Contains(card) && card.cardVisualInstance != null)
                card.cardVisualInstance.SetSortingOrder(count - i); // manter ordem visual
        }
    }

    public void SimulateDrag(CardInteraction draggedCard, float dragX)
    {
        if (!cards.Contains(draggedCard)) return;

        List<CardInteraction> tempList = new List<CardInteraction>(cards);
        tempList.Remove(draggedCard);

        float centerIndex = tempList.Count / 2f;
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
        float centerIndex = tempList.Count / 2f;
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
        return selectedCards.Contains(card);
    }

    public void SelectCard(CardInteraction card)
    {
        if (selectedCards.Contains(card)) return;

        if (selectedCards.Count >= maxSelected)
        {
            CardInteraction oldest = selectedCards[0];
            if (oldest.cardVisualInstance != null)
            {
                int order = cards.IndexOf(oldest);
                oldest.cardVisualInstance.ResetSortingOrder(order);
                oldest.cardVisualInstance.DeselectVisual();
                oldest.cardVisualInstance.SetHoverState(false);
                oldest.cardVisualInstance.SetSelectedState(false);
            }
            selectedCards.RemoveAt(0);
        }

        selectedCards.Add(card);

        if (card != null && card.cardVisualInstance != null)
        {
            card.cardVisualInstance.SelectVisual();
            card.cardVisualInstance.SetSelectedState(true);
        }

        LayoutCards();
    }

    public void DeselectCard(CardInteraction card)
    {
        if (selectedCards.Contains(card))
        {
            if (card.cardVisualInstance != null)
            {
                int order = cards.IndexOf(card);
                card.cardVisualInstance.ResetSortingOrder(order);
                card.cardVisualInstance.DeselectVisual();
                card.cardVisualInstance.SetSelectedState(false);
            }
            selectedCards.Remove(card);
        }
        LayoutCards();
    }

    public void DeselectAllExcept(CardInteraction exceptCard)
    {
        for (int i = selectedCards.Count - 1; i >= 0; i--)
        {
            var c = selectedCards[i];
            if (c != exceptCard && c.cardVisualInstance != null)
            {
                c.cardVisualInstance.DeselectVisual();
                c.cardVisualInstance.SetSelectedState(false);
                c.cardVisualInstance.ReturnSortingLayer();
                selectedCards.RemoveAt(i);
            }
        }

        LayoutCards();
    }

    private void ApplyLayoutFromList(List<CardInteraction> list, CardInteraction exclude = null)
    {
        int count = list.Count;
        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            var card = list[i];
            if (card == exclude) continue;

            float xPos = (i - centerIndex) * dynamicSpacing;
            float normalizedX = centerIndex != 0 ? (i - centerIndex) / centerIndex : 0f;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);
            if (selectedCards.Contains(card))
                targetPos += Vector3.up * selectRaise;

            card.SetLocalPositionInstant(targetPos);
        }
    }

    public int GetCardOrder(CardInteraction card)
    {
        return cards.IndexOf(card);
    }

    public void DeselectAll()
    {
        for (int i = selectedCards.Count - 1; i >= 0; i--)
        {
            var c = selectedCards[i];
            if (c.cardVisualInstance != null)
            {
                c.cardVisualInstance.DeselectVisual();
                c.cardVisualInstance.SetSelectedState(false);
                c.cardVisualInstance.ReturnSortingLayer();
            }
            selectedCards.RemoveAt(i);
        }

        LayoutCards();
    }
}