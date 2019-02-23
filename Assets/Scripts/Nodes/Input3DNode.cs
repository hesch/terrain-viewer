using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node (false, "Scripts/Node/Input3DNode")]
public class Input3DNode : Node 
{
	public const string ID = "Input3D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Input Node"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	public Vector3Int value = Vector3Int.zero;

	[ValueConnectionKnob("X", Direction.Out, "Int")]
		public ValueConnectionKnob xConnection;
	[ValueConnectionKnob("Y", Direction.Out, "Int")]
		public ValueConnectionKnob yConnection;
	[ValueConnectionKnob("Z", Direction.Out, "Int")]
		public ValueConnectionKnob zConnection;

	public override void NodeGUI () 
	{
		name = RTEditorGUI.TextField (name);

		foreach (ValueConnectionKnob knob in connectionKnobs) 
			knob.DisplayLayout ();
	}

	public override bool Calculate() {
		xConnection.SetValue<int>(value.x);
		yConnection.SetValue<int>(value.y);
		zConnection.SetValue<int>(value.z);
		return true;
	}
}
