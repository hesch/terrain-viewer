using UnityEngine;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;
using System.Reflection;
using System;
using System.Linq;

public class NoiseGUI {
	public INoise noiseFunction;
	private Type[] noiseFunctions; 
	private System.Object[] noiseParameters; 

	private int noiseTypeIndex = 0;

	public bool changed { get; set; }

	public NoiseGUI() {
	  noiseFunctions = ReflectionUtility.getSubTypes(typeof(Noise));
	  Type t = noiseFunctions[0];
	  ConstructorInfo ctor = noiseFunctions[0].GetConstructors()[0];
	  noiseParameters = defaultParams(ctor);
	  noiseFunction = (Noise)ctor.Invoke(noiseParameters);
	}

	public INoise Display() {
	  changed = false;
	  string[] names = noiseFunctions.Select(n => n.Name).ToArray();
	  RTEditorGUI.Popup (new GUIContent ("Noise", "The noise type to use"), noiseTypeIndex, names, selected => {
	      noiseTypeIndex = selected;
	      Type func = noiseFunctions[selected];
	      ConstructorInfo ctor = func.GetConstructors()[0];

	      noiseParameters = defaultParams(ctor);
	      noiseFunction = (Noise)ctor.Invoke(noiseParameters);

	      changed = true;
	  });

	  GUILayout.BeginVertical();
	  ConstructorInfo ctor2 = noiseFunctions[noiseTypeIndex].GetConstructors()[0];
	  ParameterInfo[] pInfo = ctor2.GetParameters();
	  for(int i = 0; i < pInfo.Length; i++) {
	    GUILayout.BeginHorizontal();
	    GUILayout.Label(pInfo[i].Name);
	    if(pInfo[i].ParameterType == typeof(int)) {
	      noiseParameters[i] = RTEditorGUI.IntField((int)noiseParameters[i]);
	    } else if (pInfo[i].ParameterType == typeof(float)) {
	      noiseParameters[i] = RTEditorGUI.FloatField((float)noiseParameters[i]);
	    }
	    GUILayout.EndHorizontal();
	  }
	  GUILayout.EndVertical();

	  if (GUI.changed) {
	    noiseFunction = (Noise)ctor2.Invoke(noiseParameters);
	    changed = true;
	  }

	  return noiseFunction;
	}

	private System.Object[] defaultParams(ConstructorInfo ctor) {
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
	    return parameters;
	}
}
