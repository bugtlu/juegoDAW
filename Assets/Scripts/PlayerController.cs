using UnityEngine;

/// <summary>
/// Mueve la bola del jugador con física (WASD / flechas).
/// El movimiento se aplica en FixedUpdate a través de linearVelocity, conservando la Y
/// para que la gravedad siga funcionando.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody rb;
    private Vector3 inputDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Bloqueamos la rotación en X y Z para que la bola no ruede.
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        // La lectura de input se hace en Update (cada frame) para no perder pulsaciones.
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        inputDirection = new Vector3(horizontal, 0f, vertical);

        // Evita que en diagonal se vaya más rápido.
        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection.Normalize();
        }
    }

    private void FixedUpdate()
    {
        // La física se aplica en FixedUpdate. Conservamos la velocidad vertical (gravedad).
        Vector3 velocity = inputDirection * speed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }
}
