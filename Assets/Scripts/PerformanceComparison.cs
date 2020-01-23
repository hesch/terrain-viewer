using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MarchingCubesProject;
using ProceduralNoiseProject;
using System.Diagnostics;

class PerformanceComparison : MonoBehaviour
{
    private const int ROUNDS = 2;
    Stopwatch globalStopwatch;
    ComputeShader shader;
    PMB pmb;

    Marching marching = new MarchingCubes();

    Vector3Int[] testSizes = new Vector3Int[]
    {
        new Vector3Int(64, 64, 64),
        new Vector3Int(128, 64, 64),
        new Vector3Int(128, 128, 64),
        new Vector3Int(128, 128, 128),
        new Vector3Int(256, 256, 256),
        new Vector3Int(512, 512, 512),
        new Vector3Int(1024, 1024, 1024)
    };

    int sizeCounter = 0;
    bool finished = false;

    private void Awake()
    {
        shader = FindObjectOfType<PMBShader>().shaderRef;
        pmb = new PMB(shader);
        globalStopwatch = Stopwatch.StartNew();
        
    }

    bool first = true;

    private void Update()
    {
        if (finished)
        {
            UnityEngine.Debug.LogFormat("Finished Test in {0}ms", globalStopwatch.ElapsedMilliseconds);
            globalStopwatch.Stop();
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
            return;
        }

        try
        {
            if (first)
            {
                UnityEngine.Debug.Log("Starting Coroutine");
                StartCoroutine(testCheckerboard());
                first = false;
            }
        }
        finally
        {
            //finished = true;
        }
    }

    public IEnumerator<object> testCheckerboard()
    {
        int CPU_LIMIT = 2;
        int limitCounter = 0;
        UnityEngine.Debug.Log("Starting Test Checkerboard:");

        foreach (Vector3Int size in testSizes)
        {
            UnityEngine.Debug.Log("size: " + size);
            float[] voxel = new float[size.x * size.y * size.z];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        voxel[z * size.x * size.y + y * size.x + x] = (x + y + z) % 2;
                    }
                }
            }

            Stopwatch checkerboardWatch = Stopwatch.StartNew();

            if (limitCounter < CPU_LIMIT)
            {
                List<Vector3> verts = new List<Vector3>();
                List<int> indices = new List<int>();
                List<Vector3> normals = new List<Vector3>();
                marching.Generate(voxel, size.x, size.y, size.z, verts, indices, normals);

                UnityEngine.Debug.LogFormat("\tCPU took {0}ms", checkerboardWatch.ElapsedMilliseconds);
                checkerboardWatch.Restart();
                limitCounter++;
            }

            VoxelBlock<Voxel> block = InitBlock(size);

            pmb.ReInit(block);
            pmb.calculate(voxel, size.x, size.y, size.z, 0.5f);

            UnityEngine.Debug.LogFormat("\tPMB took {0}ms", checkerboardWatch.ElapsedMilliseconds);
            checkerboardWatch.Stop();
            yield return null;
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
}
