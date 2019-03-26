using UnityEngine;
using System.Collections.Generic;

public class VertexDisplay : MonoBehaviour
{
  public Material m_material;

  private List<GameObject> meshes;

  private static List<VertexDisplay> _instances = new List<VertexDisplay>();
  public static void RenderNewMeshes(List<GameObject> meshes) {
    foreach(VertexDisplay instance in _instances) {
      instance.Render(meshes);
    }
  }

  public void OnEnable() {
    _instances.Add(this);
  }

  public void Render(List<GameObject> newMeshes)
  {
    Debug.Log("Got new meshes to render");
    Delete();
    meshes = newMeshes;
    foreach(GameObject mesh in meshes) {
      mesh.transform.parent = transform;
      mesh.GetComponent<Renderer>().material = m_material;
    }
  }

  void OnDestroy() {
    Delete();
  }

  void Delete() {
    if (meshes == null) return;
    foreach(GameObject o in meshes) { Destroy(o); }
    meshes = new List<GameObject>();
  }

}
