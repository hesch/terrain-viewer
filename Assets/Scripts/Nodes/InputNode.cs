using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

public abstract class InputNode<T> : Node where T : Voxel
{
	[ValueConnectionKnob("Output", Direction.Out, "Block")]
		public ValueConnectionKnob output;

	public int OffsetX { get; set; }
	public int OffsetY { get; set; }

	protected abstract VoxelBlock<T> InitBlock(VoxelBlock<T> block);

	public override bool Calculate() {
	  VoxelBlock<T> block = new VoxelBlock<T>();
	  block.OffsetX = OffsetX;
	  block.OffsetY = OffsetY;
	  output.SetValue(InitBlock(block));
	  return true;
	}
}
