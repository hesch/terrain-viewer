using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesTest : MonoBehaviour
{
  public ComputeShader shader;
  private int kernelIndex;
    void Start()
    {
      kernelIndex = shader.FindKernel("computeSizes");

      ComputeBuffer buffer = new ComputeBuffer(8, sizeof(int));
      buffer.SetData(new []{1, 1, 0, 0, 1, 0, 1, 1});

      shader.SetBuffer(kernelIndex, "buffer", buffer);
      shader.Dispatch(kernelIndex, 1, 1, 1);
      int[] result = new int[8];
      buffer.GetData(result);
      Debug.Log(result);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
