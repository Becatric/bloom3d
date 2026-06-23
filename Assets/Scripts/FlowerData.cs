using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFlowerData",
    menuName = "Flower Shop/Flower Data"
)]
public class FlowerData : ScriptableObject
{
    [Header("Identification")]
    public string id;
    public string displayName;
    public string scientificName;
    public string markerImageName;

    [Header("Information")]
    [TextArea(3, 6)]
    public string description;

    public string origin;
    public string meaning;
    public string habitat;
    public string birthflower;

    [Header("Care")]
    public string floweringSeason;
    public string sunlight;
    public string watering;
    public string difficulty;

    [Header("AR Display")]
    [Min(0.0001f)]
    public float arScale = 0.05f;

    [Header("Assets")]
    public Sprite flowerImage;
    public GameObject flowerPrefab;
}