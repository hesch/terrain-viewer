using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NodeEditorFramework;

public class VoxelGenerator : MonoBehaviour {
  public NodeCanvas canvas;

  public void AssertCanvas() {
    if(canvas == null)
      throw new UnityException("No canvas specified to calculate on " + name + "!");
  }


  public void CalculateCanvas () {
    canvas = GetComponent<NoiseNodeEditor>().GetCanvas();
    AssertCanvas();
    NodeEditor.checkInit(false);
    canvas.Validate();
    canvas.TraverseAll();
    DebugOutputResults();
  }

  private void DebugOutputResults() {
    AssertCanvas();
    List<Node> outputNodes = canvas.nodes.Where((Node node) => node.isOutput()).ToList();

    foreach(Node outputNode in outputNodes) {
      string outString = outputNode.name + ": ";
      List<ConnectionKnob> knobs = outputNode.outputKnobs;
      foreach(ValueConnectionKnob knob in knobs.OfType<ValueConnectionKnob>()) {
	string knobValue = knob.IsValueNull ? "NULL" : knob.GetValue().ToString();
	outString += knob.styleID + " " + knob.name + " = " + knobValue + "; ";
      }
      Debug.Log(outString);
    }
  }
}
