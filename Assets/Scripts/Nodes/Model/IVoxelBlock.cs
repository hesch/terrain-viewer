using UnityEngine;

public abstract class IVoxelBlock
{
    public abstract int Width { get; }
    public abstract int Height { get; }
    public abstract int Length { get; }

    public abstract Vector3Int VoxelCount { get; }
    public abstract int Overlap { get; set; }
    public Vector2Int Offset { get; set; }
}
