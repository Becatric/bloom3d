using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FlowerImageTracker : MonoBehaviour
{
    [Serializable]
    public class FlowerMarker
    {
        public string imageName;
        public GameObject flowerPrefab;
    }

    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    [SerializeField]
    private List<FlowerMarker> flowerMarkers = new();

    private readonly Dictionary<TrackableId, GameObject> spawnedFlowers = new();

    private void Awake()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager = GetComponent<ARTrackedImageManager>();
        }
    }

    private void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(
            OnTrackedImagesChanged
        );
    }

    private void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(
            OnTrackedImagesChanged
        );
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
            if (spawnedFlowers.TryGetValue(
                removedImage.Key,
                out GameObject flower
            ))
            {
                Destroy(flower);
                spawnedFlowers.Remove(removedImage.Key);
            }
        }
    }

    private void SpawnFlower(ARTrackedImage trackedImage)
    {
        string detectedImageName = trackedImage.referenceImage.name;

        FlowerMarker marker = flowerMarkers.Find(
            item => item.imageName == detectedImageName
        );

        if (marker == null || marker.flowerPrefab == null)
        {
            Debug.LogWarning(
                $"No flower assigned for image: {detectedImageName}"
            );

            return;
        }

        GameObject flower = Instantiate(
            marker.flowerPrefab,
            trackedImage.transform
        );

        flower.transform.localPosition = Vector3.zero;
        flower.transform.localRotation = Quaternion.identity;

        spawnedFlowers[trackedImage.trackableId] = flower;

        UpdateFlower(trackedImage);
    }

    private void UpdateFlower(ARTrackedImage trackedImage)
    {
        if (!spawnedFlowers.TryGetValue(
            trackedImage.trackableId,
            out GameObject flower
        ))
        {
            SpawnFlower(trackedImage);
            return;
        }

        flower.SetActive(
            trackedImage.trackingState != TrackingState.None
        );
    }
}