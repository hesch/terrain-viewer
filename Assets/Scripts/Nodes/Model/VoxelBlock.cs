using System.Collections;

public class VoxelBlock<T> where T : Voxel {
  // These are the horizontal Layers of the VoxelBlock on top of each other. (y-coordinate)
  public VoxelLayer<T>[] Layers { get; set; }
  public int Width {
    get {
      return Layers[0].Layer.GetLength(0);
    }
  }
  
  public int Height {
    get {
      return Layers.Length;
    }
  }
  
  public int Length {
    get {
      return Layers[0].Layer.GetLength(1);
    }
  }

  public int OffsetX { get; set; }
  public int OffsetY { get; set; }

  public VoxelBlock(VoxelLayer<T>[] layers) {
    this.Layers = layers;
  }
}
