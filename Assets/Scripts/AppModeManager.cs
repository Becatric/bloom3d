using UnityEngine;
using UnityEngine.SceneManagement;

public enum AppMode
{
    Learning,
    Bouquet
}

public class AppModeManager : MonoBehaviour
{
    private const string SelectedModeKey = "SelectedMode";

    public void StartLearningMode()
    {
        SaveMode(AppMode.Learning);
        SceneManager.LoadScene("FlowerScannerScene");
    }

    public void StartBouquetMode()
    {
        SaveMode(AppMode.Bouquet);
        SceneManager.LoadScene("BouquetBuilder");
    }

    public void SaveMode(AppMode mode)
    {
        PlayerPrefs.SetInt(SelectedModeKey, (int)mode);
        PlayerPrefs.Save();
    }

    private void SaveModeAndOpenAR(AppMode mode)
    {
        PlayerPrefs.SetInt(SelectedModeKey, (int)mode);
        PlayerPrefs.Save();

        SceneManager.LoadScene("FlowerScannerScene");
    }

    public static AppMode GetSelectedMode()
    {
        int savedMode = PlayerPrefs.GetInt(
            SelectedModeKey,
            (int)AppMode.Learning
        );

        return (AppMode)savedMode;
    }
}