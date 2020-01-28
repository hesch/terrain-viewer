using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;
using MarchingCubesProject;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public enum VerteGenerationMode
{
    PMB,
    Cubes,
    Tetrahedron,
    Voxel
};

[Node(false, "Vertex")]
public class VertexNode : Node
{
    public const string ID = "Vertex";
    public override string GetID { get { return ID; } }

    public override string Title { get { return "Vertex Generation"; } }
    public override Vector2 DefaultSize { get { return new Vector2(150, 100); } }

    [ValueConnectionKnob("Input", Direction.In, "Block")]
    public ValueConnectionKnob input;
    [ValueConnectionKnob("Surface", Direction.In, "Float")]
    public ValueConnectionKnob surfaceConnection;

    public VerteGenerationMode mode = VerteGenerationMode.Cubes;
    public float surface = 0.5f;

    private PMB pmb;

    public ComputeBuffer Vertices { get; set; }
    public ComputeBuffer Indices { get; set; }
    public ComputeBuffer Normals { get; set; }
    public VoxelBlock<Voxel> Block { get; set; }

    public void Awake()
    {
        PMBShader shader = FindObjectOfType<PMBShader>();
        if (shader == null)
        {
            UnityEngine.Debug.LogWarning("could not find object of Type PMBShader");
            return;
        }
        pmb = new PMB(shader.shaderRef);
    }

    public void OnDestroy()
    {
        if (pmb != null)
        {
            pmb.Dispose();
        }
        if (Vertices != null)
        {
            Vertices.Dispose();
        }
        if (Indices != null)
        {
            Indices.Dispose();
        }
        if (Normals != null)
        {
            Normals.Dispose();
        }
    }

    public override void NodeGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        input.DisplayLayout();

        GUILayout.BeginHorizontal();
        GUILayout.Label(surfaceConnection.name);
        if (!surfaceConnection.connected())
            surface = RTEditorGUI.FloatField(GUIContent.none, surface);

        surfaceConnection.SetPosition();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        RTEditorGUI.EnumPopup(new GUIContent("Generation", "The type of Vertex generation"), mode, m =>
        {
            if (mode != m)
            {
                mode = m;
                NodeEditor.curNodeCanvas.OnNodeChange(this);
            }
        });

        if (GUI.changed)
        {
            NodeEditor.curNodeCanvas.OnNodeChange(this);
        }
    }

    private void weldVertices(List<Vector3> verts, List<int> indices, List<Vector3> normals)
    {
        Dictionary<Vector3, int> indexMap = new Dictionary<Vector3, int>();
        Dictionary<Vector3, Vector3> normalMap = new Dictionary<Vector3, Vector3>();
        for (int i = 0; i < verts.Count; i++)
        {
            if (!indexMap.ContainsKey(verts[i]))
            {
                indexMap.Add(verts[i], indexMap.Count);
                normalMap.Add(verts[i], normals[i]);
            }
        }

        for (int i = 0; i < indices.Count; i++)
        {
            indices[i] = indexMap[verts[indices[i]]];
        }

        verts.Clear();
        verts.InsertRange(0, indexMap.OrderBy(kv => kv.Value).Select(kv => kv.Key));
        normals.Clear();
        normals.InsertRange(0, indexMap.OrderBy(kv => kv.Value).Select(kv => normalMap[kv.Key]));
    }

    public override bool Calculate()
    {
        bool isPMB = false;
        if (!input.connected())
        {
            return false;
        }
        VoxelBlock<Voxel> block = input.GetValue<VoxelBlock<Voxel>>();
        if (surfaceConnection.connected())
        {
            surface = surfaceConnection.GetValue<float>();
        }

        Marching marching = null;
        switch (mode)
        {
            case VerteGenerationMode.Tetrahedron:
                marching = new MarchingTertrahedron();
                break;
            case VerteGenerationMode.Cubes:
                marching = new MarchingCubes();
                break;
            case VerteGenerationMode.Voxel:
                marching = new VoxelGeneration();
                break;
            case VerteGenerationMode.PMB:
                isPMB = true;
                break;
        }

        //The size of voxel array.
        Vector3Int count = block.VoxelCount;
        int width = count.x;
        int height = count.y;
        int length = count.z;

        float[] voxels = new float[width * height * length];

        for (int y = 0; y < height; y++)
        {
            Voxel[,] voxelLayer = block.Layers[y].Layer;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    int idx = x + y * width + z * width * height;
                    voxels[idx] = voxelLayer[x, z].GetValue();
                }
            }
        }

        if (isPMB)
        {
            UnityEngine.Debug.Log("scheduling PMB on main thread");
            var pmbTask = MainThreadHelper.instance().scheduleOnMainThread(() =>
            {
                pmb.ReInit(block);
                Stopwatch pmbWatch = Stopwatch.StartNew();
                RenderBuffers buffers = pmb.calculate(voxels, width, height, length, surface);
                Vertices = buffers.vertexBuffer;
                Indices = buffers.indexBuffer;
                Normals = buffers.normalBuffer;
                pmbWatch.Stop();
                UnityEngine.Debug.LogFormat("PMB took {0}ms\n\t{1} voxels\n\t{2} triangles", pmbWatch.ElapsedMilliseconds, voxels.Count(), Indices.count / 3);
            });

            UnityEngine.Debug.Log("waiting for PMB on main thread");
            pmbTask.wait();

            if (!pmbTask.completed)
            {
                return false;
            }

            UnityEngine.Debug.Log("finished PMB on main thread");
            Block = block;
            return true;
        }

        //Surface is the value that represents the surface of mesh
        //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
        //The target value does not have to be the mid point it can be any value with in the range.
        //
        //This should be accesible by an input
        marching.Surface = surface;

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        Stopwatch sw = Stopwatch.StartNew();

        marching.Generate(voxels, width, height, length, verts, indices, normals);

        sw.Stop();

        UnityEngine.Debug.LogFormat("Marching took {0}ms\n\t{1} vertices; {2} triangles", sw.ElapsedMilliseconds, verts.Count(), indices.Count() / 3);

        sw.Restart();
        weldVertices(verts, indices, normals);
        sw.Stop();

        UnityEngine.Debug.LogFormat("Vertex welding took {0}ms\n\t {1} vertices left", sw.ElapsedMilliseconds, verts.Count());

        var task = MainThreadHelper.instance().scheduleOnMainThread(() =>
        {
            Vertices = new ComputeBuffer(verts.Count, sizeof(float) * 3);
            Indices = new ComputeBuffer(indices.Count, sizeof(int));
            Normals = new ComputeBuffer(normals.Count, sizeof(float) * 3);

            Vertices.SetData(verts);
            Indices.SetData(indices);
            Normals.SetData(normals);
        });

        task.wait();

        Block = block;

        return true;
    }
}