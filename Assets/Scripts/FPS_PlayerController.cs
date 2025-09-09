using UnityEngine;

public class FPS_PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 5f;
    public float drag_ = 6f;
    public float playerHeight = 2f;
    public LayerMask whatIsGround;

    [Header("References")]
    public Transform orientation;
    public Animator anim_;

    Rigidbody rb;
    bool grounded;
    float horizontalInput, verticalInput;
    Vector3 moveDir;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        if (GameManager.I.isWin) return;
        if (GameManager.I.isLose) return;

        inputMove();

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        rb.drag = grounded ? drag_ : 0f;

        // Handle animations if needed
        anim_?.SetFloat("speed", moveDir.magnitude);
    }

    private void FixedUpdate()
    {
        if (GameManager.I.isWin) return;
        if (GameManager.I.isLose) return;

        MovingPlayer();
    }

    void inputMove()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDir.y = 0;
    }

    void MovingPlayer()
    {
        rb.AddForce(moveDir.normalized * speed * 10, ForceMode.Force);

    
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > speed)
        {
            Vector3 limited = flatVel.normalized * speed;
            rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
        }
    }
}
