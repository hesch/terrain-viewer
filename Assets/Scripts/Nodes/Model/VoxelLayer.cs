public class VoxelLayer<T> where T : Voxel {
  // indices are x, z
  public T[,] Layer { get; set; }

  public VoxelLayer(T[,] layer) {
    this.Layer = layer;
  }

  public T this[int x, int z] {
    get { return Layer[x, z]; }
  }
}
