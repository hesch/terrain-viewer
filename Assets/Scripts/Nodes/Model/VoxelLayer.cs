using UnityEngine;

public class VoxelLayer<T> where T : Voxel {
  public int Overlap { get; set; }
  // indices are x, z
  public T[,] Layer { get; set; }

  public int Width {
    get {
      return Layer.GetLength(0) - 2*Overlap;
    }
  }

  public int Length {
    get {
      return Layer.GetLength(1) - 2*Overlap;
    }
  }

  public Vector2Int VoxelCount {
    get {
      return new Vector2Int(Width + 2*Overlap, Length + 2*Overlap);
    }
  }

  public VoxelLayer(T[,] layer) {
    this.Layer = layer;
  }

  public VoxelLayer(VoxelLayer<T> l) {
    this.Overlap = l.Overlap;
    this.Layer = new T[l.Layer.GetLength(0), l.Layer.GetLength(1)];
    for(int x = 0; x < l.Layer.GetLength(0); x++) {
      for(int z = 0; z < l.Layer.GetLength(1); z++) {
	this.Layer[x,z] = (T) l.Layer[x,z].Clone();
      }
    }
  }

  public T this[int x, int z] {
    get { return Layer[x+Overlap, z+Overlap]; }
  }
}
