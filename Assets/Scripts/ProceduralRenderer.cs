using UnityEngine;

public class ProceduralRenderer : MonoBehaviour
{
    public int numVertices = 0;
    public Material material;

    private Transform transform;

    public void Awake()
    {
        transform = GetComponent<Transform>();
    }

    void OnRenderObject()
    {
        Matrix4x4 mvp = transform.localToWorldMatrix;

        material.SetPass(0);
        material.SetMatrix("mvp", mvp);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, numVertices, 1);
    }
}