using UnityEngine;
using System.Collections;

public class VortexSpell : MonoBehaviour
{
    [Header("Charge Settings")]
    [SerializeField] private float _chargeUpDuration = 2f;
    [SerializeField] private AnimationCurve _chargeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Activation")]
    [SerializeField] private float _activationDuration = 5f;
    [SerializeField] private float _damagePerSecond = 50f;

    private Material _groundCrackMaterial;
    private Material _vortexMaterial;
    private Material _energyPillarMaterial;

    private float _currentCharge;
    private bool _isActivated;
    private float _activationTime;

    void Awake()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            string name = child.name.ToLower();
            Material instanceMat = new Material(renderer.sharedMaterial);
            renderer.material = instanceMat;

            if (name.Contains("crack"))
            {
                _groundCrackMaterial = instanceMat;
                _groundCrackMaterial.SetFloat("_Intensity", 0f);
            }
            else if (name.Contains("vortex") || name.Contains("core"))
            {
                _vortexMaterial = instanceMat;
                _vortexMaterial.SetFloat("_ChargeAmount", 0f);
                _vortexMaterial.SetFloat("_EmissionStrength", 3f);
            }
            else if (name.Contains("pillar") || name.Contains("energy"))
            {
                _energyPillarMaterial = instanceMat;
                _energyPillarMaterial.SetFloat("_ChargeAmount", 0f);
                _energyPillarMaterial.SetFloat("_RiseAmount", 0f);
            }
        }
    }

    public void ActivateSpell(float chargeTime)
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
        if (_vortexMaterial != null)
        {
            _vortexMaterial.SetFloat("_ChargeAmount", charge);
            _vortexMaterial.SetFloat("_EmissionStrength", Mathf.Lerp(3f, 15f, charge));
            _vortexMaterial.SetFloat("_VortexSpeed", Mathf.Lerp(1.5f, 4f, charge));
            _vortexMaterial.SetFloat("_SpiralTightness", Mathf.Lerp(3f, 9f, charge));
        }

        if (_groundCrackMaterial != null)
        {
            _groundCrackMaterial.SetFloat("_Intensity", charge);
            _groundCrackMaterial.SetFloat("_CrackGlow", Mathf.Lerp(0.5f, 3f, charge));
        }

        if (_energyPillarMaterial != null)
        {
            _energyPillarMaterial.SetFloat("_ChargeAmount", charge);
            _energyPillarMaterial.SetFloat("_RiseAmount", Mathf.Clamp01(charge * 1.2f));
            _energyPillarMaterial.SetFloat("_PillarIntensity", Mathf.Lerp(1f, 5f, charge));
            _energyPillarMaterial.SetFloat("_RimPower", Mathf.Lerp(4f, 2f, charge));
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
            //système santé à implémenter
        }
    }

    void OnDestroy()
    {
        if (_groundCrackMaterial != null) DestroyImmediate(_groundCrackMaterial);
        if (_vortexMaterial != null) DestroyImmediate(_vortexMaterial);
        if (_energyPillarMaterial != null) DestroyImmediate(_energyPillarMaterial);
    }
}