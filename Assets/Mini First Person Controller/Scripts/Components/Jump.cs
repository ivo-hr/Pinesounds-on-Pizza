using UnityEngine;

public class Jump : MonoBehaviour
{
    Rigidbody rigidbody;
    public float jumpStrength = 2;
    public bool stepUp = true;
    public float stepUpStrength = 1;
    public float stepUpDistance = 2;

    
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private float stepCheckDistance = 0.5f;

    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    GroundCheck groundCheck;

    private int jumpCount = 0;  // Contador de saltos

    private int terrainLayer;
    void Reset()
    {
        // Try to get groundCheck.
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        // Get rigidbody.
        rigidbody = GetComponent<Rigidbody>();

        terrainLayer = LayerMask.GetMask("Terrain");
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

        if (stepUp)
        {
            StepUp();
        }
    }


    void StepUp()
    {
        if (!groundCheck.isGrounded) return;

        RaycastHit lowerHit;
        RaycastHit upperHit;

        // Check for obstacles at the lower step height
        Vector3 lowerOrigin = transform.position + Vector3.up * 0.1f; // Slightly above ground level
        if (Physics.Raycast(lowerOrigin, transform.forward, out lowerHit, stepCheckDistance, terrainLayer))
        {
            // Check if there's space at the step height
            Vector3 upperOrigin = transform.position + Vector3.up * stepHeight;
            if (!Physics.Raycast(upperOrigin, transform.forward, out upperHit, stepCheckDistance, terrainLayer))
            {
                // No obstacle above the step; move the player up
                transform.position += Vector3.up * stepHeight;
            }
        }
    }


}