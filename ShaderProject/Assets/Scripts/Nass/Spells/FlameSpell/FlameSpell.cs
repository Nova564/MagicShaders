using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlameSpell : MonoBehaviour
{
    [Header("Charge Settings")]
    [SerializeField] private float _chargeUpDuration = 1.5f;
    [SerializeField] private AnimationCurve _chargeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Projectile Settings")]
    [SerializeField] private float _projectileSpeed = 20f;
    [SerializeField] private float _projectileLifetime = 5f;
    [SerializeField] private float _projectileDamage = 150f;

    [Header("Camera Shake - Charge")]
    [SerializeField] private bool _enableChargeShake = true;
    [SerializeField] private float _chargeShakeIntensity = 1.5f;
    [SerializeField] private AnimationCurve _chargeShakeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Camera Shake - Launch")]
    [SerializeField] private bool _enableLaunchShake = true;
    [SerializeField] private float _launchShakeIntensity = 4f;
    [SerializeField] private float _launchShakeDuration = 0.3f;

    [Header("Camera Shake - Hit")]
    [SerializeField] private bool _enableHitShake = true;
    [SerializeField] private float _hitShakeIntensity = 6f;
    [SerializeField] private float _hitShakeDuration = 0.4f;
    [SerializeField] private AnimationCurve _hitShakeCurve;

    [Header("Activation")]
    [SerializeField] private float _damagePerSecond = 75f;

    [Header("Point Light")]
    [SerializeField] private Light _pointLight;
    [SerializeField] private float _minLightIntensity = 0f;
    [SerializeField] private float _maxLightIntensity = 100f;
    [SerializeField] private AnimationCurve _lightIntensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    private List<Material> _flameMaterials = new List<Material>();
    private float _currentCharge;
    private bool _isActivated;
    private bool _isLaunched;
    private Vector3 _launchDirection;
    private float _launchTime;
    private Collider _projectileCollider;

    void Awake()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            Material instanceMat = new Material(renderer.sharedMaterial);
            renderer.material = instanceMat;
            _flameMaterials.Add(instanceMat);

            if (instanceMat.HasProperty("_Intensity"))
            {
                instanceMat.SetFloat("_Intensity", 0f);
            }
            if (instanceMat.HasProperty("_ChargeAmount"))
            {
                instanceMat.SetFloat("_ChargeAmount", 0f);
            }
        }

        if (_hitShakeCurve == null || _hitShakeCurve.length == 0)
        {
            _hitShakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        }
        _projectileCollider = GetComponent<Collider>();
        if (_projectileCollider == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 0.5f;
            _projectileCollider = sphere;
        }
        else
        {
            _projectileCollider.isTrigger = true;

        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;


        }

        if (_pointLight == null)
        {
            _pointLight = GetComponentInChildren<Light>();
        }

        if (_pointLight != null)
        {
            _pointLight.intensity = _minLightIntensity;
        }
    }

    public void ActivateSpell(Vector3 direction)
    {
        _isActivated = true;
        _launchDirection = direction.normalized;

        StartCoroutine(ChargeAndLaunch());
    }

    private IEnumerator ChargeAndLaunch()
    {
        if (_enableChargeShake && CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeCamera(_chargeShakeIntensity, _chargeUpDuration, _chargeShakeCurve);
        }

        float elapsed = 0f;

        while (elapsed < _chargeUpDuration)
        {
            elapsed += Time.deltaTime;
            _currentCharge = _chargeCurve.Evaluate(elapsed / _chargeUpDuration);
            UpdateShaderProperties(_currentCharge);
            UpdatePointLight(_currentCharge);
            yield return null;
        }

        _currentCharge = 1f;
        UpdateShaderProperties(1f);
        UpdatePointLight(1f);
        if (_enableChargeShake && CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.StopShake();
        }
        if (_enableLaunchShake && CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeHeavy(_launchShakeIntensity, _launchShakeDuration);
        }
        LaunchProjectile();
    }

    void LaunchProjectile()
    {
        _isLaunched = true;
        _launchTime = Time.time;
    }

    void UpdateShaderProperties(float charge)
    {
        foreach (Material material in _flameMaterials)
        {
            if (material != null)
            {
                if (material.HasProperty("_ChargeAmount"))
                {
                    material.SetFloat("_ChargeAmount", charge);
                }
                if (material.HasProperty("_Intensity"))
                {
                    material.SetFloat("_Intensity", Mathf.Lerp(0f, 5f, charge));
                }
                if (material.HasProperty("_EmissionStrength"))
                {
                    material.SetFloat("_EmissionStrength", Mathf.Lerp(1f, 15f, charge));
                }
            }
        }
    }

    void UpdatePointLight(float charge)
    {
        if (_pointLight != null)
        {
            float curveValue = _lightIntensityCurve.Evaluate(charge);
            _pointLight.intensity = Mathf.Lerp(_minLightIntensity, _maxLightIntensity, curveValue);
        }
    }

    void Update()
    {
        if (_isLaunched)
        {
            transform.position += _launchDirection * _projectileSpeed * Time.deltaTime;
            if (Time.time - _launchTime >= _projectileLifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {

        if (!_isLaunched)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {

            if (_enableHitShake && CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeCamera(_hitShakeIntensity, _hitShakeDuration, _hitShakeCurve);
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            return;
        }
        else if (!other.isTrigger)
        {

            if (_enableHitShake && CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeQuick(_hitShakeIntensity * 0.4f, _hitShakeDuration * 0.5f);
            }

            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        foreach (Material material in _flameMaterials)
        {
            if (material != null) DestroyImmediate(material);
        }
        _flameMaterials.Clear();
    }
}