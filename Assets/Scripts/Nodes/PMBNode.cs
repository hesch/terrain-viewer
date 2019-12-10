using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;
using System.Collections.Generic;


[Node (false, "PMB")]
class PMBNode : Node {
	public const string ID = "PMB";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "PMB Generation"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	[ValueConnectionKnob("Input", Direction.In, "Block")]
		public ValueConnectionKnob input;
	[ValueConnectionKnob("Surface", Direction.In, "Float")]
		public ValueConnectionKnob surfaceConnection;

	public float surface = 0.5f;

	public List<Vector3> Vertices { get; set; }
	public List<int> Indices { get; set; }
	public List<Vector3> Normals { get; set; }
	public VoxelBlock<Voxel> Block { get; set; }
	public override void NodeGUI () 
	{
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		input.DisplayLayout ();

		GUILayout.BeginHorizontal();
		GUILayout.Label (surfaceConnection.name);
		if (!surfaceConnection.connected ())
		  surface = RTEditorGUI.FloatField (GUIContent.none, surface);

		surfaceConnection.SetPosition ();
		GUILayout.EndHorizontal();
		
		GUILayout.EndVertical();
		GUILayout.EndHorizontal ();

		if (GUI.changed) {
		  NodeEditor.curNodeCanvas.OnNodeChange(this);
		}
	}


}
