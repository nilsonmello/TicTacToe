using UnityEngine;

public class ShowCard : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardParent;

    private CardLayoutManager layoutManager;

    void Start()
    {
        layoutManager = cardParent.GetComponent<CardLayoutManager>();
        if (layoutManager == null)
        {
            Debug.LogError("CardLayoutManager não encontrado no cardParent.");
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            Card carta = CardDatabase.GetRandomCard();
            if (carta != null)
            {
                GameObject go = Instantiate(cardPrefab, cardParent);

                CardInteraction interaction = go.GetComponent<CardInteraction>();

                CardVisual visual = go.GetComponentInChildren<CardVisual>();
                interaction.cardVisual = visual;

                visual.Setup(carta);

                layoutManager.cards.Add(interaction);
            }
        }

        layoutManager.LayoutCards();
    }
}
