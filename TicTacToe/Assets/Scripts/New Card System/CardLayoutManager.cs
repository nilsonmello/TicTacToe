using System.Collections.Generic;
using UnityEngine;

public class CardLayoutManager : MonoBehaviour
{

    [Header("Cards and list panels")]
    public List<CardInteraction> cards = new List<CardInteraction>(); //list of cards in this panel
    public static List<CardLayoutManager> AllPanels = new List<CardLayoutManager>(); //static list of all panels

    [Header("Layout Settings")]
    public float dynamicSpacing = 150f; //horizontal spacing between cards
    public float curveHeight = 100f; //height of curve for card layout
    public float selectRaise = 40f; //vertical raise of selected card
    private CardInteraction selectedCard; //currently selected card

    [Header("Panel Type")]
    public CardPanel panelData; //panel metadata or behavior

    private void OnEnable()
    {
        if (!AllPanels.Contains(this))
            AllPanels.Add(this); //add this panel to global list on enable
    }

    private void OnDisable()
    {
        if (AllPanels.Contains(this))
            AllPanels.Remove(this); //remove this panel from global list on disable
    }

    public void LayoutCards()
    {
        int count = cards.Count;
        if (count == 0) return;

        float maxWidth = 1000;
        float effectiveSpacing = Mathf.Min(dynamicSpacing, maxWidth / Mathf.Max(1, count - 1));

        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            var card = cards[i];

            if (card.IsDragging)
                continue;

            float xPos = (i - centerIndex) * effectiveSpacing;
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
        if (!cards.Contains(draggedCard)) return; //ignore if card not in this panel

        List<CardInteraction> tempList = new List<CardInteraction>(cards);
        tempList.Remove(draggedCard); //temporarily remove dragged card

        float centerIndex = (tempList.Count) / 2f;
        int newIndex = 0;
        float closestDist = float.MaxValue;

        //find closest slot based on dragX position
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
        tempList.Insert(newIndex, draggedCard); //insert dragged card at closest slot

        ApplyLayoutFromList(tempList, draggedCard); //apply layout to all cards except dragged
    }

    public void ReorderCard(CardInteraction card)
    {
        if (!cards.Contains(card)) return; //ignore if card not in list

        List<CardInteraction> tempList = new List<CardInteraction>(cards);
        tempList.Remove(card); //remove card temporarily

        float localPosX = transform.InverseTransformPoint(card.transform.position).x; //get local x position of card

        float centerIndex = (tempList.Count) / 2f;
        int newIndex = 0;
        float closestDist = float.MaxValue;

        //find closest index based on card position
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
        tempList.Insert(newIndex, card); //insert card back at new position

        cards = tempList; //update cards list
        ApplyLayoutFromList(cards); //apply layout
    }

    public bool IsSelected(CardInteraction card)
    {
        return selectedCard == card; //check if card is currently selected
    }

    public void SelectCard(CardInteraction card)
    {
        if (selectedCard == card)
            return; //ignore if already selected

        if (selectedCard != null && selectedCard.cardVisualInstance != null)
        {
            int order = cards.IndexOf(selectedCard);
            selectedCard.cardVisualInstance.ResetSortingOrder(order); //reset sorting of old selected card
            selectedCard.cardVisualInstance.DeselectVisual(); //play deselect animation
            selectedCard.cardVisualInstance.SetHoverState(false); //clear hover state
        }

        selectedCard = card; //set new selected card

        if (selectedCard != null && selectedCard.cardVisualInstance != null)
        {
            selectedCard.cardVisualInstance.SelectVisual(); //play select animation
        }

        LayoutCards(); //update layout with selection changes
    }

    public void DeselectCard()
    {
        if (selectedCard != null && selectedCard.cardVisualInstance != null)
        {
            int order = cards.IndexOf(selectedCard);
            selectedCard.cardVisualInstance.ResetSortingOrder(order); //reset sorting order
            selectedCard.cardVisualInstance.DeselectVisual(); //play deselect animation
        }

        selectedCard = null; //clear selected card
        LayoutCards(); //update layout
    }

    public void DeselectAllExcept(CardInteraction exceptCard)
    {
        if (selectedCard != null && selectedCard != exceptCard && selectedCard.cardVisualInstance != null)
        {
            selectedCard.cardVisualInstance.DeselectVisual(); //deselect current card visually
            selectedCard.cardVisualInstance.SetSelectedState(false); //update selection state
            selectedCard.cardVisualInstance.ReturnSortingLayer(); //return sorting to original
        }

        if (exceptCard == null || selectedCard != exceptCard)
            selectedCard = null; //clear selection if different from exceptCard

        LayoutCards(); //update layout
    }

    private void OnDrawGizmos()
    {
        if (cards == null || cards.Count == 0)
            return; //skip if no cards

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

                Debug.DrawLine(cardWorldPos, slotWorldPos, Color.green); //draw line from dragged card to target slot
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
                continue; //skip excluded card

            float xPos = (i - centerIndex) * dynamicSpacing;
            float normalizedX = centerIndex != 0 ? (i - centerIndex) / centerIndex : 0f;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);

            if (card == selectedCard)
                targetPos += Vector3.up * selectRaise; //raise selected card

            card.SetLocalPositionInstant(targetPos); //instantly set position for smooth layout
        }
    }

    public int GetCardOrder(CardInteraction card)
    {
        return cards.IndexOf(card); //return index of card in list for sorting
    }

}