using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

[Node (false, "Scripts/Node/Noise3DNode")]
public class Noise3DNode : Node 
{
	public const string ID = "Noise3D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Noise"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	private INoise perlin;
	private FractalNoise fractal;

	[ValueConnectionKnob("X", Direction.In, "Float")]
		public ValueConnectionKnob xConnection;
	[ValueConnectionKnob("Y", Direction.In, "Float")]
		public ValueConnectionKnob yConnection;
	[ValueConnectionKnob("Z", Direction.In, "Float")]
		public ValueConnectionKnob zConnection;
	[ValueConnectionKnob("Noise", Direction.Out, "Float")]
		public ValueConnectionKnob outputConnection;

	public Noise3DNode() {
	  perlin = new PerlinNoise(1337, 2.0f);
	  fractal = new FractalNoise(perlin, 3, 1.0f);
	}

	/*public override void NodeGUI () 
	{
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		xConnection.DisplayLayout ();
		yConnection.DisplayLayout ();
		zConnection.DisplayLayout ();
		GUILayout.EndVertical();
		outputConnection.DisplayLayout();
		GUILayout.EndHorizontal ();
	}*/

	public override bool Calculate() {
	  float x = xConnection.GetValue<float>();
	  float y = yConnection.GetValue<float>();
	  float z = zConnection.GetValue<float>();
	  float noiseValue = fractal.Sample3D(x, y, z);
	  Debug.Log("inputs: " + x + ", " + y + ", " + z);
	  Debug.Log("Calculated noise: " + noiseValue);
	  outputConnection.SetValue<float>(noiseValue);
	  return true;
	}
}

