using ProceduralNoiseProject;
using UnityEngine;

public class test : Noise {
  private float freq = 3.0f;
  private float amplitude = 3.0f;
  public test(float frequency, float amplitude) {
    freq = frequency;
    this.amplitude = amplitude;
  }

  public override float Sample1D(float x) {
    return 0.5f;
  }

  public override float Sample2D(float x, float y) {
    return amplitude*(1+Mathf.Sin(freq*x)*Mathf.Sin(freq*y))/2;
  }

  public override float Sample3D(float x, float y, float z) {
    return (Mathf.Sin(freq*x*2*Mathf.PI)*Mathf.Sin(freq*y*2*Mathf.PI)*Mathf.Sin(freq*z*2*Mathf.PI)+1)/2;
  }

  public override void UpdateSeed(int seed) {

  }
}
