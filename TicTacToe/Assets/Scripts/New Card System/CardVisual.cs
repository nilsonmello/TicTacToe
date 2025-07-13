using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardVisual : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    private Card card;
    private Vector3 originalScale;
    private Vector3 raisedScale;
    private float scaleAmount = 1.1f;
    private float animationDuration = 0.2f;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Transform originalParent;

    private CardLayoutManager layoutManager;

    private void Awake()
    {
        originalScale = transform.localScale;
        raisedScale = originalScale * scaleAmount;

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // CORREÇÃO: usa GetComponent em vez de AddComponent
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        layoutManager = GetComponentInParent<CardLayoutManager>();
    }

    public void Setup(Card cardData)
    {
        card = cardData;
        nameText.text = card.getName();
        descriptionText.text = card.getDescription();
        costText.text = card.getEnergyCost().ToString();
    }

    public void SelectVisual()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(raisedScale));
    }

    public void DeselectVisual()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (layoutManager != null)
        {
            if (layoutManager.IsSelected(this))
                layoutManager.DeselectCard(); // desseleciona se já era a selecionada
            else
                layoutManager.SelectCard(this); // seleciona normalmente
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // (opcional: destaque ao passar o mouse, ex: cartas do mesmo tipo)
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        canvasGroup.blocksRaycasts = false; // importante para o drop funcionar
        transform.SetParent(canvas.transform, true); // move para o topo do canvas para arrastar sobre tudo
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );
        transform.localPosition = pos;
        layoutManager.SimulateDrag(this, transform.localPosition.x);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, true); // volta para o painel de cartas

        // Reordenar baseado na nova posição
        layoutManager.ReorderCard(this, transform.localPosition.x);
    }

    private System.Collections.IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float time = 0f;

        while (time < animationDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
