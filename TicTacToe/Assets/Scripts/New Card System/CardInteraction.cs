using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("references")]
    private Canvas canvas;                          //canvas reference
    private CanvasGroup canvasGroup;                //canvas group for raycast control
    private RectTransform rectTransform;            //rect transform for positioning
    private Transform originalParent;               //original parent before drag
    public CardVisual cardVisual;                   //reference to card visual
    private CardLayoutManager layoutManager;        //reference to layout manager

    [Header("smooth movement")]
    private Coroutine moveCoroutine;                //movement coroutine reference
    public float moveSpeed = 1000f;                 //speed for tween movement

    [Header("drag control")]
    private Vector3 targetDragPosition;             //target position during drag
    private Vector3 velocity = Vector3.zero;        //velocity for smooth damp
    public float followSmoothTime = 0.05f;          //smooth follow time
    private bool isDragging = false;                //dragging state flag
    private Vector3 dragOffset;                     //offset from mouse during drag

    [Header("rotation control")]
    public float rotationMultiplier = 0.1f;         //multiplier for tilt based on drag
    public float rotationReturnSpeed = 10f;         //speed to return to original rotation
    private Quaternion originalRotation;            //original local rotation of card

    [Header("sorting order")]
    private Canvas cardCanvas;                      //canvas to override sorting
    private int originalSortingOrder;               //original sorting order
    public bool IsDragging => isDragging;           //property to expose dragging status

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
            //move toward drag position
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                targetDragPosition,
                ref velocity,
                followSmoothTime
            );

            //update rotation based on drag delta
            float horizontalDelta = targetDragPosition.x - transform.localPosition.x;
            UpdateRotation(horizontalDelta, 10f);
        }
        else
        {
            //return to original rotation when not dragging
            ReturnToOriginalRotation();
        }
    }

    //update rotation using drag delta
    private void UpdateRotation(float horizontalDelta, float lerpSpeed)
    {
        float angle = horizontalDelta * rotationMultiplier;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * lerpSpeed);
    }

    //return to original rotation
    private void ReturnToOriginalRotation()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * rotationReturnSpeed);
    }

    //set rotation instantly (used after drag)
    private void SetRotationInstant(float horizontalDelta, float multiplier)
    {
        float angle = horizontalDelta * multiplier;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;

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
            layoutManager.DeselectAllExcept(null);

        cardVisual?.KillAllTweens();

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
        cardVisual.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : Camera.main;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            cam,
            out Vector3 worldMousePos))
        {
            Vector3 targetWorldPos = worldMousePos + dragOffset;

            // Este cálculo funciona bem mesmo com câmera perspective
            targetDragPosition = canvas.transform.InverseTransformPoint(targetWorldPos);
        }
        else
        {
            Debug.LogWarning("Couldn't convert screen to world position.");
        }

        layoutManager?.SimulateDrag(this, targetDragPosition.x);
        cardVisual?.OnDragVisual();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, true);

        float horizontalDelta = targetDragPosition.x - transform.localPosition.x;
        SetRotationInstant(horizontalDelta, rotationMultiplier * 1.5f);

        isDragging = false;
        layoutManager?.ReorderCard(this, transform.localPosition.x);
        cardVisual?.OnEndDragVisual();

        targetDragPosition = transform.localPosition;
        cardVisual.enabled = true;
    }

    public void MoveToLocalPosition(Vector3 targetPos)
    {
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

            float horizontalDelta = endPos.x - transform.localPosition.x;
            float clampedAngle = Mathf.Clamp(horizontalDelta * rotationMultiplier * 15f, -20f, 20f);
            Quaternion targetRotation = Quaternion.Euler(0, 0, clampedAngle);
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