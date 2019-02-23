using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node (false, "Scripts/Node/Noise3DNode")]
public class Noise3DNode : Node 
{
	public const string ID = "Noise3D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Noise"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	[ValueConnectionKnob("X", Direction.In, "Int")]
		public ValueConnectionKnob xConnection;
	[ValueConnectionKnob("Y", Direction.In, "Int")]
		public ValueConnectionKnob yConnection;
	[ValueConnectionKnob("Z", Direction.In, "Int")]
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

