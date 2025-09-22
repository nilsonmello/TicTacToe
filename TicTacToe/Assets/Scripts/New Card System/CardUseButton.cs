using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardUseButton : MonoBehaviour
{
    [Header("References")]
    public Button useButton;
    public CardLayoutManager layoutManager;
    public CardLayoutManager discardPanel;

    private void Start()
    {
        if (useButton != null)
            useButton.onClick.AddListener(OnUseCards);

        UpdateButtonState();
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (useButton == null || layoutManager == null) return;

        bool hasSelected = layoutManager.SelectedCount > 0;
        useButton.interactable = hasSelected;
    }

    private void OnUseCards()
    {
        if (layoutManager == null || discardPanel == null) return;

        List<CardInteraction> selected = layoutManager.GetSelectedCards();
        if (selected.Count == 0) return;

        string names = string.Join(", ", selected.ConvertAll(c => c.name));
        Debug.Log("Usou cartas: " + names);

        float centerIndex = (selected.Count - 1) / 2f;
        for (int i = 0; i < selected.Count; i++)
        {
            CardInteraction card = selected[i];

            layoutManager.RemoveCard(card);

            card.layoutManager = discardPanel;
            Vector3 worldPos = card.transform.position;
            Quaternion worldRot = card.transform.rotation;

            card.transform.SetParent(discardPanel.transform, false);
            card.transform.position = worldPos;
            card.transform.rotation = worldRot;

            discardPanel.cards.Add(card);

            // calcular posição no descarte
            float xPos = (i - centerIndex) * discardPanel.dynamicSpacing;
            float normalizedX = centerIndex != 0 ? (i - centerIndex) / centerIndex : 0f;
            float yPos = -Mathf.Pow(normalizedX, 2) * discardPanel.curveHeight + discardPanel.curveHeight;
            Vector3 targetLocalPos = new Vector3(xPos, yPos, 0f);

            card.MoveToLocalPosition(targetLocalPos);

            card.canInteract = false;

            discardPanel.panelData?.OnCardDropped(card);
        }

        layoutManager.DeselectAll();

        discardPanel.LayoutCards();
    }
}
