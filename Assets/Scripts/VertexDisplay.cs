using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

public class VertexDisplay : MonoBehaviour {
  public Material m_material;

  private Dictionary<Vector2Int, GameObject> Meshes = new Dictionary<Vector2Int, GameObject>();
  private ConcurrentQueue<(List<Vector3>, List<int>, List<Vector3>, IVoxelBlock)> meshQueue = new ConcurrentQueue<(List<Vector3>, List<int>, List<Vector3>, IVoxelBlock)>();

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
  private Vector2Int OnlyShowObjectAt;
  private bool hiddenState = false;

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

  public void PushNewMeshForOffset(List<Vector3> meshVertices, List<int> meshIndices, List<Vector3> meshNormals, IVoxelBlock block) {
    meshQueue.Enqueue((meshVertices, meshIndices, meshNormals, block));
  }

  public void hideAllBut(GameObject it) {
    OnlyShowObjectAt = it.GetComponent<BlockInfo>().Block.Offset;
    hiddenState = true;
    foreach(GameObject o in Meshes.Values) {
      if (o != it) {
	o.SetActive(false);
      }
    }
  }

  public void clearHideState() {
    hiddenState = false;
  }

  public void Update() {
    TryAddBlock();
  }

  private void TryAddBlock() {
    (List<Vector3>, List<int>, List<Vector3>, IVoxelBlock) tuple;
    if(meshQueue.TryDequeue(out tuple)) {
      var (vertices, indices, normals, block) = tuple;
      
      if(Meshes.ContainsKey(block.Offset)) {
	  GameObject oldMesh = Meshes[block.Offset];
	  Destroy(oldMesh);
      }

      GameObject go = BlockConverter.BlockToGameObject(vertices, indices, normals, block, m_material, MeshEventDelegate);
      go.transform.parent = transform;
      go.GetComponent<LineRenderer>().enabled = gridlinesVisible;
      if (hiddenState && block.Offset != OnlyShowObjectAt) {
	go.SetActive(false);
      }

      Meshes[block.Offset] = go;
      MeshAddedDelegate(go);
    }
  }
  
}
