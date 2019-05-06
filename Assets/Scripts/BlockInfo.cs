using UnityEngine;
using System.Collections.Generic;

public class BlockInfo : MonoBehaviour {
  public IVoxelBlock Block { get; set; }


  private Mesh mesh;
  private bool first = true;
  public void Start() {
    mesh = GetComponent<MeshFilter>().mesh;
  }

  public void Update() {
    Vector3[] norms = mesh.normals;
    Vector3[] verts = mesh.vertices;

    if (Block.Offset.x == 0 && Block.Offset.y == 0) {
      for(int i = 0; i < norms.Length; i++) {
	verts[i] = transform.TransformPoint(verts[i]);
	norms[i] = transform.TransformDirection(norms[i]);
	  Debug.DrawLine(verts[i], verts[i] + norms[i], Color.yellow, 0.0f, true);
      }
    }
  }
}
