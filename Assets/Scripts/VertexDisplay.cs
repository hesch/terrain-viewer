using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

public class VertexDisplay : MonoBehaviour {
  public Material m_material;

  private Dictionary<Vector2Int, GameObject> Meshes = new Dictionary<Vector2Int, GameObject>();
  private ConcurrentQueue<(List<Vector3>, List<int>, IVoxelBlock)> meshQueue = new ConcurrentQueue<(List<Vector3>, List<int>, IVoxelBlock)>();

  private bool gridlinesVisible = true;
  public bool GridlinesVisible {
    get {
      return gridlinesVisible;
    }
    set {
      gridlinesVisible = value;
      foreach(GameObject mesh in Meshes.Values) {
	mesh.GetComponent<LineRenderer>().enabled = value;
      }
    }
  }

  private Action<PointerEventData, GameObject> MeshEventDelegate = (_, _1) => {};
  private Action<GameObject> MeshAddedDelegate = _ => {};

  public void addMeshEventDelegate(Action<PointerEventData, GameObject> del) {
    MeshEventDelegate += del;
  }

  public void removeMeshEventDelegate(Action<PointerEventData, GameObject> del) {
    MeshEventDelegate -= del;
  }

  public void addMeshAddedDelegate(Action<GameObject> del) {
    MeshAddedDelegate += del;
  }

  public void removeMeshAddedDelegate(Action<GameObject> del) {
    MeshAddedDelegate -= del;
  }

  public void PushNewMeshForOffset(List<Vector3> meshVertices, List<int> meshIndices, IVoxelBlock block) {
    meshQueue.Enqueue((meshVertices, meshIndices, block));
  }

  public void Update() {
    TryAddBlock();
  }

  private void TryAddBlock() {
    (List<Vector3>, List<int>, IVoxelBlock) tuple;
    if(meshQueue.TryDequeue(out tuple)) {
      var (vertices, indices, block) = tuple;
      
      if(Meshes.ContainsKey(block.Offset)) {
	  GameObject oldMesh = Meshes[block.Offset];
	  Destroy(oldMesh);
      }

      GameObject go = BlockConverter.BlockToGameObject(vertices, indices, block, m_material, MeshEventDelegate);
      go.transform.parent = transform;
      go.GetComponent<LineRenderer>().enabled = gridlinesVisible;

      Meshes[block.Offset] = go;
      MeshAddedDelegate(go);
    }
  }
  
}
