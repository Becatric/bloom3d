using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class ARFlowerManipulator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera arCamera;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.25f;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 0.0005f;

    [Header("Scaling")]
    [SerializeField] private float pinchSpeed = 0.005f;
    [SerializeField] private float minimumScale = 0.3f;
    [SerializeField] private float maximumScale = 3f;

    private bool isSelected;
    private Vector3 initialScale;

    private void Awake()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }

        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        var touches = Touch.activeTouches;

        if (touches.Count == 0)
        {
            return;
        }

        if (touches.Count == 1)
        {
            HandleSingleTouch(touches[0]);
        }
        else if (touches.Count >= 2)
        {
            HandleTwoTouches(touches[0], touches[1]);
        }
    }

    private void HandleSingleTouch(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            isSelected = WasThisObjectTouched(touch.screenPosition);
        }

        if (!isSelected)
        {
            return;
        }

        if (touch.phase == TouchPhase.Moved)
        {
            float rotationAmount =
                -touch.delta.x * rotationSpeed;

            transform.Rotate(
                Vector3.up,
                rotationAmount,
                Space.Self
            );
        }

        if (
            touch.phase == TouchPhase.Ended ||
            touch.phase == TouchPhase.Canceled
        )
        {
            isSelected = false;
        }
    }

    private void HandleTwoTouches(Touch firstTouch, Touch secondTouch)
    {
        if (!isSelected)
        {
            Vector2 midpoint =
                (firstTouch.screenPosition +
                 secondTouch.screenPosition) / 2f;

            isSelected = WasThisObjectTouched(midpoint);

            if (!isSelected)
            {
                return;
            }
        }

        HandlePinch(firstTouch, secondTouch);
        HandleMovement(firstTouch, secondTouch);

        bool firstEnded =
            firstTouch.phase == TouchPhase.Ended ||
            firstTouch.phase == TouchPhase.Canceled;

        bool secondEnded =
            secondTouch.phase == TouchPhase.Ended ||
            secondTouch.phase == TouchPhase.Canceled;

        if (firstEnded || secondEnded)
        {
            isSelected = false;
        }
    }

    private void HandlePinch(Touch firstTouch, Touch secondTouch)
    {
        Vector2 firstPreviousPosition =
            firstTouch.screenPosition - firstTouch.delta;

        Vector2 secondPreviousPosition =
            secondTouch.screenPosition - secondTouch.delta;

        float previousDistance = Vector2.Distance(
            firstPreviousPosition,
            secondPreviousPosition
        );

        float currentDistance = Vector2.Distance(
            firstTouch.screenPosition,
            secondTouch.screenPosition
        );

        float distanceDifference =
            currentDistance - previousDistance;

        float scaleMultiplier =
            1f + distanceDifference * pinchSpeed;

        Vector3 newScale =
            transform.localScale * scaleMultiplier;

        float relativeScale =
            newScale.x / initialScale.x;

        relativeScale = Mathf.Clamp(
            relativeScale,
            minimumScale,
            maximumScale
        );

        transform.localScale =
            initialScale * relativeScale;
    }

    private void HandleMovement(
        Touch firstTouch,
        Touch secondTouch
    )
    {
        Vector2 averageDelta =
            (firstTouch.delta + secondTouch.delta) / 2f;

        Vector3 movement = new Vector3(
            averageDelta.x,
            0f,
            averageDelta.y
        );

        transform.localPosition +=
            movement * movementSpeed;
    }

    private bool WasThisObjectTouched(Vector2 screenPosition)
    {
        if (arCamera == null)
        {
            return false;
        }

        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            return false;
        }

        return hit.transform == transform ||
               hit.transform.IsChildOf(transform);
    }
}