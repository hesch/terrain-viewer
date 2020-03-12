using MarchingCubesProject;
using ProceduralNoiseProject;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

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
    public int cpuTriangles;
    public int pmbTriangles;
}

enum Tests
{
    Checkerboard,
    Empty,
    SimpleNoise,
}

class PerformanceComparison : MonoBehaviour
{
    private static string performanceDataDir = Path.Combine(Directory.GetCurrentDirectory(), "performance_data");
    private const int ROUNDS = 5;
    Stopwatch globalStopwatch;
    ComputeShader shader;
    PMB pmb;
    MainThreadHelper helper;

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

    static bool finished = false;

    private void Awake()
    {
        Directory.CreateDirectory(performanceDataDir);
        if (finished)
        {
            return;
        }
        int numTests = Enum.GetNames(typeof(Tests)).Length;
        perfData = new PerfData[numTests];

        shader = FindObjectOfType<PMBShader>().shaderRef;
        pmb = new PMB(shader);
        globalStopwatch = Stopwatch.StartNew();

        helper = FindObjectOfType<MainThreadHelper>();

        Noise n = new PerlinNoise(42, 5f);
        UnityEngine.Debug.Log("Starting Task");
        Task.Factory.StartNew(() => {
            if (finished)
            {
                return;
            }
            var checkerboard = genericTest(Tests.Checkerboard, "checkerboard", 2, 5, (s, x, y, z) => (x + y + z) % 2);
            while (checkerboard.MoveNext());
            var empty = genericTest(Tests.Empty, "empty", 5, 6, (s, x, y, z) => 0);
            while (empty.MoveNext());
            var simpleNoise = genericTest(Tests.SimpleNoise, "simple-noise", 5, 5, (s, x, y, z) => n.Sample3D(x, y, z));
            while (simpleNoise.MoveNext());
            UnityEngine.Debug.Log("finish testing");
            finishTesting();
        });
    }

    public void Update()
    {
        if (finished)
        {
            UnityEngine.Debug.LogWarning("still trying to quit");
            helper.cancelAllPendingTasks();
#if (UNITY_EDITOR)
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        }
    }

    public void OnDestroy()
    {
        pmb.Dispose();
    }

    void finishTesting()
    {
        exportCSV();
        helper.scheduleOnMainThread(() =>
        {
            UnityEngine.Debug.LogFormat("Finished Test in {0}ms", globalStopwatch.ElapsedMilliseconds);
            globalStopwatch.Stop();
            pmb.Dispose();
#if (UNITY_EDITOR)
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        });

        finished = true;
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
                cpuTriangles = 0,
                pmbTriangles = 0,
            };


            float[] voxel;

            string path = Path.Combine(performanceDataDir, string.Format("{0}{1}.dat", name, perfData[(int)type].sizes[i].sizeName));
            if (File.Exists(path))
            {
                UnityEngine.Debug.Log("loading voxelData from file:");
                Stream voxelStream = File.OpenRead(path);
                BinaryFormatter deserializer = new BinaryFormatter();
                voxel = (float[])deserializer.Deserialize(voxelStream);
                voxelStream.Close();
            }
            else
            {
                UnityEngine.Debug.Log("recreating voxelData:");
                voxel = new float[size.x * size.y * size.z];
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
                Stream SaveFileStream = File.Create(path);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(SaveFileStream, voxel);
                SaveFileStream.Close();
            }
            
            UnityEngine.Debug.Log("executing Test:");

            for (int round = 0; round < ROUNDS; round++)
            {
                Stopwatch watch = Stopwatch.StartNew();

                if (limitCounter < CPU_LIMIT)
                {
                    List<Vector3> verts = new List<Vector3>();
                    List<int> indices = new List<int>();
                    List<Vector3> normals = new List<Vector3>();
                    marching.Generate(voxel, size.x, size.y, size.z, verts, indices, normals);

                    watch.Stop();

                    UnityEngine.Debug.LogFormat("\tCPU took {0}ms", watch.ElapsedMilliseconds);
                    perfData[(int)type].sizes[i].cpuTime[round] = (int)watch.ElapsedMilliseconds;
                    perfData[(int)type].sizes[i].cpuTriangles = indices.Count / 3;
                }
                else
                {
                    UnityEngine.Debug.Log("\tCPU skipped");
                    perfData[(int)type].sizes[i].cpuTime[round] = -1;
                }

                if (limitCounter < PMB_LIMIT)
                {
                    VoxelBlock<Voxel> block = InitBlock(size);
                    UnityEngine.Debug.Log(voxel.Length + " " + voxel[0] + " " + voxel[1] + " " + voxel[2] + " " + voxel[3] + " " + voxel[4] + " " + voxel[5] + " " + voxel[6] + " " + voxel[7]);
                    helper.scheduleOnMainThread(() =>
                    {
                        watch = Stopwatch.StartNew();
                        pmb.ReInit(block);
                        RenderBuffers buffers = pmb.calculate(voxel, size.x, size.y, size.z, 0.5f);

                        int[] args = new int[4];
                        buffers.argsBuffer.GetData(args);
                        watch.Stop();

                        UnityEngine.Debug.Log("args: " + string.Join(",", args));

                        UnityEngine.Debug.LogFormat("\tPMB took {0}ms", watch.ElapsedMilliseconds);
                        perfData[(int)type].sizes[i].pmbTime[round] = (int)watch.ElapsedMilliseconds;
                        perfData[(int)type].sizes[i].pmbTriangles = args[0] / 3;

                        buffers.vertexBuffer.Dispose();
                        buffers.indexBuffer.Dispose();
                        buffers.normalBuffer.Dispose();
                        buffers.argsBuffer.Dispose();

                        UnityEngine.Debug.Log("PMB triags inside thread: " + perfData[(int)type].sizes[i].pmbTriangles);
                    }).wait();
                    UnityEngine.Debug.Log("PMB triags after thread: " + perfData[(int)type].sizes[i].pmbTriangles);
                }
                else
                {
                    UnityEngine.Debug.Log("\tPMB skipped");
                    perfData[(int)type].sizes[i].pmbTime[round] = -1;
                    watch.Stop();
                }

                yield return null;
            }
            limitCounter++;
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
        UnityEngine.Debug.Log("exporting CSV");
        StreamWriter file = new StreamWriter("performance.csv");

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("Test-Title");
        StringBuilder cpuHeader = new StringBuilder();
        StringBuilder pmbHeader = new StringBuilder();
        cpuHeader.Append(";CPU-numTris");
        pmbHeader.Append(";PMB-numTris");
        for (int i = 0; i < ROUNDS; i++)
        {
            cpuHeader.AppendFormat(";CPU-R{0}", i);
            pmbHeader.AppendFormat(";PMB-R{0}", i);
        }
        stringBuilder.Append(cpuHeader);
        stringBuilder.Append(pmbHeader);

        foreach (PerfData data in perfData)
        {
            foreach (TestSize size in data.sizes)
            {
                stringBuilder.AppendFormat("\n{0}{1};{2};{3};{4};{5}", data.testName, size.sizeName, size.cpuTriangles, string.Join(";", size.cpuTime), size.pmbTriangles, string.Join(";", size.pmbTime));
            }
        }

        UnityEngine.Debug.Log("write call:");
        file.WriteLine(stringBuilder.ToString());
        UnityEngine.Debug.Log("flush call:");
        file.Flush();
        UnityEngine.Debug.Log("close call:");
        file.Close();
        UnityEngine.Debug.Log("finished writing CSV");
    }
}
