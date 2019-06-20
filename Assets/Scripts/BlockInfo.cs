using UnityEngine;
using System.Collections.Generic;

public class BlockInfo : MonoBehaviour {
  public IVoxelBlock Block { get; set; }


  private Mesh mesh;
  private bool first = true;
  public void Start() {
    mesh = GetComponent<MeshFilter>().mesh;
  }

  /*public void Update() {
    Vector3[] norms = mesh.normals;
    Vector3[] verts = mesh.vertices;

    if (Block.Offset.x == 0 && Block.Offset.y == 0) {
      for(int x = -Block.Overlap; x < Block.VoxelCount.x+1; x++) {
	for(int y = -Block.Overlap; y < Block.VoxelCount.y+1; y++) {
	  for(int z = -Block.Overlap; z < Block.VoxelCount.z+1; z++) {
	    Debug.DrawLine(transform.TransformPoint(new Vector3(-Block.Overlap, y, z)), transform.TransformPoint(new Vector3(Block.VoxelCount.x, y, z)));
	    Debug.DrawLine(transform.TransformPoint(new Vector3(x, -Block.Overlap, z)), transform.TransformPoint(new Vector3(x, Block.VoxelCount.y, z)));
	    Debug.DrawLine(transform.TransformPoint(new Vector3(x, y, -Block.Overlap)), transform.TransformPoint(new Vector3(x, y, Block.VoxelCount.z)));
	  }
	}
      }
      for(int i = 0; i < norms.Length; i++) {
	Vector3 vert = transform.TransformPoint(verts[i]);
	Vector3 normal = transform.TransformDirection(norms[i]);
	  Debug.DrawLine(vert, vert + 3.0f*normal, Color.yellow, 0.0f, true);
      }
    }
  }
*/
}
