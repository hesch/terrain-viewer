public abstract class VoxelNode<T> : LayerNode<T> where T : Voxel
{
    protected abstract bool CalculateVoxel(T voxel, int x, int y, int z);

    protected override bool CalculateLayer(VoxelLayer<T> layer, int index)
    {
        for (int x = -layer.Overlap; x < layer.Width + layer.Overlap; x++)
        {
            for (int z = -layer.Overlap; z < layer.Length + layer.Overlap; z++)
            {
                if (!CalculateVoxel(layer[x, z], x, index, z))
                    return false;
            }
        }
        return true;
    }
}
