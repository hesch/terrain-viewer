using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

[Node (false, "Noise3D")]
public class Noise3DNode : VoxelNode<Voxel> 
{
	public const string ID = "Noise3D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Noise3D"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	private INoise perlin;
	private FractalNoise fractal;

	private int width = 0;
	private int height = 0;
	private int length = 0;
	private Vector2Int offset;

	public Noise3DNode() {
	  perlin = new PerlinNoise(1337, 2.0f);
	  fractal = new FractalNoise(perlin, 3, 1.0f);
	}

	protected override void CalculationSetup(VoxelBlock<Voxel> block) {
	 width = block.Width;
	 height = block.Height;
	 length = block.Length;
	 offset = block.Offset;
	}

	protected override bool CalculateVoxel(Voxel voxel, int x, int y, int z) {
	  float noiseValue = fractal.Sample3D(offset.x + x/(float)width, y/(float)height, offset.y + z/(float)length);
	  float normalizedNoise = 1-(noiseValue+1)/2;
	  voxel.Data = normalizedNoise;
	  return true;
	}
}

