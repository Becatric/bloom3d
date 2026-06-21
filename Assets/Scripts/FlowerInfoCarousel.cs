using TMPro;
using UnityEngine;

public class FlowerInfoCarousel : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject infoPanel;

    [Header("Slides")]
    [SerializeField] private GameObject overviewSlide;
    [SerializeField] private GameObject careSlide;
    [SerializeField] private GameObject descriptionSlide;

    [Header("Overview Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text scientificNameText;
    [SerializeField] private TMP_Text originText;
    [SerializeField] private TMP_Text meaningText;

    [Header("Care Text")]
    [SerializeField] private TMP_Text sunlightText;
    [SerializeField] private TMP_Text wateringText;
    [SerializeField] private TMP_Text difficultyText;

    [Header("Description Text")]
    [SerializeField] private TMP_Text descriptionText;

    [Header("Navigation")]
    [SerializeField] private TMP_Text pageIndicator;

    private GameObject[] slides;
    private int currentSlideIndex;

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
            return;
        }

        titleText.text = flowerData.displayName;
        scientificNameText.text =
            $"Scientific name: {flowerData.scientificName}";
        originText.text =
            $"Origin: {flowerData.origin}";
        meaningText.text =
            $"Meaning: {flowerData.meaning}";

        sunlightText.text =
            $"Sunlight: {flowerData.sunlight}";
        wateringText.text =
            $"Watering: {flowerData.watering}";
        difficultyText.text =
            $"Difficulty: {flowerData.difficulty}";

        descriptionText.text = flowerData.description;

        infoPanel.SetActive(true);

        currentSlideIndex = 0;
        UpdateSlide();
    }

    public void NextSlide()
    {
        currentSlideIndex++;

        if (currentSlideIndex >= slides.Length)
        {
            currentSlideIndex = 0;
        }

        UpdateSlide();
    }

    public void PreviousSlide()
    {
        currentSlideIndex--;

        if (currentSlideIndex < 0)
        {
            currentSlideIndex = slides.Length - 1;
        }

        UpdateSlide();
    }

    public void HidePanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    private void UpdateSlide()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            slides[i].SetActive(i == currentSlideIndex);
        }

        if (pageIndicator != null)
        {
            pageIndicator.text =
                $"{currentSlideIndex + 1} / {slides.Length}";
        }
    }
}