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

    [Header("Activation")]
    [SerializeField] private float _damagePerSecond = 75f;

    private List<Material> _flameMaterials = new List<Material>();
    private float _currentCharge;
    private bool _isActivated;
    private bool _isLaunched;
    private Vector3 _launchDirection;
    private float _launchTime;

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
    }

    public void ActivateSpell(Vector3 direction)
    {
        _isActivated = true;
        _launchDirection = direction.normalized;
        StartCoroutine(ChargeAndLaunch());
    }

    private IEnumerator ChargeAndLaunch()
    {
        // Phase de charge
        float elapsed = 0f;

        while (elapsed < _chargeUpDuration)
        {
            elapsed += Time.deltaTime;
            _currentCharge = _chargeCurve.Evaluate(elapsed / _chargeUpDuration);
            UpdateShaderProperties(_currentCharge);
            yield return null;
        }

        _currentCharge = 1f;
        UpdateShaderProperties(1f);

        // Lancer le projectile
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

    void Update()
    {
        if (_isLaunched)
        {
            // Déplacer le projectile
            transform.position += _launchDirection * _projectileSpeed * Time.deltaTime;

            // Détruire après le lifetime
            if (Time.time - _launchTime >= _projectileLifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isLaunched && other.CompareTag("Enemy"))
        {
            Debug.Log($"Fireball hit {other.name} for {_projectileDamage} damage");
            // Appliquer les dégâts ici
            // other.GetComponent<EnemyHealth>()?.TakeDamage(_projectileDamage);

            Destroy(gameObject);
        }
        else if (_isLaunched && !other.CompareTag("Player") && !other.isTrigger)
        {
            // Détruire si collision avec un obstacle
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