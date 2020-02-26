using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System.Collections.Generic;

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

    Vector3Int voxelCount = block.VoxelCount;
        
        bool success = true;

        for (int x = 0; x < voxelCount.x && success; x++)
        {
            for (int z = 0; z < voxelCount.z; z++)
            {
                float height;

                if (!CalculateHeight(out height, x - block.Overlap, z - block.Overlap))
                {
                    success = false;
                    break;
                }

                for (int y = -block.Overlap; y < voxelCount.y - block.Overlap; y++)
                {
                    float voxelHeight = y / (float)block.Height;

                    if (height > voxelHeight)
                    {
                        float nextVoxelHeight = (y + 1) / (float)block.Height;
                        if (height >= nextVoxelHeight)
                        {
                            block[x - block.Overlap, y, z - block.Overlap].Data = 1.0f;
                        }
                        else
                        {
                            block[x - block.Overlap, y, z - block.Overlap].Data = (height - voxelHeight)*(float)block.Height;
                        }
                    }
                    else
                    {
                        block[x - block.Overlap, y, z - block.Overlap].Data = 0.0f;
                    }
                }
            }
        }

    if(!success) {
      CalculationTeardown();
      return false;
    }
    
    CalculationTeardown();
    output.SetValue(block);

    return true;
  }
}
