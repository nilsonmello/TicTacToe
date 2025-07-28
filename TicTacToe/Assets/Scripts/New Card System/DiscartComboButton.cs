using UnityEngine;
using System.Collections.Generic;

public class DiscardComboButton : MonoBehaviour
{
    public CardLayoutManager comboPanel;
    public CardLayoutManager discardPanel;

    public void DiscardAllComboCards()
    {
        if (comboPanel == null || discardPanel == null)
        {
            Debug.LogWarning("Combo or Discard panel reference is missing.");
            return;
        }

        List<CardInteraction> cardsToDiscard = new List<CardInteraction>(comboPanel.cards);

        comboPanel.cards.Clear();

        float centerIndex = (cardsToDiscard.Count - 1) / 2f;

        for (int i = 0; i < cardsToDiscard.Count; i++)
        {
            CardInteraction card = cardsToDiscard[i];

            card.layoutManager = discardPanel;

            Vector3 worldPos = card.transform.position;
            Quaternion worldRot = card.transform.rotation;

            card.transform.SetParent(discardPanel.transform, false);

            card.transform.position = worldPos;
            card.transform.rotation = worldRot;

            discardPanel.cards.Add(card);

            float xPos = (i - centerIndex) * discardPanel.dynamicSpacing;
            float normalizedX = centerIndex != 0 ? (i - centerIndex) / centerIndex : 0f;
            float yPos = -Mathf.Pow(normalizedX, 2) * discardPanel.curveHeight + discardPanel.curveHeight;
            Vector3 targetLocalPos = new Vector3(xPos, yPos, 0f);

            card.MoveToLocalPosition(targetLocalPos);

            card.SetOriginalSortingOrder(i);
            card.SetSortingOrder(i);
            card.canInteract = false;

            discardPanel.panelData?.OnCardDropped(card);
        }

        discardPanel.LayoutCards();
    }
}
