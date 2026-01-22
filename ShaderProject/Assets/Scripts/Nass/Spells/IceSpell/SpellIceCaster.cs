using UnityEngine;
using UnityEngine.InputSystem;

public class SpellIceCaster : MonoBehaviour
{
    [Header("Spell prefabs")]
    [SerializeField] private GameObject _iceTelegraphPrefab;
    [SerializeField] private GameObject _iceSpellPrefab;

    [Header("Input")]
    [SerializeField] private InputActionProperty _castActionKey;
    [SerializeField] private KeyCode _castKey = KeyCode.Y;
    [SerializeField] private KeyCode _cancelKey = KeyCode.Mouse1;

    [Header("Settings")]
    [SerializeField] private float _maxRange = 10f;
    [SerializeField] private bool _useNewInputSystem = false;
    [SerializeField] private float _chainHeightOffset = 0.5f;

    [Header("Visual Feedback")]
    [SerializeField] private Color _validColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    [SerializeField] private Color _invalidColor = new Color(1f, 0.2f, 0.2f, 0.5f);
    [SerializeField] private float _chainWidth = 0.2f;

    private enum CastState { Idle, Previewing }
    private CastState _currentState = CastState.Idle;

    private GameObject _currentTelegraph;
    private Material _telegraphMaterial;
    private Camera _mainCamera;
    private Transform _targetEnemy;
    private bool _isValidPosition;
    private LineRenderer _chainLineRenderer;

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
            if (Input.GetKeyDown(_castKey))
            {
                HandleCastInput();
            }
            if (Input.GetKeyDown(_cancelKey)) CancelPreview();
        }

        if (_currentState == CastState.Previewing)
        {
            UpdateTelegraphChain();
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
        if (_iceTelegraphPrefab == null)
        {
            Debug.LogError("ice telegraph prefab is null");
            return;
        }

        _currentState = CastState.Previewing;
        _targetEnemy = FindNearestEnemy();

        if (_targetEnemy == null)
        {
            Debug.LogWarning("no enemy found in range");
            _isValidPosition = false;
            _currentState = CastState.Idle;
            return;
        }

        Vector3 spawnPosition = transform.position + Vector3.up * _chainHeightOffset;
        _currentTelegraph = Instantiate(_iceTelegraphPrefab, spawnPosition, Quaternion.identity);

        MeshRenderer[] meshRenderers = _currentTelegraph.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.enabled = false;
        }

        _chainLineRenderer = _currentTelegraph.GetComponent<LineRenderer>();
        if (_chainLineRenderer == null)
        {
            _chainLineRenderer = _currentTelegraph.AddComponent<LineRenderer>();
        }

        _chainLineRenderer.positionCount = 2;
        _chainLineRenderer.startWidth = _chainWidth;
        _chainLineRenderer.endWidth = _chainWidth;
        _chainLineRenderer.useWorldSpace = true;

        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader != null)
        {
            _telegraphMaterial = new Material(shader);
            _telegraphMaterial.color = _validColor;
            _chainLineRenderer.material = _telegraphMaterial;
        }

        _isValidPosition = true;
    }

    void UpdateTelegraphChain()
    {
        if (_currentTelegraph == null) return;

        _targetEnemy = FindNearestEnemy();

        if (_targetEnemy == null)
        {
            _isValidPosition = false;
            UpdateChainColor();
            return;
        }

        float distanceToEnemy = Vector3.Distance(transform.position, _targetEnemy.position);
        _isValidPosition = distanceToEnemy <= _maxRange;

        Vector3 playerPosition = transform.position + Vector3.up * _chainHeightOffset;
        Vector3 enemyPosition = _targetEnemy.position + Vector3.up * _chainHeightOffset;

        if (_chainLineRenderer != null)
        {
            _chainLineRenderer.SetPosition(0, playerPosition);
            _chainLineRenderer.SetPosition(1, enemyPosition);
        }

        UpdateChainColor();
    }

    void UpdateChainColor()
    {
        Color targetColor = _isValidPosition ? _validColor : _invalidColor;

        if (_chainLineRenderer != null)
        {
            _chainLineRenderer.startColor = targetColor;
            _chainLineRenderer.endColor = targetColor;
        }

        if (_telegraphMaterial != null)
        {
            _telegraphMaterial.color = targetColor;
        }
    }

    public void CastSpell()
    {
        if (_iceSpellPrefab == null)
        {
            Debug.LogError("no ice spell prefab");
            return;
        }

        if (_targetEnemy == null)
        {
            Debug.LogError("No target enemy");
            return;
        }

        if (_currentState != CastState.Previewing || !_isValidPosition) return;

        if (_currentTelegraph != null)
        {
            Destroy(_currentTelegraph);
        }

        Vector3 playerPosition = transform.position + Vector3.up * _chainHeightOffset;
        Vector3 enemyPosition = _targetEnemy.position + Vector3.up * _chainHeightOffset;
        float distance = Vector3.Distance(playerPosition, enemyPosition);

        GameObject iceSpell = Instantiate(_iceSpellPrefab, playerPosition, Quaternion.identity);

        IceSpell spellComponent = iceSpell.GetComponent<IceSpell>();
        if (spellComponent != null)
        {
            spellComponent.ActivateSpell(_targetEnemy, distance, transform);
        }

        _currentState = CastState.Idle;
        _isValidPosition = false;
        _targetEnemy = null;
    }

    Transform FindNearestEnemy()
    {
        Collider[] allColliders = Physics.OverlapSphere(transform.position, _maxRange);

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in allColliders)
        {
            if (col.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = col.transform;
                }
            }
        }

        if (nearest == null)
        {
            Debug.LogWarning("No enemies in range or detected");
        }

        return nearest;
    }

    public void CancelPreview()
    {
        if (_currentState == CastState.Previewing && _currentTelegraph != null)
        {
            Destroy(_currentTelegraph);
            _currentTelegraph = null;
            _currentState = CastState.Idle;
            _isValidPosition = false;
            _targetEnemy = null;
        }
    }
}