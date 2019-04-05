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

	private NoiseType noiseType;
	private INoise noiseFunction;
	private FractalNoise fractal;

	private int width = 0;
	private int height = 0;
	private int length = 0;
	private Vector2Int offset;

	public Noise3DNode() {
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

