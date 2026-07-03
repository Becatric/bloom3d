using UnityEngine;

// Sits on each instantiated flower model so the manager can grab its transform.
// Add it to your flower prefabs, or BouquetManager adds it automatically at runtime.
public class Flower : MonoBehaviour
{
    public FlowerData Data { get; set; }
}