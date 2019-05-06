using UnityEngine;
using MarchingCubesProject;
using System.Collections.Generic;
using System.Linq;

public class VoxelGeneration : Marching {

  private float[] OtherCube = new float[7];
  
  public override void Generate(IList<float> voxels, int width, int height, int depth, IList<Vector3> verts, IList<int> indices, IList<Vector3> normals)
  {

    if (Surface > 0.0f)
    {
      WindingOrder[0] = 0;
      WindingOrder[1] = 1;
      WindingOrder[2] = 2;
    }
    else
    {
      WindingOrder[0] = 2;
      WindingOrder[1] = 1;
      WindingOrder[2] = 0;
    }

    int x, y, z, i;
    int ix, iy, iz;
    for (x = 0; x < width - 1; x++)
    {
      for (y = 0; y < height - 1; y++)
      {
	for (z = 0; z < depth - 1; z++)
	{
	  //Get the values in the 8 neighbours which make up a cube
	  for (i = 0; i < 7; i++)
	  {
	    ix = x + VoxelValueOffset[i, 0];
	    iy = y + VoxelValueOffset[i, 1];
	    iz = z + VoxelValueOffset[i, 2];

	    int idx = ix + iy * width + iz * width * height;
	    OtherCube[i] = idx >= voxels.Count() || idx < 0 ? 0.0f : voxels[idx];
	  }

	  //Perform algorithm
	  March(x, y, z, OtherCube, verts, indices, normals);
	}
      }
    }

  }

  private int[,] VoxelValueOffset = new int[,] {
    {0, 0, 0},
    {-1, 0, 0}, {1, 0, 0},
    {0, -1, 0}, {0, 1, 0},
    {0, 0, -1}, {0, 0, 1}
  };

  protected override void March(float x, float y, float z, float[] cube, IList<Vector3> vertList, IList<int> indexList, IList<Vector3> normalList) {
    if (cube[0] < Surface) {
      return;
    }

    if (cube.All(c => c >= Surface)) {
      return;
    }
    int count = vertList.Count();

    //TODO: reduce generated Vertices

    (indexList as List<int>).AddRange(Indices.Reverse().Select(i => i + count));

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
    0,4,2,
    2,4,6,
    1,0,3,
    3,0,2,
    2,6,3,
    3,6,7,
    5,1,7,
    7,1,3,
    4,5,6,
    6,5,7,
    1,5,0,
    0,5,1
  };
}
