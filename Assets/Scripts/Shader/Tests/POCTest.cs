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

        [Test]
        public void generateTrianglesWorks()
        {
            float isoValue = .5f;
            int blockMultiplier = 1;
            int width = (blockDim.x - 1) * blockMultiplier + 1;
            int height = (blockDim.y - 1) * blockMultiplier + 1;
            int depth = (blockDim.z - 1) * blockMultiplier + 1;
            int size = width * height * depth;

            float[] voxels = new float[size];
            for (int i = 0; i < size; i++)
            {
                voxels[i] = 0.0f;
            }

            voxels[0] = 1.0f;
            voxels[2] = 1.0f;

            ComputeBuffer minMaxBuffer = new ComputeBuffer(blockMultiplier * blockMultiplier * blockMultiplier, sizeof(float) * 2);
            ComputeBuffer voxelBuffer = new ComputeBuffer(voxels.Length, sizeof(float));
            voxelBuffer.SetData(voxels);

            shader.SetInts("numBlocks", new int[] { blockMultiplier, blockMultiplier, blockMultiplier });
            shader.SetInts("size", new int[] { width, height, depth });
            shader.SetBuffer(minMaxKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(minMaxKernelIndex, "voxelBuffer", voxelBuffer);

            shader.Dispatch(minMaxKernelIndex, blockMultiplier, blockMultiplier, blockMultiplier);


            shader.SetFloat("isoValue", isoValue);

            ComputeBuffer compactedBlkArrayBuffer = new ComputeBuffer(blockMultiplier * blockMultiplier * blockMultiplier, sizeof(int));
            ComputeBuffer activeBlkNumBuffer = new ComputeBuffer(1, sizeof(int));
            activeBlkNumBuffer.SetData(new int[] { 0 });
            shader.SetBuffer(compactActiveBlocksKernelIndex, "minMaxBuffer", minMaxBuffer);
            shader.SetBuffer(compactActiveBlocksKernelIndex, "compactedBlkArray", compactedBlkArrayBuffer);
            shader.SetBuffer(compactActiveBlocksKernelIndex, "activeBlkNum", activeBlkNumBuffer);

            shader.Dispatch(compactActiveBlocksKernelIndex, 1, 1, 1);

            int[] activeBlkNum = new int[1];
            activeBlkNumBuffer.GetData(activeBlkNum);

            ComputeBuffer vertexBuffer = new ComputeBuffer(activeBlkNum[0] * 8 * 4 * 4 * 3, sizeof(float) * 3);
            ComputeBuffer normalBuffer = new ComputeBuffer(activeBlkNum[0] * 8 * 4 * 4 * 3, sizeof(float) * 3);
            ComputeBuffer indexBuffer = new ComputeBuffer(activeBlkNum[0] * 8 * 4 * 4 * 3 * 5, sizeof(int));

            ComputeBuffer globalVertexOffset = new ComputeBuffer(1, sizeof(int));
            ComputeBuffer globalIndexOffset = new ComputeBuffer(1, sizeof(int));

            // These buffers need to be reset because we are only doing InterlockedAdd on both
            globalIndexOffset.SetData(new int[] { 0 });
            globalVertexOffset.SetData(new int[] { 0 });

            ComputeBuffer marchingCubesEdgeTableBuffer = new ComputeBuffer(256*16, sizeof(int));

            int[] table = new int[PMB.marchingCubesEdgeTable.Length];
            int k = 0;
            for (int i = 0; i < PMB.marchingCubesEdgeTable.GetLength(0); i++)
            {
                for (int j = 0; j < PMB.marchingCubesEdgeTable.GetLength(1); j++)
                {
                    table[k++] = PMB.marchingCubesEdgeTable[i, j];
                }
            }

            marchingCubesEdgeTableBuffer.SetData(table);
            shader.SetBuffer(generateTrianglesKernelIndex, "voxelBuffer", voxelBuffer);
            shader.SetBuffer(generateTrianglesKernelIndex, "compactedBlkArray", compactedBlkArrayBuffer);
            shader.SetBuffer(generateTrianglesKernelIndex, "vertexBuffer", vertexBuffer);
            shader.SetBuffer(generateTrianglesKernelIndex, "normalBuffer", normalBuffer);
            shader.SetBuffer(generateTrianglesKernelIndex, "indexBuffer", indexBuffer);
            shader.SetBuffer(generateTrianglesKernelIndex, "globalIndexOffset", globalIndexOffset);
            shader.SetBuffer(generateTrianglesKernelIndex, "globalVertexOffset", globalVertexOffset);
            shader.SetBuffer(generateTrianglesKernelIndex, "marchingCubesEdgeTable", marchingCubesEdgeTableBuffer);

            shader.Dispatch(generateTrianglesKernelIndex, activeBlkNum[0], 1, 1);

            Assert.AreEqual(1, activeBlkNum[0]);
            int[] numIndices = new int[1];
            globalIndexOffset.GetData(numIndices);
            Assert.AreEqual(9, numIndices[0]);

            Vector3[] vertices = new Vector3[vertexBuffer.count];
            vertexBuffer.GetData(vertices);

            int[] indices = new int[indexBuffer.count];
            indexBuffer.GetData(indices);

            Debug.Log("vertices: " + string.Join(",", vertices));
            Debug.Log("indices: " + string.Join(",", indices));

            //without borrowing
            Assert.AreEqual(new Vector3(0.5f, 0, 0), vertices[0]);
            Assert.AreEqual(new Vector3(0, 0.5f, 0), vertices[1]);
            Assert.AreEqual(new Vector3(0, 0, 0.5f), vertices[2]);

            //with borrowing
            Assert.AreEqual(new Vector3(1.5f, 0, 0), vertices[3]);
            Assert.AreEqual(new Vector3(2.5f, 0, 0), vertices[4]);
            Assert.AreEqual(new Vector3(2.0f, 0.5f, 0), vertices[5]);
            Assert.AreEqual(new Vector3(2.0f, 0, 0.5f), vertices[6]);

            //without borrowing
            Assert.AreEqual(0, indices[0]);
            Assert.AreEqual(1, indices[1]);
            Assert.AreEqual(2, indices[2]);

            //with borrowing
            Assert.AreEqual(3, indices[3]);
            Assert.AreEqual(6, indices[4]);
            Assert.AreEqual(5, indices[5]);

            // This test case is only consistent because all the data is generated in one warp.
            // If you add more test cases make sure they all fit within the 32 threads inside a warp.

            minMaxBuffer.Release();
            voxelBuffer.Release();
            compactedBlkArrayBuffer.Release();
            activeBlkNumBuffer.Release();
            marchingCubesEdgeTableBuffer.Release();
            voxelBuffer.Release();
            normalBuffer.Release();
            indexBuffer.Release();
            globalIndexOffset.Release();
            globalVertexOffset.Release();
        }
    }
}
