using System.Collections;
using UnityEngine;

public class TrailScript : MonoBehaviour
{
    public float activeTime = 2f;
    private bool isTrailActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    [Header("Mesh")]
    public float meshRefreshRate = 0.1f;
    public Transform positionToSpawn;
    public float meshDestroyDelay = 2f;
    [Header("Shader related")]
    public Material mat;
    public string shaderRef;
    public float shaderRate = 0.1f;
    public float shaderRefreshRate = 0.05f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    public void ActivateTrailExternal(float duration)
    {
        if (!isTrailActive)
        {
            StartCoroutine(ActivateTrail(duration));
        }
    }

    IEnumerator ActivateTrail(float timeActive)
    {
        isTrailActive = true;

        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            if (skinnedMeshRenderers == null)
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject trailObj = new GameObject();
                trailObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);


                MeshRenderer meshRender = trailObj.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = trailObj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                meshFilter.mesh = mesh;
                meshRender.material = mat;

                StartCoroutine(AnimateAlpha(meshRender.material, 0, shaderRate, shaderRefreshRate));

                Destroy(trailObj, meshDestroyDelay);
            }
            yield return new WaitForSeconds(meshRefreshRate);
        }
        isTrailActive = false;
    }

    IEnumerator AnimateAlpha(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnim = mat.GetFloat(shaderRef);

        while (valueToAnim > goal)
        {
            valueToAnim -= rate;
            mat.SetFloat(shaderRef, valueToAnim);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}