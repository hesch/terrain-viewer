using UnityEngine;
using System.Collections.Generic;

using ProceduralNoiseProject;

namespace MarchingCubesProject
{

    public enum MARCHING_MODE {  CUBES, TETRAHEDRON };

    public class Example : MonoBehaviour
    {

        public Material m_material;

        public MARCHING_MODE mode = MARCHING_MODE.CUBES;
	public int resolution = 3;

        public int seed = 0;

        List<GameObject> meshes = new List<GameObject>();

        void Start()
        {
	    foreach(GameObject o in meshes) { o.transform.parent = null; Destroy(o); }
	    meshes = new List<GameObject>();
            INoise perlin = new PerlinNoise(seed, 2.0f);
            FractalNoise fractal = new FractalNoise(perlin, 3, 1.0f);

            //Set the mode used to create the mesh.
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
            Marching marching = null;
            if(mode == MARCHING_MODE.TETRAHEDRON)
                marching = new MarchingTertrahedron();
            else
                marching = new MarchingCubes();

            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            //The size of voxel array.
            int width = 32 * resolution;
            int height = 32 * resolution;
            int length = 32 * resolution;

            float[] voxels = new float[width * height * length];

            //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                        float fx = x / (width - 1.0f);
                        float fz = z / (length - 1.0f);

                        float value = fractal.Sample2D(fx, fz);
                    for (int y = 0; y < height; y++)
                    {
                        int idx = x + y * width + z * width * height;
			  voxels[idx] = y < (value+1.0f)/2.0f*height ? 1 : -1;
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Vector3> normals = new List<Vector3>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels, width, height, length, verts, indices, normals);

            //A mesh in unity can only be made up of 65000 verts.
            //Need to split the verts between multiple meshes.

            int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
            int numMeshes = verts.Count / maxVertsPerMesh + 1;

            for (int i = 0; i < numMeshes; i++)
            {

                List<Vector3> splitVerts = new List<Vector3>();
                List<int> splitIndices = new List<int>();
                List<Vector3> splitNormals = new List<Vector3>();

                for (int j = 0; j < maxVertsPerMesh; j++)
                {
                    int idx = i * maxVertsPerMesh + j;

                    if (idx < verts.Count)
                    {
                        splitVerts.Add(verts[idx]);
                        splitNormals.Add(normals[idx]);
                        splitIndices.Add(j);
                    }
                }

                if (splitVerts.Count == 0) continue;

                Mesh mesh = new Mesh();
                mesh.SetVertices(splitVerts);	
                mesh.SetTriangles(splitIndices, 0);
                mesh.SetNormals(splitNormals);
                mesh.RecalculateBounds();

                GameObject go = new GameObject("Mesh");
                go.transform.parent = transform;
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.GetComponent<Renderer>().material = m_material;
                go.GetComponent<MeshFilter>().mesh = mesh;
                go.transform.localPosition = new Vector3(-width/resolution / 2, -height/resolution / 2, -length/resolution / 2);
		go.transform.localScale = new Vector3(1.0f/resolution, 1.0f/resolution, 1.0f/resolution);

                meshes.Add(go);
            }

        }

        void Update()
        {
//            transform.Rotate(Vector3.up, 10.0f * Time.deltaTime);
        }
        void OnValidate() {
	  Start();
	}

    }

}
