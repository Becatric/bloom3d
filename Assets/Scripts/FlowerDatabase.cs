using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "FlowerDatabase",
    menuName = "Flower Shop/Flower Database"
)]
public class FlowerDatabase : ScriptableObject
{
    [SerializeField]
    private List<FlowerData> flowers =
        new List<FlowerData>();

    public FlowerData GetFlowerByMarkerName(
        string markerName
    )
    {
        return flowers.Find(
            flower =>
                flower != null &&
                flower.markerImageName == markerName
        );
    }

    public FlowerData GetFlowerById(
        string flowerId
    )
    {
        return flowers.Find(
            flower =>
                flower != null &&
                flower.id == flowerId
        );
    }
}