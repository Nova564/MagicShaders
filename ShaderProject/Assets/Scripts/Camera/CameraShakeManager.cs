using Unity.Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera _virtualCamera;

    private CinemachineBasicMultiChannelPerlin _perlinNoise;
    private Coroutine _shakeCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_virtualCamera != null)
        {
            _perlinNoise = _virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

            if (_perlinNoise == null)
            {
                Debug.LogError("Multichannel perlin not found in virtual cam");
            }
            else
            {
                _perlinNoise.AmplitudeGain = 0f;
                _perlinNoise.FrequencyGain = 0f;
            }
        }
        else
        {
            Debug.LogError("virtualcam not assigned");
        }
    }

    void Start()
    {
        StopShake();
    }

    public void ShakeCamera(float intensity, float duration)
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }
        _shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration, null));
    }

    public void ShakeCamera(float intensity, float duration, AnimationCurve curve)
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }
        _shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration, curve));
    }

    public void ShakeQuick(float intensity = 3f, float duration = 0.2f)
    {
        AnimationCurve quickCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        ShakeCamera(intensity, duration, quickCurve);
    }

    public void ShakeHeavy(float intensity = 5f, float duration = 0.5f)
    {
        AnimationCurve heavyCurve = new AnimationCurve(
            new Keyframe(0, 1),
            new Keyframe(0.1f, 1),
            new Keyframe(1, 0)
        );
        ShakeCamera(intensity, duration, heavyCurve);
    }

    public void ShakeExplosion(float intensity = 8f, float duration = 0.8f)
    {
        AnimationCurve explosionCurve = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(0.1f, 1),
            new Keyframe(0.5f, 0.5f),
            new Keyframe(1, 0)
        );
        ShakeCamera(intensity, duration, explosionCurve);
    }

    public void StopShake()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null; 
        }

        if (_perlinNoise != null)
        {
            _perlinNoise.AmplitudeGain = 0f;
            _perlinNoise.FrequencyGain = 0f;
        }
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration, AnimationCurve curve)
    {
        if (_perlinNoise == null)
        {
            Debug.LogWarning("perlin null");
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float currentIntensity = intensity;
            if (curve != null)
            {
                currentIntensity *= curve.Evaluate(progress);
            }

            _perlinNoise.AmplitudeGain = currentIntensity;
            _perlinNoise.FrequencyGain = currentIntensity;

            yield return null;
        }
        _perlinNoise.AmplitudeGain = 0f;
        _perlinNoise.FrequencyGain = 0f;
        _shakeCoroutine = null; 
    }

    public void SetVirtualCamera(CinemachineCamera newCamera)
    {
        _virtualCamera = newCamera;
        if (_virtualCamera != null)
        {
            _perlinNoise = _virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (_perlinNoise != null)
            {
                _perlinNoise.AmplitudeGain = 0f;
                _perlinNoise.FrequencyGain = 0f;
            }
        }
    }
    void OnApplicationQuit()
    {
        StopShake();
    }
}