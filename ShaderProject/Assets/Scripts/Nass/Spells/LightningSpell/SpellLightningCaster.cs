using UnityEngine;
using UnityEngine.InputSystem;

public class SpellLightningCaster : MonoBehaviour
{
    [Header("Spell prefabs")]
    [SerializeField] private GameObject _lightningTelegraphPrefab;
    [SerializeField] private GameObject _lightningSpellPrefab;

    [Header("Input")]
    [SerializeField] private InputActionProperty _castActionKey;
    [SerializeField] private KeyCode _castKey = KeyCode.T;
    [SerializeField] private KeyCode _cancelKey = KeyCode.Mouse1;

    [Header("Settings")]
    [SerializeField] private float _lightningLength = 15f;
    [SerializeField] private bool _useNewInputSystem = false;
    [SerializeField] private float _telegraphHeightOffset = 0.05f;
    [SerializeField] private float _lightningSpellHeightOffset = 0.01f;
    [SerializeField] private float _playerOffset = 1.5f;

    [Header("Visual Feedback")]
    [SerializeField] private Color _validColor = new Color(0.5f, 0.8f, 1f, 0.7f);
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

        if (_telegraphMaterial != null)
        {
            Destroy(_telegraphMaterial);
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
        if (_lightningTelegraphPrefab == null) return;

        _currentState = CastState.Previewing;
        _targetDirection = GetMouseDirection();

        Vector3 spawnPosition = transform.position + _targetDirection * _playerOffset + Vector3.up * _telegraphHeightOffset;
        _currentTelegraph = Instantiate(_lightningTelegraphPrefab, spawnPosition, Quaternion.identity);

        _currentTelegraph.transform.localScale = new Vector3(1f, 1f, _lightningLength);

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
            Quaternion targetRotation = Quaternion.LookRotation(_targetDirection) * Quaternion.Euler(90, 310, 45);
            _currentTelegraph.transform.rotation = targetRotation;
        }

        if (_telegraphMaterial != null)
        {
            _telegraphMaterial.color = _isValidPosition ? _validColor : _invalidColor;
        }
    }

    public void CastSpell()
    {
        if (_lightningSpellPrefab == null) return;

        if (_currentTelegraph != null)
        {
            Destroy(_currentTelegraph);
        }

        Vector3 lightningPosition = transform.position + _targetDirection * _playerOffset + Vector3.up * _lightningSpellHeightOffset;
        Quaternion lightningRotation = Quaternion.LookRotation(_targetDirection);
        GameObject lightningSpell = Instantiate(_lightningSpellPrefab, lightningPosition, lightningRotation);

        lightningSpell.transform.localScale = new Vector3(1f, 1f, _lightningLength);

        LightningSpell spellComponent = lightningSpell.GetComponent<LightningSpell>();
        if (spellComponent != null)
        {
            spellComponent.ActivateSpell(_lightningLength);
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