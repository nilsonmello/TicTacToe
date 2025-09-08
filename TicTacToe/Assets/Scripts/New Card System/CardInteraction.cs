using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

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
        rectTransform = GetComponent<RectTransform>(); //cache rect transform reference
        canvasGroup = GetComponent<CanvasGroup>(); //cache canvas group reference
        canvas = GetComponentInParent<Canvas>(); //find parent canvas
        layoutManager = GetComponentInParent<CardLayoutManager>(); //find parent layout manager

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>(); //add canvas group if missing

        cardCanvas = GetComponent<Canvas>(); //try to get canvas component
        if (cardCanvas == null)
        {
            cardCanvas = gameObject.AddComponent<Canvas>(); //add canvas if missing
            cardCanvas.overrideSorting = true; //enable override sorting
        }

        originalSortingOrder = cardCanvas.sortingOrder; //store original sorting order
        originalRotation = transform.localRotation; //store original local rotation
        lastPosition = transform.localPosition; //store last local position
    }

    private void Start()
    {
        if (cardVisualPrefab != null)
        {
            GameObject visualObj = Instantiate(cardVisualPrefab, transform.position, transform.rotation); //instantiate visual prefab
            cardVisualInstance = visualObj.GetComponent<CardVisual>();

            if (cardVisualInstance != null)
            {
                visualObj.transform.SetParent(canvas.transform, false); //set visual as child of canvas (not card)

                cardVisualInstance.SetFollowTarget(transform); //assign card transform for visual to follow
            }
            else
            {
                Debug.LogWarning("Cardvisual component missing on prefab."); //warn if prefab missing CardVisual script
            }
        }
        else
        {
            Debug.LogWarning("No cardvisual prefab assigned to cardinteraction."); //warn if prefab not assigned
        }
    }

    private void Update()
    {
        if (isMoving) return; //skip if currently moving by coroutine

        if (isDragging)
        {
            transform.localPosition = targetDragPosition; //directly set position while dragging
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (cardVisualInstance == null || !cardVisualInstance.gameObject.activeSelf) return; //skip if no visual or inactive

        RectTransform cardRect = transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cardRect, eventData.position, eventData.enterEventCamera, out Vector2 localPoint))
        {
            Vector2 normalizedPos = new Vector2(
                localPoint.x / (cardRect.rect.width / 2f),
                localPoint.y / (cardRect.rect.height / 2f)
            );

            normalizedPos.x = Mathf.Clamp(normalizedPos.x, -0.5f, 0.5f); //clamp normalized hover position x
            normalizedPos.y = Mathf.Clamp(normalizedPos.y, -0.5f, 0.5f); //clamp normalized hover position y

            cardVisualInstance.UpdateHoverMousePosition(normalizedPos); //update visual hover position
        }
    }

    public void MoveToLocalPosition(Vector3 targetPos)
    {
        if (isDragging || isMoving) return; //ignore if dragging or already moving

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine); //stop any existing move coroutine
            moveCoroutine = null;
        }

        isMoving = false;
        moveCoroutine = StartCoroutine(MoveRoutine(targetPos)); //start new smooth move coroutine
    }

    private IEnumerator MoveRoutine(Vector3 endPos)
    {
        isMoving = true; //flag moving true

        while (Vector3.Distance(transform.localPosition, endPos) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPos, moveSpeed * Time.deltaTime); //move smoothly towards target
            yield return null;
        }

        transform.localPosition = endPos; //ensure exact final position

        moveCoroutine = null; //clear coroutine reference
        isMoving = false; //flag moving false
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging || isMoving || !canInteract) return; //ignore clicks while dragging or moving or disabled

        if (layoutManager != null)
        {
            bool wasSelected = layoutManager.IsSelected(this);

            if (wasSelected)
            {
                layoutManager.DeselectCard(); //deselect card in layout
                if (cardVisualInstance != null)
                {
                    cardVisualInstance.DeselectVisual(); //play deselect animation
                    cardVisualInstance.SetSelectedState(false); //update visual selected state
                    cardVisualInstance.ReturnSortingLayer(); //restore sorting order
                }
            }
            else
            {
                layoutManager.SelectCard(this); //select this card
                if (cardVisualInstance != null)
                {
                    cardVisualInstance.SelectVisual(); //play select animation
                    cardVisualInstance.SetSelectedState(true); //update visual selected state
                    cardVisualInstance.SetSortingOrder(1000); //bring to front visually
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canInteract) return; //ignore if interaction disabled

        previousLayoutManager = layoutManager; //store current layout manager

        if (layoutManager != null)
            layoutManager.DeselectAllExcept(null); //deselect all cards

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine); //stop moving coroutine if running
            moveCoroutine = null;
        }

        isMoving = false;

        originalParent = transform.parent; //store original parent transform
        canvasGroup.blocksRaycasts = false; //disable raycast blocking while dragging

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldMousePos))
        {
            dragOffset = transform.position - worldMousePos; //calculate offset between card and pointer
        }
        else
        {
            dragOffset = Vector3.zero;
        }

        transform.SetParent(canvas.transform, true); //reparent card to canvas while dragging
        isDragging = true; //flag dragging true
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canInteract) return; //ignore if interaction disabled

        if (cardVisualInstance != null)
        {
            cardVisualInstance.IndragTweens(); //play drag animation on visual
        }

        Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : Camera.main;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            cam,
            out Vector3 worldMousePos))
        {
            Vector3 targetWorldPos = worldMousePos + dragOffset;
            targetDragPosition = canvas.transform.InverseTransformPoint(targetWorldPos); //convert to local canvas pos
        }
        else
        {
            Debug.LogWarning("Couldn't convert screen to world position."); //warn on conversion failure
        }

        layoutManager?.SimulateDrag(this, targetDragPosition.x); //notify layout manager of drag position
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false; //flag dragging false

        if (!canInteract) return; //ignore if interaction disabled

        if (cardVisualInstance != null)
        {
            cardVisualInstance.OffDragTweens(); //play drag end animation
        }

        canvasGroup.blocksRaycasts = true; //reenable raycast blocking

        CardLayoutManager targetLayout = null;

        //find target panel under mouse that accepts this card
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
            if (layoutManager != null)
                layoutManager.cards.Remove(this); //remove from old layout

            layoutManager = targetLayout; //set new layout manager
            layoutManager.cards.Add(this); //add to new layout
            layoutManager.LayoutCards(); //relayout cards

            layoutManager.panelData.OnCardDropped(this); //notify panel of drop
        }
        else
        {
            layoutManager = previousLayoutManager; //revert to previous layout

            if (!layoutManager.cards.Contains(this))
                layoutManager.cards.Add(this); //add back if missing

            layoutManager.LayoutCards();
        }

        transform.SetParent(layoutManager.transform, true); //reparent to layout
        MoveToLocalPosition(transform.localPosition); //move smoothly to local position
        layoutManager.ReorderCard(this); //reorder cards in layout
        UpdateVisualSortingOrder(layoutManager.GetCardOrder(this)); //update sorting order
        layoutManager.DeselectCard(); //deselect all cards
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (layoutManager.IsSelected(this)) return; //ignore hover if selected

        if (cardVisualInstance != null)
        {
            cardVisualInstance.SetHoverState(true); //set hover true
            cardVisualInstance.PlayPoniterEnter(); //play hover enter animation
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (cardVisualInstance != null)
        {
            cardVisualInstance.SetHoverState(false); //clear hover

            if (!layoutManager.IsSelected(this))
                cardVisualInstance.PlayPointerExit(); //play hover exit animation if not selected
        }
    }

    public void SetLocalPositionInstant(Vector3 pos)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine); //stop move coroutine if running
            moveCoroutine = null;
        }

        isMoving = false; //flag not moving
        transform.localPosition = pos; //set position instantly
    }

    public void UpdateVisualSortingOrder(int order)
    {
        if (cardVisualInstance != null)
            cardVisualInstance.SetOriginalSortingOrder(order); //update visual sorting order
    }

    public void Initialize(Card card)
    {
        StartCoroutine(DelayedSetCard(card)); //start coroutine to set card data
    }

    private IEnumerator DelayedSetCard(Card card)
    {
        yield return new WaitUntil(() => cardVisualInstance != null); //wait until visual is instantiated
        cardVisualInstance.SetCard(card); //set card data on visual
    }

}