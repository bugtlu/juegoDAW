using UnityEngine;

/// <summary>
/// Moneda recogible. Necesita un SphereCollider con Is Trigger = true.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class Coin : MonoBehaviour
{
    [SerializeField] private int points = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        GameManager.Instance.AddScore(points);
        Debug.Log($"Coin collected! Score: {GameManager.Instance.Score}");
        Destroy(gameObject);
    }
}
