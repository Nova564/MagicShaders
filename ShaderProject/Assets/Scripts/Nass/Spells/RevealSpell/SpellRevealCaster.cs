using UnityEngine;

public class SpellRevealCaster : MonoBehaviour
{
    [SerializeField] private GameObject _revealSpherePrefab;
    [SerializeField] private float _revealRadius = 5f;
    [SerializeField] private float _revealDuration = 8f;
    [SerializeField] private LayerMask _revealableLayer;

    private GameObject _currentRevealSphere;
    private float _revealEndTime;
    private bool _isRevealing = false;

    public void CastSpell()
    {

        if (_isRevealing)
        {
            DeactivateReveal();
            return;
        }

        Vector3 spawnPosition = transform.position;
        spawnPosition.y += 1f;

        if (_revealSpherePrefab == null)
        {
            Debug.LogError("no revealsphere prefab");
            return;
        }

        _currentRevealSphere = Instantiate(_revealSpherePrefab, spawnPosition, Quaternion.identity);
        _currentRevealSphere.transform.localScale = Vector3.one * _revealRadius * 2f;


        _revealEndTime = Time.time + _revealDuration;
        _isRevealing = true;
    }

    private void Update()
    {
        if (_isRevealing)
        {
            if (_currentRevealSphere != null)
            {
                Vector3 newPos = transform.position + Vector3.up;
                _currentRevealSphere.transform.position = newPos;
            }

            if (Time.time >= _revealEndTime)
            {
                DeactivateReveal();
            }
        }
    }

    public void DeactivateReveal()
    {
        if (_currentRevealSphere != null)
        {
            Destroy(_currentRevealSphere);
        }
        _isRevealing = false;
    }

    private void OnDestroy()
    {
        DeactivateReveal();
    }
}