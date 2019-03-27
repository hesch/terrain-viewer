using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node(false, "VoxelInput")]
public class VoxelInputNode : InputNode<Voxel>
{
	public const string ID = "VoxelInput";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Input Node"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	protected override VoxelBlock<Voxel> InitBlock(VoxelBlock<Voxel> block) {
	  int size = 64;
	  Debug.Log("Block offset (" + block.OffsetX + ", " + block.OffsetY);
	  VoxelLayer<Voxel>[] layers = new VoxelLayer<Voxel>[64];
	  for(int y = 0; y < size; y++) {
	    Voxel[,] voxelLayer = new Voxel[size, size];
	    for(int x = 0; x < size; x++) {
	      for(int z = 0; z < size; z++) {
		voxelLayer[x, z] = new Voxel();
		if (y < size/2) {
		  voxelLayer[x, z].Data = 1.0f;
		}
	      }
	    }
	    layers[y] = new VoxelLayer<Voxel>(voxelLayer);
	  }
	  block.Layers = layers;
	  Debug.Log("InitBlock End");
	  return block;
	}
}
