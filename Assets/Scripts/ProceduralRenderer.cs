using UnityEngine;

public class ProceduralRenderer : MonoBehaviour
{
    public int numVertices = 0;
    private Material material;

    private RenderBuffers _buffers;
    public RenderBuffers buffers {
        set
        {
            disposeBuffers();
            _buffers = value;
            if (_buffers.indexBuffer == null || _buffers.vertexBuffer == null || _buffers.normalBuffer == null)
            {
                Debug.Log("some of the buffers were null while setting");
            }
            if (material != null)
            {
                Vector3[] vertices = new Vector3[_buffers.vertexBuffer.count];
                _buffers.vertexBuffer.GetData(vertices);
                Debug.Log("The vertices:");
                Debug.Log(string.Join(",", vertices));
                material.SetBuffer("vertexBuffer", _buffers.vertexBuffer);
                material.SetBuffer("indexBuffer", _buffers.indexBuffer);
                material.SetBuffer("normalBuffer", _buffers.normalBuffer);
            } else
            {
                Debug.Log("tried setting buffers, but material was null!");
            }
        }
    }
    public Bounds bounds = new Bounds();

    private void Awake()
    {
        material = new Material(Shader.Find("Custom/BufferShader"));
        material.SetBuffer("vertexBuffer", _buffers.vertexBuffer);
        material.SetBuffer("indexBuffer", _buffers.indexBuffer);
        material.SetBuffer("normalBuffer", _buffers.normalBuffer);
    }

    void LateUpdate()
    {
        if (material == null || _buffers.argsBuffer == null)
        {
            Debug.LogWarning("material or argsBuffer null");
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
        Graphics.DrawProceduralIndirect(
            material,
            bounds,
            MeshTopology.Triangles,
            _buffers.argsBuffer,
            0,
            GetComponent<Camera>(),
            new MaterialPropertyBlock(),
            UnityEngine.Rendering.ShadowCastingMode.Off,
            false,
            0
        );
    }

    public void OnDestroy()
    {
        disposeBuffers();
    }

    private void disposeBuffers()
    {
        if (_buffers.vertexBuffer != null)
        {
            _buffers.vertexBuffer.Release();
        }
        if (_buffers.indexBuffer != null)
        {
            _buffers.indexBuffer.Release();
        }
        if (_buffers.normalBuffer != null)
        {
            _buffers.normalBuffer.Release();
        }
        if (_buffers.argsBuffer != null)
        {
            _buffers.argsBuffer.Release();
        }
    }
}
