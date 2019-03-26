using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

public abstract class VoxelNode<T> : LayerNode<T> where T : Voxel
{
  protected abstract bool CalculateVoxel(T voxel, int x, int y, int z);

  protected override bool CalculateLayer(VoxelLayer<T> layer, int index) {
    T[][] voxelLayer = layer.Layer;
    for(int x = 0; x < voxelLayer.Length; x++) {
      for(int z = 0; z < voxelLayer[x].Length; z++) {
	if(!CalculateVoxel(voxelLayer[x][z], x, index, z))
	  return false;
      }
    }
    return true;
  }
}
