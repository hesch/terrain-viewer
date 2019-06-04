using ProceduralNoiseProject;
using UnityEngine;

public class test : Noise {
  public override float Sample1D(float x) {
    return 0.7f;
  }

  public override float Sample2D(float x, float y) {
    return x+y;
  }

  public override float Sample3D(float x, float y, float z) {
    return Mathf.Sin(x);
  }

  public override void UpdateSeed(int seed) {

  }
}
