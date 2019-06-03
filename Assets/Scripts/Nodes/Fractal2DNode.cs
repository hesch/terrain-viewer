
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

[Node (false, "Fractal2D")]
public class Fractal2DNode : HeightMapNode<Voxel> {
	public const string ID = "Fractal2D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Fractal2D"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 200); } }

	private INoise noiseFunction;
	private FractalNoise fractalNoise;

	private int width = 0;
	private int height = 0;
	private int length = 0;

	private Vector2Int offset;

	private NoiseGUI noiseGUI;

	private int octaves = 1;
	private float frequency = 1.0f;
	private float amplitude = 1.0f;

	public Fractal2DNode() {
	  noiseGUI = new NoiseGUI();
	  noiseFunction = noiseGUI.noiseFunction;
	  fractalNoise = new FractalNoise(noiseFunction, octaves, frequency, amplitude);
	}

	public override void NodeGUI() {
	  base.NodeGUI();

	  GUILayout.BeginHorizontal();
	  GUILayout.BeginVertical();
	  GUILayout.Label("octaves:");
	  GUILayout.Label("frequency:");
	  GUILayout.Label("amplitude:");
	  GUILayout.EndVertical();
	  GUILayout.BeginVertical();
	  octaves = RTEditorGUI.IntField(octaves);
	  frequency = RTEditorGUI.FloatField(frequency);
	  amplitude = RTEditorGUI.FloatField(amplitude);
	  GUILayout.EndVertical();
	  GUILayout.EndHorizontal();


	  noiseFunction = noiseGUI.Display();

	  if (GUI.changed || noiseGUI.changed) {
	    fractalNoise = new FractalNoise(noiseFunction, octaves, frequency, amplitude);
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
	  float noiseValue = fractalNoise.Sample2D(offset.x+x/(float)width, offset.y + z/(float)length);
	  height = noiseValue;
	  return true;
	}
}
