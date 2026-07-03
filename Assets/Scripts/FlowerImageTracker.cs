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

    [Header("Tracking Behavior")]
    [SerializeField]
    private bool hideContentWhenMarkerIsLost = true;

    private readonly Dictionary<TrackableId, GameObject> spawnedFlowers =
        new Dictionary<TrackableId, GameObject>();

    private TrackableId? currentlyVisibleTrackableId;

    private void Awake()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager =
                GetComponent<ARTrackedImageManager>();
        }
    }

    private void Start()
    {
        ClearAllFlowers();
    }

    private void OnEnable()
    {
        ClearAllFlowers();

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

    private void OnDestroy()
    {
        ClearAllFlowers();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            ClearAllFlowers();
        }
    }

    private void OnTrackedImagesChanged(
        ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs
    )
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (
            KeyValuePair<TrackableId, ARTrackedImage> removedImage
            in eventArgs.removed
        )
        {
            RemoveFlower(removedImage.Key);
        }
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        bool markerIsVisible =
            trackedImage.trackingState == TrackingState.Tracking;

        if (!markerIsVisible)
        {
            if (hideContentWhenMarkerIsLost)
            {
                HideFlower(trackedImage.trackableId);
            }

            return;
        }

        ShowFlower(trackedImage);
    }

    private void ShowFlower(ARTrackedImage trackedImage)
    {
        HideOtherFlowers(trackedImage.trackableId);

        if (!spawnedFlowers.TryGetValue(
            trackedImage.trackableId,
            out GameObject flower
        ))
        {
            flower = SpawnFlower(trackedImage);
        }

        if (flower == null)
        {
            return;
        }

        flower.SetActive(true);
        currentlyVisibleTrackableId = trackedImage.trackableId;
    }

    private GameObject SpawnFlower(ARTrackedImage trackedImage)
    {
        if (flowerDatabase == null)
        {
            Debug.LogError(
                "Flower Database is not assigned in FlowerImageTracker."
            );

            return null;
        }

        string markerName =
            trackedImage.referenceImage.name;

        FlowerData flowerData =
            flowerDatabase.GetFlowerByMarkerName(markerName);

        if (flowerData == null)
        {
            Debug.LogWarning(
                $"No FlowerData found for marker: {markerName}"
            );

            return null;
        }

        if (flowerData.flowerPrefab == null)
        {
            Debug.LogWarning(
                $"No flower prefab assigned for: {flowerData.displayName}"
            );

            return null;
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

        Debug.Log(
            $"Spawned {flowerData.displayName} for marker {markerName}"
        );

        return flower;
    }

    private void HideFlower(TrackableId trackableId)
    {
        if (spawnedFlowers.TryGetValue(
            trackableId,
            out GameObject flower
        ))
        {
            flower.SetActive(false);
        }

        if (
            currentlyVisibleTrackableId.HasValue &&
            currentlyVisibleTrackableId.Value == trackableId
        )
        {
            currentlyVisibleTrackableId = null;

            if (flowerInfoCarousel != null)
            {
                flowerInfoCarousel.HidePanel();
            }
        }
    }

    private void HideOtherFlowers(TrackableId visibleTrackableId)
    {
        foreach (
            KeyValuePair<TrackableId, GameObject> flowerEntry
            in spawnedFlowers
        )
        {
            if (
                flowerEntry.Key != visibleTrackableId &&
                flowerEntry.Value != null
            )
            {
                flowerEntry.Value.SetActive(false);
            }
        }
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

        if (flower != null)
        {
            Destroy(flower);
        }

        spawnedFlowers.Remove(trackableId);

        if (
            currentlyVisibleTrackableId.HasValue &&
            currentlyVisibleTrackableId.Value == trackableId
        )
        {
            currentlyVisibleTrackableId = null;

            if (flowerInfoCarousel != null)
            {
                flowerInfoCarousel.HidePanel();
            }
        }
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
        currentlyVisibleTrackableId = null;

        if (flowerInfoCarousel != null)
        {
            flowerInfoCarousel.HidePanel();
        }
    }
}