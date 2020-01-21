using UnityEngine;

public class ProceduralRenderer : MonoBehaviour
{
    public int numVertices = 0;
    public Material material;

    public ComputeBuffer vertices;
    public ComputeBuffer indices;
    public ComputeBuffer normals;

    void OnRenderObject()
    {
        Matrix4x4 mvp = transform.localToWorldMatrix;

        material.SetPass(0);
        material.SetMatrix("mvp", mvp);
        //material.SetMatrix("normalTransform", transform.localToWorldMatrix.inverse.transpose);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, numVertices, 1);
    }

    public void OnDestroy()
    {
        if (vertices != null)
        {
            vertices.Release();
        }
        if (indices != null)
        {
            indices.Release();
        }
        if (normals != null)
        {
            normals.Release();
        }
    }
}
