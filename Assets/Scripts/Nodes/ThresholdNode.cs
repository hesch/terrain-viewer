using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

[Node (false, "Threshold")]
public class ThresholdNode : VoxelNode<Voxel>
{
	public const string ID = "Threshold";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Threshold"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	[ValueConnectionKnob("Threshold", Direction.In, "Float")]
		public ValueConnectionKnob thresholdConnection;

	private float threshold = 0.5f;
	public override void NodeGUI () 
	{
	  base.NodeGUI();
	  GUILayout.BeginHorizontal ();
	  GUILayout.BeginVertical ();

	  // First input
	  if (thresholdConnection.connected ())
	    GUILayout.Label (thresholdConnection.name);
	  else
	    threshold = RTEditorGUI.FloatField (GUIContent.none, threshold);
	  thresholdConnection.SetPosition ();

	  GUILayout.EndVertical ();
	  GUILayout.EndHorizontal ();

	  if (GUI.changed)
	    NodeEditor.curNodeCanvas.OnNodeChange (this);
	}

	protected override bool CalculateVoxel(Voxel voxel, int x, int y, int z) {
	  if(thresholdConnection.connected()) {
	    threshold = thresholdConnection.GetValue<float>();
	  }
	  voxel.Data = voxel.Data > threshold ? 1.0f : 0.0f;
	  return true;
	}
}

