using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(
            "MainMenu",
            LoadSceneMode.Single
        );
    }

    public void OpenFlowerScannerScene()
    {
        SceneManager.LoadScene(
            "FlowerScannerScene",
            LoadSceneMode.Single
        );
    }

    public void OpenBouquetScene()
    {
        SceneManager.LoadScene(
            "BouquetBuilder",
            LoadSceneMode.Single
        );
    }

    public void OpenAboutUS()
    {
        SceneManager.LoadScene(
            "AboutUs",
            LoadSceneMode.Single
        );
    }
}