using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetCircle : MonoBehaviour
{

    [Header("Target Settings")]
    public Transform target;

    [Header("UI Settings")]
    public RectTransform uiElement;

    [Header("Options")]
    public bool hideWhenBehindCamera = true;

    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();

        if (uiElement == null)
        {
            uiElement = GetComponent<RectTransform>();
        }

        if (target == null)
        {
            Debug.LogWarning("UIFollowGameObject: No target assigned!");
        }

        if (mainCamera == null)
        {
            Debug.LogError("UIFollowGameObject: No main camera found!");
        }
    }

    void LateUpdate()
    {
        if (target == null || mainCamera == null || uiElement == null)
            return;

        // Get the target's world position
        Vector3 worldPosition = target.position;

        // Convert to viewport to check if behind camera
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPosition);

        if (hideWhenBehindCamera && viewportPoint.z < 0)
        {
            uiElement.gameObject.SetActive(false);
            return;
        }



        // Convert world position to screen position
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPosition);

        // Get canvas rect
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Convert screen point to canvas position
        Vector2 canvasPosition = new Vector2(
            (screenPoint.x / Screen.width) * canvasSize.x,
            (screenPoint.y / Screen.height) * canvasSize.y
        );

        // Adjust for canvas anchor (center)
        canvasPosition -= canvasSize / 2f;

        // Set position
        uiElement.anchoredPosition = canvasPosition;
    }

}
