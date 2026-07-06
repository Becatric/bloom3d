using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FlowerImageTracker : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Flower Database")]
    [SerializeField] private FlowerDatabase flowerDatabase;

    [Header("Flower Info UI")]
    [SerializeField] private FlowerInfoCarousel flowerInfoCarousel;

    [Header("Tracking Behavior")]
    [SerializeField] private bool hideContentWhenMarkerIsLost = true;

    [Header("Debug")]
    [SerializeField] private ARDebugDisplay debugDisplay;

    private readonly Dictionary<TrackableId, GameObject> spawnedFlowers =
        new Dictionary<TrackableId, GameObject>();

    private TrackableId? currentlyVisibleTrackableId;

    private void Awake()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager = GetComponent<ARTrackedImageManager>();
        }
    }

    private void Start()
    {
        ClearAllFlowers();
    }

    private void OnEnable()
    {
        ClearAllFlowers();

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
        bool shouldRefreshFlowerUI =
            !currentlyVisibleTrackableId.HasValue ||
            currentlyVisibleTrackableId.Value != trackedImage.trackableId;

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

        FlowerData flowerData = GetFlowerData(trackedImage);

        if (flowerData != null)
        {
            // Do not reset the carousel while the same marker
            // continues receiving tracking updates.
            if (shouldRefreshFlowerUI)
            {
                UpdateFlowerUI(flowerData);
            }

            UpdateDebugUI(flower, flowerData);
        }
    }

    private GameObject SpawnFlower(ARTrackedImage trackedImage)
    {
        FlowerData flowerData = GetFlowerData(trackedImage);

        if (flowerData == null)
        {
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

        ARFlowerManipulator manipulator =
            flower.GetComponent<ARFlowerManipulator>();

        if (manipulator != null)
        {
            manipulator.InitializeDebug(
                debugDisplay,
                flowerData.displayName,
                flowerData.arScale
            );
        }

        Debug.Log(
            $"Spawned {flowerData.displayName} for marker {trackedImage.referenceImage.name}"
        );

        return flower;
    }

    private FlowerData GetFlowerData(ARTrackedImage trackedImage)
    {
        if (flowerDatabase == null)
        {
            Debug.LogError(
                "Flower Database is not assigned in FlowerImageTracker."
            );

            return null;
        }

        string markerName = trackedImage.referenceImage.name;

        FlowerData flowerData =
            flowerDatabase.GetFlowerByMarkerName(markerName);

        if (flowerData == null)
        {
            Debug.LogWarning(
                $"No FlowerData found for marker: {markerName}"
            );
        }

        return flowerData;
    }

    private void UpdateFlowerUI(FlowerData flowerData)
    {
        if (flowerInfoCarousel != null)
        {
            flowerInfoCarousel.ShowFlower(flowerData);
        }
    }

    private void UpdateDebugUI(GameObject flower, FlowerData flowerData)
    {
        if (debugDisplay == null)
        {
            return;
        }

        ARFlowerManipulator manipulator =
            flower.GetComponent<ARFlowerManipulator>();

        float scaleMultiplier = 1f;

        if (manipulator != null)
        {
            scaleMultiplier = manipulator.CurrentScaleMultiplier;
        }

        debugDisplay.ShowFlowerScale(
            flowerData.displayName,
            flowerData.arScale,
            scaleMultiplier,
            flower.transform.localScale
        );
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