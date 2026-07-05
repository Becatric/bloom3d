using TMPro;
using UnityEngine;

public class ARDebugDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text debugText;

    public void ShowFlowerScale(
        string flowerName,
        float arDisplayScale,
        float pinchMultiplier,
        Vector3 finalScale
    )
    {
        if (debugText == null)
        {
            return;
        }

        float scalePercent = pinchMultiplier * 100f;

        debugText.text =
            $"Flower: {flowerName}\n" +
            $"AR Display scale: {arDisplayScale:F4}\n" +
            $"Pinch multiplier: {pinchMultiplier:F2}x\n" +
            $"Final scale: {finalScale.x:F4}\n" +
            $"Scale percent: {scalePercent:F0}%";
    }
}