using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

[Node (false, "Noise")]
public class ThresholdNode : Node 
{
	public const string ID = "Threshold";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Threshold"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	[ValueConnectionKnob("Input", Direction.In, "Float")]
		public ValueConnectionKnob inputConnection;
	[ValueConnectionKnob("Threshold", Direction.In, "Float")]
		public ValueConnectionKnob thresholdConnection;
	[ValueConnectionKnob("Noise", Direction.Out, "Float")]
		public ValueConnectionKnob outputConnection;

	private float threshold = 0.5f;
	public override void NodeGUI () 
	{
	  GUILayout.BeginHorizontal ();
	  GUILayout.BeginVertical ();

	  inputConnection.DisplayLayout();

	  // First input
	  if (thresholdConnection.connected ())
	    GUILayout.Label (thresholdConnection.name);
	  else
	    threshold = RTEditorGUI.FloatField (GUIContent.none, threshold);
	  thresholdConnection.SetPosition ();

	  GUILayout.EndVertical ();
	  GUILayout.BeginVertical ();

	  // Output
	  outputConnection.DisplayLayout ();

	  GUILayout.EndVertical ();
	  GUILayout.EndHorizontal ();

	  if (GUI.changed)
	    NodeEditor.curNodeCanvas.OnNodeChange (this);
	}

	public override bool Calculate() {
	  float input = inputConnection.GetValue<float>();
	  if(thresholdConnection.connected()) {
	    threshold = thresholdConnection.GetValue<float>();
	  }
	  outputConnection.SetValue<float>(input > threshold ? 0.0f : 1.0f);
	  return true;
	}
}

