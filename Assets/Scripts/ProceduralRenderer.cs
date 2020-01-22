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
        Matrix4x4 model_matrix = transform.localToWorldMatrix;

        material.SetPass(0);
        material.SetMatrix("model_matrix", model_matrix);
        //reset translation
        model_matrix[0, 3] = 0;
        model_matrix[1, 3] = 0;
        model_matrix[2, 3] = 0;
        model_matrix[3, 3] = 1;
        model_matrix[3, 0] = 0;
        model_matrix[3, 1] = 0;
        model_matrix[3, 2] = 0;
        material.SetMatrix("inv_model_matrix", model_matrix.inverse);
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
