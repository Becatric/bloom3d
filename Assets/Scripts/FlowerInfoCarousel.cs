using System.Collections;
using TMPro;
using UnityEngine;

public class FlowerInfoCarousel : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField]
    private GameObject infoPanel;

    [SerializeField]
    private RectTransform panelRectTransform;

    [Header("Panel Movement")]
    [SerializeField]
    private float expandedY = 425f;

    [SerializeField]
    private float collapsedY = -300f;

    [SerializeField]
    private float animationDuration = 0.3f;

    [Header("Slides")]
    [SerializeField]
    private GameObject overviewSlide;

    [SerializeField]
    private GameObject careSlide;

    [SerializeField]
    private GameObject descriptionSlide;

    [Header("Overview Text")]
    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text scientificNameText;

    [SerializeField]
    private TMP_Text originText;

    [SerializeField]
    private TMP_Text meaningText;

    [Header("Care Text")]
    [SerializeField]
    private TMP_Text sunlightText;

    [SerializeField]
    private TMP_Text wateringText;

    [SerializeField]
    private TMP_Text difficultyText;

    [Header("Description Text")]
    [SerializeField]
    private TMP_Text descriptionText;

    [Header("Navigation")]
    [SerializeField]
    private TMP_Text pageIndicator;

    [Header("Panel Toggle")]
    [SerializeField]
    private TMP_Text toggleArrowText;

    private GameObject[] slides;
    private int currentSlideIndex;

    private bool isExpanded = true;
    private Coroutine movementCoroutine;

    private void Awake()
    {
        slides = new[]
        {
            overviewSlide,
            careSlide,
            descriptionSlide
        };

        HidePanel();
    }

    public void ShowFlower(FlowerData flowerData)
    {
        if (flowerData == null)
        {
            Debug.LogWarning(
                "FlowerInfoCarousel received null FlowerData."
            );

            return;
        }

        if (infoPanel == null)
        {
            Debug.LogError(
                "Info Panel is not assigned."
            );

            return;
        }

        SetTextValues(flowerData);

        infoPanel.SetActive(true);

        currentSlideIndex = 0;
        UpdateSlide();

        ExpandPanelImmediately();
    }

    private void SetTextValues(FlowerData flowerData)
    {
        if (titleText != null)
        {
            titleText.text = flowerData.displayName;
        }

        if (scientificNameText != null)
        {
            scientificNameText.text =
                $"Scientific name: {flowerData.scientificName}";
        }

        if (originText != null)
        {
            originText.text =
                $"Origin: {flowerData.origin}";
        }

        if (meaningText != null)
        {
            meaningText.text =
                $"Meaning: {flowerData.meaning}";
        }

        if (sunlightText != null)
        {
            sunlightText.text =
                $"Sunlight: {flowerData.sunlight}";
        }

        if (wateringText != null)
        {
            wateringText.text =
                $"Watering: {flowerData.watering}";
        }

        if (difficultyText != null)
        {
            difficultyText.text =
                $"Difficulty: {flowerData.difficulty}";
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                flowerData.description;
        }
    }

    public void NextSlide()
    {
        if (slides == null || slides.Length == 0)
        {
            return;
        }

        currentSlideIndex++;

        if (currentSlideIndex >= slides.Length)
        {
            currentSlideIndex = 0;
        }

        UpdateSlide();
    }

    public void PreviousSlide()
    {
        if (slides == null || slides.Length == 0)
        {
            return;
        }

        currentSlideIndex--;

        if (currentSlideIndex < 0)
        {
            currentSlideIndex = slides.Length - 1;
        }

        UpdateSlide();
    }

    public void TogglePanel()
    {
        if (infoPanel == null || !infoPanel.activeSelf)
        {
            return;
        }

        if (isExpanded)
        {
            CollapsePanel();
        }
        else
        {
            ExpandPanel();
        }
    }

    public void CollapsePanel()
    {
        if (!isExpanded)
        {
            return;
        }

        isExpanded = false;

        if (toggleArrowText != null)
        {
            toggleArrowText.text = "▲";
        }

        StartPanelMovement(collapsedY);
    }

    public void ExpandPanel()
    {
        if (isExpanded)
        {
            return;
        }

        isExpanded = true;

        if (toggleArrowText != null)
        {
            toggleArrowText.text = "▼";
        }

        StartPanelMovement(expandedY);
    }

    public void HidePanel()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    private void ExpandPanelImmediately()
    {
        if (panelRectTransform == null)
        {
            Debug.LogError(
                "Panel Rect Transform is not assigned."
            );

            return;
        }

        Vector2 position =
            panelRectTransform.anchoredPosition;

        position.y = expandedY;

        panelRectTransform.anchoredPosition = position;
        isExpanded = true;

        if (toggleArrowText != null)
        {
            toggleArrowText.text = "▼";
        }
    }

    private void StartPanelMovement(float targetY)
    {
        if (panelRectTransform == null)
        {
            Debug.LogError(
                "Panel Rect Transform is not assigned."
            );

            return;
        }

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine =
            StartCoroutine(MovePanel(targetY));
    }

    private IEnumerator MovePanel(float targetY)
    {
        Vector2 startPosition =
            panelRectTransform.anchoredPosition;

        Vector2 targetPosition = new Vector2(
            startPosition.x,
            targetY
        );

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / animationDuration
            );

            progress = Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

            panelRectTransform.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    progress
                );

            yield return null;
        }

        panelRectTransform.anchoredPosition =
            targetPosition;

        movementCoroutine = null;
    }

    private void UpdateSlide()
    {
        if (slides == null)
        {
            return;
        }

        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null)
            {
                slides[i].SetActive(
                    i == currentSlideIndex
                );
            }
        }

        if (pageIndicator != null)
        {
            pageIndicator.text =
                $"{currentSlideIndex + 1}/{slides.Length}";
        }
    }
}