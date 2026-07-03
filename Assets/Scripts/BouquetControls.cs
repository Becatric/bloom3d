using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BouquetControls : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject _controlPanel;

    [Header("Elements")]
    [SerializeField] private Slider _flowerCountSlider;
    [SerializeField] private BouquetManager _manager;
    [SerializeField] private TextMeshProUGUI _flowerCountSliderText;

    void Start()
    {
        _flowerCountSlider.onValueChanged.AddListener((v) =>
        {
            _manager.SetFlowerCount((int)v);
            _manager.ArrangeFlowers(new List<int>());
            _flowerCountSliderText.text = string.Concat("Flower Count: ", v.ToString());
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
