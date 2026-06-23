using System;
using UnityEngine;

// One "kind" of flower (rose, tulip, ...). Create assets via
// Assets > Create > Bouquet > Flower Type. Each asset owns a SET of
// model prefabs, and picks one per instance.
[CreateAssetMenu(fileName = "FlowerType", menuName = "Bouquet/Flower Type")]
public class FlowerType : ScriptableObject, IComparable<FlowerType>
{
    [Tooltip("Name shown in menus / debugging.")]
    public string displayName = "New Flower";

    [Tooltip("Lower numbers sort first. SortedSet<FlowerType> uses this to order types.")]
    public int sortOrder = 0;

    [Tooltip("The set of 3D models (prefabs) for this flower. One is chosen per instance.")]
    public GameObject[] modelVariants;

    // Choose a random model from the set.
    public GameObject GetRandomModel()
    {
        if (modelVariants == null || modelVariants.Length == 0)
        {
            Debug.LogError($"FlowerType '{displayName}' has no model variants assigned.");
            return null;
        }
        // UnityEngine.Random.Range(int, int) is max-exclusive.
        return modelVariants[UnityEngine.Random.Range(0, modelVariants.Length)];
    }

    // Choose a specific model from the set (clamped to a valid index).
    public GameObject GetModel(int index)
    {
        if (modelVariants == null || modelVariants.Length == 0) return null;
        return modelVariants[Mathf.Clamp(index, 0, modelVariants.Length - 1)];
    }

    // SortedSet<FlowerType> needs this; without it, adding a second element throws.
    public int CompareTo(FlowerType other)
    {
        if (other == null) return 1;
        int bySort = sortOrder.CompareTo(other.sortOrder);
        if (bySort != 0) return bySort;
        return string.Compare(displayName, other.displayName, StringComparison.Ordinal);
    }
}