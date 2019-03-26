using System.Collections;

public class VoxelBlock<T> where T : Voxel {
  // These are the horizontal Layers of the VoxelBlock on top of each other. (y-coordinate)
  public VoxelLayer<T>[] Layers { get; set; }

  public VoxelBlock(VoxelLayer<T>[] layers) {
    this.Layers = layers;
  }
}
