using UnityEngine;

public class Jump : MonoBehaviour
{
    Rigidbody rigidbody;
    public float jumpStrength = 2;
    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    GroundCheck groundCheck;

    private int jumpCount = 0;  // Contador de saltos

    void Reset()
    {
        // Try to get groundCheck.
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        // Get rigidbody.
        rigidbody = GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        // Si el jugador presiona el botón de salto
        if (Input.GetButtonDown("Jump"))
        {
            // Si estamos en el suelo o el contador de saltos es menor que 2 (permitiendo el doble salto)
            if (groundCheck && groundCheck.isGrounded)
            {
                jumpCount = 0;  // Reiniciamos el contador cuando tocamos el suelo
            }

            if (jumpCount < 2)  // Si el contador de saltos es menor a 2, permitimos saltar
            {
                rigidbody.AddForce(Vector3.up * 100 * jumpStrength);
                jumpCount++;  // Aumentamos el contador de saltos
                Jumped?.Invoke();
            }
        }
    }
}