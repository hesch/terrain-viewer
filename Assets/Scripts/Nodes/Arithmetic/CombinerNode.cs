using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System;

public enum CombiningMode {
  Average,
  Add,
  Subtract,
  Divide,
  Multiply,
  Threshold
}

[Node(false, "Combiner")]
public class CombinerNode : Node {

  public const string ID = "Combiner";
  public override string GetID { get { return ID; } }

  public override string Title { get { return "Combiner"; } }
  public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

  public CombiningMode mode = CombiningMode.Average;
  
  [ValueConnectionKnob("Block 1", Direction.In, "Block")]
    public ValueConnectionKnob block1Connection;
  [ValueConnectionKnob("Block 2", Direction.In, "Block")]
    public ValueConnectionKnob block2Connection;
  [ValueConnectionKnob("Output", Direction.Out, "Block")]
    public ValueConnectionKnob outputConnection;

  public override void NodeGUI() {
    GUILayout.BeginHorizontal ();
    GUILayout.BeginVertical ();

    GUILayout.Label (block1Connection.name);
    block1Connection.SetPosition ();

    GUILayout.Label (block2Connection.name);
    block2Connection.SetPosition ();

    GUILayout.EndVertical ();
    GUILayout.BeginVertical ();

    GUILayout.Label (outputConnection.name);
    outputConnection.SetPosition ();

    GUILayout.EndVertical ();
    GUILayout.EndHorizontal ();
    RTEditorGUI.EnumPopup (new GUIContent ("Combine", "The combination type to use"), mode, m => {
      if (mode != m) {
	mode = m;
	NodeEditor.curNodeCanvas.OnNodeChange(this);
      }
    });

        if (GUI.changed)
        {
            NodeEditor.curNodeCanvas.OnNodeChange(this);
        }
  }

  public override bool Calculate() {
    if(!block1Connection.connected() || !block2Connection.connected()) {
      return false;
    }
    VoxelBlock<Voxel> block1 = block1Connection.GetValue<VoxelBlock<Voxel>>();
        VoxelBlock<Voxel> block2 = block2Connection.GetValue<VoxelBlock<Voxel>>();

        int overlap = block1.Overlap;
        int width = block1.Width;
        int height = block1.Height;
        int length = block1.Length;
        for (int y = 0; y < height + 2*overlap; y++)
    {
            VoxelLayer<Voxel> l1 = block1.Layers[y];
            VoxelLayer<Voxel> l2 = block2.Layers[y];
        for (int x = 0; x < width + 2*overlap; x++)
        {
            for (int z = 0; z < length + 2*overlap; z++)
            {
                    var v1 = l1.Layer[x, z];
                    var v2 = l2.Layer[x, z];
                    switch (mode)
                {
                    case CombiningMode.Average:
                        v1.Data = (v1.Data + v2.Data) / 2;
                        break;
                    case CombiningMode.Add:
                            v1.Data = v1.Data + v2.Data;
                        break;
                    case CombiningMode.Subtract:
                            v1.Data = v1.Data - v2.Data;
                        break;
                    case CombiningMode.Divide:
                            if (v2.Data != 0)
                            {
                                v1.Data = v1.Data / v2.Data;
                            }
                        break;
                    case CombiningMode.Multiply:
                            v1.Data = v1.Data * v2.Data;
                        break;
                }
            }
        }
    }
    outputConnection.SetValue(block1);
	
    return true;
  }
}
