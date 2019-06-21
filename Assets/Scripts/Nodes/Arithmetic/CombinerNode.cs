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
      NodeEditor.curNodeCanvas.OnNodeChange (this);
  }

  public override bool Calculate() {
    if(!block1Connection.connected() || !block2Connection.connected()) {
      return false;
    }
    VoxelBlock<Voxel> block1 = new VoxelBlock<Voxel>(block1Connection.GetValue<VoxelBlock<Voxel>>());
    VoxelBlock<Voxel> block2 = new VoxelBlock<Voxel>(block2Connection.GetValue<VoxelBlock<Voxel>>());

    bool success = false;
    switch (mode) {
      case CombiningMode.Average:	
	success = IterateBlocks(block1, block2, (v1, v2) => {
	    v1.Data = (v1.Data + v2.Data) / 2;
	    return true;
	});
      break;
      case CombiningMode.Add:
	success = IterateBlocks(block1, block2, (v1, v2) => {
	    v1.Data = v1.Data + v2.Data;
	    return true;
	});
      break;
      case CombiningMode.Subtract:
	success = IterateBlocks(block1, block2, (v1, v2) => {
	    v1.Data = v1.Data - v2.Data;
	    return true;
	});
      break;
      case CombiningMode.Divide:
	success = IterateBlocks(block1, block2, (v1, v2) => {
	    if (v2.Data == 0)
	      return false;
	    v1.Data = v1.Data / v2.Data;
	    return true;
	});
      break;
      case CombiningMode.Multiply:
	success = IterateBlocks(block1, block2, (v1, v2) => {
	    v1.Data = v1.Data * v2.Data;
	    return true;
	});
      break;
    }

    outputConnection.SetValue(block1);
	
    return success;
  }

  private bool IterateBlocks(VoxelBlock<Voxel> b1, VoxelBlock<Voxel> b2, Func<Voxel, Voxel, bool> combiner) {
    for(int x = -b1.Overlap; x < b1.Width + b1.Overlap; x++) {
      for(int y = -b1.Overlap; y < b1.Height + b1.Overlap; y++) {
	for(int z = -b1.Overlap; z < b1.Length + b1.Overlap; z++) {
	  if(!combiner(b1[x, y, z], b2[x, y, z]))
	    return false;
	}
      }
    }
    return true;
  }
}
