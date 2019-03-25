using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

public enum MARCHING_MODE {  CUBES, TETRAHEDRON };

[Node (false, "Vertex")]
public class Noise3DNode : Node 
{
	public const string ID = "Vertex";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Vertex Generation"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	[ValueConnectionKnob("Input", Direction.In, "Block")]
		public ValueConnectionKnob input;

	private MARCHING_MODE mode = MARCHING_MODE.CUBES;

	public override void NodeGUI () 
	{
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		input.DisplayLayout ();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal ();
		mode = (MARCHING_MODE)RTEditorGUI.EnumPopup (new GUIContent ("Vertex Generation", "The type of Vertex generation"), mode);
	}

	public override bool Calculate() {
	  return true;
	}
}

