using System.Collections;
using System;
using System.Linq;
using UnityEngine;

public class VoxelBlock<T> : IVoxelBlock where T : Voxel {
  // These are the horizontal Layers of the VoxelBlock on top of each other. (y-coordinate)
  private VoxelLayer<T>[] layers;
  public VoxelLayer<T>[] Layers {
    get {
      return layers;
    }
    set {
      layers = value;
      setLayerOverlap(this.Overlap);
    }
  }

  public override int Width {
    get {
      if (Layers == null || Layers[0] == null || Layers[0].Layer == null)
	return 0;
      return Layers[0].Layer.GetLength(0)-overlap*2;
    }
  }
  
  public override int Height {
    get {
      if (Layers == null)
	return 0;
      return Layers.Length-overlap*2;
    }
  }
  
  public override int Length {
    get {
      if (Layers == null || Layers[0] == null || Layers[0].Layer == null)
	return 0;
      return Layers[0].Layer.GetLength(1)-overlap*2;
    }
  }

  /**
   * The real VoxelCount. Overlapping Voxels are used to connect different VoxelBlocks.
   */
  public override Vector3Int VoxelCount {
    get {
      return new Vector3Int(Width+overlap*2, Height+overlap*2, Length+overlap*2);
    }
  }

  private int overlap = 1;
  public override int Overlap { 
    get {
      return overlap;
    }
    set {
      overlap = value;
      setLayerOverlap(overlap);
    }
  }
  
  private void setLayerOverlap(int val) {
      Array.ForEach(layers, layer => {
	  layer.Overlap = val;
      });
  }

  public VoxelBlock() {
  }
  
  public VoxelBlock(VoxelLayer<T>[] layers) {
    this.layers = layers;
    this.Overlap = 1;
  }

  public VoxelBlock(VoxelBlock<T> b) {
    this.Offset = b.Offset;
    this.Layers = b.Layers.Select(l => new VoxelLayer<T>(l)).ToArray();
    this.Overlap = b.Overlap;
  }
    

  public T this[int x, int y, int z] {
    get { return Layers[y+Overlap][x,z]; }
  }
}
