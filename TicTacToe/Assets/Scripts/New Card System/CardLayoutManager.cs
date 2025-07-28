using System.Collections.Generic;
using UnityEngine;

public class CardLayoutManager : MonoBehaviour
{
    public static List<CardLayoutManager> AllPanels = new List<CardLayoutManager>();

    [Header("Cards")]
    public List<CardInteraction> cards = new List<CardInteraction>();

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
            if (cards[i].IsDragging)
                continue;

            float xPos = (i - centerIndex) * dynamicSpacing;

            float normalizedX = centerIndex != 0 ? (i - centerIndex) / centerIndex : 0f;

            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);

            if (cards[i] == selectedCard)
                targetPos += Vector3.up * selectRaise;

            cards[i].MoveToLocalPosition(targetPos);
            cards[i].SetOriginalSortingOrder(i);
            cards[i].SetSortingOrder(i);
        }
    }

    public void SimulateDrag(CardInteraction draggedCard, float dragX)
    {
        if (!cards.Contains(draggedCard)) return;

        int count = cards.Count;
        if (count == 0) return;

        float centerIndex = (count - 1) / 2f;

        cards.Remove(draggedCard);

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

        newIndex = Mathf.Clamp(newIndex, 0, cards.Count);
        cards.Insert(newIndex, draggedCard);

        LayoutCards();
    }

    public void ReorderCard(CardInteraction card, float localPosX)
    {
        if (!cards.Contains(card)) return;

        int count = cards.Count;
        if (count == 0) return;

        float centerIndex = (count - 1) / 2f;

        cards.Remove(card);

        int newIndex = 0;
        float closestDist = float.MaxValue;
        for (int i = 0; i <= cards.Count; i++)
        {
            float xPos = (i - centerIndex) * dynamicSpacing;
            float dist = Mathf.Abs(localPosX - xPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                newIndex = i;
            }
        }

        newIndex = Mathf.Clamp(newIndex, 0, cards.Count);
        cards.Insert(newIndex, card);

        LayoutCards();
    }

    public bool IsSelected(CardInteraction card)
    {
        return selectedCard == card;
    }

    public void SelectCard(CardInteraction card)
    {
        if (selectedCard == card)
            return;

        selectedCard = card;
        LayoutCards();

        if (selectedCard != null)
        {
            selectedCard.cardVisual.DeselectVisual();
            selectedCard.RestoreOriginalSortingOrder();
        }
    }

    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard.cardVisual.DeselectVisual();
            selectedCard.RestoreOriginalSortingOrder();
        }

        selectedCard = null;
        LayoutCards();
    }

    public void DeselectAllExcept(CardInteraction exceptCard)
    {
        if (exceptCard == null)
            selectedCard = null;
        else if (selectedCard != exceptCard)
            selectedCard = null;

        LayoutCards();
    }
}
