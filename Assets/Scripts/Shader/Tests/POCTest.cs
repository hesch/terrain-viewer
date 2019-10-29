using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class POCTest
    {
	POC poc = Component.FindObjectOfType<POC>();

        [Test]
        public void POCTestSimplePasses()
        {
	    float[] output = new float[1024];

	    for(int i = 0; i < output.Length; i++) {
		output[i] = (float)i;
	    }

	    ComputeBuffer buffer = new ComputeBuffer(1024, sizeof(float));
	    buffer.SetData(output);
	    poc.invokeShader("dataTransferTest", buffer);
	    buffer.GetData(output);

	    for(int i = 0; i < output.Length; i++) {
	      Assert.AreEqual(true, output[i] == 1.0f);
	    }

	    buffer.Release();
        }

        [Test]
        public void POCTestMarchingPasses()
        {
	    float[] voxels = new float[512];
	    Vector3[] vertices;
	    int[] indices;

	    for(int i = 0; i < voxels.Length; i++) {
		voxels[i] = (float)i;
	    }

	    poc.computeSurface(voxels, 8, 8, 8, out vertices, out indices);

	    for(int i = 0; i < indices.Length; i++) {
	      Assert.AreEqual(1, indices[i]);
	    }
        }

    }
}
