using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class VertexDisplay : MonoBehaviour
{
  public Material m_material;

  private Dictionary<Vector2Int, GameObject> Meshes = new Dictionary<Vector2Int, GameObject>();
  private static ConcurrentQueue<(List<Vector3>, List<int>, IVoxelBlock)> meshQueue = new ConcurrentQueue<(List<Vector3>, List<int>, IVoxelBlock)>();

  private static List<VertexDisplay> _instances = new List<VertexDisplay>();
  public static void PushNewMeshForOffset(List<Vector3> meshVertices, List<int> meshIndices, IVoxelBlock block) {
    meshQueue.Enqueue((meshVertices, meshIndices, block));
  }

  public void OnEnable() {
    _instances.Add(this);
  }

  public void Update() {
    (List<Vector3>, List<int>, IVoxelBlock) tuple;
    if(meshQueue.TryDequeue(out tuple)) {
      var (vertices, indices, block) = tuple;
      
      if(Meshes.ContainsKey(block.Offset)) {
	  GameObject oldMesh = Meshes[block.Offset];
	  Destroy(oldMesh);
      }

      if(vertices.Count > UInt16.MaxValue) {
	Debug.LogError("too many vertices");
	return;
      }

      Mesh mesh = new Mesh();
      mesh.SetVertices(vertices);
      mesh.SetTriangles(indices, 0);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();

      GameObject go = new GameObject(String.Format("Block({0}, {1})", block.Offset.x, block.Offset.y));
      go.transform.parent = transform;
      go.AddComponent<MeshFilter>();
      go.AddComponent<MeshRenderer>();
      go.GetComponent<Renderer>().material = m_material;
      go.GetComponent<MeshFilter>().mesh = mesh;
      go.transform.localPosition = new Vector3((block.Offset.x - 0.5f)*block.Width, -block.Height / 2, (block.Offset.y - 0.5f)*block.Length);
      go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

      Meshes[block.Offset] = go;
    }
  }

  void OnDestroy() {
    Delete();
  }

  void Delete() {
    //if (meshes == null) return;
    //foreach(GameObject o in meshes) { Destroy(o); }
    //meshes = new List<GameObject>();
  }

}
