using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]

public class PlayerMovementNass : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _sprintSpeed = 10f;
    [SerializeField] float _jumpForce = 5f;
    [SerializeField] float _dashForce = 15f;
    [SerializeField] float _dashDuration = 0.2f;
    [SerializeField] float _dashCooldown = 1f;
    [SerializeField] ParticleSystem _dashTrailEffect;

    private Rigidbody rb;
    private bool isGrounded = false;
    private float _lastDashTime = -999f;
    private bool _isDashing = false;
    private float _dashEndTime;
    private Vector3 _lastDashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.Log("Need rigidbody component");
        }
    }

    void Update()
    {
        if (_isDashing && Time.time >= _dashEndTime)
        {
            _isDashing = false;
        }

        HandleDash();
        if (!_isDashing)
        {
            HandleMovement();
        }

        HandleJump();
    }

    private void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            direction += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction -= transform.forward;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += transform.right;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction -= transform.right;
        }
        direction.Normalize();

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? _sprintSpeed : _moveSpeed;
        rb.linearVelocity = new Vector3(direction.x * currentSpeed, rb.linearVelocity.y, direction.z * currentSpeed);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, _jumpForce, rb.linearVelocity.z);
        }
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= _lastDashTime + _dashCooldown && !_isDashing)
        {
            Vector3 dashDirection = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                dashDirection += transform.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dashDirection -= transform.forward;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dashDirection += transform.right;
            }
            if (Input.GetKey(KeyCode.A))
            {
                dashDirection -= transform.right;
            }

            if (dashDirection == Vector3.zero)
            {
                dashDirection = transform.forward;
            }

            dashDirection.Normalize();
            _lastDashDirection = dashDirection;

            rb.linearVelocity = new Vector3(dashDirection.x * _dashForce, rb.linearVelocity.y, dashDirection.z * _dashForce);

            _lastDashTime = Time.time;
            _isDashing = true;
            _dashEndTime = Time.time + _dashDuration;

            if (_dashTrailEffect != null)
            {
                StartCoroutine(PlayDashEffectDelayed(dashDirection));
            }
        }
    }

    private IEnumerator PlayDashEffectDelayed(Vector3 dashDirection)
    {
        yield return null;

        var shape = _dashTrailEffect.shape;
        shape.rotation = new Vector3(0, Mathf.Atan2(-dashDirection.x, -dashDirection.z) * Mathf.Rad2Deg, 0);

        _dashTrailEffect.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}