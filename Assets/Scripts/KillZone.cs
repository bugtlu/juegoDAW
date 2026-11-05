using UnityEngine;

/// <summary>
/// Zona de muerte bajo el nivel. Si el jugador entra, se reinicia la escena.
/// Necesita un Collider con Is Trigger = true.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Player killed!");
        GameManager.Instance.RestartLevel();
    }
}
