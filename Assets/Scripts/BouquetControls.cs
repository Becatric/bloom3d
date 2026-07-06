using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BouquetControls : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject _controlPanel;

    [Header("Manager")]
    [SerializeField] private BouquetManager _manager;
    
    [SerializeField] private BouquetModeController _modeController;

    [Header("Flower Count")]
    [SerializeField] private Slider _flowerCountSlider;
    [SerializeField] private TextMeshProUGUI _flowerCountSliderText;

    [Header("Bouquet Spread")]
    [SerializeField] private Slider _maxBouquetSpreadSlider;
    [SerializeField] private TextMeshProUGUI _maxBouquetSpreadSliderText;

    [Header("First Ring Count")]
    [SerializeField] private Slider _firstRingCountSlider;
    [SerializeField] private TextMeshProUGUI _firstRingCountSliderText;

    [Header("Ring Growth Rate")]
    [SerializeField] private Slider _ringGrowthRateSlider;
    [SerializeField] private TextMeshProUGUI _ringGrowthRateSliderText;
    
    [Header("Bouquet Height Offset")]
    [SerializeField] private Slider _bouquetHeightOffsetSlider;

    [SerializeField]
    private TextMeshProUGUI _bouquetHeightOffsetSliderText;
    
    [Header("Panel Dropdown")]
    [SerializeField] private float collapsedOffsetPercent = -0.25f;
    [SerializeField] private float animationDuration = 0.3f;

    private bool isExpanded = true;
    private Coroutine panelCoroutine;
    private Vector2 expandedPosition;
    private bool hasSavedExpandedPosition;

    private void Start()
    {
        if (_manager == null)
        {
            Debug.LogWarning("BouquetControls: BouquetManager is not assigned.");
            return;
        }

        SetupSlidersFromManager();
        RegisterSliderListeners();
        RefreshAllLabels();
        RefreshBouquetHeightOffsetLabel();
        SaveExpandedPosition();
    }

    private void SetupSlidersFromManager()
    {
        if (_flowerCountSlider != null)
        {
            _flowerCountSlider.wholeNumbers = true;
            _flowerCountSlider.SetValueWithoutNotify(_manager.flowerCount);
        }

        if (_maxBouquetSpreadSlider != null)
        {
            _maxBouquetSpreadSlider.wholeNumbers = true;
            _maxBouquetSpreadSlider.SetValueWithoutNotify(_manager.maxBouquetSpread);
        }

        if (_firstRingCountSlider != null)
        {
            _firstRingCountSlider.wholeNumbers = true;
            _firstRingCountSlider.SetValueWithoutNotify(_manager.firstRingCount);
        }

        if (_ringGrowthRateSlider != null)
        {
            _ringGrowthRateSlider.wholeNumbers = true;
            _ringGrowthRateSlider.SetValueWithoutNotify(_manager.ringGrowthRate);
        }
        if (
            _bouquetHeightOffsetSlider != null &&
            _modeController != null
        )
        {
            _bouquetHeightOffsetSlider.wholeNumbers = false;

            _bouquetHeightOffsetSlider.SetValueWithoutNotify(
                _modeController.BouquetHeightOffset
            );
        }
        
    }

    private void RegisterSliderListeners()
    {
        if (_flowerCountSlider != null)
            _flowerCountSlider.onValueChanged.AddListener(OnFlowerCountChanged);

        if (_maxBouquetSpreadSlider != null)
            _maxBouquetSpreadSlider.onValueChanged.AddListener(OnMaxBouquetSpreadChanged);

        if (_firstRingCountSlider != null)
            _firstRingCountSlider.onValueChanged.AddListener(OnFirstRingCountChanged);

        if (_ringGrowthRateSlider != null)
            _ringGrowthRateSlider.onValueChanged.AddListener(OnRingGrowthRateChanged);
        
        if (_bouquetHeightOffsetSlider != null)
        {
            _bouquetHeightOffsetSlider.onValueChanged.AddListener(
                OnBouquetHeightOffsetChanged
            );
        }
    }

    private void OnFlowerCountChanged(float value)
    {
        int flowerCount = Mathf.RoundToInt(value);
        _manager.SetFlowerCount(flowerCount);
        RebuildBouquet();
        RefreshFlowerCountLabel();
    }

    private void OnMaxBouquetSpreadChanged(float value)
    {
        int maxBouquetSpread = Mathf.RoundToInt(value);
        _manager.SetMaxBouquetSpread(maxBouquetSpread);
        RebuildBouquet();
        RefreshMaxBouquetSpreadLabel();
    }

    private void OnFirstRingCountChanged(float value)
    {
        int firstRingCount = Mathf.RoundToInt(value);
        _manager.SetFirstRingCount(firstRingCount);
        RebuildBouquet();
        RefreshFirstRingCountLabel();
    }

    private void OnRingGrowthRateChanged(float value)
    {
        int ringGrowthRate = Mathf.RoundToInt(value);
        _manager.SetRingGrowthRate(ringGrowthRate);
        RebuildBouquet();
        RefreshRingGrowthRateLabel();
    }

    private void RebuildBouquet()
    {
        _manager.ArrangeFlowers(new List<int>());
    }

    private void RefreshAllLabels()
    {
        RefreshFlowerCountLabel();
        RefreshMaxBouquetSpreadLabel();
        RefreshFirstRingCountLabel();
        RefreshRingGrowthRateLabel();
    }

    private void RefreshFlowerCountLabel()
    {
        if (_flowerCountSliderText != null)
            _flowerCountSliderText.text = $"Flower Count: {_manager.flowerCount}";
    }

    private void RefreshMaxBouquetSpreadLabel()
    {
        if (_maxBouquetSpreadSliderText != null)
            _maxBouquetSpreadSliderText.text = $"Max Bouquet Spread: {_manager.maxBouquetSpread}°";
    }

    private void RefreshFirstRingCountLabel()
    {
        if (_firstRingCountSliderText != null)
            _firstRingCountSliderText.text = $"First Ring Count: {_manager.firstRingCount}";
    }

    private void RefreshRingGrowthRateLabel()
    {
        if (_ringGrowthRateSliderText != null)
            _ringGrowthRateSliderText.text = $"Ring Growth Rate: {_manager.ringGrowthRate}";
    }
    
    private void OnBouquetHeightOffsetChanged(float value)
    {
        if (_modeController == null)
        {
            return;
        }

        // Only moves the flowers. It does not rebuild them
        // and does not move the wrapping objects.
        _modeController.SetBouquetHeightOffset(value);

        RefreshBouquetHeightOffsetLabel();

    }
    
    private void RefreshBouquetHeightOffsetLabel()
    {
        if (
            _bouquetHeightOffsetSliderText == null ||
            _modeController == null
        )
        {
            return;
        }

        float heightInCentimeters =
            _modeController.BouquetHeightOffset * 100f;

        _bouquetHeightOffsetSliderText.text =
            $"Bouquet Height: {heightInCentimeters:0} cm";
    }

    private float GetPanelYFromPercent(RectTransform panelRect, float yPercent)
    {
        RectTransform parentRect = panelRect.parent as RectTransform;

        if (parentRect == null)
            return yPercent;

        return parentRect.rect.height * yPercent;
    }

    private IEnumerator MovePanel(RectTransform panelRect, float targetY)
    {
        Vector2 startPosition = panelRect.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, targetY);

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            panelRect.anchoredPosition =
                Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        panelRect.anchoredPosition = targetPosition;
    }
    
    private void SaveExpandedPosition()
    {
        if (_controlPanel == null)
            return;

        RectTransform panelRect = _controlPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        expandedPosition = panelRect.anchoredPosition;
        hasSavedExpandedPosition = true;
    }

    public void TogglePanel()
    {
        if (_controlPanel == null)
            return;

        RectTransform panelRect = _controlPanel.GetComponent<RectTransform>();

        if (panelRect == null || !hasSavedExpandedPosition)
            return;

        float parentHeight = GetParentHeight(panelRect);
        float collapsedOffset = parentHeight * collapsedOffsetPercent;

        Vector2 targetPosition = isExpanded
            ? expandedPosition + new Vector2(0f, collapsedOffset)
            : expandedPosition;

        isExpanded = !isExpanded;

        if (panelCoroutine != null)
            StopCoroutine(panelCoroutine);

        panelCoroutine = StartCoroutine(MovePanel(panelRect, targetPosition));
    }

    private float GetParentHeight(RectTransform panelRect)
    {
        RectTransform parentRect = panelRect.parent as RectTransform;

        if (parentRect == null)
            return Screen.height;

        return parentRect.rect.height;
    }

    private IEnumerator MovePanel(RectTransform panelRect, Vector2 targetPosition)
    {
        Vector2 startPosition = panelRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            panelRect.anchoredPosition =
                Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        panelRect.anchoredPosition = targetPosition;
        panelCoroutine = null;
    }
}
