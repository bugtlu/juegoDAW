using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script del menú principal. Asignar PlayGame() al OnClick del botón "Play".
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private string levelSceneName = "Level1";

    public void PlayGame()
    {
        SceneManager.LoadScene(levelSceneName);
    }
}
