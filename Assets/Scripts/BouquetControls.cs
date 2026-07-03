using UnityEngine;
using UnityEngine.UI;

public class BouquetControls : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject _controlPanel;

    [Header("Elements")]
    [SerializeField] private Slider _flowerCountSlider;
    [SerializeField] private BouquetManager _manager;

    void Start()
    {
        if (AppModeManager.GetSelectedMode() != AppMode.Bouquet)
        {
            _controlPanel.SetActive(false);
        }

        _flowerCountSlider.onValueChanged.AddListener((v) =>
        {
            _manager.SetFlowerCount((int)v);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
