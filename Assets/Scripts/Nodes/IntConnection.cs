using UnityEngine;
using System;
using NodeEditorFramework;

public class IntConnectionType : ValueConnectionType
{
  public override string Identifier { get { return "Int"; } }
  public override Color Color { get { return Color.green; } }
  public override Type Type { get { return typeof(int); } }
}
