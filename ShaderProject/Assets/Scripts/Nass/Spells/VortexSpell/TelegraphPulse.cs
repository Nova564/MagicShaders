using UnityEngine;

public class TelegraphPulse : MonoBehaviour
{
    [SerializeField] private float _pulseSpeed = 2f;
    [SerializeField] private float _minScale = 0.9f;
    [SerializeField] private float _maxScale = 1.1f;

    private Vector3 _baseScale;

    void Start()
    {
        _baseScale = transform.localScale;
    }

    void Update()
    {
        float scale = Mathf.Lerp(_minScale, _maxScale, (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f);
        transform.localScale = _baseScale * scale;
    }
}