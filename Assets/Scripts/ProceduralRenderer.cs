using UnityEngine;

public class ProceduralRenderer : MonoBehaviour
{
    public int numVertices = 0;
    public Material material;

    public RenderBuffers buffers;

    void OnRenderObject()
    {
        if (material == null || buffers.argsBuffer == null)
        {
            return;
        }
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
        Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, buffers.argsBuffer, 0);
    }

    public void OnDestroy()
    {
        if (buffers.vertexBuffer != null)
        {
            buffers.vertexBuffer.Release();
        }
        if (buffers.indexBuffer != null)
        {
            buffers.indexBuffer.Release();
        }
        if (buffers.normalBuffer != null)
        {
            buffers.normalBuffer.Release();
        }
        if (buffers.argsBuffer != null)
        {
            buffers.argsBuffer.Release();
        }
    }
}
