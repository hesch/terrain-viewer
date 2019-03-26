using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

public abstract class InputNode<T> : Node where T : Voxel
{
	[ValueConnectionKnob("Output", Direction.Out, "Block")]
		public ValueConnectionKnob output;

	protected abstract VoxelBlock<T> InitBlock();

	public override bool Calculate() {
	  output.SetValue(InitBlock());
		return true;
	}
}
