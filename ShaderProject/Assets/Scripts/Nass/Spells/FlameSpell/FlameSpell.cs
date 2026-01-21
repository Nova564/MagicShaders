using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlameSpell : MonoBehaviour
{
    [Header("Charge Settings")]
    [SerializeField] private float _chargeUpDuration = 1.5f;
    [SerializeField] private AnimationCurve _chargeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Activation")]
    [SerializeField] private float _activationDuration = 3f;
    [SerializeField] private float _damagePerSecond = 75f;

    private List<Material> _flameMaterials = new List<Material>();
    private float _currentCharge;
    private bool _isActivated;
    private float _activationTime;

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

    public void ActivateSpell()
    {
        _isActivated = true;
        _activationTime = Time.time;
        StartCoroutine(ChargeUpAnimation());
    }

    private IEnumerator ChargeUpAnimation()
    {
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
                    material.SetFloat("_EmissionStrength", Mathf.Lerp(1f, 10f, charge));
                }
            }
        }
    }

    void Update()
    {
        if (_isActivated && Time.time - _activationTime >= _chargeUpDuration + _activationDuration)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (_isActivated && _currentCharge >= 1f && other.CompareTag("Enemy"))
        {
            // Système de santé à implémenter
            // damage = damagepersecond * time.deltatime
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