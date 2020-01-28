using MarchingCubesProject;
using ProceduralNoiseProject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

struct PerfData
{
    public string testName;
    public TestSize[] sizes;
}

struct TestSize
{
    public string sizeName;
    public int[] cpuTime;
    public int[] pmbTime;
}

enum Tests
{
    Checkerboard,
    Empty,
    SimpleNoise,
}

class PerformanceComparison : MonoBehaviour
{
    private const int ROUNDS = 2;
    Stopwatch globalStopwatch;
    ComputeShader shader;
    PMB pmb;

    Marching marching = new MarchingCubes();

    PerfData[] perfData;

    Vector3Int[] testSizes = new Vector3Int[]
    {
        new Vector3Int(64, 64, 64),
        new Vector3Int(128, 64, 64),
        new Vector3Int(128, 128, 64),
        new Vector3Int(128, 128, 128),
        new Vector3Int(256, 256, 256),
        new Vector3Int(512, 512, 512)
    };

    bool[] finished;

    private void Awake()
    {
        int numTests = Enum.GetNames(typeof(Tests)).Length;
        perfData = new PerfData[numTests];
        finished = new bool[numTests];

        shader = FindObjectOfType<PMBShader>().shaderRef;
        pmb = new PMB(shader);
        globalStopwatch = Stopwatch.StartNew();

        Noise n = new PerlinNoise(42, 1.2f);
        UnityEngine.Debug.Log("Starting Coroutines");
        StartCoroutine(genericTest(Tests.Checkerboard, "checkerboard", 1, 3, (s, x, y, z) => (x + y + z) % 2));
        StartCoroutine(genericTest(Tests.Empty, "empty", 3, 3, (s, x, y, z) => 0));
        StartCoroutine(genericTest(Tests.SimpleNoise, "simple-noise", 2, 3, (s, x, y, z) => n.Sample3D(x, y, z)));
    }

    public void OnDestroy()
    {
        pmb.Dispose();
    }

    void finishTesting()
    {
        UnityEngine.Debug.LogFormat("Finished Test in {0}ms", globalStopwatch.ElapsedMilliseconds);
        globalStopwatch.Stop();
        pmb.Dispose();
        exportCSV();
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public IEnumerator<object> genericTest(Tests type, string name, int cpuLimit, int pmbLimit, Func<Vector3Int, int, int, int, float> voxelGenerator)
    {
        int CPU_LIMIT = cpuLimit;
        int PMB_LIMIT = pmbLimit;
        UnityEngine.Debug.Log("Starting Test " + name);
        perfData[(int)type] = new PerfData
        {
            testName = name,
            sizes = new TestSize[testSizes.Length],
        };

        int limitCounter = 0;
        for (int i = 0; i < testSizes.Length; i++)
        {
            Vector3Int size = (Vector3Int)testSizes[i];
            UnityEngine.Debug.Log("size: " + size);
            perfData[(int)type].sizes[i] = new TestSize
            {
                sizeName = string.Format("({0}, {1}, {2})", size.x, size.y, size.z),
                cpuTime = new int[ROUNDS],
                pmbTime = new int[ROUNDS],
            };
            float[] voxel = new float[size.x * size.y * size.z];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        voxel[z * size.x * size.y + y * size.x + x] = voxelGenerator(size, x, y, z);
                    }
                }
            }

            for (int round = 0; round < ROUNDS; round++)
            {
                Stopwatch watch = Stopwatch.StartNew();

                if (limitCounter < CPU_LIMIT)
                {
                    List<Vector3> verts = new List<Vector3>();
                    List<int> indices = new List<int>();
                    List<Vector3> normals = new List<Vector3>();
                    marching.Generate(voxel, size.x, size.y, size.z, verts, indices, normals);

                    UnityEngine.Debug.LogFormat("\tCPU took {0}ms", watch.ElapsedMilliseconds);
                    perfData[(int)type].sizes[i].cpuTime[round] = (int)watch.ElapsedMilliseconds;

                    watch.Restart();
                }
                else
                {
                    perfData[(int)type].sizes[i].cpuTime[round] = -1;
                }

                if (limitCounter < PMB_LIMIT)
                {
                    VoxelBlock<Voxel> block = InitBlock(size);

                    pmb.ReInit(block);
                    RenderBuffers buffers = pmb.calculate(voxel, size.x, size.y, size.z, 0.5f);

                    watch.Stop();
                    buffers.vertexBuffer.Dispose();
                    buffers.indexBuffer.Dispose();
                    buffers.normalBuffer.Dispose();
                    UnityEngine.Debug.LogFormat("\tPMB took {0}ms", watch.ElapsedMilliseconds);
                    perfData[(int)type].sizes[i].pmbTime[round] = (int)watch.ElapsedMilliseconds;
                }
                else
                {
                    perfData[(int)type].sizes[i].pmbTime[round] = -1;
                    watch.Stop();
                }

                yield return null;
            }
            limitCounter++;
        }

        finished[(int)type] = true;
        if (finished.All(f => f))
        {
            finishTesting();
        }
    }

    VoxelBlock<Voxel> InitBlock(Vector3Int size)
    {
        VoxelBlock<Voxel> block = new VoxelBlock<Voxel>();
        VoxelLayer<Voxel>[] layers = new VoxelLayer<Voxel>[size.y];
        for (int y = 0; y < size.y; y++)
        {
            Voxel[,] voxelLayer = new Voxel[size.x, size.z];
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    voxelLayer[x, z] = new Voxel();
                }
            }
            layers[y] = new VoxelLayer<Voxel>(voxelLayer);
        }
        block.Layers = layers;
        return block;
    }

    private void exportCSV()
    {
        StreamWriter file = new StreamWriter("performance.csv");

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("Test-Title");
        StringBuilder cpuHeader = new StringBuilder();
        StringBuilder pmbHeader = new StringBuilder();
        for (int i = 0; i < ROUNDS; i++)
        {
            cpuHeader.AppendFormat(",CPU-R{0}", i);
            pmbHeader.AppendFormat(",PMB-R{0}", i);
        }
        stringBuilder.Append(cpuHeader);
        stringBuilder.Append(pmbHeader);

        foreach (PerfData data in perfData)
        {
            foreach (TestSize size in data.sizes)
            {
                stringBuilder.AppendFormat("\n{0}{1},{2},{3}", data.testName, size.sizeName, string.Join(",", size.cpuTime), string.Join(",", size.pmbTime));
            }
        }
        file.WriteLine(stringBuilder.ToString());
        file.Flush();
        file.Close();
    }
}
