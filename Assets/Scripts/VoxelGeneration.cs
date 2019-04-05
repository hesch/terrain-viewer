using UnityEngine;
using MarchingCubesProject;
using System.Collections.Generic;
using System.Linq;

public class VoxelGeneration : Marching {

  protected override void March(float x, float y, float z, float[] cube, IList<Vector3> vertList, IList<int> indexList) {
    if (cube[0] < Surface) {
      return;
    }
    int count = vertList.Count();

    //TODO: reduce generated Vertices

    (indexList as List<int>).AddRange(Indices.Select(i => i + count));

    (vertList as List<Vector3>).AddRange(VertexOffset.Select(o => new Vector3(x, y, z) + o * 0.5f));
  }

  private Vector3[] VertexOffset = new Vector3[] {
    new Vector3(-1, -1, -1),
    new Vector3(-1, -1,  1),
    new Vector3(-1,  1, -1),
    new Vector3(-1,  1,  1),
    new Vector3( 1, -1, -1),
    new Vector3( 1, -1,  1),
    new Vector3( 1,  1, -1),
    new Vector3( 1,  1,  1),
  };

  private int[] Indices = new int[] {
    0,1,2,
    1,2,3,
    0,1,4,
    1,4,5,
    0,2,4,
    2,4,6,
    4,5,6,
    5,6,7,
    2,3,6,
    3,6,7,
    1,3,5,
    3,5,7
  };
}
