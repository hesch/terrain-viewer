using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace MarchingCubesProject
{
    public abstract class Marching : IMarching
    {

        public float Surface { get; set; }

        private float[] Cube;

        /// <summary>
        /// Winding order of triangles use 2,1,0 or 0,1,2
        /// </summary>
        protected int[] WindingOrder { get; private set; }

        public Marching(float surface = 0.5f)
        {
            Surface = surface;
            Cube = new float[32];
            WindingOrder = new int[] { 0, 1, 2 };
        }

        public virtual void Generate(IList<float> voxels, int width, int height, int depth, IList<Vector3> verts, IList<int> indices, IList<Vector3> normals)
        {
	  float[] v = new float[voxels.Count];
	  voxels.CopyTo(v, 0);

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
            for (x = 1; x < width - 2; x++)
            {
                for (y = 1; y < height - 2; y++)
                {
                    for (z = 1; z < depth - 2; z++)
                    {
                        //Get the values in the 8 neighbours which make up a cube + the surrounding values for normal calculation
                        for (i = 0; i < 32; i++)
                        {
                            ix = x + VertexOffset[i, 0];
                            iy = y + VertexOffset[i, 1];
                            iz = z + VertexOffset[i, 2];

                            Cube[i] = v[ix + iy * width + iz * width * height];
                        }

                        //Perform algorithm
                        March(x, y, z, Cube, verts, indices, normals);
                    }
                }
            }

        }

         /// <summary>
        /// MarchCube performs the Marching algorithm on a single cube
        /// </summary>
        protected abstract void March(float x, float y, float z, float[] cube, IList<Vector3> vertList, IList<int> indexList, IList<Vector3> normalList);

        /// <summary>
        /// GetOffset finds the approximate point of intersection of the surface
        /// between two points with the values v1 and v2
        /// </summary>
        protected virtual float GetOffset(float v1, float v2)
        {
            float delta = v2 - v1;
            return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
        }

	/// <summary>
	/// VertexSurround lists the index of the vertex, that lie one step in the positive/negative direction
	/// on the specified axis.
	/// (axis, cubeIndex, pos/neg)
	/// VertexSurround[3][8][2]
	/// </summary>
	protected static readonly int[,,] VertexSurround = new int[,,]
	{
	  { 
	    {8,1}, {0,9}, {3,10}, {11,2},
	    {12,5}, {4,13}, {7,14}, {15,6}
	  },
	  {
	    {16,3}, {17,2}, {1,18}, {0,19},
	    {20,7}, {21,6}, {5,22}, {4,23}
	  },
	  {
	    {4,24}, {5,25}, {6,26}, {7,27},
	    {28,0}, {29,1}, {30,2}, {31,3}
	  }
	};

        /// <summary>
        /// VertexOffset lists the positions, relative to vertex0, 
        /// of each of the 8 vertices of a cube.
        /// vertexOffset[8][3]
        /// </summary>
        protected static readonly int[,] VertexOffset = new int[,]
	    {
		// normal cube
	        {0, 0, 0},{1, 0, 0},{1, 1, 0},{0, 1, 0},
	        {0, 0, 1},{1, 0, 1},{1, 1, 1},{0, 1, 1},

		// offsets for normal calculation
	        {-1, 0, 0},{2, 0, 0},{2, 1, 0},{-1, 1, 0},
	        {-1, 0, 1},{2, 0, 1},{2, 1, 1},{-1, 1, 1},

	        {0, -1, 0},{1, -1, 0},{1, 2, 0},{0, 2, 0},
	        {0, -1, 1},{1, -1, 1},{1, 2, 1},{0, 2, 1},

	        {0, 0, -1},{1, 0, -1},{1, 1, -1},{0, 1, -1},
	        {0, 0,  2},{1, 0,  2},{1, 1,  2},{0, 1,  2}
	    };

    }

}
