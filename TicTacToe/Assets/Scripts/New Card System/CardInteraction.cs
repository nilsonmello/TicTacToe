using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class CardInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("references")]
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    public CardLayoutManager layoutManager;

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
    private CardLayoutManager previousLayoutManager;
    public bool canInteract = true;

    [Header("Visual Prefab")]
    public GameObject cardVisualPrefab;
    public CardVisual cardVisualInstance;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        layoutManager = GetComponentInParent<CardLayoutManager>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

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

    private void Start()
    {
        if (cardVisualPrefab != null)
        {
            GameObject visualObj = Instantiate(cardVisualPrefab, transform.position, transform.rotation);
            cardVisualInstance = visualObj.GetComponent<CardVisual>();

            if (cardVisualInstance != null)
            {
                visualObj.transform.SetParent(canvas.transform, false);
                cardVisualInstance.SetFollowTarget(transform);

                cardVisualInstance.DisableRaycastOnVisual();
            }
            else
            {
                Debug.LogWarning("CardVisual component missing on prefab.");
            }
        }
        else
        {
            Debug.LogWarning("No cardVisual prefab assigned to CardInteraction.");
        }
    }

    private void Update()
    {
        if (isMoving) return;

        if (isDragging)
            transform.localPosition = targetDragPosition;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                layoutManager.DeselectAll();
            }
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!canInteract) return;
        if (cardVisualInstance == null || !cardVisualInstance.gameObject.activeSelf) return;

        RectTransform cardRect = transform as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cardRect, eventData.position, eventData.enterEventCamera, out Vector2 localPoint))
        {
            Vector2 normalizedPos = new Vector2(
                localPoint.x / (cardRect.rect.width / 2f),
                localPoint.y / (cardRect.rect.height / 2f)
            );

            normalizedPos.x = Mathf.Clamp(normalizedPos.x, -0.5f, 0.5f);
            normalizedPos.y = Mathf.Clamp(normalizedPos.y, -0.5f, 0.5f);

            cardVisualInstance.UpdateHoverMousePosition(normalizedPos);
        }
    }

    public void MoveToLocalPosition(Vector3 targetPos)
    {
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
        isMoving = true;

        while (Vector3.Distance(transform.localPosition, endPos) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.localPosition = endPos;
        moveCoroutine = null;
        isMoving = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging || isMoving || !canInteract) return;
        if (layoutManager != null)
            layoutManager.SelectCard(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canInteract) return;

        previousLayoutManager = layoutManager;
        layoutManager?.DeselectAllExcept(null);

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        isMoving = false;
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

        cardVisualInstance?.SetSortingOrder(10000);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canInteract) return;

        cardVisualInstance?.IndragTweens();

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

        layoutManager?.SimulateDrag(this, targetDragPosition.x);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (!canInteract) return;

        cardVisualInstance?.OffDragTweens();

        canvasGroup.blocksRaycasts = true;

        CardLayoutManager targetLayout = null;

        foreach (var layout in CardLayoutManager.AllPanels)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(layout.GetComponent<RectTransform>(), Input.mousePosition, canvas.worldCamera))
            {
                var panel = layout.panelData;
                if (panel != null && panel.AcceptsCard(this))
                {
                    targetLayout = layout;
                    break;
                }
            }
        }

        if (targetLayout != null)
        {
            layoutManager?.cards.Remove(this);

            layoutManager = targetLayout;
            layoutManager.cards.Add(this);
            layoutManager.LayoutCards();

            layoutManager.panelData.OnCardDropped(this);
        }
        else
        {
            layoutManager = previousLayoutManager;

            if (!layoutManager.cards.Contains(this))
                layoutManager.cards.Add(this);

            layoutManager.LayoutCards();
        }

        transform.SetParent(layoutManager.transform, true);
        MoveToLocalPosition(transform.localPosition);
        layoutManager.ReorderCard(this);

        int orderIndex = layoutManager.GetCardOrder(this);
        UpdateVisualSortingOrder(orderIndex * 10);

        layoutManager.DeselectAllExcept(null);
        previousLayoutManager?.LayoutCards();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (layoutManager.IsSelected(this)) return;
        if (!canInteract) return;
        cardVisualInstance?.SetHoverState(true);
        cardVisualInstance?.PlayPoniterEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!canInteract) return;

        cardVisualInstance?.SetHoverState(false);
        if (!layoutManager.IsSelected(this))
            cardVisualInstance?.PlayPointerExit();
    }

    public void SetLocalPositionInstant(Vector3 pos)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        isMoving = false;
        transform.localPosition = pos;
    }

    public void UpdateVisualSortingOrder(int order)
    {
        cardVisualInstance?.SetOriginalSortingOrder(order + 5);
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = order;
            cardCanvas.overrideSorting = true;
        }
    }

    public void Initialize(Card card)
    {
        StartCoroutine(DelayedSetCard(card));
    }

    private IEnumerator DelayedSetCard(Card card)
    {
        yield return new WaitUntil(() => cardVisualInstance != null);
        cardVisualInstance.SetCard(card);
    }

    public static CardInteraction GetTopCardUnderPointer()
    {
        if (EventSystem.current == null) return null;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            var ci = r.gameObject.GetComponentInParent<CardInteraction>();
            if (ci != null)
                return ci;
        }

        return null;
    }
}
