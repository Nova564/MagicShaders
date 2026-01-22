using UnityEngine;
using UnityEngine.InputSystem;

public class SpellVoidCaster : MonoBehaviour
{
    [Header("Spell Prefabs")]
    [SerializeField] private GameObject _telegraphPrefab;
    [SerializeField] private GameObject _voidTrapPrefab;

    [Header("Input")]
    [SerializeField] private InputActionProperty _castSpellAction;
    [SerializeField] private KeyCode _castKey = KeyCode.E;
    [SerializeField] private KeyCode _cancelKey = KeyCode.Mouse1;

    [Header("Settings")]
    [SerializeField] private float _maxCastDistance = 100f;
    [SerializeField] private float _maxCastRange = 15f;
    [SerializeField] private LayerMask _groundLayer = -1;
    [SerializeField] private bool _useNewInputSystem = false;
    [SerializeField] private float _telegraphHeightOffset = 0.05f;
    [SerializeField] private float _voidTrapHeightOffset = 0.01f;

    [Header("Visual Feedback")]
    [SerializeField] private Color _validColor = Color.green;
    [SerializeField] private Color _invalidColor = Color.red;

    private enum CastState { Idle, Previewing }
    private CastState _currentState = CastState.Idle;

    private GameObject _currentTelegraph;
    private Material _telegraphMaterial;
    private Camera _mainCamera;
    private Vector3 _targetPosition;
    private bool _isValidPosition;
    private bool _hasValidGroundHit;

    void Start()
    {
        _mainCamera = Camera.main;

        if (_useNewInputSystem && _castSpellAction.action != null)
        {
            _castSpellAction.action.Enable();
            _castSpellAction.action.performed += OnCastPressed;
        }
    }

    void OnDestroy()
    {
        if (_useNewInputSystem && _castSpellAction.action != null)
        {
            _castSpellAction.action.performed -= OnCastPressed;
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
            UpdateTelegraphPosition();
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
        else if (_currentState == CastState.Previewing && _isValidPosition && _hasValidGroundHit)
        {
            CastSpell();
        }
    }

    public void ActivateTelegraph()
    {
        if (_telegraphPrefab == null) return;

        _currentState = CastState.Previewing;
        _targetPosition = GetGroundPosition(_telegraphHeightOffset, out _hasValidGroundHit);
        _currentTelegraph = Instantiate(_telegraphPrefab, _targetPosition, Quaternion.Euler(90, 0, 0));

        Renderer renderer = _currentTelegraph.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            _telegraphMaterial = new Material(renderer.material);
            renderer.material = _telegraphMaterial;
        }

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.RequestUnlock();
        }
    }

    void UpdateTelegraphPosition()
    {
        if (_currentTelegraph == null) return;

        _targetPosition = GetGroundPosition(_telegraphHeightOffset, out _hasValidGroundHit);
        _isValidPosition = Vector3.Distance(transform.position, _targetPosition) <= _maxCastRange && _hasValidGroundHit;

        _currentTelegraph.transform.position = _targetPosition;

        if (_telegraphMaterial != null)
        {
            _telegraphMaterial.color = _isValidPosition ? _validColor : _invalidColor;
        }
    }

    public void CastSpell()
    {
        if (_voidTrapPrefab == null) return;

        if (_currentState != CastState.Previewing || !_isValidPosition || !_hasValidGroundHit) return;

        if (_currentTelegraph != null)
        {
            Destroy(_currentTelegraph);
        }

        Vector3 vortexPosition = GetGroundPosition(_voidTrapHeightOffset, out _);
        GameObject voidTrap = Instantiate(_voidTrapPrefab, vortexPosition, Quaternion.identity);

        VortexSpell spellTelegraph = voidTrap.GetComponent<VortexSpell>();
        if (spellTelegraph != null)
        {
            spellTelegraph.ActivateSpell(0f);
        }

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ReleaseUnlock();
        }

        _currentState = CastState.Idle;
        _isValidPosition = false;
        _hasValidGroundHit = false;
    }

    Vector3 GetGroundPosition(float heightOffset, out bool hitGround)
    {
        hitGround = false;

        if (_mainCamera == null)
        {
            return transform.position + transform.forward * 5f + Vector3.up * heightOffset;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxCastDistance, _groundLayer))
        {
            hitGround = true;
            return hit.point + Vector3.up * heightOffset;
        }

        return ray.origin + ray.direction.normalized * _maxCastDistance;
    }

    public void CancelPreview()
    {
        if (_currentState == CastState.Previewing && _currentTelegraph != null)
        {
            Destroy(_currentTelegraph);
            _currentTelegraph = null;
            _currentState = CastState.Idle;
            _isValidPosition = false;
            _hasValidGroundHit = false;

            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.ReleaseUnlock();
            }
        }
    }
}