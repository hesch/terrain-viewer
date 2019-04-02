using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

public abstract class InputNode<T> : Node where T : Voxel
{
	[ValueConnectionKnob("Output", Direction.Out, "Block")]
		public ValueConnectionKnob output;

	public Vector2Int Offset { get; set; }

	protected abstract VoxelBlock<T> InitBlock(VoxelBlock<T> block);

	public override bool Calculate() {
	  VoxelBlock<T> block = new VoxelBlock<T>();
	  block.Offset = Offset;
	  output.SetValue(InitBlock(block));
	  return true;
	}
}
