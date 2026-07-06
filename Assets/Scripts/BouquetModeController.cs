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
    
    [Header("Bouquet Position")]
    [SerializeField]
    private float bouquetHeightOffset = 0f;
    
    public float BouquetHeightOffset => bouquetHeightOffset;

    [Header("Wrapping Markers")]
    [SerializeField]
    private string paperMarkerName = "paper";

    [SerializeField]
    private string bowMarkerName = "bow";

    [Header("Wrapping Prefabs")]
    [SerializeField]
    private GameObject paperWrappingPrefab;

    [SerializeField]
    private GameObject bowWrappingPrefab;

    [Header("Bouquet Control UI")]
    [SerializeField]
    private BouquetControls bouquetControls;

    // Flower markers currently visible. The bouquet, paper, and bow markers are excluded from this set.
    private readonly Dictionary<TrackableId, string> visibleFlowerMarkers =
        new Dictionary<TrackableId, string>();

    // The bouquet marker's transform while it is tracking; null otherwise.
    private Transform bouquetAnchor;
    private TrackableId bouquetAnchorId;

    private TrackableId paperWrappingMarkerId;
    private TrackableId bowWrappingMarkerId;
    private bool isPaperWrappingVisible;
    private bool isBowWrappingVisible;

    private GameObject paperWrappingInstance;
    private GameObject bowWrappingInstance;

    // Guards against rebuilding the bouquet every frame.
    // Re-arrange only when the visible marker set changes.
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
            trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }

        visibleFlowerMarkers.Clear();
        bouquetAnchor = null;
        bouquetAnchorId = default;
        paperWrappingMarkerId = default;
        bowWrappingMarkerId = default;
        isPaperWrappingVisible = false;
        isBowWrappingVisible = false;
        lastSignature = null;
        HideBouquet();
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (ARTrackedImage image in eventArgs.added)
        {
            ApplyMarkerState(image);
        }

        foreach (ARTrackedImage image in eventArgs.updated)
        {
            ApplyMarkerState(image);
        }

        foreach (KeyValuePair<TrackableId, ARTrackedImage> removed in eventArgs.removed)
        {
            RemoveMarker(removed.Key);
        }

        Rebuild();
    }

    // Add or remove markers to keep track of:
    // bouquet marker = anchor, flower markers = flower list, wrapping markers = optional decorations.
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

        if (IsPaperWrappingMarker(markerName))
        {
            isPaperWrappingVisible = isVisible;
            paperWrappingMarkerId = isVisible ? image.trackableId : default;
            return;
        }

        if (IsBowWrappingMarker(markerName))
        {
            isBowWrappingVisible = isVisible;
            bowWrappingMarkerId = isVisible ? image.trackableId : default;
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

        if (trackableId == paperWrappingMarkerId)
        {
            isPaperWrappingVisible = false;
            paperWrappingMarkerId = default;
        }

        if (trackableId == bowWrappingMarkerId)
        {
            isBowWrappingVisible = false;
            bowWrappingMarkerId = default;
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
        
        bouquetTransform.localPosition =
            Vector3.up * bouquetHeightOffset;
        
        bouquetTransform.localRotation = Quaternion.identity;
        bouquetTransform.localScale = Vector3.one * bouquetScale;

        bouquetManager.flowers = flowers;
        bouquetManager.gameObject.SetActive(true);

        // Empty pattern -> BouquetManager walks every palette slot in order, cycling
        // to fill its configured flowerCount.
        bouquetManager.ArrangeFlowers(new List<int>());

        UpdateWrappingInstances();
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
            FlowerData data = flowerDatabase.GetFlowerByMarkerName(markerName);

            if (data == null || data.flowerPrefab == null)
            {
                continue;
            }

            if (!result.Contains(data))
            {
                result.Add(data);
            }
        }

        // The same marker set always yields the same bouquet.
        result.Sort((a, b) => string.Compare(a.id, b.id, System.StringComparison.Ordinal));
        return result;
    }

    private void UpdateWrappingInstances()
    {
        SetWrappingVisible(
            ref paperWrappingInstance,
            paperWrappingPrefab,
            isPaperWrappingVisible
        );

        SetWrappingVisible(
            ref bowWrappingInstance,
            bowWrappingPrefab,
            isBowWrappingVisible
        );
    }

    private void SetWrappingVisible(
        ref GameObject instance,
        GameObject prefab,
        bool isVisible)
    {
        if (prefab == null)
        {
            return;
        }

        if (bouquetAnchor == null)
        {
            if (instance != null)
            {
                instance.SetActive(false);
            }

            return;
        }

        if (instance == null)
        {
            instance = Instantiate(prefab);
        }

        // Wrapping is attached directly to the bouquet marker,
        // not to BouquetManager.
        if (instance.transform.parent != bouquetAnchor)
        {
            instance.transform.SetParent(bouquetAnchor, false);
        }

        // Preserve the transform saved in the prefab.
        instance.transform.localPosition =
            prefab.transform.localPosition;

        instance.transform.localRotation =
            prefab.transform.localRotation;

        instance.transform.localScale =
            prefab.transform.localScale;

        instance.SetActive(isVisible);
    }

    // Cheap change-detection key: anchor presence + flowers + wrapping markers.
    private string BuildSignature(List<FlowerData> flowers)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(bouquetAnchor != null ? "A|" : "-| ");
        builder.Append(isPaperWrappingVisible ? "P|" : "-| ");
        builder.Append(isBowWrappingVisible ? "B|" : "-| ");

        foreach (FlowerData flower in flowers)
        {
            builder.Append(flower.GetEntityId());
            builder.Append(',');
        }

        return builder.ToString();
    }

    private void HideBouquet()
    {
        if (paperWrappingInstance != null)
        {
            paperWrappingInstance.SetActive(false);
        }

        if (bowWrappingInstance != null)
        {
            bowWrappingInstance.SetActive(false);
        }

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

    private bool IsPaperWrappingMarker(string markerName)
    {
        return string.Equals(
            markerName,
            paperMarkerName,
            System.StringComparison.OrdinalIgnoreCase
        );
    }

    private bool IsBowWrappingMarker(string markerName)
    {
        return string.Equals(
            markerName,
            bowMarkerName,
            System.StringComparison.OrdinalIgnoreCase
        );
    }
    
    public void SetBouquetHeightOffset(float value)
    {
        bouquetHeightOffset = value;

        if (bouquetManager != null && bouquetAnchor != null)
        {
            bouquetManager.transform.localPosition =
                Vector3.up * bouquetHeightOffset;
        }
    }
}
