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
	  Debug.Log("Calculating Input Node");
	  VoxelBlock<T> block = new VoxelBlock<T>();
	  block.OffsetX = OffsetX;
	  block.OffsetY = OffsetY;
	  Debug.Log("Before SetValue");
	  output.SetValue(InitBlock(block));
	  Debug.Log("After SetValue");
	  return true;
	}
}
