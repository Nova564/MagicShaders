using UnityEngine;
using System.Collections;

public class LightningSpell : MonoBehaviour
{
    [Header("Lightning Settings")]
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private float _damage = 150f;

    [Header("Visual Effects")]
    [SerializeField] private float _maxIntensity = 10f;
    [SerializeField] private float _flickerSpeed = 20f;

    private Material _lightningMaterial;
    private float _activationTime;
    private bool _isActivated;
    private bool _hasDamaged;
    private float _currentLength;

    void Awake()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            Material instanceMat = new Material(renderer.sharedMaterial);
            renderer.material = instanceMat;
            _lightningMaterial = instanceMat;

            if (_lightningMaterial.HasProperty("_Intensity"))
            {
                _lightningMaterial.SetFloat("_Intensity", _maxIntensity);
            }
            if (_lightningMaterial.HasProperty("_EmissionStrength"))
            {
                _lightningMaterial.SetFloat("_EmissionStrength", _maxIntensity);
            }
        }
    }

    public void ActivateSpell(float length)
    {
        _isActivated = true;
        _activationTime = Time.time;
        _currentLength = length;
        _hasDamaged = false;
        StartCoroutine(FadeOutAnimation());
        StartCoroutine(DamageEnemies());
    }

    private IEnumerator FadeOutAnimation()
    {
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float fadeValue = _fadeCurve.Evaluate(elapsed / _fadeDuration);

            float flicker = Mathf.PerlinNoise(Time.time * _flickerSpeed, 0f);
            float intensity = fadeValue * _maxIntensity * (0.8f + flicker * 0.2f);

            UpdateShaderProperties(intensity);
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator DamageEnemies()
    {
        yield return new WaitForSeconds(0.05f);

        if (!_hasDamaged)
        {
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, _currentLength);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    // santé système dégat unique au contact
                    Debug.Log($"Lightning hit enemy {hit.collider.name} for {_damage} damage");
                }
            }

            _hasDamaged = true;
        }
    }

    void UpdateShaderProperties(float intensity)
    {
        if (_lightningMaterial != null)
        {
            if (_lightningMaterial.HasProperty("_Intensity"))
            {
                _lightningMaterial.SetFloat("_Intensity", intensity);
            }
            if (_lightningMaterial.HasProperty("_EmissionStrength"))
            {
                _lightningMaterial.SetFloat("_EmissionStrength", intensity);
            }
            if (_lightningMaterial.HasProperty("_Alpha"))
            {
                _lightningMaterial.SetFloat("_Alpha", intensity / _maxIntensity);
            }
        }
    }

    void OnDestroy()
    {
        if (_lightningMaterial != null) DestroyImmediate(_lightningMaterial);
    }
}