using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node(false, "Normalize")]
public class NormalizeNode : VoxelNode<Voxel> {
  public const string ID = "Normalize";
  public override string GetID { get { return ID; } }

  public override string Title { get { return "Normalize"; } }
  public override Vector2 DefaultSize { get { return new Vector2 (200, 100); } }

  public float inputLowerBound = -1.0f;
  public float inputUpperBound = 1.0f;
  public float outputLowerBound = 0.0f;
  public float outputUpperBound = 1.0f;

  private float dInput = 0.0f;
  private float dOutput = 0.0f;

  public override void NodeGUI() {
    base.NodeGUI();
    GUILayout.BeginHorizontal();
    GUILayout.BeginVertical();
    GUILayout.Label("");
    GUILayout.Label("upper");
    GUILayout.Label("lower");
    GUILayout.EndVertical();
    GUILayout.BeginVertical();
    GUILayout.Label("input");
    inputUpperBound = RTEditorGUI.FloatField (GUIContent.none, inputUpperBound);
    inputLowerBound = RTEditorGUI.FloatField (GUIContent.none, inputLowerBound);
    GUILayout.EndVertical();
    GUILayout.BeginVertical();
    GUILayout.Label("output");
    outputUpperBound = RTEditorGUI.FloatField (GUIContent.none, outputUpperBound);
    outputLowerBound = RTEditorGUI.FloatField (GUIContent.none, outputLowerBound);
    GUILayout.EndVertical();
    GUILayout.EndHorizontal();
  }

  protected override void CalculationSetup(VoxelBlock<Voxel> block) {
    dInput = inputUpperBound - inputLowerBound;
    dOutput = outputUpperBound - outputLowerBound;
  }

  protected override bool CalculateVoxel(Voxel voxel, int x, int y, int z) {
    if (dInput == 0.0f) {
      return false;
    }
    voxel.Data = (voxel.Data - inputLowerBound)/dInput * dOutput + outputLowerBound;
    return true;
  }

}
