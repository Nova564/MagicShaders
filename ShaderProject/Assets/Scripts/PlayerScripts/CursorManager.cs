using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private static CursorManager _instance;
    public static CursorManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("CursorManager");
                _instance = go.AddComponent<CursorManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private int _unlockRequestCount = 0;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RequestUnlock()
    {
        _unlockRequestCount++;
        UnlockCursor();
    }

    public void ReleaseUnlock()
    {
        _unlockRequestCount--;
        if (_unlockRequestCount <= 0)
        {
            _unlockRequestCount = 0;
            LockCursor();
        }
    }

    public void ForceReset()
    {
        _unlockRequestCount = 0;
        LockCursor();
    }
}