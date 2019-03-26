using System.Collections;

public class VoxelBlock<T> where T : Voxel {
  // These are the horizontal Layers of the VoxelBlock on top of each other. (y-coordinate)
  public VoxelLayer<T>[] Layers { get; set; }
  public int Width {
    get {
      if (Layers == null || Layers[0] == null || Layers[0].Layer == null)
	return 0;
      return Layers[0].Layer.GetLength(0);
    }
  }
  
  public int Height {
    get {
      if (Layers == null)
	return 0;
      return Layers.Length;
    }
  }
  
  public int Length {
    get {
      if (Layers == null || Layers[0] == null || Layers[0].Layer == null)
	return 0;
      return Layers[0].Layer.GetLength(1);
    }
  }

  public int OffsetX { get; set; }
  public int OffsetY { get; set; }

  public VoxelBlock() {
  }
  
  public VoxelBlock(VoxelLayer<T>[] layers) {
    this.Layers = layers;
  }
}
