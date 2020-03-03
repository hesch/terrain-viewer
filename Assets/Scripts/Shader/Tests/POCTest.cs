using NUnit.Framework;
using System;
using UnityEngine;

namespace Tests
{
    public class POCTest
    {
        ComputeShader shader;
        int minMaxKernelIndex;
        int compactActiveBlocksKernelIndex;
        int generateTrianglesKernelIndex;

        private static Vector3Int blockDim = new Vector3Int(8, 4, 4);

        public POCTest()
        {
            PMBShader pmbShader = Component.FindObjectOfType<PMBShader>();
            shader = pmbShader.shaderRef;
            minMaxKernelIndex = shader.FindKernel("minMax");
            compactActiveBlocksKernelIndex = shader.FindKernel("compactActiveBlocks");
            generateTrianglesKernelIndex = shader.FindKernel("generateTriangles");
        }

        [Test]
        public void MinMaxShaderOfZeroReturnsZero()
        {
            int width = blockDim.x;
            int height = blockDim.y;
            int depth = blockDim.z;
            int size = width * height * depth;

            float[] voxels = new float[size];
            for (int i = 0; i < size; i++)
            {
                voxels[i] = 0.0f;
            }

            ComputeBuffer minMaxBuffer = new ComputeBuffer(1, sizeof(float)*2);
            ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
            voxelBuffer.SetData(voxels);

            shader.SetInts("numBlocks", new int[] { 1, 1, 1 });
            shader.SetInts("size", new int[] { width, height, depth });
            shader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(minMaxKernelIndex, "voxelBuffer", voxelBuffer);

            shader.Dispatch(minMaxKernelIndex, 1, 1, 1);

            MinMaxPair[] result = new MinMaxPair[1];
            minMaxBuffer.GetData(result);

            Assert.AreEqual(1, result.Length);
            foreach (MinMaxPair pair in result)
            {
                Assert.AreEqual(0.0f, pair.min);
                Assert.AreEqual(0.0f, pair.max);
            }

            minMaxBuffer.Release();
            voxelBuffer.Release();
        }

        [Test]
        public void MinMaxShaderWorksForOneBlock()
        {
            int width = blockDim.x;
            int height = blockDim.y;
            int depth = blockDim.z;
            int size = width * height * depth;

            float[] voxels = new float[size];
            for (int i = 0; i < size; i++)
            {
                voxels[i] = 0.0f;
            }

            voxels[0] = 1.0f;
            voxels[1] = .5f;
            voxels[2] = -.1f;
            voxels[7 + 3 * width + 3 * width * height] = -.1f;

            ComputeBuffer minMaxBuffer = new ComputeBuffer(1, sizeof(float) * 2);
            ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
            voxelBuffer.SetData(voxels);

            shader.SetInts("numBlocks", new int[] { 1, 1, 1 });
            shader.SetInts("size", new int[] { width, height, depth });
            shader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(minMaxKernelIndex, "voxelBuffer", voxelBuffer);

            shader.Dispatch(minMaxKernelIndex, 1, 1, 1);

            MinMaxPair[] result = new MinMaxPair[1];
            minMaxBuffer.GetData(result);

            Assert.AreEqual(1, result.Length);
            foreach (MinMaxPair pair in result)
            {
                Assert.AreEqual(-.1f, pair.min);
                Assert.AreEqual(1.0f, pair.max);
            }

            minMaxBuffer.Release();
            voxelBuffer.Release();
        }

        [Test]
        public void MinMaxShaderWorksForMultipleBlocks()
        {
            int blockMultiplier = 4;
            int width = (blockDim.x - 1) * blockMultiplier + 1;
            int height = (blockDim.y - 1) * blockMultiplier + 1;
            int depth = (blockDim.z - 1) * blockMultiplier + 1;
            int size = width * height * depth;

            float[] voxels = new float[size];
            for (int i = 0; i < size; i++)
            {
                voxels[i] = 0.0f;
            }

            MinMaxPair[] expectedValues = new MinMaxPair[blockMultiplier * blockMultiplier * blockMultiplier];

            for (int i = 0; i < expectedValues.Length; i++)
            {
                expectedValues[i].min = (float)-i * 10;
                expectedValues[i].max = (float)i * 10;
            }

            int blockIdx = 0;
            for (int z = 1; z < depth - 1; z += blockDim.z - 1)
            {
                for (int y = 1; y < height - 1; y += blockDim.y - 1)
                {
                    for (int x = 1; x < width - 1; x += blockDim.x - 1)
                    {
                        voxels[x + y * width + z * width * height] = expectedValues[blockIdx].min;
                        voxels[(x + blockDim.x - 2) + (y + blockDim.y - 2) * width + (z + blockDim.z - 2) * width * height] = expectedValues[blockIdx].max;
                        blockIdx++;
                    }
                }
            }

            ComputeBuffer minMaxBuffer = new ComputeBuffer(expectedValues.Length, sizeof(float) * 2);
            ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
            voxelBuffer.SetData(voxels);

            shader.SetInts("numBlocks", new int[] { blockMultiplier, blockMultiplier, blockMultiplier });
            shader.SetInts("size", new int[] { width, height, depth });
            shader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(minMaxKernelIndex, "voxelBuffer", voxelBuffer);

            shader.Dispatch(minMaxKernelIndex, blockMultiplier, blockMultiplier, blockMultiplier);

            MinMaxPair[] result = new MinMaxPair[expectedValues.Length];
            minMaxBuffer.GetData(result);

            Assert.AreEqual(expectedValues.Length, result.Length);
            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(expectedValues[i].min, result[i].min);
                Assert.AreEqual(expectedValues[i].max, result[i].max);
            }

            minMaxBuffer.Release();
            voxelBuffer.Release();
        }

        [Test]
        public void MinMaxShaderWorksForMisalignedData()
        {
            int blockMultiplier = 4;
            int width = (blockDim.x - 1) * blockMultiplier + 5;
            int height = (blockDim.y - 1) * blockMultiplier + 2;
            int depth = (blockDim.z - 1) * blockMultiplier + 3;
            int size = width * height * depth;

            float[] voxels = new float[size];
            for (int i = 0; i < size; i++)
            {
                voxels[i] = 0.0f;
            }

            MinMaxPair[] expectedValues = new MinMaxPair[Mathf.CeilToInt((float)width / (blockDim.x - 1)) * Mathf.CeilToInt((float)height / (blockDim.y - 1)) * Mathf.CeilToInt((float)depth / (blockDim.z - 1))];

            for (int i = 0; i < expectedValues.Length; i++)
            {
                expectedValues[i].min = (float)-i * 10;
                expectedValues[i].max = (float)i * 10;
            }

            int blockIdx = 0;
            for (int z = 1; z < depth; z += blockDim.z - 1)
            {
                for (int y = 1; y < height; y += blockDim.y - 1)
                {
                    for (int x = 1; x < width; x += blockDim.x - 1)
                    {
                        voxels[x + y * width + z * width * height] = expectedValues[blockIdx].min;

                        int maxBlockX = Math.Min(x + blockDim.x - 2, width) - 1;
                        int maxBlockY = Math.Min(y + blockDim.y - 2, height) - 1;
                        int maxBlockZ = Math.Min(z + blockDim.z - 2, depth) - 1;
                        voxels[maxBlockX + maxBlockY * width + maxBlockZ * width * height] = expectedValues[blockIdx].max;

                        blockIdx++;
                    }
                }
            }

            ComputeBuffer minMaxBuffer = new ComputeBuffer(expectedValues.Length, sizeof(float) * 2);
            ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
            voxelBuffer.SetData(voxels);

            shader.SetInts("numBlocks", new int[] { blockMultiplier+1, blockMultiplier+1, blockMultiplier+1 });
            shader.SetInts("size", new int[] { width, height, depth });
            shader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(minMaxKernelIndex, "voxelBuffer", voxelBuffer);

            shader.Dispatch(minMaxKernelIndex, blockMultiplier+1, blockMultiplier+1, blockMultiplier+1);

            MinMaxPair[] result = new MinMaxPair[expectedValues.Length];
            minMaxBuffer.GetData(result);

            Assert.AreEqual(expectedValues.Length, result.Length);
            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(expectedValues[i].min, result[i].min);
                Assert.AreEqual(expectedValues[i].max, result[i].max);
            }

            minMaxBuffer.Release();
            voxelBuffer.Release();
        }
        /*
        [Test]
        public void addPaddingWorks()
        {
            int width = 123;
            int height = 498;
            int depth = 3;
            int origSize = width * height * depth;
            int expectedWidth = 127;
            int expectedHeight = 499;
            int expectedDepth = 4;
            int expectedSize = expectedWidth * expectedHeight * expectedDepth;

            float[] voxels = new float[origSize];

            poc.addPadding(ref voxels, ref width, ref height, ref depth);

            Assert.AreEqual(expectedWidth, width);
            Assert.AreEqual(expectedHeight, height);
            Assert.AreEqual(expectedDepth, depth);
            Assert.AreEqual(expectedSize, voxels.Length);
        }*/

        [Test]
        public void compactActiveBlocksWorksWithSingleZeroBlock()
        {
            float isoValue = 0.5f;
            int width = blockDim.x;
            int height = blockDim.y;
            int depth = blockDim.z;
            int size = width * height * depth;

            float[] voxels = new float[size];
            for (int i = 0; i < size; i++)
            {
                voxels[i] = 0.0f;
            }

            ComputeBuffer minMaxBuffer = new ComputeBuffer(1, sizeof(float) * 2);
            ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
            voxelBuffer.SetData(voxels);

            shader.SetInts("numBlocks", new int[] { 1, 1, 1 });
            shader.SetInts("size", new int[] { width, height, depth });
            shader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(minMaxKernelIndex, "voxelBuffer", voxelBuffer);

            shader.Dispatch(minMaxKernelIndex, 1, 1, 1);


            shader.SetFloat("isoValue", isoValue);

            ComputeBuffer compactedBlkArrayBuffer = new ComputeBuffer(1, sizeof(int));
            ComputeBuffer activeBlkNumBuffer = new ComputeBuffer(1, sizeof(int));
            activeBlkNumBuffer.SetData(new int[] { 0 });
            shader.SetBuffer(compactActiveBlocksKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(compactActiveBlocksKernelIndex, "compactedBlkArray", compactedBlkArrayBuffer);
            shader.SetBuffer(compactActiveBlocksKernelIndex, "activeBlkNum", activeBlkNumBuffer);

            shader.Dispatch(compactActiveBlocksKernelIndex, 1, 1, 1);

            int[] activeBlkNum = new int[1];

            activeBlkNumBuffer.GetData(activeBlkNum);

            Assert.AreEqual(0, activeBlkNum[0]);

            activeBlkNumBuffer.Release();
            minMaxBuffer.Release();
            voxelBuffer.Release();
            compactedBlkArrayBuffer.Release();
        }
        
        [Test]
        public void compactActiveBlocksWorksForMultipleBlocks()
        {
            float isoValue = .01f;
            int blockMultiplier = 4;
            int width = (blockDim.x - 1) * blockMultiplier + 1;
            int height = (blockDim.y - 1) * blockMultiplier + 1;
            int depth = (blockDim.z - 1) * blockMultiplier + 1;
            int size = width * height * depth;

            float[] voxels = new float[size];
            for (int i = 0; i < size; i++)
            {
                voxels[i] = 0.0f;
            }

            int blockIdx = 0;
            for (int z = 1; z < depth; z += blockDim.z - 1)
            {
                for (int y = 1; y < height; y += blockDim.y - 1)
                {
                    for (int x = 1; x < width; x += blockDim.x - 1)
                    {
                        if (blockIdx % 2 == 0)
                        {
                            voxels[x + y * width + z * width * height] = -(float)blockIdx / 10.0f;
                            voxels[(x + blockDim.x - 3) + (y + blockDim.y - 3) * width + (z + blockDim.z - 3) * width * height] = (float)blockIdx / 10.0f;
                        }
                        blockIdx++;
                    }
                }
            }

            ComputeBuffer minMaxBuffer = new ComputeBuffer(blockMultiplier*blockMultiplier*blockMultiplier, sizeof(float) * 2);
            ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
            voxelBuffer.SetData(voxels);

            shader.SetInts("numBlocks", new int[] { blockMultiplier, blockMultiplier, blockMultiplier });
            shader.SetInts("size", new int[] { width, height, depth });
            shader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(minMaxKernelIndex, "voxelBuffer", voxelBuffer);

            shader.Dispatch(minMaxKernelIndex, blockMultiplier, blockMultiplier, blockMultiplier);


            shader.SetFloat("isoValue", isoValue);

            ComputeBuffer compactedBlkArrayBuffer = new ComputeBuffer(blockMultiplier*blockMultiplier*blockMultiplier, sizeof(int));
            ComputeBuffer activeBlkNumBuffer = new ComputeBuffer(1, sizeof(int));
            activeBlkNumBuffer.SetData(new int[] { 0 });
            shader.SetBuffer(compactActiveBlocksKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(compactActiveBlocksKernelIndex, "compactedBlkArray", compactedBlkArrayBuffer);
            shader.SetBuffer(compactActiveBlocksKernelIndex, "activeBlkNum", activeBlkNumBuffer);

            shader.Dispatch(compactActiveBlocksKernelIndex, 1, 1, 1);

            int[] activeBlkNum = new int[1];
            int[] compactedBlkArray = new int[blockMultiplier*blockMultiplier*blockMultiplier];

            activeBlkNumBuffer.GetData(activeBlkNum);
            compactedBlkArrayBuffer.GetData(compactedBlkArray);

            // only half of the blocks are active. -1 because first block is filled with 0.
            Assert.AreEqual(blockMultiplier*blockMultiplier*blockMultiplier / 2 - 1, activeBlkNum[0]);

            for (int i = 0; i < activeBlkNum[0]; i++)
            {
                Assert.AreEqual((i+1)*2, compactedBlkArray[i]);
            }

            minMaxBuffer.Release();
            voxelBuffer.Release();
            compactedBlkArrayBuffer.Release();
            activeBlkNumBuffer.Release();
        }

        /*
        [Test]
        public void parallelMarchingBlocksWorks()
        {
            float isoValue = .5f;
            int blockMultiplier = 1;
            int width = (blockDim.x - 1) * (blockMultiplier + 1);
            int height = (blockDim.y - 1) * blockMultiplier + 1;
            int depth = (blockDim.z - 1) * blockMultiplier + 1;
            int size = width * height * depth;

            float[] voxels = new float[size];
            for (int i = 0; i < size; i++)
            {
                voxels[i] = i % 56 < 28 ? 0.0f : 1.0f;
            }

            string voxelString = "";
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        voxelString += voxels[z * width * height + y * width + x] + ",";
                    }
                    voxelString += "\n";
                }
                voxelString += "\n";
            }

            MinMaxPair[] minMax = poc.computeMinMax(voxels, width, height, depth);
            int[] compactedBlkArray = poc.compactBlockArray(voxels, width, height, depth, isoValue);

            Vector3[] vertices;
            Vector3[] normals;
            int[] indices;

            poc.parallelMarchingBlocks(voxels, width, height, depth, isoValue, out vertices, out indices, out normals);

            Debug.Log("voxels: " + voxelString);
            Debug.Log("minMax: " + string.Join(",", minMax));
            Debug.Log("compactedBlkArray: " + string.Join(",", compactedBlkArray));
            Debug.Log("numVertices: " + vertices.Length);
            Debug.Log("vertices: " + string.Join(",", vertices));
            Debug.Log("numIndices: " + indices.Length);
            string indicesString = "";

            for (int z = 0; z < depth + 1; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        indicesString += indices[x + y * width + z * width * height] + ",";
                    }
                    indicesString += "\n";
                }
                indicesString += "\n";
            }
            Debug.Log("indices: " + indicesString);
        }*/
    }
}
