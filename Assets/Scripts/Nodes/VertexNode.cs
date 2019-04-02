using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;
using MarchingCubesProject;
using System.Collections.Generic;
using System.Linq;

public enum MarchingMode {  Cubes, Tetrahedron };

[Node (false, "Vertex")]
public class VertexNode: Node
{
	public const string ID = "Vertex";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Vertex Generation"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	[ValueConnectionKnob("Input", Direction.In, "Block")]
		public ValueConnectionKnob input;

	private MarchingMode mode = MarchingMode.Cubes;

	public List<GameObject> Meshes { get; set; }

	public override void NodeGUI () 
	{
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		input.DisplayLayout ();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal ();
		RTEditorGUI.EnumPopup (new GUIContent ("Marching", "The type of Vertex generation"), mode, m => {
		    if (mode != m) {
		      mode = m;
		      NodeEditor.curNodeCanvas.OnNodeChange(this);
		    }
		});
	}

	private void weldVertices(List<Vector3> verts, List<int> indices) {
	  Dictionary<Vector3, int> indexMap = new Dictionary<Vector3, int>();
	  for(int i = 0; i < verts.Count; i++) {
	      if (!indexMap.ContainsKey(verts[i])) {
		indexMap.Add(verts[i], indexMap.Count);
	      }
	  }

	  for(int i = 0; i < indices.Count; i++) {
	    indices[i] = indexMap[verts[indices[i]]];
	  }

	  verts.Clear();
	  verts.InsertRange(0, indexMap.OrderBy(kv => kv.Value).Select(kv => kv.Key));
	}

	public override bool Calculate() {
	  Meshes = new List<GameObject>();
	  VoxelBlock<Voxel> block = input.GetValue<VoxelBlock<Voxel>>();

	  Marching marching = null;
	  if(mode == MarchingMode.Tetrahedron)
	    marching = new MarchingTertrahedron();
	  else
	    marching = new MarchingCubes();

	  //Surface is the value that represents the surface of mesh
	  //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
	  //The target value does not have to be the mid point it can be any value with in the range.
	  //
	  //This should be accesible by an input
	  marching.Surface = 0.5f;

	  //The size of voxel array.
	  Vector3Int count = block.VoxelCount;
	  int width = count.x;
	  int height = count.y;
	  int length = count.z;

	  float[] voxels = new float[width * height * length];

	  for(int y = 0; y < height; y++) {
	    Voxel[,] voxelLayer = block.Layers[y].Layer;
	    for(int x = 0; x < width; x++) {
	      for(int z = 0; z < length; z++) {
		int idx = x + y * width + z * width * height;
		voxels[idx] = voxelLayer[x,z].GetValue();
	      }
	    }
	  }

	  List<Vector3> verts = new List<Vector3>();
	  List<int> indices = new List<int>();

	  //The mesh produced is not optimal. There is one vert for each index.
	  //Would need to weld vertices for better quality mesh.
	  marching.Generate(voxels, width, height, length, verts, indices);

	  weldVertices(verts, indices);

	  VertexDisplay.PushNewMeshForOffset(verts, indices, block);

	  return true;
	}
}

