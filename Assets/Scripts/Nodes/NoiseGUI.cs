using UnityEngine;
using NodeEditorFramework.Utilities;
using ProceduralNoiseProject;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

public class NoiseGUI {
	public INoise noiseFunction;
	private Type[] noiseFunctions; 
	public System.Object[] noiseParameters; 

	public int noiseTypeIndex = 0;
	public string selectedNoiseType;

	public bool changed { get; set; }
	private bool outOfBandChange = false;

	private bool initialized = false;

	public NoiseGUI(string noiseDesc) {
	  if (noiseDesc != null) {
	    string[] splits = noiseDesc.Split(new char[]{';'}, 2);
	    if (splits.Count() == 2) {
	      selectedNoiseType = splits[0];

	      XmlSerializer serializer = new XmlSerializer(typeof(object[]));
	      noiseParameters = (object[])serializer.Deserialize(new StringReader(splits[1]));
	    } else {
	      Debug.Log("invalid NoiseDesc: " + noiseDesc);
	    }
	  }
	  init(Loader.currentAssembly);
	  Loader.listener += ass => init(ass);
	}

	public NoiseGUI() : this(null) {
	}

	private void init(Assembly special) {
	  noiseFunctions = ReflectionUtility.getSubTypes(typeof(Noise));
	  if (special != null) {
	    List<Type> lazyLoadedNoise = special.GetTypes ()
	      .Where ((Type T) => 
		      (T.IsClass && !T.IsAbstract) 
		      && T.IsSubclassOf (typeof(Noise))).ToList();

	    lazyLoadedNoise.AddRange(noiseFunctions);
	    noiseFunctions = lazyLoadedNoise.ToArray();
	  }

	  noiseTypeIndex = Array.FindIndex(noiseFunctions, t => t.ToString() == selectedNoiseType);

	  bool useDefault = noiseTypeIndex == -1;

	  if(useDefault) {
	    noiseTypeIndex = 0;
	  }

	  selectedNoiseType = noiseFunctions[noiseTypeIndex].ToString();
	  ConstructorInfo ctor = noiseFunctions[noiseTypeIndex].GetConstructors()[0];

	  if(useDefault) {
	    noiseParameters = defaultParams(ctor);
	  }

	  noiseFunction = (Noise)ctor.Invoke(noiseParameters);
	  outOfBandChange = true;
	}

	public INoise Display() {
	  changed = outOfBandChange;
	  outOfBandChange = false;
	  string[] names = noiseFunctions.Select(n => n.Name).ToArray();
	  RTEditorGUI.Popup (new GUIContent ("Noise", "The noise type to use"), noiseTypeIndex, names, selected => {
	      noiseTypeIndex = selected;
	      Type selectedNoise = noiseFunctions[selected];
	      selectedNoiseType = selectedNoise.ToString();
	      ConstructorInfo ctor = selectedNoise.GetConstructors()[0];

	      noiseParameters = defaultParams(ctor);
	      noiseFunction = (Noise)ctor.Invoke(noiseParameters);

	      outOfBandChange = true;
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

	public string noiseDesc() {
	  string res = "" + noiseFunction + ";";

	  XmlSerializer xmlSerializer = new XmlSerializer(typeof(object[]));
	  StringWriter stringWriter = new StringWriter();
	  xmlSerializer.Serialize(stringWriter, noiseParameters);
	  res += stringWriter.ToString();

	  return res;
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
