using UnityEngine;
using System;
using NodeEditorFramework;

public class LayerConnectionType : ValueConnectionType
{
  public override string Identifier { get { return "Layer"; } }
  public override Color Color { get { return Color.blue; } }
  public override Type Type { get { return typeof(VoxelLayer); } }
}
