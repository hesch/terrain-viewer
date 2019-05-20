using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;
using System.Reflection;
using System;
using System.Linq;

[Node (false, "Noise2D")]
public class Noise2DNode : HeightMapNode<Voxel> 
{
	public const string ID = "Noise2D";
	public override string GetID { get { return ID; } }

	public override string Title { get { return "Noise2D"; } }
	public override Vector2 DefaultSize { get { return new Vector2 (150, 200); } }

	private NoiseType noiseType;
	private INoise noiseFunction;
	private FractalNoise fractal;

	private int width = 0;
	private int height = 0;
	private int length = 0;

	private Vector2Int offset;

	private Type[] noiseFunctions; 

	private int noiseTypeIndex = 0;

	public Noise2DNode() {
	  noiseFunction = new PerlinNoise(1337, 2.0f);
	  fractal = new FractalNoise(noiseFunction, 3, 1.0f);
	  noiseFunctions = ReflectionUtility.getSubTypes(typeof(Noise));
	  foreach(Type func in noiseFunctions) {
	    String typeInfo = func.Name;
	    ConstructorInfo[] ctors = func.GetConstructors();
	    ParameterInfo[] parameters = ctors[0].GetParameters();
	    foreach(ParameterInfo p in parameters) {
	     typeInfo += "; " + p.ParameterType.Name + " " + p.Name; 
	    }
	    Debug.Log(typeInfo);
	  }
	}

	public override void NodeGUI() {
	  base.NodeGUI();
	  string[] names = noiseFunctions.Select(n => n.Name).ToArray();
	  RTEditorGUI.Popup (new GUIContent ("Noise", "The noise type to use"), noiseTypeIndex, names, selected => {
	      Debug.Log("selected: " + selected);
	      noiseTypeIndex = selected;
	      Type func = noiseFunctions[selected];
	      ConstructorInfo ctor = func.GetConstructors()[0];
	      ParameterInfo[] paramsMetadata = ctor.GetParameters();
	      System.Object[] parameters = new System.Object[paramsMetadata.Length];
	      for(int i = 0; i < paramsMetadata.Length; i++) {
		if (paramsMetadata[i].HasDefaultValue) {
		  parameters[i] = paramsMetadata[i].DefaultValue;
		} else {
		  if(paramsMetadata[i].ParameterType == typeof(int)) {
		    parameters[i] = 1;
		  } else if (paramsMetadata[i].ParameterType == typeof(float)) {
		    parameters[i] = 1.0f;
		  }
		}
	      }

	      noiseFunction = (Noise)ctor.Invoke(parameters);

	      fractal = new FractalNoise(noiseFunction, 3, 1.0f);
	      NodeEditor.curNodeCanvas.OnNodeChange(this); 
	  });

	  GUILayout.BeginVertical();
	  ConstructorInfo ctor2 = noiseFunctions[noiseTypeIndex].GetConstructors()[0];
	  foreach(ParameterInfo p in ctor2.GetParameters()) {
	    GUILayout.BeginHorizontal();
	    GUILayout.Label(p.Name);
	    RTEditorGUI.FloatField(1.0f);
	    GUILayout.EndHorizontal();
	  }
	  GUILayout.EndVertical();
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

