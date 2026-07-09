using UnityEngine;

public class EarthLighting : MonoBehaviour
{
    [SerializeField] private Transform sun;
    private Material mat;
    private static readonly int SunDirID = Shader.PropertyToID("_SunDir");

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();

        if (rend == null)
            rend = GetComponentInParent<Renderer>();

        if (rend == null)
        {
            Debug.LogError($"{name}: no Renderer found on this object or its parent. " +
                           "Disabling EarthLighting.");
            enabled = false;
            return;
        }

        mat = rend.material;

        if (sun == null)
            Debug.LogWarning($"{name}: Sun reference not assigned, day/night shader will not update.");
    }

    void Update()
    {
        if (sun == null || mat == null) return;

        Vector3 sunDir = (sun.position - transform.position).normalized;
        mat.SetVector(SunDirID, sunDir);
    }
}