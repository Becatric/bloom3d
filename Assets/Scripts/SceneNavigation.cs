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
            "BouquetScene",
            LoadSceneMode.Single
        );
    }
}