using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BouquetModeController : MonoBehaviour
{
    [Header("AR")]
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    [Header("Flower Database")]
    [SerializeField]
    private FlowerDatabase flowerDatabase;

    [Header("Bouquet Manager")]
    [SerializeField]
    private BouquetManager bouquetManager;

    [SerializeField]
    private string bouquetMarkerName = "bouquet";

    [SerializeField]
    private float bouquetScale = 1f;

    [Header("Bouquet Control UI")]
    [SerializeField]
    private BouquetControls bouquetControls;

    // Flower markers currently visible (the bouquet marker is excluded from this set).
    private readonly Dictionary<TrackableId, string> visibleFlowerMarkers =
        new Dictionary<TrackableId, string>();

    // The bouquet marker's transform while it is tracking; null otherwise.
    private Transform bouquetAnchor;
    private TrackableId bouquetAnchorId;

    // Guards against rebuilding the bouquet every frame 
    // only re-arrange when the visible set of markers change
    private string lastSignature;

    private void Awake()
    {

        if (trackedImageManager == null)
        {
            trackedImageManager = GetComponent<ARTrackedImageManager>();
        }

        // Start hidden; nothing to show until a flower marker and the bouquet marker appear.
        if (bouquetManager != null)
        {
            bouquetManager.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
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

        visibleFlowerMarkers.Clear();
        bouquetAnchor = null;
        bouquetAnchorId = default;
        lastSignature = null;
        HideBouquet();
    }

    private void OnTrackedImagesChanged(
        ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs
    )
    {
        foreach (ARTrackedImage image in eventArgs.added)
        {
            ApplyMarkerState(image);
        }

        foreach (ARTrackedImage image in eventArgs.updated)
        {
            ApplyMarkerState(image);
        }

        foreach (
            KeyValuePair<TrackableId, ARTrackedImage> removed
            in eventArgs.removed
        )
        {
            RemoveMarker(removed.Key);
        }

        Rebuild();
    }

    // Add or remove markers to keep track of:
    // For flowers add them to the bouquet list, for the bouquet marker save important info
    private void ApplyMarkerState(ARTrackedImage image)
    {
        bool isVisible = image.trackingState == TrackingState.Tracking;
        string markerName = image.referenceImage.name;

        if (IsBouquetMarker(markerName))
        {
            if (isVisible)
            {
                bouquetAnchor = image.transform;
                bouquetAnchorId = image.trackableId;
            }
            else if (image.trackableId == bouquetAnchorId)
            {
                bouquetAnchor = null;
                bouquetAnchorId = default;
            }

            return;
        }

        if (isVisible)
        {
            visibleFlowerMarkers[image.trackableId] = markerName;
        }
        else
        {
            visibleFlowerMarkers.Remove(image.trackableId);
        }
    }

    private void RemoveMarker(TrackableId trackableId)
    {
        visibleFlowerMarkers.Remove(trackableId);

        if (trackableId == bouquetAnchorId)
        {
            bouquetAnchor = null;
            bouquetAnchorId = default;
        }
    }

    // Re-arrange only when something meaningful changed (see lastSignature).
    private void Rebuild()
    {
        List<FlowerData> flowers = CollectVisibleFlowers();
        string signature = BuildSignature(flowers);

        if (signature == lastSignature)
        {
            return;
        }

        lastSignature = signature;

        if (bouquetManager == null)
        {
            return;
        }

        // Nothing to show without both an anchor to render on and at least one flower.
        if (bouquetAnchor == null || flowers.Count == 0)
        {
            HideBouquet();
            return;
        }

        Transform bouquetTransform = bouquetManager.transform;
        bouquetTransform.SetParent(bouquetAnchor, false);
        bouquetTransform.localPosition = Vector3.zero;
        bouquetTransform.localRotation = Quaternion.identity;
        bouquetTransform.localScale = Vector3.one * bouquetScale;

        bouquetManager.flowers = flowers;
        bouquetManager.gameObject.SetActive(true);

        // Empty pattern -> BouquetManager walks every palette slot in order, cycling
        // to fill its configured flowerCount.
        bouquetManager.ArrangeFlowers(new List<int>());
    }

    // Distinct, deterministically-ordered FlowerData contributed by visible markers.
    private List<FlowerData> CollectVisibleFlowers()
    {
        List<FlowerData> result = new List<FlowerData>();

        if (flowerDatabase == null)
        {
            return result;
        }

        foreach (string markerName in visibleFlowerMarkers.Values)
        {
            FlowerData data =
                flowerDatabase.GetFlowerByMarkerName(markerName);

            if (data == null || data.flowerPrefab == null)
            {
                continue;
            }

            if (!result.Contains(data))
            {
                result.Add(data);
            }
        }

        result.Sort((a, b) => string.Compare(a.id, b.id, System.StringComparison.Ordinal));
        // so the same set of markers always yields the same bouquet
        return result;
    }

    // Cheap change-detection key: anchor presence + the set of flower instance IDs.
    private string BuildSignature(List<FlowerData> flowers)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(bouquetAnchor != null ? "A|" : "-|");

        foreach (FlowerData flower in flowers)
        {
            builder.Append(flower.GetEntityId());
            builder.Append(',');
        }

        return builder.ToString();
    }

    private void HideBouquet()
    {
        if (bouquetManager != null)
        {
            bouquetManager.gameObject.SetActive(false);
        }
    }

    private bool IsBouquetMarker(string markerName)
    {
        return string.Equals(
            markerName,
            bouquetMarkerName,
            System.StringComparison.OrdinalIgnoreCase
        );
    }
}
