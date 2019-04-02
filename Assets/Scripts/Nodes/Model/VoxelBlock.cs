using System.Collections;

public class VoxelBlock<T> : IVoxelBlock where T : Voxel {
  // These are the horizontal Layers of the VoxelBlock on top of each other. (y-coordinate)
  public VoxelLayer<T>[] Layers { get; set; }

  public override int Width {
    get {
      if (Layers == null || Layers[0] == null || Layers[0].Layer == null)
	return 0;
      return Layers[0].Layer.GetLength(0);
    }
  }
  
  public override int Height {
    get {
      if (Layers == null)
	return 0;
      return Layers.Length;
    }
  }
  
  public override int Length {
    get {
      if (Layers == null || Layers[0] == null || Layers[0].Layer == null)
	return 0;
      return Layers[0].Layer.GetLength(1);
    }
  }

  public VoxelBlock() {
  }
  
  public VoxelBlock(VoxelLayer<T>[] layers) {
    this.Layers = layers;
  }
}
