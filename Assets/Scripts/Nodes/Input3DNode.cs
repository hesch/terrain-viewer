using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node (false, "Noise")]
public class Input3DNode : Node 
{
	public const string ID = "Input3D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Input Node"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	public Vector3 value = Vector3.zero;

	[ValueConnectionKnob("X", Direction.Out, "Float")]
		public ValueConnectionKnob xConnection;
	[ValueConnectionKnob("Y", Direction.Out, "Float")]
		public ValueConnectionKnob yConnection;
	[ValueConnectionKnob("Z", Direction.Out, "Float")]
		public ValueConnectionKnob zConnection;

	public override void NodeGUI () 
	{
		name = RTEditorGUI.TextField (name);

		foreach (ValueConnectionKnob knob in connectionKnobs) 
			knob.DisplayLayout ();
	}

	public override bool Calculate() {
		xConnection.SetValue<float>(value.x);
		yConnection.SetValue<float>(value.y);
		zConnection.SetValue<float>(value.z);
		return true;
	}
}
