using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;

[Node (false, "Noise2D")]
public class Noise2DNode : HeightMapNode<Voxel> 
{
	public const string ID = "Noise2D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Noise2D"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 100); } }

	private NoiseType noiseType;
	private INoise noiseFunction;
	private FractalNoise fractal;

	private int width = 0;
	private int height = 0;
	private int length = 0;

	private Vector2Int offset;

	public Noise2DNode() {
	  noiseFunction = new PerlinNoise(1337, 2.0f);
	  fractal = new FractalNoise(noiseFunction, 3, 1.0f);
	}

	public override void NodeGUI() {
	  base.NodeGUI();
	  RTEditorGUI.EnumPopup (new GUIContent ("Noise", "The noise type to use"), noiseType, n => {
	      if (noiseType != n) {
	      noiseType = n;
	      switch (noiseType) {
	      case NoiseType.Perlin:
		noiseFunction = new PerlinNoise(1337, 2.0f);
	      break;
	      case NoiseType.Simplex:
		noiseFunction = new SimplexNoise(1337, 2.0f);
	      break;
	      case NoiseType.Value:
		noiseFunction = new ValueNoise(1337, 2.0f);
	      break;
	      case NoiseType.Voronoi:
		noiseFunction = new VoronoiNoise(1337, 2.0f);
	      break;
	      case NoiseType.Worley:
		noiseFunction = new WorleyNoise(1337, 2.0f, 1.0f);
	      break;
	      }
	      fractal = new FractalNoise(noiseFunction, 3, 1.0f);
	      NodeEditor.curNodeCanvas.OnNodeChange(this);
	    }
	  });
	}

	protected override void CalculationSetup(VoxelBlock<Voxel> block) {
	  base.CalculationSetup(block);
	  width = block.Width;
	  height = block.Height;
	  length = block.Length;
	  offset = block.Offset;
	}

	protected override bool CalculateHeight(out float height, int x, int z) {
	  float noiseValue = fractal.Sample2D(offset.x+x/(float)width, offset.y + z/(float)length);
	  float normalizedNoise = 1-(noiseValue+1)/2;
	  height = normalizedNoise;
	  return true;
	}
}

