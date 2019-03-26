using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System.Linq;

public abstract class LayerNode<T> : Node where T: Voxel
{
	[ValueConnectionKnob("Input", Direction.In, "Block")]
		public ValueConnectionKnob input;
	[ValueConnectionKnob("Output", Direction.Out, "Block")]
		public ValueConnectionKnob output;

	protected virtual void CalculationSetup() {}
	protected virtual void CalculationTeardown() {}
	protected abstract bool CalculateLayer(VoxelLayer<T> layer, int index);

	public override bool Calculate() {
	  VoxelBlock<T> block = input.GetValue<VoxelBlock<T>>();

	  CalculationSetup();
	  bool success = block.Layers.Select((layer, i) => CalculateLayer(layer, i)).All(x => x);
	  CalculationTeardown();

	  output.SetValue(block);

	  return success;
	}
}
