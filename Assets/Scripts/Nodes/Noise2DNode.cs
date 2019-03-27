using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

[Node (false, "Noise2D")]
public class Noise2DNode : VoxelNode<Voxel> 
{
	public const string ID = "Noise2D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Noise2D"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	private INoise perlin;
	private FractalNoise fractal;

	private int width = 0;
	private int height = 0;
	private int length = 0;

	private int offsetX = 0;
	private int offsetY = 0;

	public Noise2DNode() {
	  perlin = new PerlinNoise(1337, 2.0f);
	  fractal = new FractalNoise(perlin, 3, 1.0f);
	}

	protected override void CalculationSetup(VoxelBlock<Voxel> block) {
	 width = block.Width;
	 height = block.Height;
	 length = block.Length;
	 offsetX = block.OffsetX;
	 offsetY = block.OffsetY;
	}

	protected override bool CalculateVoxel(Voxel voxel, int x, int y, int z) {
	  float noiseValue = fractal.Sample2D(offsetX + x/(float)width, offsetY + z/(float)length);
	  float normalizedNoise = 1-(noiseValue+1)/2;
	  voxel.Data = y/(float)height > normalizedNoise ? 0.0f : 1.0f;
	  return true;
	}
}

