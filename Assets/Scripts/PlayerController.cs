using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float forwardSpeed = 5f;
    public float sideSpeed = 4f;
    public float jumpForce = 7f;
    public float speedBoostMultiplier = 2f;
    public float boostDuration = 3f;
    public Transform startPoint;

    public bool IsBoosting { get; private set; }
    public float boostTimer { get; private set; }

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 spawnPosition;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnPosition = startPoint != null ? startPoint.position : transform.position;
    }

void Update()
    {
        if (GameStore.Instance != null && GameStore.Instance.IsGameOver) return;
        HandleBoost();
        float speed = IsBoosting ? forwardSpeed * speedBoostMultiplier : forwardSpeed;
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
        float h = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * h * sideSpeed * Time.deltaTime, Space.World);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
        if (transform.position.y < -5f)
            Respawn(loseLife: true);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pit"))
            Respawn(loseLife: true);
        if (other.CompareTag("Finish"))
        {
            forwardSpeed = 0f;
            if (GameManager.Instance != null)
                GameManager.Instance.OnFinish();
        }
    }

    void HandleBoost()
    {
        if (Input.GetKey(KeyCode.LeftShift) && boostTimer < boostDuration)
        {
            IsBoosting = true;
            boostTimer += Time.deltaTime;
            if (boostTimer > boostDuration) boostTimer = boostDuration;
        }
        else
        {
            IsBoosting = false;
        }
        if (!Input.GetKey(KeyCode.LeftShift) && boostTimer > 0f)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer < 0f) boostTimer = 0f;
        }
    }

public void Respawn(bool loseLife = true)
    {
        if (loseLife && GameStore.Instance != null && !GameStore.Instance.IsGameOver)
            GameStore.Instance.LoseLife();
        if (GameStore.Instance != null && GameStore.Instance.IsGameOver)
            return;
        transform.position = spawnPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isGrounded = true;
        if (GameManager.Instance != null)
            GameManager.Instance.OnRespawn();
    }
}
