using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralNoiseProject;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      Debug.Log("test");
      int width = 10;
      List<float> voxels = new List<float>();
      Noise noiseGenerator = new PerlinNoise(1337, 1.0f, 1.0f);
      for(int x = 0; x < width; x++) {
        for(int y = 0; y < width; y++) {
          for(int z = 0; z < width; z++) {
             float value = noiseGenerator.Sample3D(x,y,z);
	     voxels.Add(value);
          }
        }
      }
	
      gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        // make changes to the Mesh by creating arrays which contain the new values
        mesh.vertices = new Vector3[] {new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0)};
        mesh.uv = new Vector2[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)};
        mesh.triangles =  new int[] {0, 1, 2};
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
