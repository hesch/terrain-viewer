using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System.Collections.Generic;
using System.Linq;

public abstract class HeightMapNode<T> : Node where T: Voxel
{
  [ValueConnectionKnob("Input", Direction.In, "Block")]
    public ValueConnectionKnob input;
  [ValueConnectionKnob("Output", Direction.Out, "Block")]
    public ValueConnectionKnob output;

  protected virtual void CalculationSetup(VoxelBlock<T> block) {}
  protected virtual void CalculationTeardown() {}

  protected abstract bool CalculateHeight(out float height, int x, int z);
  
  public override bool Calculate() {
    VoxelBlock<T> block = input.GetValue<VoxelBlock<T>>();
    if(block == null || block.Layers == null) {
      return false;
    }

    CalculationSetup(block);

    float[,] heightMap = new float[block.Width, block.Length];
    bool success = Enumerable.Range(0, block.Width)
      .SelectMany(_ => Enumerable.Range(0, block.Length), (x, z) => (x: x, z: z))
      .AsParallel()
      .Select(coord => {
	  float height;
	  bool s = CalculateHeight(out height, coord.x, coord.z);
	  heightMap[coord.x, coord.z] = height;
	  return s;
	  })
    .All(s => s);

    if(!success) {
      CalculationTeardown();
      return false;
    }

    for(int x = 0; x < block.Width; x++) {
      for(int y = 0; y < block.Height; y++) {
	for(int z = 0; z < block.Length; z++) {
	  float voxelHeight = y/(float)block.Height;
	  float calculatedHeight = heightMap[x, z];

	  if (calculatedHeight > voxelHeight) {
	    float nextVoxelHeight = (y+1)/(float)block.Height;
	    if (calculatedHeight >= nextVoxelHeight) {
	      block[x, y, z].Data = 1.0f;
	    } else {
	      block[x, y, z].Data = 1.0f - (calculatedHeight - voxelHeight);
	    }
	  } else {
	    block[x, y, z].Data = 0.0f;
	  }
	}
      }
    }

    CalculationTeardown();
    output.SetValue(block);

    return true;
  }
}
