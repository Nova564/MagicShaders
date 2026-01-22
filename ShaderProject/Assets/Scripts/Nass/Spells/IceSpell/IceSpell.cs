using UnityEngine;
using System.Collections;
using System;

public class IceSpell : MonoBehaviour
{
    [Header("Ice Settings")]
    [SerializeField] private float _chargeDuration = 2f;
    [SerializeField] private AnimationCurve _chargeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float _damage = 100f;
    [SerializeField] private float _maxRange = 10f;

    [Header("Visual Effects")]
    [SerializeField] private float _maxIntensity = 8f;
    [SerializeField] private float _chainWidth = 0.25f;
    [SerializeField] private GameObject _explosionParticlePrefab;
    [SerializeField] private Material _customIceMaterial;

    private Material _iceMaterial;
    private LineRenderer _chainLineRenderer;
    private Transform _targetEnemy;
    private Transform _casterTransform;
    private float _currentCharge;
    private bool _isActivated;
    private float _chainLength;
    private bool _hasDamaged;

    public event Action<Transform> OnExplode;

    void Awake()
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.enabled = false;
        }

        _chainLineRenderer = GetComponent<LineRenderer>();
        if (_chainLineRenderer == null)
        {
            _chainLineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        _chainLineRenderer.positionCount = 2;
        _chainLineRenderer.startWidth = _chainWidth;
        _chainLineRenderer.endWidth = _chainWidth;
        _chainLineRenderer.useWorldSpace = true;

        if (_customIceMaterial != null)
        {
            _iceMaterial = new Material(_customIceMaterial);
            _chainLineRenderer.material = _iceMaterial;
        }
        else
        {
            Shader shader = Shader.Find("Unlit/IceSpell");
            if (shader == null)
            {
                Debug.LogWarning("Icespell shader not found going default");
                shader = Shader.Find("Unlit/Color");
            }

            if (shader != null)
            {
                _iceMaterial = new Material(shader);
                _chainLineRenderer.material = _iceMaterial;
            }
        }

        if (_iceMaterial != null && _iceMaterial.HasProperty("_FogIntensity"))
        {
            _iceMaterial.SetFloat("_FogIntensity", 0f);
        }
    }

    public void ActivateSpell(Transform targetEnemy, float distance, Transform caster)
    {
        _isActivated = true;
        _targetEnemy = targetEnemy;
        _chainLength = distance;
        _casterTransform = caster;
        _hasDamaged = false;
        StartCoroutine(ChargeUpAnimation());
    }

    private IEnumerator ChargeUpAnimation()
    {
        float elapsed = 0f;

        while (elapsed < _chargeDuration)
        {
            if (_targetEnemy == null || _casterTransform == null)
            {
                Destroy(gameObject);
                yield break;
            }

            float currentDistance = Vector3.Distance(_casterTransform.position, _targetEnemy.position);
            if (currentDistance > _maxRange)
            {
                Destroy(gameObject);
                yield break;
            }

            elapsed += Time.deltaTime;
            _currentCharge = _chargeCurve.Evaluate(elapsed / _chargeDuration);

            UpdateChainPosition();
            UpdateShaderProperties(_currentCharge);

            yield return null;
        }

        _currentCharge = 1f;
        UpdateShaderProperties(1f);
        TriggerExplosion();
        ApplyDamage();

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    void UpdateChainPosition()
    {
        if (_targetEnemy == null || _casterTransform == null) return;

        Vector3 playerPosition = _casterTransform.position + Vector3.up * 0.5f;
        Vector3 enemyPosition = _targetEnemy.position + Vector3.up * 0.5f;

        if (_chainLineRenderer != null)
        {
            _chainLineRenderer.SetPosition(0, playerPosition);
            _chainLineRenderer.SetPosition(1, enemyPosition);
        }
    }

    void UpdateShaderProperties(float charge)
    {
        Color iceColor = Color.Lerp(
            new Color(0.5f, 0.8f, 1f, 0.3f),
            new Color(0.3f, 0.7f, 1f, 0.9f),
            charge
        );

        if (_iceMaterial != null)
        {
            _iceMaterial.color = iceColor;
            if (_iceMaterial.HasProperty("_FogIntensity"))
            {
                float fogIntensity = Mathf.Pow(charge, 2f);
                _iceMaterial.SetFloat("_FogIntensity", fogIntensity);

                float emissionIntensity = Mathf.Lerp(0.5f, _maxIntensity, charge);
                _iceMaterial.SetFloat("_EmissionIntensity", emissionIntensity);
            }
        }

        if (_chainLineRenderer != null)
        {
            _chainLineRenderer.startColor = iceColor;
            _chainLineRenderer.endColor = iceColor;
        }
    }

    void TriggerExplosion()
    {
        if (_targetEnemy == null) return;

        OnExplode?.Invoke(_targetEnemy);

        if (_explosionParticlePrefab != null)
        {
            Vector3 explosionPos = _targetEnemy.position + Vector3.up * 0.5f;
            GameObject particles = Instantiate(_explosionParticlePrefab, explosionPos, Quaternion.identity);

            Destroy(particles, 3f);
        }

        Debug.Log($"Ice spell exploded on {_targetEnemy.name}!");
    }

    void ApplyDamage()
    {
        if (_hasDamaged || _targetEnemy == null) return;

        if (_targetEnemy.CompareTag("Enemy"))
        {
            Debug.Log($"Ice spell hit enemy {_targetEnemy.name} for {_damage} damage");

            // effet de slow à faire
        }

        _hasDamaged = true;
    }

    void OnDestroy()
    {
        if (_iceMaterial != null) DestroyImmediate(_iceMaterial);
    }
}