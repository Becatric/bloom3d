using TMPro;
using UnityEngine;

public class ModeDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text modeLabel;

    private void Start()
    {
        if (modeLabel == null)
        {
            Debug.LogError(
                "Mode Label is not assigned in ModeDisplay."
            );

            return;
        }

        AppMode selectedMode =
            AppModeManager.GetSelectedMode();

        modeLabel.text = selectedMode switch
        {
            AppMode.Learning => "Learning Mode",
            AppMode.Bouquet => "Bouquet Mode",
            AppMode.AboutUs => "About Us Mode",
            _ => "Unknown Mode"
        };

        Debug.Log($"Current app mode: {selectedMode}");
    }
}