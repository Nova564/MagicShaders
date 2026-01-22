using UnityEngine;
using UnityEngine.InputSystem;

public class SpellFlameCaster : MonoBehaviour
{
    [Header("Spell prefabs")]
    [SerializeField] private GameObject _flameTelegraphPrefab;
    [SerializeField] private GameObject _flameSpellPrefab;

    [Header("Input")]
    [SerializeField] private InputActionProperty _castActionKey;
    [SerializeField] private KeyCode _castKey = KeyCode.R;
    [SerializeField] private KeyCode _cancelKey = KeyCode.Mouse1;

    [Header("Settings")]
    [SerializeField] private float _range = 15f;
    [SerializeField] private bool _useNewInputSystem = false;
    [SerializeField] private float _telegraphHeightOffset = 0.05f;
    [SerializeField] private float _flameSpellHeightOffset = 0.01f;
    [SerializeField] private float _playerOffset = 1f;

    [Header("Cast Settings")]
    [SerializeField] private float _chargeDuration = 1.5f;

    [Header("Visual Feedback")]
    [SerializeField] private Color _validColor = Color.green;
    [SerializeField] private Color _invalidColor = Color.red;

    private enum CastState { Idle, Previewing, Casting }
    private CastState _currentState = CastState.Idle;

    private GameObject _currentTelegraph;
    private Material _telegraphMaterial;
    private Camera _mainCamera;
    private Vector3 _targetDirection;
    private bool _isValidPosition;
    private PlayerMove _playerMovement;

    void Start()
    {
        _mainCamera = Camera.main;
        _playerMovement = GetComponent<PlayerMove>();

        if (_useNewInputSystem && _castActionKey.action != null)
        {
            _castActionKey.action.Enable();
            _castActionKey.action.performed += OnCastPressed;
        }
    }

    void OnDestroy()
    {
        if (_useNewInputSystem && _castActionKey.action != null)
        {
            _castActionKey.action.performed -= OnCastPressed;
        }
    }

    void Update()
    {
        if (!_useNewInputSystem)
        {
            if (Input.GetKeyDown(_castKey)) HandleCastInput();
            if (Input.GetKeyDown(_cancelKey)) CancelPreview();
        }

        if (_currentState == CastState.Previewing)
        {
            UpdateTelegraphDirection();
        }
    }

    void OnCastPressed(InputAction.CallbackContext ctx)
    {
        HandleCastInput();
    }

    void HandleCastInput()
    {
        if (_currentState == CastState.Idle)
        {
            ActivateTelegraph();
        }
        else if (_currentState == CastState.Previewing && _isValidPosition)
        {
            CastSpell();
        }
    }

    public void ActivateTelegraph()
    {
        if (_flameTelegraphPrefab == null) return;

        _currentState = CastState.Previewing;
        _targetDirection = GetMouseDirection();

        Vector3 spawnPosition = transform.position + _targetDirection * _playerOffset + Vector3.up * _telegraphHeightOffset;
        _currentTelegraph = Instantiate(_flameTelegraphPrefab, spawnPosition, Quaternion.identity);

        Renderer renderer = _currentTelegraph.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            _telegraphMaterial = new Material(renderer.material);
            renderer.material = _telegraphMaterial;
        }

        _isValidPosition = true;
    }

    void UpdateTelegraphDirection()
    {
        if (_currentTelegraph == null) return;

        _targetDirection = GetMouseDirection();

        Vector3 telegraphPosition = transform.position + _targetDirection * _playerOffset + Vector3.up * _telegraphHeightOffset;
        _currentTelegraph.transform.position = telegraphPosition;

        if (_targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_targetDirection) * Quaternion.Euler(90, 260, 0);
            _currentTelegraph.transform.rotation = targetRotation;
        }

        if (_telegraphMaterial != null)
        {
            _telegraphMaterial.color = _validColor;
        }
    }

    public void CastSpell()
    {
        if (_flameSpellPrefab == null) return;
        if (_currentState != CastState.Previewing) return;

        Vector3 finalDirection = _targetDirection;

        if (_currentTelegraph != null)
        {
            Destroy(_currentTelegraph);
        }
        _currentState = CastState.Casting;
        if (_playerMovement != null)
        {
            _playerMovement.SetMovementEnabled(false);
        }


        Vector3 flamePosition = transform.position + finalDirection * _playerOffset + Vector3.up * _flameSpellHeightOffset;
        Quaternion flameRotation = Quaternion.LookRotation(finalDirection) * Quaternion.Euler(0, 180f, 0);
        GameObject flameSpell = Instantiate(_flameSpellPrefab, flamePosition, flameRotation);

        FlameSpell spellComponent = flameSpell.GetComponent<FlameSpell>();
        if (spellComponent != null)
        {
            spellComponent.ActivateSpell(finalDirection);
        }
        StartCoroutine(ReenableMovementAfterCast());
    }

    private System.Collections.IEnumerator ReenableMovementAfterCast()
    {
        yield return new WaitForSeconds(_chargeDuration);

        if (_playerMovement != null)
        {
            _playerMovement.SetMovementEnabled(true);
        }

        _currentState = CastState.Idle;
        _isValidPosition = false;
    }

    Vector3 GetMouseDirection()
    {
        if (_mainCamera == null)
        {
            return transform.forward;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0;
            return direction;
        }

        return transform.forward;
    }

    public void CancelPreview()
    {
        if (_currentState == CastState.Previewing && _currentTelegraph != null)
        {
            Destroy(_currentTelegraph);
            _currentTelegraph = null;
            _currentState = CastState.Idle;
            _isValidPosition = false;
        }
    }
}