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

    [Header("Visual Feedback")]
    [SerializeField] private Color _validColor = Color.green;
    [SerializeField] private Color _invalidColor = Color.red;

    private enum CastState { Idle, Previewing }
    private CastState _currentState = CastState.Idle;

    private GameObject _currentTelegraph;
    private Material _telegraphMaterial;
    private Camera _mainCamera;
    private Vector3 _targetDirection;
    private bool _isValidPosition;

    void Start()
    {
        _mainCamera = Camera.main;

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

    void ActivateTelegraph()
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
            Quaternion targetRotation = Quaternion.LookRotation(_targetDirection) * Quaternion.Euler(90, 180, 45);
            _currentTelegraph.transform.rotation = targetRotation;
        }

        if (_telegraphMaterial != null)
        {
            _telegraphMaterial.color = _isValidPosition ? _validColor : _invalidColor; //inutile vu qu'il n'y a pas de position valide/invalide
        }
    }

    public void CastSpell()
    {
        if (_flameSpellPrefab == null) return;

        if (_currentTelegraph != null)
        {
            Destroy(_currentTelegraph);
        }

        Vector3 flamePosition = transform.position + _targetDirection * _playerOffset + Vector3.up * _flameSpellHeightOffset;
        Quaternion flameRotation = Quaternion.LookRotation(_targetDirection) * Quaternion.Euler(0, 180f, 0);
        GameObject flameSpell = Instantiate(_flameSpellPrefab, flamePosition, flameRotation);

        FlameSpell spellComponent = flameSpell.GetComponent<FlameSpell>();
        if (spellComponent != null)
        {
            spellComponent.ActivateSpell();
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