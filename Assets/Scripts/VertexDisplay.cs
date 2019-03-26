using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class VertexDisplay : MonoBehaviour
{
  public Material m_material;

  private GameObject[,] Meshes = new GameObject[10,10];
  private static ConcurrentQueue<(Func<Transform, Material, GameObject>, int, int)> meshQueue = new ConcurrentQueue<(Func<Transform, Material, GameObject>, int, int)>();

  private static List<VertexDisplay> _instances = new List<VertexDisplay>();
  public static void PushNewMeshForOffset(Func<Transform, Material, GameObject> meshAction, int x, int y) {
    meshQueue.Enqueue((meshAction, x, y));
  }

  public void OnEnable() {
    _instances.Add(this);
  }

  public void Update() {
    (Func<Transform, Material, GameObject>, int, int) tuple;
    if(meshQueue.TryDequeue(out tuple)) {
      if(Meshes[tuple.Item2, tuple.Item3] != null) {
	  Destroy(Meshes[tuple.Item2, tuple.Item3]);
      }
      Meshes[tuple.Item2, tuple.Item3] = tuple.Item1(transform, m_material);
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
