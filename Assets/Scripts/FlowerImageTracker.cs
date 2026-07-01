using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FlowerImageTracker : MonoBehaviour
{
    [Header("AR")]
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    [Header("Flower Database")]
    [SerializeField]
    private FlowerDatabase flowerDatabase;

    [Header("Flower Info UI")]
    [SerializeField]
    private FlowerInfoCarousel flowerInfoCarousel;

    private readonly Dictionary<TrackableId, GameObject> spawnedFlowers =
        new Dictionary<TrackableId, GameObject>();

    private void Awake()
    {
        if (AppModeManager.GetSelectedMode() != AppMode.Learning) { enabled = false; return; }

        if (trackedImageManager == null)
        {
            trackedImageManager =
                GetComponent<ARTrackedImageManager>();
        }
    }

    private void OnEnable()
    {
        if (!enabled) return;
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.AddListener(
                OnTrackedImagesChanged
            );
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(
                OnTrackedImagesChanged
            );
        }
        ClearAllFlowers();
    }

    private void OnTrackedImagesChanged(
        ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs
    )
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            SpawnFlower(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateFlower(trackedImage);
        }

        foreach (
            KeyValuePair<TrackableId, ARTrackedImage> removedImage
            in eventArgs.removed
        )
        {
            RemoveFlower(removedImage.Key);
        }
    }

    private void SpawnFlower(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.Tracking)
        {
            return;
        }

        if (spawnedFlowers.ContainsKey(trackedImage.trackableId))
        {
            return;
        }

        if (flowerDatabase == null)
        {
            Debug.LogError("Flower Database is not assigned.");
            return;
        }

        string markerName = trackedImage.referenceImage.name;

        FlowerData flowerData =
            flowerDatabase.GetFlowerByMarkerName(markerName);

        if (flowerData == null)
        {
            Debug.LogWarning(
                $"No FlowerData found for marker: {markerName}"
            );

            return;
        }

        if (flowerData.flowerPrefab == null)
        {
            Debug.LogWarning(
                $"No prefab assigned for {flowerData.displayName}"
            );

            return;
        }

        GameObject flower = Instantiate(
            flowerData.flowerPrefab,
            trackedImage.transform
        );

        flower.transform.localPosition = Vector3.zero;
        flower.transform.localRotation = Quaternion.identity;
        flower.transform.localScale =
            Vector3.one * flowerData.arScale;

        spawnedFlowers.Add(
            trackedImage.trackableId,
            flower
        );

        if (flowerInfoCarousel != null)
        {
            flowerInfoCarousel.ShowFlower(flowerData);
        }
    }

    private void UpdateFlower(ARTrackedImage trackedImage)
    {
        if (!spawnedFlowers.TryGetValue(
            trackedImage.trackableId,
            out GameObject flower
        ))
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                SpawnFlower(trackedImage);
            }

            return;
        }

        bool isTracking =
            trackedImage.trackingState == TrackingState.Tracking;

        flower.SetActive(isTracking);
    }

    private void RemoveFlower(TrackableId trackableId)
    {
        if (!spawnedFlowers.TryGetValue(
            trackableId,
            out GameObject flower
        ))
        {
            return;
        }

        Destroy(flower);
        spawnedFlowers.Remove(trackableId);
    }

    private void ClearAllFlowers()
    {
        foreach (GameObject flower in spawnedFlowers.Values)
        {
            if (flower != null)
            {
                Destroy(flower);
            }
        }

        spawnedFlowers.Clear();

        if (flowerInfoCarousel != null)
        {
            flowerInfoCarousel.HidePanel();
        }
    }
}