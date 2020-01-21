using NodeEditorFramework;
using System;
using UnityEngine;

public class BlockConnectionType : ValueConnectionType
{
    public override string Identifier { get { return "Block"; } }
    public override Color Color { get { return Color.green; } }
    public override Type Type { get { return typeof(VoxelBlock<Voxel>); } }
}
