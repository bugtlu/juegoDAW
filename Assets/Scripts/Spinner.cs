using UnityEngine;

/// <summary>
/// Ampliación (+1): gira el objeto sobre el eje Y y lo hace flotar arriba y abajo
/// con Mathf.PingPong (+1).
/// </summary>
public class Spinner : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;   // grados por segundo
    [SerializeField] private bool bob = true;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 1f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);

        if (bob)
        {
            float offset = Mathf.PingPong(Time.time * bobSpeed, bobHeight);
            transform.position = startPosition + Vector3.up * offset;
        }
    }
}
