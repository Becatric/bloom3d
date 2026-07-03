using System.Collections.Generic;
using UnityEngine;

public class BouquetManager : MonoBehaviour
{
    [Tooltip("How many flowers the innermost ring holds. Each ring outward grows in size according to the ring growth rate.")]
    public int firstRingCount = 2;

    [Tooltip("The ratio of how many flowers every further outer ring can hold. 2 = number doubles every ring")]
    public int ringGrowthRate = 2;

    [Tooltip("Set the amount of flowers the bouquet should contain")]
    public int flowerCount = 8;

    [Tooltip("Set the density of the rows, by changing the maximum angle the bouquet extends to")]
    public int maxBouquetSpread = 48;

    [Tooltip("The flowers this bouquet can use (FlowerData from the FlowerDatabase). The pattern " +
             "references these BY INDEX, so swapping a slot here (or via SetFlower) re-skins the " +
             "bouquet without touching the pattern.")]
    public List<FlowerData> flowers = new List<FlowerData>();

    Flower[] flowerArrangement = new Flower[0];

    // --- Flower palette management (index-based) -------------------------------

    // Append a new flower; returns the index it landed at.
    public int AddFlower(FlowerData flower)
    {
        flowers.Add(flower);
        return flowers.Count - 1;
    }

    // Replace whatever flower currently sits at 'index'. This is the "swap a flower
    // in the bouquet" entry point: change the slot, re-run ArrangeFlowers with the
    // same pattern, and every position that used that index updates.
    public void SetFlower(int index, FlowerData flower)
    {
        if (index < 0) return;
        while (flowers.Count <= index) flowers.Add(null);
        flowers[index] = flower;
    }

    // --- Arrangement ----------------------------------------------------------

    public void SetFlowerCount(int count)
    {
        flowerCount = count;
    }

    // Convenience: "00100 11011" -> indices [0,0,1,0,0, 1,1,0,1,1]. Spaces (and any
    // non-digit) are ignored, so you can group for readability

    public void ArrangeFlowers(string indexPattern)
    {
        List<int> pattern = new List<int>();
        if (indexPattern != null)
        {
            foreach (char c in indexPattern)
            {
                if (char.IsDigit(c)) pattern.Add(c - '0');
            }
        }
        ArrangeFlowers(pattern);
    }

    // The pattern is a list of INDICES into 'flowers', not flower references.
    // It repeats (cycles) to fill flowerCount: pattern [0,0,1] over 7 flowers ->
    // 0,0,1,0,0,1,0. Indices outside the palette wrap around.
    public void ArrangeFlowers(List<int> arrangementPattern)
    {
        if (flowers.Count == 0)
        {
            Debug.LogWarning("BouquetManager: no flowers assigned.");
            return;
        }

        // No pattern given -> walk through every palette slot in order.
        if (arrangementPattern == null || arrangementPattern.Count == 0)
        {
            arrangementPattern = new List<int>();
            for (int i = 0; i < flowers.Count; i++) arrangementPattern.Add(i);
        }

        ClearArrangement();
        flowerArrangement = new Flower[Mathf.Max(0, flowerCount)];

        for (int i = 0; i < flowerArrangement.Length; i++)
        {
            int slot = arrangementPattern[i % arrangementPattern.Count];
            int flowerIndex = ((slot % flowers.Count) + flowers.Count) % flowers.Count;
            FlowerData data = flowers[flowerIndex];
            if (data != null)
            {
                flowerArrangement[i] = CreateFlower(data);
            }
        }

        PositionFlowers();
    }

    Flower CreateFlower(FlowerData data)
    {
        GameObject prefab = data.flowerPrefab;
        if (prefab == null) return null;

        GameObject instance = Instantiate(prefab, transform);
        instance.transform.localPosition = Vector3.zero; // every stem shares the binding point
        instance.transform.localScale = Vector3.one * data.arScale;

        Flower flower = instance.GetComponent<Flower>();
        if (flower == null) flower = instance.AddComponent<Flower>();
        flower.Data = data;
        return flower;
    }

    void ClearArrangement()
    {
        foreach (Flower f in flowerArrangement)
        {
            if (f != null) Destroy(f.gameObject);
        }
    }

    // --- Positioning ----------------------------------------------------------

    // Rotates every flower so the bouquet fans out from one origin. Ring sizes
    // double outward; outer rings tilt further, the last reaching maxBouquetSpread.
    void PositionFlowers()
    {
        int baseCount = Mathf.Max(1, firstRingCount);
        bool hasCenterFlower = flowerArrangement.Length % 2 == 1;
        int index = 0;

        // Odd count -> one flower stands straight up in the middle (angle 0).
        if (hasCenterFlower)
        {
            if (flowerArrangement[0] != null)
                flowerArrangement[0].transform.localRotation = Quaternion.identity;
            index = 1;
        }

        int flowersInRings = flowerArrangement.Length - index;
        if (flowersInRings <= 0) return;

        int ringCount = CountRings(flowersInRings, baseCount);

        int ringSize = baseCount;
        for (int ring = 0; ring < ringCount; ring++)
        {
            int countThisRing = Mathf.Min(ringSize, flowerArrangement.Length - index);

            // Outer rings tilt further; the last ring reaches maxBouquetSpread.
            float spread = maxBouquetSpread * (ring + 1) / (float)ringCount;

            // Stagger alternate rings so blooms nest between the ring beneath them.
            float ringOffset = (ring % 2 == 1) ? (180f / countThisRing) : 0f;

            for (int j = 0; j < countThisRing; j++, index++)
            {
                if (flowerArrangement[index] == null) continue;

                float azimuth = 360f * j / countThisRing + ringOffset;

                // Tilt away from the up-axis (spread), THEN spin around it (azimuth).
                Quaternion rotation =
                    Quaternion.AngleAxis(azimuth, Vector3.up) *
                    Quaternion.AngleAxis(spread, Vector3.forward);

                Quaternion correction = Quaternion.Euler(-90f, 0f, 0f);
                flowerArrangement[index].transform.localRotation = rotation * correction;
            }

            ringSize *= 2; // each new ring holds double the previous one
        }
    }

    // How many rings are needed to seat 'flowers' (e.g. base, 2*base, 4*base...).
    int CountRings(int flowers, int baseCount)
    {
        int rings = 0, ringSize = Mathf.Max(1, baseCount), placed = 0;
        while (placed < flowers)
        {
            placed += ringSize;
            ringSize *= ringGrowthRate;
            rings++;
        }
        return rings;
    }
}