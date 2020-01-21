using NodeEditorFramework;
using System.Linq;

public abstract class LayerNode<T> : Node where T : Voxel
{
    [ValueConnectionKnob("Input", Direction.In, "Block")]
    public ValueConnectionKnob input;
    [ValueConnectionKnob("Output", Direction.Out, "Block")]
    public ValueConnectionKnob output;

    protected virtual void CalculationSetup(VoxelBlock<T> block) { }
    protected virtual void CalculationTeardown() { }
    protected abstract bool CalculateLayer(VoxelLayer<T> layer, int index);

    public override bool Calculate()
    {
        VoxelBlock<T> block = input.GetValue<VoxelBlock<T>>();
        if (block == null || block.Layers == null)
        {
            return false;
        }

        CalculationSetup(block);
        bool success = true;
        for (int i = 0; i < block.Layers.Count(); i++)
        {
            if (!CalculateLayer(block.Layers[i], i - block.Overlap))
            {
                success = false;
                break;
            }
        }
        CalculationTeardown();

        output.SetValue(block);

        return success;
    }
}
