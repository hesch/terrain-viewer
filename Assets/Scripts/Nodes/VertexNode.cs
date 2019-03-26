using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

public enum MarchingMode {  Cubes, Tetrahedron };

[Node (false, "Vertex")]
public class VertexNode : Node 
{
	public const string ID = "Vertex";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Vertex Generation"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	[ValueConnectionKnob("Input", Direction.In, "Block")]
		public ValueConnectionKnob input;

	private MarchingMode mode = MarchingMode.Cubes;

	public override void NodeGUI () 
	{
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		input.DisplayLayout ();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal ();
		mode = (MarchingMode)RTEditorGUI.EnumPopup (new GUIContent ("Vertex Generation", "The type of Vertex generation"), mode);
	}

	public override bool Calculate() {
	  return true;
	}
}

