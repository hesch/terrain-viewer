using ProceduralNoiseProject;
using UnityEngine;

public class test : Noise {
  
	public float[,] test1 = new float[,] {
	  {0.5f, 0f, 0.5f},
	  {1f, 1f, 1f},
	  {0.5f, 0f, 0.5f},
	};
  public test(float frequency, float amplitude) {
  }

  public override float Sample1D(float x) {
    return 0.5f;
  }

  public override float Sample2D(float x, float y) {
    return test1[((int)(x*3))%3, ((int)(y*3))%3];
  }

  public override float Sample3D(float x, float y, float z) {
    return (int)(x*64+y*64+z*64)%2;
  }

  public override void UpdateSeed(int seed) {

  }
}
