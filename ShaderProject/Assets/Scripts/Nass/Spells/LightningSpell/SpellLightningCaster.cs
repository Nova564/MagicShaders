using UnityEngine;
using UnityEngine.InputSystem;

public class SpellLightningCaster : MonoBehaviour
{
    [Header("Spell Prefabs")]
    [SerializeField] private GameObject _telegraphPrefab;
    [SerializeField] private GameObject _lightningSpellPrefab;

    [Header("Input")]
    [SerializeField] private InputActionProperty _castActionKey;
    [SerializeField] private KeyCode _castKey = KeyCode.T;
    [SerializeField] private KeyCode _cancelKey = KeyCode.Mouse1;

    [Header("Settings")]
    [SerializeField] private float _maxCastDistance = 100f;
    [SerializeField] private float _maxCastRange = 15f;
    [SerializeField] private LayerMask _groundLayer = -1;
    [SerializeField] private bool _useNewInputSystem = false;
    [SerializeField] private float _telegraphHeightOffset = 0.05f;
    [SerializeField] private float _lightningSpellHeightOffset = 0.01f;
    [SerializeField] private float _lightningSpawnYOffset = 4f; 

    [Header("Spell Duration")]
    [SerializeField] private float _spellLifetime = 3f;

    [Header("Visual Feedback")]
    [SerializeField] private Color _validColor = new Color(0.5f, 0.8f, 1f, 0.7f);
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

    void ActivateTelegraph()
    {
        if (_telegraphPrefab == null)
        {
            Debug.LogError("Telegraph prefab is null");
            return;
        }

        _currentState = CastState.Previewing;
        _targetPosition = GetGroundPosition(_telegraphHeightOffset, out _hasValidGroundHit);
        _currentTelegraph = Instantiate(_telegraphPrefab, _targetPosition, Quaternion.Euler(90, 0, 0));

        Renderer renderer = _currentTelegraph.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            _telegraphMaterial = new Material(renderer.material);
            renderer.material = _telegraphMaterial;
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
        if (_lightningSpellPrefab == null)
        {
            Debug.LogError("Lightning  prefab  null");
            return;
        }

        if (_currentTelegraph != null)
        {
            Destroy(_currentTelegraph);
        }

        Vector3 groundPosition = GetGroundPosition(_lightningSpellHeightOffset, out _);
        Vector3 lightningPosition = groundPosition + Vector3.up * _lightningSpawnYOffset;

        GameObject lightningSpell = Instantiate(_lightningSpellPrefab, lightningPosition, Quaternion.identity);

        ParticleSystem[] particleSystems = lightningSpell.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Play();
            }
        }

        Destroy(lightningSpell, _spellLifetime);

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
        }
    }
}