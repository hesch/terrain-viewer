using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POC : MonoBehaviour
{
    public ComputeShader POCShader;
    public Material mat;

    ComputeBuffer vertexBuffer;
    ComputeBuffer indexBuffer;
    int vertexCount = 4;
    int indexCount = 6;

    // Start is called before the first frame update
    void Start()
    {
	float size = 5.0f;
	vertexBuffer = new ComputeBuffer(vertexCount, 3*sizeof(float));
	indexBuffer = new ComputeBuffer(indexCount, sizeof(int));

	float[] points = new float[vertexCount*3];
	int[] indices = new int[indexCount];
   
	vertexBuffer.SetData(points);
	indexBuffer.SetData(indices);

	int kernelIndex = POCShader.FindKernel("renderTest");
	POCShader.SetBuffer(kernelIndex, "testVertex", vertexBuffer);
	POCShader.SetBuffer(kernelIndex, "testIndex", indexBuffer);
	POCShader.Dispatch(kernelIndex, 1, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float reduce1(float[] input) {
	int kernelIndex = POCShader.FindKernel("reduce1");
	ComputeBuffer buffer = new ComputeBuffer(input.Length, sizeof(float));
	buffer.SetData(input);
	POCShader.SetBuffer(kernelIndex, "g_data", buffer);
	POCShader.Dispatch(kernelIndex, 1, 1, 1);
	buffer.GetData(input);
	buffer.Release();
	return input[0];
    }


    public ComputeBuffer invokeShader(string name, ComputeBuffer input) {
	int kernelIndex = POCShader.FindKernel(name);
	POCShader.SetBuffer(kernelIndex, "input", input);
	POCShader.Dispatch(kernelIndex, 32, 32, 1);
	return input;
    }

    public void computeSurface(float[] voxels, int width, int height, int depth, out Vector3[] vertices, out int[] indices) {
      int kernelIndex = POCShader.FindKernel("MarchingCubes");
      ComputeBuffer buffer = new ComputeBuffer(voxels.Length, sizeof(float));
      ComputeBuffer vertexBuffer = new ComputeBuffer(voxels.Length, sizeof(float)*3);
      ComputeBuffer indexBuffer = new ComputeBuffer(voxels.Length*3, sizeof(int));
      buffer.SetData(voxels);

      POCShader.SetBuffer(kernelIndex, "voxels", buffer);
      POCShader.SetBuffer(kernelIndex, "vertices", vertexBuffer);
      POCShader.SetBuffer(kernelIndex, "indices", indexBuffer);
      POCShader.Dispatch(kernelIndex, width, height, depth);

      vertices = new Vector3[voxels.Length];
      float[] vertexOutput = new float[voxels.Length*3];
      vertexBuffer.GetData(vertexOutput);
      for(int i = 0; i < vertices.Length; i++) {
	vertices[i] = new Vector3(vertexOutput[i*3], vertexOutput[i*3+1], vertexOutput[i*3+2]);
      }

      indices = new int[voxels.Length*3];
      indexBuffer.GetData(indices);

      buffer.Release();
      vertexBuffer.Release();
      indexBuffer.Release();
    }

    void OnPostRender()
    {
        mat.SetPass(0);
        mat.SetBuffer("vertexBuffer", vertexBuffer);
	mat.SetBuffer("indexBuffer", indexBuffer);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, indexCount, 1);
    }
 
    void OnDestroy()
    {
        vertexBuffer.Release();
	indexBuffer.Release();
    }

}
