using System.Collections.Generic;
using UnityEngine;

public class CardLayoutManager : MonoBehaviour
{
    public List<CardInteraction> cards = new List<CardInteraction>();
    public float spacing = 150f;
    public float curveHeight = 30f;
    public float selectRaise = 20f;
    public float scaleAmount = 1.1f;

    private CardInteraction selectedCard;

    void Start()
    {
        LayoutCards();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && selectedCard != null)
            {
                DeselectCard();
            }
        }
    }

    public void LayoutCards()
    {
        int count = cards.Count;
        if (count == 0) return;

        float centerIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float xPos = (i - centerIndex) * spacing;

            float normalizedX = (i - centerIndex) / centerIndex;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);
            cards[i].transform.localPosition = targetPos;

            cards[i].transform.localRotation = Quaternion.identity;
            cards[i].transform.localScale = Vector3.one;

            if (cards[i] == selectedCard)
            {
                cards[i].transform.localPosition += Vector3.up * selectRaise;
                cards[i].transform.localScale = Vector3.one * scaleAmount;
            }
        }
    }

    public void SelectCard(CardInteraction card)
    {
        if (!cards.Contains(card)) return;
        if (selectedCard == card) return;

        if (selectedCard != null)
            selectedCard.cardVisual.DeselectVisual();

        selectedCard = card;
        selectedCard.cardVisual.SelectVisual();

        LayoutCards();
    }

    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard.cardVisual.DeselectVisual();
            selectedCard = null;
            LayoutCards();
        }
    }

    public void ReorderCard(CardInteraction draggedCard, float draggedX)
    {
        cards.Remove(draggedCard);

        int insertIndex = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (draggedX > cards[i].transform.localPosition.x)
            {
                insertIndex = i + 1;
            }
        }

        cards.Insert(insertIndex, draggedCard);

        LayoutCards();
    }

    public bool IsSelected(CardInteraction card)
    {
        return selectedCard == card;
    }

    public void SimulateDrag(CardInteraction draggedCard, float draggedX)
    {
        if (!cards.Contains(draggedCard)) return;

        List<CardInteraction> simulated = new List<CardInteraction>(cards);
        simulated.Remove(draggedCard);

        int insertIndex = 0;
        for (int i = 0; i < simulated.Count; i++)
        {
            if (draggedX > simulated[i].transform.localPosition.x)
            {
                insertIndex = i + 1;
            }
        }

        simulated.Insert(insertIndex, draggedCard);

        float centerIndex = (simulated.Count - 1) / 2f;

        for (int i = 0; i < simulated.Count; i++)
        {
            Vector3 pos;
            float xPos = (i - centerIndex) * spacing;
            float normalizedX = (i - centerIndex) / centerIndex;
            float yPos = -Mathf.Pow(normalizedX, 2) * curveHeight + curveHeight;

            pos = new Vector3(xPos, yPos, 0);

            if (simulated[i] != draggedCard)
            {
                simulated[i].transform.localPosition = pos;
                simulated[i].transform.localRotation = Quaternion.identity;
                simulated[i].transform.localScale = Vector3.one;
            }
        }
    }
}
