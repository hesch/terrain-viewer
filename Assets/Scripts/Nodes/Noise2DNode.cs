using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

[Node (false, "Noise2D")]
public class Noise2DNode : HeightMapNode<Voxel> 
{
	public const string ID = "Noise2D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Noise2D"; } }
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
	  noiseDesc = noiseGUI.noiseDesc();

	  if (GUI.changed || noiseGUI.changed) {
	    NodeEditor.curNodeCanvas.OnNodeChange(this);
	  }
	}

	protected override void CalculationSetup(VoxelBlock<Voxel> block) {
	  base.CalculationSetup(block);
	  width = block.Width;
	  height = block.Height;
	  length = block.Length;
	  offset = block.Offset;
	}

	protected override bool CalculateHeight(out float height, int x, int z) {
	  float noiseValue = noiseFunction.Sample2D(offset.x+x/(float)width, offset.y + z/(float)length);
	  height = noiseValue;
	  return true;
	}
}

