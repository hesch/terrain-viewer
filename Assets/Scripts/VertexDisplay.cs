using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class VertexDisplay : MonoBehaviour
{
  public Material m_material;

  private Dictionary<(int,int), List<GameObject>> Meshes = new Dictionary<(int,int), List<GameObject>>();
  private static ConcurrentQueue<(Func<Transform, Material, List<GameObject>>, (int, int))> meshQueue = new ConcurrentQueue<(Func<Transform, Material, List<GameObject>>, (int, int))>();

  private static List<VertexDisplay> _instances = new List<VertexDisplay>();
  public static void PushNewMeshForOffset(Func<Transform, Material, List<GameObject>> meshAction, int x, int y) {
    meshQueue.Enqueue((meshAction, (x, y)));
  }

  public void OnEnable() {
    _instances.Add(this);
  }

  public void Update() {
    (Func<Transform, Material, List<GameObject>>, (int, int)) tuple;
    if(meshQueue.TryDequeue(out tuple)) {
      if(Meshes.ContainsKey(tuple.Item2)) {
	  Meshes[tuple.Item2].ForEach(m => Destroy(m));
      }
      Debug.Log("setting new Mesh at: " + tuple.Item2);
      Debug.Log(transform);
      Debug.Log(m_material);
      Meshes[tuple.Item2] = tuple.Item1(transform, m_material);
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
