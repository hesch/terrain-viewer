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
    VoxelBlock<T> block = new VoxelBlock<T>(input.GetValue<VoxelBlock<T>>());
    if(block == null || block.Layers == null) {
      return false;
    }

    CalculationSetup(block);

    Vector3Int voxelCount = block.VoxelCount;

    float[,] heightMap = new float[voxelCount.x, voxelCount.z];
    bool success = Enumerable.Range(-block.Overlap, voxelCount.x)
      .SelectMany(_ => Enumerable.Range(-block.Overlap, voxelCount.z), (x, z) => (x: x, z: z))
      .AsParallel()
      .Select(coord => {
	  float height;
	  bool s = CalculateHeight(out height, coord.x, coord.z);
	  heightMap[coord.x+block.Overlap, coord.z+block.Overlap] = height;
	  return s;
	  })
    .All(s => s);

    if(!success) {
      CalculationTeardown();
      return false;
    }

    for(int x = -block.Overlap; x < voxelCount.x-block.Overlap; x++) {
      for(int y = -block.Overlap; y < voxelCount.y-block.Overlap; y++) {
	for(int z = -block.Overlap; z < voxelCount.z-block.Overlap; z++) {
	  float voxelHeight = y/(float)block.Height;
	  float calculatedHeight = heightMap[x+block.Overlap, z+block.Overlap];

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
