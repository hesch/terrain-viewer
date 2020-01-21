using UnityEngine;

public class VoxelLayer<T> where T : Voxel
{
    private int overlap;
    public int Overlap
    {
        get
        {
            return overlap;
        }
        set
        {
            overlap = value;
            recalcDim();
        }
    }
    // indices are x, z
    private T[,] layer;
    public T[,] Layer
    {
        get
        {
            return layer;
        }
        set
        {
            layer = value;
            recalcDim();
        }
    }

    public int Width { get; private set; }

    public int Length { get; private set; }

    public Vector2Int VoxelCount { get; private set; }

    public VoxelLayer(T[,] layer)
    {
        this.Layer = layer;
        recalcDim();
    }

    public VoxelLayer(VoxelLayer<T> l)
    {
        this.Overlap = l.Overlap;
        int width = l.layer.GetLength(0);
        int length = l.layer.GetLength(1);
        this.layer = new T[width, length];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                this.layer[x, z] = (T)l.layer[x, z].Clone();
            }
        }
        recalcDim();
    }

    public T this[int x, int z]
    {
        get { return layer[x + overlap, z + overlap]; }
    }

    private void recalcDim()
    {
        if (Layer != null)
        {
            int length = Layer.GetLength(1);
            int width = Layer.GetLength(0);
            this.Length = length - 2 * Overlap;
            this.Width = width - 2 * Overlap;
            this.VoxelCount = new Vector2Int(width, length);
        }
    }
}
