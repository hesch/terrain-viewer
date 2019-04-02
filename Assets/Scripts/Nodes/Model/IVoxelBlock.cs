public abstract class IVoxelBlock {
  public abstract int Width { get; }
  
  public abstract int Height { get; }
  
  public abstract int Length { get; }

  public int OffsetX { get; set; }
  public int OffsetY { get; set; }
}
