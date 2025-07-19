using UnityEngine;

public class SpawnCards : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject cardPrefab;            //prefab of the card to instantiate
    public Transform cardParent;             //parent transform where cards will be instantiated under
    private CardLayoutManager layoutManager; //reference to the layout manager on cardParent
    [SerializeField] private int cardTotal = 5;
    private void Start()
    {
        //try to get CardLayoutManager component from cardParent
        layoutManager = cardParent.GetComponent<CardLayoutManager>();
        if (layoutManager == null)
        {
            Debug.LogError("CardLayoutManager not found on cardParent.");
            return;
        }

        //spawn random cards and add them to the layout manager
        for (int i = 0; i < cardTotal; i++)
        {
            Card card = CardDatabase.GetRandomCard();
            if (card != null)
            {
                //instantiate card prefab as child of cardParent
                GameObject go = Instantiate(cardPrefab, cardParent);

                //set the name of the game object to the card name
                go.name = card.Name;

                //get CardInteraction component to setup card visuals and logic
                CardInteraction interaction = go.GetComponent<CardInteraction>();

                //get CardVisual component from children and assign the Card data
                CardVisual visual = go.GetComponentInChildren<CardVisual>();
                interaction.cardVisual = visual;

                //setup the card visuals with the card data
                visual.Setup(card);

                //add the card interaction to the layout manager's list
                layoutManager.cards.Add(interaction);
            }
        }

        //arrange cards in layout after spawning
        layoutManager.LayoutCards();
    }
}



