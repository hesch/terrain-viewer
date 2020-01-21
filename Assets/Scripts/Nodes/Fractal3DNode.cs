using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;
using UnityEngine;

[Node(false, "Fractal3D")]
public class Fractal3DNode : VoxelNode<Voxel>
{
    public const string ID = "Fractal3D";
    public override string GetID { get { return ID; } }

    public override string Title { get { return "Fractal3D"; } }
    public override Vector2 DefaultSize { get { return new Vector2(150, 200); } }

    private INoise noiseFunction;
    private FractalNoise fractalNoise;

    private int width = 0;
    private int height = 0;
    private int length = 0;

    private Vector2Int offset;

    public NoiseGUI noiseGUI;
    public string noiseDesc;

    public int octaves = 1;
    public float frequency = 1.0f;
    public float amplitude = 1.0f;

    public void OnEnable()
    {
        noiseGUI = new NoiseGUI(noiseDesc);
        noiseFunction = noiseGUI.noiseFunction;
        fractalNoise = new FractalNoise(noiseFunction, octaves, frequency, amplitude);
    }

    public override void NodeGUI()
    {
        base.NodeGUI();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label("octaves:");
        GUILayout.Label("frequency:");
        GUILayout.Label("amplitude:");
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        octaves = RTEditorGUI.IntField(octaves);
        frequency = RTEditorGUI.FloatField(frequency);
        amplitude = RTEditorGUI.FloatField(amplitude);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();


        noiseFunction = noiseGUI.Display();

        if (GUI.changed || noiseGUI.changed)
        {
            fractalNoise = new FractalNoise(noiseFunction, octaves, frequency, amplitude);
            noiseDesc = noiseGUI.noiseDesc();
            NodeEditor.curNodeCanvas.OnNodeChange(this);
        }
    }

    protected override void CalculationSetup(VoxelBlock<Voxel> block)
    {
        base.CalculationSetup(block);
        width = block.Width;
        height = block.Height;
        length = block.Length;
        offset = block.Offset;
    }

    protected override bool CalculateVoxel(Voxel voxel, int x, int y, int z)
    {
        float noiseValue = fractalNoise.Sample3D(offset.x + x / (float)width, y / (float)height, offset.y + z / (float)length);
        voxel.Data = noiseValue;
        return true;
    }
}
