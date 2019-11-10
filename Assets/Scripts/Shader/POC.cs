using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MinMaxPair {
  public float min;
  public float max;
}

public class POC : MonoBehaviour
{
    public ComputeShader POCShader;
    public Material mat;

    ComputeBuffer vertexBuffer;
    ComputeBuffer indexBuffer;
    int vertexCount = 4;
    int indexCount = 6;

    public static Vector3Int blockSize = new Vector3Int(8, 4, 4);

    // Start is called before the first frame update
    void Start()
    {
	float size = 5.0f;
	vertexBuffer = new ComputeBuffer(vertexCount, 3*sizeof(float));
	indexBuffer = new ComputeBuffer(indexCount, sizeof(int));
    }

    public MinMaxPair[] computeMinMax(float[] voxels, int width, int height, int depth) {
      Vector3Int numBlocks = new Vector3Int(width/blockSize.x, height/blockSize.y, depth/blockSize.z);

      ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
      ComputeBuffer minMaxBuffer = new ComputeBuffer(numBlocks.x*numBlocks.y*numBlocks.z, 2*sizeof(float));
      voxelBuffer.SetData(voxels);
      
      int minMaxKernelIndex = POCShader.FindKernel("minMax");
      
      POCShader.SetBuffer(minMaxKernelIndex, "voxelInput", voxelBuffer);
      POCShader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
      POCShader.SetInts("size", new int[]{ width, height, depth });
      POCShader.SetInts("numBlocks", new int[]{ numBlocks.x, numBlocks.y, numBlocks.z });

      POCShader.Dispatch(minMaxKernelIndex, numBlocks.x, numBlocks.y, numBlocks.z);

      MinMaxPair[] result = new MinMaxPair[numBlocks.x*numBlocks.y*numBlocks.z];
      minMaxBuffer.GetData(result);

      return result;
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
