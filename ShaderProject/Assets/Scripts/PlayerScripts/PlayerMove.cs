using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] InputActionProperty _move;
    [SerializeField] InputActionProperty _sprint;
    [SerializeField] InputActionProperty _jump;
    [SerializeField] float _speed = 5;
    [SerializeField] float _jumpForce = 4;
    [SerializeField] CharacterController _characterController;
    [SerializeField] bool _HasGravity = true;
    [SerializeField] Animator _animator;

    Vector3 _direction;
    private Vector3 velocity = Vector3.zero;

    private Vector3 finalDirection = Vector3.zero;

    private Vector2 lastMousePos;
    private float mouseStillTimer = 0f;
    private const float mouseStillThreshold = 0.05f;
    private const float mouseStillDelay = 0.1f;
    private bool _jumpPressed = false;

    void Start()
    {
        _move.action.Enable();
        _move.action.performed += StartMove;
        _move.action.canceled += StopMove;
        _sprint.action.performed += StartSprint;
        _sprint.action.canceled += StopSprint;
        _jump.action.started += StartJump;
        Cursor.lockState = CursorLockMode.Locked;   
        Cursor.visible = false;
    }

    void OnDestroy()
    {
        _move.action.performed -= StartMove;
        _move.action.canceled -= StopMove;
        _sprint.action.performed -= StartSprint;
        _sprint.action.canceled -= StopSprint;
        _jump.action.started -= StartJump;
    }

    void StartMove(InputAction.CallbackContext ctx)
    {
        if (_animator.GetBool("IsRunning"))
        {
            _speed = 8;
        }
        else
        {
            _speed = 5;
            _animator.SetBool("IsWalking", true);
        }
        var d = ctx.ReadValue<Vector2>();
        _direction = new Vector3(d.x, 0, d.y);
    }

    void StopMove(InputAction.CallbackContext ctx)
    {
        _direction = Vector3.zero;
        _animator.SetBool("IsWalking", false);
    }

    void Update()
    {
        UpdateMove();
        RotateTowardsInput();
    }

    void StartJump(InputAction.CallbackContext ctx)
    {
        if (_characterController.isGrounded)
        {
            _animator.SetTrigger("Jump");
            velocity.y = _jumpForce;
            _jumpPressed = true;
        }
    }
    
    void StartSprint(InputAction.CallbackContext ctx)
    {
        _animator.SetBool("IsRunning", true);
    }
    void StopSprint(InputAction.CallbackContext ctx)
    {
        _animator.SetBool("IsRunning", false);
    }

    public void TpTo(Vector3 TPpos)
    {
        _characterController.enabled= false;
        transform.position = TPpos;
        _characterController.enabled = true;
    }
    void UpdateMove()
    {
        finalDirection = _direction;
        if (Camera.main != null)
        {
            var forward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            var right = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z).normalized;

            finalDirection = (forward * _direction.z + right * _direction.x).normalized;
        }

        if (_characterController.isGrounded && !_jumpPressed)
            velocity.y = -1.0f;
        else if (_HasGravity)
        {
            velocity.y -= gravity * Time.deltaTime;
            if(_characterController.isGrounded) _jumpPressed = false;
        }
        Vector3 move = finalDirection * _speed;
        move += velocity;
        _characterController.Move(move * Time.deltaTime);
    }
    
    void RotateTowardsInput()
    {
        if (finalDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(finalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }
    }
}
