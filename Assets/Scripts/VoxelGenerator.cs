using System.Diagnostics;
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


  public float CalculateCanvas(float x, float y, float z) {
    canvas = GetComponent<NoiseNodeEditor>().GetCanvas();
    AssertCanvas();
    NodeEditor.checkInit(false);
    canvas.Validate();

    List<Node> inputNodes = canvas.nodes.Where((Node node) => node.isInput()).ToList();
    List<Node> outputNodes = canvas.nodes.Where((Node node) => node.isOutput()).ToList();

    foreach(Node inputNode in inputNodes) {
      if(inputNode is Input3DNode) {
	Input3DNode input = inputNode as Input3DNode;
	input.value = new Vector3(x, y, z);
      }
    }

    canvas.TraverseAll();
    
    float output = 0.0f;

    /*foreach(Node outputNode in outputNodes) {
      output = (outputNode.outputKnobs[0] as ValueConnectionKnob).GetValue<float>();
      string outString = outputNode.name + ": ";
      List<ConnectionKnob> knobs = outputNode.outputKnobs;
      foreach(ValueConnectionKnob knob in knobs.OfType<ValueConnectionKnob>()) {
	string knobValue = knob.IsValueNull ? "NULL" : knob.GetValue().ToString();
	outString += knob.styleID + " " + knob.name + " = " + knobValue + "; ";
      }
      UnityEngine.Debug.Log(outString);
    }*/

    output = (outputNodes[0].outputKnobs[0] as ValueConnectionKnob).GetValue<float>();

    return output;
  }

  public float[] GetVoxels() {
    int width = GetWidth();
    int height = GetHeight();
    int length = GetLength();

    Stopwatch stopwatch = Stopwatch.StartNew();
    float[] voxels = new float[width * height * length];
    for (int x = 0; x < width; x++)
    {
      for (int z = 0; z < length; z++)
      {
	for (int y = 0; y < height; y++)
	{
	  float fx = x / (width - 1.0f);
	  float fy = y / (height - 1.0f);
	  float fz = z / (length - 1.0f);
	  int idx = x + y * width + z * width * height;
	  float value = CalculateCanvas(fx, fy, fz);
	  voxels[idx] = value;
	}
      }
    }
    stopwatch.Stop();
    UnityEngine.Debug.Log("Calculating voxels took: " + stopwatch.Elapsed);
    return voxels;
  }

  private int size = 32;

  public int GetWidth() {
    return size;
  }

  public int GetHeight() {
    return size;
  }

  public int GetLength() {
    return size;
  }
}
