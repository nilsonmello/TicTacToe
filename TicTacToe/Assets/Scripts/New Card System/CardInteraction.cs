using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;

    public CardVisual cardVisual;
    private CardLayoutManager layoutManager;

    [Header("Smooth Movement")]
    private Coroutine moveCoroutine;
    public float moveSpeed = 1000f;

    [Header("Drag Control")]
    private Vector3 targetDragPosition;
    private Vector3 velocity = Vector3.zero;
    public float followSmoothTime = 0.05f;
    private bool isDragging = false;

    private Vector3 dragOffset;

    [Header("Rotation Control")]
    public float rotationMultiplier = 0.1f;
    public float rotationReturnSpeed = 10f;
    private Quaternion originalRotation;

    [Header("Sorting Order")]
    private Canvas cardCanvas;
    private int originalSortingOrder;
    public bool IsDragging => isDragging;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        layoutManager = GetComponentInParent<CardLayoutManager>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        cardCanvas = GetComponent<Canvas>();
        if (cardCanvas == null)
        {
            cardCanvas = gameObject.AddComponent<Canvas>();
            cardCanvas.overrideSorting = true;
        }

        originalSortingOrder = cardCanvas.sortingOrder;
        originalRotation = transform.localRotation;
    }

    private void Update()
    {
        if (isDragging)
        {
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                targetDragPosition,
                ref velocity,
                followSmoothTime
            );

            float angle = (targetDragPosition.x - transform.localPosition.x) * rotationMultiplier;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * rotationReturnSpeed);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return; // Evita seleção enquanto arrasta

        if (layoutManager != null)
        {
            if (layoutManager.IsSelected(this))
                layoutManager.DeselectCard();
            else
                layoutManager.SelectCard(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (layoutManager != null)
        {
            layoutManager.DeselectAllExcept(null); // Limpa qualquer seleção
        }

        cardVisual?.KillAllTweens();

        // Para a coroutine de movimento suave para evitar conflito com o drag
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (cardVisual != null)
        {
            cardVisual.scaleTarget.localScale = Vector3.one;
            cardVisual.scaleTarget.localRotation = Quaternion.identity;
        }

        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldMousePos))
        {
            dragOffset = transform.position - worldMousePos;
        }
        else
        {
            dragOffset = Vector3.zero;
        }

        transform.SetParent(canvas.transform, true);
        isDragging = true;

        cardVisual?.OnBeginDragVisual();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldMousePos))
        {
            Vector3 targetWorldPos = worldMousePos + dragOffset;
            targetDragPosition = canvas.transform.InverseTransformPoint(targetWorldPos);
        }

        layoutManager?.SimulateDrag(this, targetDragPosition.x);
        cardVisual?.OnDragVisual();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, true);

        float angle = (targetDragPosition.x - transform.localPosition.x) * rotationMultiplier * 1.5f;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        isDragging = false;
        layoutManager?.ReorderCard(this, transform.localPosition.x);
        cardVisual?.OnEndDragVisual();

        targetDragPosition = transform.localPosition;
    }

    public void MoveToLocalPosition(Vector3 targetPos)
    {
        // Não tenta mover suavemente se estiver arrastando para não conflitar com drag no Update
        if (isDragging) return;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(targetPos));
    }

    public void SetLocalPositionInstant(Vector3 pos)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        transform.localPosition = pos;
    }

    private IEnumerator MoveRoutine(Vector3 endPos)
    {
        while (Vector3.Distance(transform.localPosition, endPos) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPos, moveSpeed * Time.deltaTime);

            float angleZ = Mathf.Clamp((endPos.x - transform.localPosition.x) * rotationMultiplier * 15f, -20f, 20f);
            Quaternion targetRotation = Quaternion.Euler(0, 0, angleZ);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 8f);

            yield return null;
        }

        transform.localPosition = endPos;

        float t = 0f;
        while (Quaternion.Angle(transform.localRotation, originalRotation) > 0.5f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, t);
            t += Time.deltaTime * 4f;
            yield return null;
        }

        transform.localRotation = originalRotation;
        moveCoroutine = null;
    }

    public void SetSortingOrder(int order)
    {
        cardCanvas.sortingOrder = order;
    }

    public void RestoreOriginalSortingOrder()
    {
        cardCanvas.sortingOrder = originalSortingOrder;
    }

    public void SetOriginalSortingOrder(int order)
    {
        originalSortingOrder = order;
    }
}
