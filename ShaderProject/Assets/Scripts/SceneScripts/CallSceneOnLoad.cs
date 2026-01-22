using UnityEngine;
using UnityEngine.SceneManagement;

public class CallSceneOnLoad : MonoBehaviour
{
    [SerializeField] string SceneName;
    [SerializeField] LoadSceneMode Scenemode;
    
    private void Start()
    {
        SwitchSceneManager.Instance.LoadScene(SceneName, Scenemode);
    }
}
