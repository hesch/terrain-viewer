using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POC : MonoBehaviour
{
    public ComputeShader POCShader;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
