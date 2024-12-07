using UnityEngine;

public class Jump : MonoBehaviour
{
    Rigidbody rigidbody;
    public float jumpStrength = 2;
    public bool autoJump = true;
    public float autoJumpStrength = 1;
    public float autoJumpDistance = 2;
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
    }


    void AutoJump()
    {
        // Si el jugador no está en el suelo y el salto automático está activado
        if (!groundCheck.isGrounded && autoJump)
        {
            //Mandar dos rayos hacia delante para ver si hay un obstáculo
            //Uno a la altura de los pies y otro desde el centro del jugador
            RaycastHit feet;
            RaycastHit center;
            if (Physics.Raycast((transform.position - Vector3.down * 3), Vector3.forward, out feet, autoJumpDistance, terrainLayer) && 
                !Physics.Raycast(transform.position, Vector3.forward, out center, autoJumpDistance, terrainLayer) &&
                groundCheck && groundCheck.isGrounded)
            {
                // Si no hay obstáculo, saltar
                rigidbody.AddForce(Vector3.up * 100 * autoJumpStrength);
                Jumped?.Invoke();

            }
        }
    }

}