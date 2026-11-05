using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton que guarda la puntuación y permite reiniciar el nivel.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int score;

    public int Score => score;

    private void Awake()
    {
        // Patrón Singleton: si ya existe otra instancia, destruimos esta.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
