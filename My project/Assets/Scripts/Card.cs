using UnityEngine;
using UnityEngine.EventSystems;

public enum CardType { Ataque, Defesa, Suporte }

[RequireComponent(typeof(RectTransform))]
public class Card : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public RotatingSlotManager slotManager;

    public float hoverLift = 20f;
    public CardType cardType;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = Vector3.one * 1.1f;
    private bool isVisuallyLifted = false;
    private bool isHovered = false;

    public bool IsLifted => isVisuallyLifted;

    void Start()
    {
        transform.localScale = normalScale;
        cardType = (CardType)Random.Range(0, 3);

        if (slotManager == null)
            slotManager = GetComponentInParent<RotatingSlotManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotManager == null || !slotManager.cards.Contains(this))
        {
            Debug.LogWarning("slotManager inválido ou carta ainda não registrada no deck.");
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            slotManager.SelectCard(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Clique direito na carta: " + name);

            if (slotManager.otherManager != null)
            {
                slotManager.TransferCardToOtherManager(this, slotManager.otherManager);
                Debug.Log("Carta movida para outro deck.");
            }
            else
            {
                Debug.LogWarning("OtherManager não está atribuído!");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        transform.localScale = hoverScale;

        if (slotManager != null)
            slotManager.HighlightCardsOfSameType(cardType, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isVisuallyLifted)
        {
            isHovered = false;
            transform.localScale = normalScale;

            if (slotManager != null)
                slotManager.HighlightCardsOfSameType(cardType, false);
        }
    }

    public void ResetVisualLift(bool keepLift = false)
    {
        isVisuallyLifted = keepLift;
        transform.localPosition = keepLift ? new Vector3(0, hoverLift, 0) : Vector3.zero;
        transform.localScale = keepLift ? hoverScale : normalScale;
        isHovered = false;
    }

    public void SetHoverScale(bool isHovering)
    {
        if (!isVisuallyLifted)
            transform.localScale = isHovering ? hoverScale : normalScale;
    }
}
