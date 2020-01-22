using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

[Node (false, "Noise3D")]
public class Noise3DNode : VoxelNode<Voxel> 
{
	public const string ID = "Noise3D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Noise3D"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 200); } }

	private INoise noiseFunction;

	private int width = 0;
	private int height = 0;
	private int length = 0;
	private Vector2Int offset;

	public NoiseGUI noiseGUI;
	public string noiseDesc;

	public void OnEnable() {
	  noiseGUI = new NoiseGUI(noiseDesc);
	  noiseFunction = noiseGUI.noiseFunction;
	}

	public override void NodeGUI() {
	  base.NodeGUI();

	  noiseFunction = noiseGUI.Display();

	  if (GUI.changed || noiseGUI.changed) {
	    noiseDesc = noiseGUI.noiseDesc();
	    NodeEditor.curNodeCanvas.OnNodeChange(this);
	  }
	}

	protected override void CalculationSetup(VoxelBlock<Voxel> block) {
	 width = block.Width;
	 height = block.Height;
	 length = block.Length;
	 offset = block.Offset;
	}

	protected override bool CalculateVoxel(Voxel voxel, int x, int y, int z) {
	  float noiseValue = noiseFunction.Sample3D(offset.x + x/(float)width, y/(float)height, offset.y + z/(float)length);
	  voxel.Data = noiseValue;
	  return true;
	}
}

