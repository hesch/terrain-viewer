using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MinMaxPair {
  public float min;
  public float max;

  public override string ToString() {
    return string.Format("(min: {0}; max: {1})", min, max);
  }
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
      addPadding(ref voxels, ref width, ref height, ref depth);
      Vector3Int numBlocks = new Vector3Int(width/blockSize.x, height/blockSize.y, depth/blockSize.z);

      ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
      ComputeBuffer minMaxBuffer = new ComputeBuffer(numBlocks.x*numBlocks.y*numBlocks.z, 2*sizeof(float));
      voxelBuffer.SetData(voxels);
      
      int minMaxKernelIndex = POCShader.FindKernel("minMax");
      
      POCShader.SetBuffer(minMaxKernelIndex, "voxelBuffer", voxelBuffer);
      POCShader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
      POCShader.SetInts("size", new int[]{ width, height, depth });
      POCShader.SetInts("numBlocks", new int[]{ numBlocks.x, numBlocks.y, numBlocks.z });

      POCShader.Dispatch(minMaxKernelIndex, numBlocks.x, numBlocks.y, numBlocks.z);

      MinMaxPair[] result = new MinMaxPair[numBlocks.x*numBlocks.y*numBlocks.z];
      minMaxBuffer.GetData(result);

      voxelBuffer.Release();
      minMaxBuffer.Release();

      return result;
    }

    public void addPadding(ref float[] voxels, ref int width, ref int height, ref int depth) {
      int widthRest = width % blockSize.x;
      int heightRest = height % blockSize.y;
      int depthRest = depth % blockSize.z;
      
      if (widthRest == 0 && heightRest == 0 && depthRest == 0) {
	return;
      }

      int newWidth = width + (widthRest != 0 ? blockSize.x - widthRest : 0);
      int newHeight = height + (heightRest != 0 ? blockSize.y - heightRest : 0);
      int newDepth = depth + (depthRest != 0 ? blockSize.z - depthRest : 0);

      float[] resizedArray = new float[newWidth*newHeight*newDepth];

      for(int z = 0; z < depth; z++) {
	for(int y = 0; y < height; y++) {
	  for(int x = 0; x < width; x++) {
	    resizedArray[x+y*newWidth+z*newWidth*newHeight] = voxels[x+y*width+z*width*height];
	  }
	}
      }

      voxels = resizedArray;
      width = newWidth;
      height = newHeight;
      depth = newDepth;
    }

    public int[] compactBlockArray(float[] voxels, int width, int height, int depth, float isoValue) {
      addPadding(ref voxels, ref width, ref height, ref depth);
      Vector3Int numBlocks = new Vector3Int(width/blockSize.x, height/blockSize.y, depth/blockSize.z);

      ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
      ComputeBuffer minMaxBuffer = new ComputeBuffer(numBlocks.x*numBlocks.y*numBlocks.z, 2*sizeof(float));
      ComputeBuffer compactedBlkArray = new ComputeBuffer(numBlocks.x*numBlocks.y*numBlocks.z, sizeof(int));
      ComputeBuffer activeBlkNum = new ComputeBuffer(1, sizeof(int));
      voxelBuffer.SetData(voxels);
      
      int minMaxKernelIndex = POCShader.FindKernel("minMax");
      
      POCShader.SetBuffer(minMaxKernelIndex, "voxelBuffer", voxelBuffer);
      POCShader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
      POCShader.SetInts("size", new int[]{ width, height, depth });
      POCShader.SetInts("numBlocks", new int[]{ numBlocks.x, numBlocks.y, numBlocks.z });

      POCShader.Dispatch(minMaxKernelIndex, numBlocks.x, numBlocks.y, numBlocks.z);

      int compactActiveBlocksKernelIndex = POCShader.FindKernel("compactActiveBlocks");

      POCShader.SetFloat("isoValue", isoValue);
      POCShader.SetBuffer(compactActiveBlocksKernelIndex, "activeBlkNum", activeBlkNum);
      POCShader.SetBuffer(compactActiveBlocksKernelIndex, "minMaxBuffer", minMaxBuffer);
      POCShader.SetBuffer(compactActiveBlocksKernelIndex, "compactedBlkArray", compactedBlkArray);

      POCShader.Dispatch(compactActiveBlocksKernelIndex, 1, 1, 1);

      int[] result = new int[numBlocks.x*numBlocks.y*numBlocks.z];
      compactedBlkArray.GetData(result);

      voxelBuffer.Release();
      minMaxBuffer.Release();
      compactedBlkArray.Release();
      activeBlkNum.Release();

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
