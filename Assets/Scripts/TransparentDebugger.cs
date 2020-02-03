using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentDebugger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ProceduralRenderer renderer = gameObject.AddComponent<ProceduralRenderer>();

        Vector3[] vertices = new Vector3[] {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 100.0f),
            new Vector3(100.0f, 0.0f, 0.0f),
            new Vector3(100.0f, 0.0f, 100.0f),
        };
        Vector3[] normals = new Vector3[] {
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
        };
        int[] indices = new int[] { 0, 1, 2, 2, 1, 3 };
        int[] args = new int[] { 6, 1, 0, 0 };

        RenderBuffers buffers = new RenderBuffers
        {
            vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3),
            indexBuffer = new ComputeBuffer(indices.Length, sizeof(int)),
            normalBuffer = new ComputeBuffer(normals.Length, sizeof(float) * 3),
            argsBuffer = new ComputeBuffer(args.Length, sizeof(int), ComputeBufferType.IndirectArguments),
        };

        buffers.vertexBuffer.SetData(vertices);
        buffers.indexBuffer.SetData(indices);
        buffers.normalBuffer.SetData(normals);
        buffers.argsBuffer.SetData(args);
        Material mat = new Material(Shader.Find("Custom/BufferShader"));

        mat.SetBuffer("vertexBuffer", buffers.vertexBuffer);
        mat.SetBuffer("indexBuffer", buffers.indexBuffer);
        mat.SetBuffer("normalBuffer", buffers.normalBuffer);

        renderer.buffers = buffers;
        renderer.material = mat;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
