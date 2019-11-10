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
        public void MinMaxShader()
        {
	  int width = 8;
	  int height = 4;
	  int depth = 4;
	  int size = width*height*depth;

	  float[] voxels = new float[size];
	  for(int i = 0; i < size; i++) {
	    voxels[i] = 0.0f;
	  }

	  MinMaxPair[] result = poc.computeMinMax(voxels, width, height, depth);

	  foreach(MinMaxPair pair in result) {
	    Assert.AreEqual(pair.min, 0.0f);
	    Assert.AreEqual(pair.max, 0.0f);
	  }
        }
    }
}
