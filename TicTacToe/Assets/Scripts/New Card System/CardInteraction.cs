using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("references")]
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    public CardVisual cardVisual;
    private CardLayoutManager layoutManager;

    [Header("smooth movement")]
    private Coroutine moveCoroutine;
    public float moveSpeed = 1000f;

    [Header("drag control")]
    private Vector3 targetDragPosition;
    private Vector3 velocity = Vector3.zero;
    public float followSmoothTime = 0.05f;
    private bool isDragging = false;
    private Vector3 dragOffset;

    [Header("rotation control")]
    public float rotationMultiplier = 0.1f;
    public float rotationReturnSpeed = 10f;
    private Quaternion originalRotation;

    [Header("sorting order")]
    private Canvas cardCanvas;
    private int originalSortingOrder;
    public bool IsDragging => isDragging;
    private Vector3 lastPosition;

    private bool isMoving = false;

    private void Awake()
    {
        //cache components and setup canvas
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
        lastPosition = transform.localPosition;
    }

    private void Update()
    {
        if (isMoving) return;

        if (isDragging)
        {
            //smooth follow mouse
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                targetDragPosition,
                ref velocity,
                followSmoothTime
            );

            //apply rotation based on drag delta
            float horizontalDelta = targetDragPosition.x - transform.localPosition.x;
            float angle = Mathf.Clamp(horizontalDelta * rotationMultiplier * 50f, -80f, 80f);
            UpdateRotation(horizontalDelta, 10f);
        }
        else
        {
            //calculate return rotation when not dragging
            Vector3 currentPosition = transform.localPosition;
            float deltaX = currentPosition.x - lastPosition.x;

            float targetAngle = Mathf.Clamp(deltaX * rotationMultiplier * 40f, -8f, 8f);
            float smoothZ = Mathf.LerpAngle(cardVisual.scaleTarget.localRotation.eulerAngles.z, targetAngle, Time.deltaTime * 6f);
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, smoothZ);

            if (cardVisual != null && cardVisual.scaleTarget != null)
            {
                cardVisual.scaleTarget.localRotation = targetRotation;
            }

            lastPosition = currentPosition;
        }
    }

    public void MoveToLocalPosition(Vector3 targetPos)
    {
        //initiate smooth move to target slot
        if (isDragging || isMoving) return;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        isMoving = false;
        moveCoroutine = StartCoroutine(MoveRoutine(targetPos));
    }

    private IEnumerator MoveRoutine(Vector3 endPos)
    {
        //interpolated move and rotation toward target
        isMoving = true;

        Vector3 startPos = transform.localPosition;
        Quaternion startRotation = transform.localRotation;
        float distance = Vector3.Distance(startPos, endPos);

        float horizontalDelta = endPos.x - startPos.x;
        float rawAngle = horizontalDelta * rotationMultiplier * 2f;
        float maxAngle = 80f;
        float clampedAngle = Mathf.Sign(rawAngle) * Mathf.Min(Mathf.Abs(rawAngle), maxAngle);
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, clampedAngle);

        float t = 0f;
        while (Vector3.Distance(transform.localPosition, endPos) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPos, moveSpeed * Time.deltaTime);

            float distCovered = Vector3.Distance(startPos, transform.localPosition);
            t = Mathf.Clamp01(distCovered / distance);

            transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.localPosition = endPos;

        //smooth return to original rotation
        float rotT = 0f;
        while (Quaternion.Angle(transform.localRotation, originalRotation) > 0.5f)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, rotT);
            rotT += Time.deltaTime * 2f;
            yield return null;
        }

        transform.localRotation = originalRotation;
        moveCoroutine = null;
        isMoving = false;
    }

    private void UpdateRotation(float horizontalDelta, float lerpSpeed)
    {
        //rotate card visual with smooth angle based on delta
        float angle = horizontalDelta * rotationMultiplier;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        if (cardVisual != null && cardVisual.scaleTarget != null)
        {
            cardVisual.scaleTarget.localRotation = Quaternion.Slerp(cardVisual.scaleTarget.localRotation, targetRotation, Time.deltaTime * lerpSpeed);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //select or deselect card on click
        if (isDragging || isMoving) return;

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
        //begin dragging and detach from layout
        if (layoutManager != null)
            layoutManager.DeselectAllExcept(null);

        cardVisual?.KillAllTweens();

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        isMoving = false;

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
        //update target drag position and simulate layout
        Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : Camera.main;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            cam,
            out Vector3 worldMousePos))
        {
            Vector3 targetWorldPos = worldMousePos + dragOffset;
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
        //end drag and return to layout
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, true);

        isDragging = false;
        isMoving = false;

        layoutManager?.ReorderCard(this, transform.localPosition.x);
        cardVisual?.OnEndDragVisual();

        targetDragPosition = transform.localPosition;
        transform.localRotation = originalRotation;
        cardVisual.enabled = true;
    }

    public void SetLocalPositionInstant(Vector3 pos)
    {
        //teleport card instantly to position
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        isMoving = false;
        transform.localPosition = pos;
    }

    public void SetSortingOrder(int order)
    {
        //set sorting order manually
        cardCanvas.sortingOrder = order;
    }

    public void RestoreOriginalSortingOrder()
    {
        //reset to original sorting order
        cardCanvas.sortingOrder = originalSortingOrder;
    }

    public void SetOriginalSortingOrder(int order)
    {
        //update internal sorting order reference
        originalSortingOrder = order;
    }
}
