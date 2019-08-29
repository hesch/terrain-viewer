using ProceduralNoiseProject;
using UnityEngine;
using System;

public class test : Noise {
  
  public test(float frequency, float amplitude) {
  }

  public override float Sample1D(float x) {
    return 0.5f;
  }

  public override float Sample2D(float x, float y) {
    return rand((float)(int)(x*2f), (float)(int)(y*2f));
  }

  public override float Sample3D(float x, float y, float z) {
    return (int)(x*64+y*64+z*64)%2;
  }

  public override void UpdateSeed(int seed) {

  }

  private float rand(float x, float y) {
    float e = (float)Math.Sin((x*12.9898f + y*78.233f)*43758.5453123f);
    return e - (int) e;
  }
}
