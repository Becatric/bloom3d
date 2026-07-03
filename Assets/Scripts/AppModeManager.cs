using UnityEngine;
using UnityEngine.SceneManagement;

public enum AppMode
{
    Learning,
    Bouquet,
    AboutUs
}

public class AppModeManager : MonoBehaviour
{
    private const string SelectedModeKey = "SelectedMode";

    public void StartLearningMode()
    {
        SaveMode(AppMode.Learning);

        SceneManager.LoadScene(
            "FlowerScannerScene",
            LoadSceneMode.Single
        );
    }

    public void StartBouquetMode()
    {
        SaveMode(AppMode.Bouquet);

        SceneManager.LoadScene(
            "BouquetBuilder",
            LoadSceneMode.Single
        );
    }

    public void StartAboutUsMode()
    {
        SaveMode(AppMode.AboutUs);

        SceneManager.LoadScene(
            "AboutUs",
            LoadSceneMode.Single
        );
    }

    private void SaveMode(AppMode mode)
    {
        PlayerPrefs.SetInt(
            SelectedModeKey,
            (int)mode
        );

        PlayerPrefs.Save();
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