using System.Collections;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class POCTest
    {
	POC poc = Component.FindObjectOfType<POC>();

	private static Vector3Int blockDim = new Vector3Int(8, 4, 4);

        [Test]
        public void MinMaxShaderOfZeroReturnsZero()
        {
	  int width = blockDim.x;
	  int height = blockDim.y;
	  int depth = blockDim.z;
	  int size = width*height*depth;

	  float[] voxels = new float[size];
	  for(int i = 0; i < size; i++) {
	    voxels[i] = 0.0f;
	  }

	  MinMaxPair[] result = poc.computeMinMax(voxels, width, height, depth);

	  Assert.AreEqual(1, result.Length);
	  foreach(MinMaxPair pair in result) {
	    Assert.AreEqual(0.0f, pair.min);
	    Assert.AreEqual(0.0f, pair.max);
	  }
        }

        [Test]
        public void MinMaxShaderWorksForOneBlock()
        {
	  int width = blockDim.x;
	  int height = blockDim.y;
	  int depth = blockDim.z;
	  int size = width*height*depth;

	  float[] voxels = new float[size];
	  for(int i = 0; i < size; i++) {
	    voxels[i] = 0.0f;
	  }

	  voxels[0] = 1.0f;
	  voxels[1] = .5f;
	  voxels[2] = -.1f;
	  voxels[7+3*width+3*width*height] = -.1f;

	  MinMaxPair[] result = poc.computeMinMax(voxels, width, height, depth);

	  Assert.AreEqual(1, result.Length);
	  foreach(MinMaxPair pair in result) {
	    Assert.AreEqual(-.1f, pair.min);
	    Assert.AreEqual(1.0f, pair.max);
	  }
        }

        [Test]
        public void MinMaxShaderWorksForMultipleBlocks()
        {
	  int blockMultiplier = 4;
	  int width = blockDim.x*blockMultiplier;
	  int height = blockDim.y*blockMultiplier;
	  int depth = blockDim.z*blockMultiplier;
	  int size = width*height*depth;

	  float[] voxels = new float[size];
	  for(int i = 0; i < size; i++) {
	    voxels[i] = 0.0f;
	  }

	  MinMaxPair[] expectedValues = new MinMaxPair[blockMultiplier*blockMultiplier*blockMultiplier];

	  for(int i=0; i < expectedValues.Length; i++) {
	    expectedValues[i].min = (float)-i*10;
	    expectedValues[i].max = (float)i*10;
	  }

	  int blockIdx = 0;
	  for(int z = 0; z < depth; z += blockDim.z) {
	    for(int y = 0; y < height; y += blockDim.y) {
	      for(int x = 0; x < width; x += blockDim.x) {
		voxels[x+y*width+z*width*height] = expectedValues[blockIdx].min;
		voxels[(x+blockDim.x-1)+(y+blockDim.y-1)*width+(z+blockDim.z-1)*width*height] = expectedValues[blockIdx].max;
		blockIdx++;
	      }
	    }
	  }

	  MinMaxPair[] result = poc.computeMinMax(voxels, width, height, depth);

	  Assert.AreEqual(expectedValues.Length, result.Length);
	  for(int i = 0; i < result.Length; i++) {
	    Assert.AreEqual(expectedValues[i].min, result[i].min);
	    Assert.AreEqual(expectedValues[i].max, result[i].max);
	  }
        }

        [Test]
        public void MinMaxShaderWorksForMisalignedData()
        {
	  int blockMultiplier = 4;
	  int width = blockDim.x*blockMultiplier+4;
	  int height = blockDim.y*blockMultiplier+1;
	  int depth = blockDim.z*blockMultiplier+2;
	  int size = width*height*depth;

	  float[] voxels = new float[size];
	  for(int i = 0; i < size; i++) {
	    voxels[i] = 0.0f;
	  }

	  MinMaxPair[] expectedValues = new MinMaxPair[Mathf.CeilToInt((float)width/blockDim.x)*Mathf.CeilToInt((float)height/blockDim.y)*Mathf.CeilToInt((float)depth/blockDim.z)];

	  for(int i=0; i < expectedValues.Length; i++) {
	    expectedValues[i].min = (float)-i*10;
	    expectedValues[i].max = (float)i*10;
	  }

	  int blockIdx = 0;
	  for(int z = 0; z < depth; z += blockDim.z) {
	    for(int y = 0; y < height; y += blockDim.y) {
	      for(int x = 0; x < width; x += blockDim.x) {
		voxels[x+y*width+z*width*height] = expectedValues[blockIdx].min;

		int maxBlockX = Math.Min(x+blockDim.x, width)-1;
		int maxBlockY = Math.Min(y+blockDim.y, height)-1;
		int maxBlockZ = Math.Min(z+blockDim.z, depth)-1;
		voxels[maxBlockX+maxBlockY*width+maxBlockZ*width*height] = expectedValues[blockIdx].max;

		blockIdx++;
	      }
	    }
	  }

	  MinMaxPair[] result = poc.computeMinMax(voxels, width, height, depth);

	  Assert.AreEqual(expectedValues.Length, result.Length);
	  for(int i = 0; i < result.Length; i++) {
	    Assert.AreEqual(expectedValues[i].min, result[i].min);
	    Assert.AreEqual(expectedValues[i].max, result[i].max);
	  }
        }

	[Test]
	public void addPaddingWorks() {
	  int width = 123;
	  int height = 498;
	  int depth = 3;
	  int origSize = width*height*depth;
	  int expectedWidth = 128;
	  int expectedHeight = 500;
	  int expectedDepth = 4;
	  int expectedSize = expectedWidth*expectedHeight*expectedDepth;

	  float[] voxels = new float[origSize];

	  poc.addPadding(ref voxels, ref width, ref height, ref depth);

	  Assert.AreEqual(expectedWidth, width);
	  Assert.AreEqual(expectedHeight, height);
	  Assert.AreEqual(expectedDepth, depth);
	  Assert.AreEqual(expectedSize, voxels.Length);
	}

        [Test]
        public void compactActiveBlocksWorksWithSingleZeroBlock()
        {
	  float isoValue = 0.5f;
	  int width = blockDim.x;
	  int height = blockDim.y;
	  int depth = blockDim.z;
	  int size = width*height*depth;

	  float[] voxels = new float[size];
	  for(int i = 0; i < size; i++) {
	    voxels[i] = 0.0f;
	  }

	  int[] result = poc.compactBlockArray(voxels, width, height, depth, isoValue);

	  Assert.AreEqual(1, result.Length);
	  foreach(int index in result) {
	    Assert.AreEqual(-1, index);
	  }
        }

        [Test]
        public void compactActiveBlocksWorksWithPartial()
        {
	  float isoValue = .01f;
	  int blockMultiplier = 4;
	  int width = blockDim.x*blockMultiplier;
	  int height = blockDim.y*blockMultiplier;
	  int depth = blockDim.z*blockMultiplier;
	  int size = width*height*depth;

	  float[] voxels = new float[size];
	  for(int i = 0; i < size; i++) {
	    voxels[i] = 0.0f;
	  }

	  int blockIdx = 0;
	  for(int z = 0; z < depth; z += blockDim.z) {
	    for(int y = 0; y < height; y += blockDim.y) {
	      for(int x = 0; x < width; x += blockDim.x) {
		if (blockIdx%2==0) {
		  voxels[x+y*width+z*width*height] = -(float)blockIdx/10.0f;
		  voxels[(x+blockDim.x-1)+(y+blockDim.y-1)*width+(z+blockDim.z-1)*width*height] = (float)blockIdx/10.0f;
		}
		blockIdx++;
	      }
	    }
	  }

	  int[] compactedBlkArray = poc.compactBlockArray(voxels, width, height, depth, isoValue);

	  Assert.AreEqual(-1, compactedBlkArray[0]);

	  for(int i = 1; i < compactedBlkArray.Length; i++) {
	    Assert.AreEqual(i%2==0 ? i/2 - 1 : -1, compactedBlkArray[i]);
	  }
	}

        [Test]
        public void compactActiveBlocksWorksForMultipleBlocks()
        {
	  float isoValue = .01f;
	  int blockMultiplier = 10;
	  int width = blockDim.x*blockMultiplier;
	  int height = blockDim.y*blockMultiplier;
	  int depth = blockDim.z*blockMultiplier;
	  int size = width*height*depth;

	  float[] voxels = new float[size];
	  for(int i = 0; i < size; i++) {
	    voxels[i] = 0.0f;
	  }

	  int blockIdx = 0;
	  for(int z = 0; z < depth; z += blockDim.z) {
	    for(int y = 0; y < height; y += blockDim.y) {
	      for(int x = 0; x < width; x += blockDim.x) {
		if (blockIdx%2==0) {
		  voxels[x+y*width+z*width*height] = -(float)blockIdx/10.0f;
		  voxels[(x+blockDim.x-1)+(y+blockDim.y-1)*width+(z+blockDim.z-1)*width*height] = (float)blockIdx/10.0f;
		}
		blockIdx++;
	      }
	    }
	  }

	  int[] compactedBlkArray = poc.compactBlockArray(voxels, width, height, depth, isoValue);

	  Array.Sort(compactedBlkArray);

	  int numberOfInvalids = compactedBlkArray.Length/2+1;
	  for(int i = 0; i < compactedBlkArray.Length; i++) {
	    if (i < numberOfInvalids) {
	      Assert.AreEqual(-1, compactedBlkArray[i]);
	    } else {
	      Assert.AreEqual(i - numberOfInvalids, compactedBlkArray[i]);
	    }
	  }
	}

        [Test]
        public void parallelMarchingBlocksWorks()
        {
	  float isoValue = .01f;
	  int blockMultiplier = 1;
	  int width = blockDim.x*blockMultiplier;
	  int height = blockDim.y*blockMultiplier;
	  int depth = blockDim.z*blockMultiplier;
	  int size = width*height*depth;

	  float[] voxels = new float[size];
	  for(int i = 0; i < size; i++) {
	    voxels[i] = i > size/2 ? 0.0f : 1.0f;
	  }

	  /*
	  int blockIdx = 0;
	  for(int z = 0; z < depth; z += blockDim.z) {
	    for(int y = 0; y < height; y += blockDim.y) {
	      for(int x = 0; x < width; x += blockDim.x) {
		if (blockIdx%2==0) {
		  voxels[x+y*width+z*width*height] = -(float)blockIdx/10.0f;
		  voxels[(x+blockDim.x-1)+(y+blockDim.y-1)*width+(z+blockDim.z-1)*width*height] = (float)blockIdx/10.0f;
		}
		blockIdx++;
	      }
	    }
	  } */

	  MinMaxPair[] minMax = poc.computeMinMax(voxels, width, height, depth);
	  int[] compactedBlkArray = poc.compactBlockArray(voxels, width, height, depth, isoValue);

	  Vector3[] vertices;
	  int[] indices;
	  
	  poc.parallelMarchingBlocks(voxels, width, height, depth, isoValue, out vertices, out indices);

	  Debug.Log("voxels: " + string.Join(",", voxels));
	  Debug.Log("minMax: " + string.Join(",", minMax));
	  Debug.Log("compactedBlkArray: " + string.Join(",", compactedBlkArray));
	  Debug.Log("numVertices: " + vertices.Length);
	  Debug.Log("vertices: " + string.Join(",", vertices));
	  Debug.Log("numIndices: " + indices.Length);
	  Debug.Log("indices: " + string.Join(",", indices));
	}
    }
}
