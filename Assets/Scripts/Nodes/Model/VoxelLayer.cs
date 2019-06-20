using UnityEngine;

public class VoxelLayer<T> where T : Voxel {
  private int overlap;
  public int Overlap {
    get {
      return overlap;
    }
    set {
      overlap = value;
      recalcDim();
    }
  }
  // indices are x, z
  private T[,] layer;
  public T[,] Layer {
    get {
      return layer;
    }
    set {
      layer = value;
      recalcDim();
    }
  }

  public int Width { get; private set; }

  public int Length { get; private set; }

  public Vector2Int VoxelCount { get; private set; }

  public VoxelLayer(T[,] layer) {
    this.Layer = layer;
    recalcDim();
  }

  public VoxelLayer(VoxelLayer<T> l) {
    this.Overlap = l.Overlap;
    this.Layer = new T[l.Layer.GetLength(0), l.Layer.GetLength(1)];
    for(int x = 0; x < l.Layer.GetLength(0); x++) {
      for(int z = 0; z < l.Layer.GetLength(1); z++) {
	this.Layer[x,z] = (T) l.Layer[x,z].Clone();
      }
    }
    recalcDim();
  }

  public T this[int x, int z] {
    get { return layer[x+overlap, z+overlap]; }
  }

  private void recalcDim() {
    if (Layer != null) {
      int length = Layer.GetLength(1);
      int width = Layer.GetLength(0);
      this.Length = length - 2*Overlap;
      this.Width = width - 2*Overlap;
      this.VoxelCount = new Vector2Int(width, length);
    }
  }
}
