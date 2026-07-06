using System.Collections.Generic;
using UnityEngine;

public class BouquetManager : MonoBehaviour
{
    [Tooltip("How many flowers the innermost ring holds. Each ring outward grows in size according to the ring growth rate.")]
    public int firstRingCount = 6;

    [Tooltip("The ratio of how many flowers every further outer ring can hold. 2 = number doubles every ring.")]
    public int ringGrowthRate = 2;

    [Tooltip("Set the amount of flowers the bouquet should contain.")]
    public int flowerCount = 8;

    [Tooltip("The maximum angle in degrees between the bouquet vertical axis and the outermost flowers.")]
    public int maxBouquetSpread = 20;

    [Tooltip("The flowers this bouquet can use (FlowerData from the FlowerDatabase). The pattern references these by index.")]
    public List<FlowerData> flowers = new List<FlowerData>();

    private Flower[] flowerArrangement = new Flower[0];

    public int AddFlower(FlowerData flower)
    {
        flowers.Add(flower);
        return flowers.Count - 1;
    }

    public void SetFlower(int index, FlowerData flower)
    {
        if (index < 0) return;

        while (flowers.Count <= index)
            flowers.Add(null);

        flowers[index] = flower;
    }

    public void SetFlowerCount(int count)
    {
        flowerCount = Mathf.Max(0, count);
    }

    public void SetMaxBouquetSpread(int value)
    {
        maxBouquetSpread = Mathf.Max(0, value);
    }

    public void SetFirstRingCount(int value)
    {
        firstRingCount = Mathf.Max(1, value);
    }

    public void SetRingGrowthRate(int value)
    {
        ringGrowthRate = Mathf.Max(1, value);
    }

    public void ArrangeFlowers(string indexPattern)
    {
        List<int> pattern = new List<int>();

        if (indexPattern != null)
        {
            foreach (char c in indexPattern)
            {
                if (char.IsDigit(c))
                    pattern.Add(c - '0');
            }
        }

        ArrangeFlowers(pattern);
    }

    public void ArrangeFlowers(List<int> arrangementPattern)
    {
        if (flowers.Count == 0)
        {
            Debug.LogWarning("BouquetManager: no flowers assigned.");
            return;
        }

        if (arrangementPattern == null || arrangementPattern.Count == 0)
        {
            arrangementPattern = new List<int>();

            for (int i = 0; i < flowers.Count; i++)
                arrangementPattern.Add(i);
        }

        ClearArrangement();
        flowerArrangement = new Flower[Mathf.Max(0, flowerCount)];

        for (int i = 0; i < flowerArrangement.Length; i++)
        {
            int slot = arrangementPattern[i % arrangementPattern.Count];
            int flowerIndex = ((slot % flowers.Count) + flowers.Count) % flowers.Count;
            FlowerData data = flowers[flowerIndex];

            if (data != null)
                flowerArrangement[i] = CreateFlower(data);
        }

        PositionFlowers();
    }

    private Flower CreateFlower(FlowerData data)
    {
        GameObject prefab = data.flowerPrefab;
        if (prefab == null) return null;

        GameObject instance = Instantiate(prefab, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localScale = Vector3.one * data.arScale;

        Flower flower = instance.GetComponent<Flower>();
        if (flower == null)
            flower = instance.AddComponent<Flower>();

        flower.Data = data;
        return flower;
    }

    private void ClearArrangement()
    {
        foreach (Flower flower in flowerArrangement)
        {
            if (flower != null)
                Destroy(flower.gameObject);
        }
    }

    private void PositionFlowers()
    {
        if (flowerArrangement == null || flowerArrangement.Length == 0)
            return;

        int count = flowerArrangement.Length;
        int index = 0;

        Vector3 prefabStemDirection = Vector3.up;

        if (count % 2 == 1)
        {
            if (flowerArrangement[0] != null)
            {
                flowerArrangement[0].transform.localPosition = Vector3.zero;
                flowerArrangement[0].transform.localRotation =
                    Quaternion.FromToRotation(prefabStemDirection, Vector3.up);
            }

            index = 1;
        }

        int ringCapacity = Mathf.Max(1, firstRingCount);
        int safeGrowthRate = Mathf.Max(1, ringGrowthRate);
        int estimatedRingCount = CountRings(count - index, ringCapacity, safeGrowthRate);

        int currentRing = 0;

        while (index < count)
        {
            int flowersThisRing = Mathf.Min(ringCapacity, count - index);
            float spread = maxBouquetSpread * (currentRing + 1) / (float)estimatedRingCount;
            float ringOffset = currentRing % 2 == 0 ? 0f : 180f / flowersThisRing;

            for (int j = 0; j < flowersThisRing; j++, index++)
            {
                if (flowerArrangement[index] == null)
                    continue;

                float azimuth = 360f * j / flowersThisRing + ringOffset;
                Quaternion aroundCenter = Quaternion.AngleAxis(azimuth, Vector3.up);

                Vector3 tiltedDirection =
                    aroundCenter *
                    Quaternion.AngleAxis(spread, Vector3.forward) *
                    Vector3.up;

                Quaternion rotation = Quaternion.FromToRotation(prefabStemDirection, tiltedDirection);

                flowerArrangement[index].transform.localPosition = Vector3.zero;
                flowerArrangement[index].transform.localRotation = rotation;
            }

            ringCapacity *= safeGrowthRate;
            currentRing++;
        }
    }

    private int CountRings(int flowersToPlace, int firstCapacity, int growthRate)
    {
        int rings = 0;
        int placed = 0;
        int capacity = Mathf.Max(1, firstCapacity);
        int safeGrowthRate = Mathf.Max(1, growthRate);

        while (placed < flowersToPlace)
        {
            placed += capacity;
            capacity *= safeGrowthRate;
            rings++;
        }

        return Mathf.Max(1, rings);
    }
}
