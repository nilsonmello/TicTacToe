using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    private Canvas canvas;
    private CanvasGroup canvasGroup;           //controls raycast blocking during drag
    private RectTransform rectTransform;       //cached RectTransform
    private Transform originalParent;          //where the card returns after dragging

    public CardVisual cardVisual;              //visual component of the card
    private CardLayoutManager layoutManager;   //reference to layout manager controlling positioning

    [Header("Smooth Movement")]
    private Coroutine moveCoroutine;           //coroutine for smooth movement
    public float moveSpeed = 1000f;            //speed of card movement when repositioning

    [Header("Drag Control")]
    private Vector3 targetDragPosition;        // target position during drag
    private Vector3 velocity = Vector3.zero;   //velocity reference for SmoothDamp
    public float followSmoothTime = 0.05f;     //smoothDamp time for following the mouse
    private bool isDragging = false;           //whether the card is being dragged

    [Header("Sorting Order")]
    private Canvas cardCanvas;                 //canvas used to control sorting order
    private int originalSortingOrder;          //default sorting order to return to

    private void Awake()
    {
        //cache references
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        layoutManager = GetComponentInParent<CardLayoutManager>();

        //add CanvasGroup if missing
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        //ensure card has its own Canvas for sorting control
        cardCanvas = GetComponent<Canvas>();
        if (cardCanvas == null)
        {
            cardCanvas = gameObject.AddComponent<Canvas>();
            cardCanvas.overrideSorting = true;
        }

        //save the initial sorting order
        originalSortingOrder = cardCanvas.sortingOrder;
    }

    private void Update()
    {
        //smoothly follow target position during drag
        if (isDragging)
        {
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                targetDragPosition,
                ref velocity,
                followSmoothTime
            );
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //select or deselect the card on click
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
        //begin dragging: move to root canvas, disable raycasts
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform, true);
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //update target drag position and simulate layout reordering
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos
        );

        targetDragPosition = pos;
        layoutManager.SimulateDrag(this, targetDragPosition.x);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //return card to layout, re-enable raycasts
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, true);
        isDragging = false;
        layoutManager.ReorderCard(this, transform.localPosition.x);
    }

    public void MoveToLocalPosition(Vector3 targetPos)
    {
        //move smoothly to a local position
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(targetPos));
    }

    public void SetLocalPositionInstant(Vector3 pos)
    {
        //instantly move to position, cancelling movement coroutine
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        transform.localPosition = pos;
    }

    private IEnumerator MoveRoutine(Vector3 endPos)
    {
        //smoothly move the card to the target position
        while (Vector3.Distance(transform.localPosition, endPos) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = endPos;
        moveCoroutine = null;
    }

    public void SetSortingOrder(int order)
    {
        //set the current sorting order of the card
        cardCanvas.sortingOrder = order;
    }

    public void RestoreOriginalSortingOrder()
    {
        //restore the original sorting order saved at startup
        cardCanvas.sortingOrder = originalSortingOrder;
    }

    public void SetOriginalSortingOrder(int order)
    {
        //override the original sorting order manually
        originalSortingOrder = order;
    }
}
