using UnityEngine;
using System;
using NodeEditorFramework;

public class VoxelConnectionType : ValueConnectionType
{
  public override string Identifier { get { return "Voxel"; } }
  public override Color Color { get { return Color.red; } }
  public override Type Type { get { return typeof(Voxel); } }
}
