using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class BlockConverterTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void BlockConverterTestSimplePasses()
        {
	  List<int> input = new List<int> { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17 };
	  Debug.Log("stuff");
	  List<(List<int>, int)> output = global::BlockConverter.splitIndices(input, 5);
	  Debug.Log("after run");
	  Assert.That((input, 0), Is.EqualTo((new List<int> {0,1,2}, 0)));
	  Debug.Log("after assert");
            // Use the Assert class to test conditions
        }

        [Test]
        public void BlockConverterTestFails()
        {
	  Assert.Fail();
	}
    }
}
