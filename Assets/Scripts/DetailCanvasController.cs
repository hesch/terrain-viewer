using UnityEngine;
using UnityEngine.UI;
using System;
using NodeEditorFramework;

public class DetailCanvasController : MonoBehaviour {
  private Canvas canvas;
  private LayerTexture texture;
  private VertexDisplay display;
  private NoiseCanvasType noiseCanvas;
  public Camera camera;

  public bool Active {
    get {
      return canvas.enabled;
    }
    set {
      canvas.enabled = value;
      camera.rect = value ? new Rect(0.5f, 0.5f, 0.5f, 0.5f) : new Rect(0.5f, 0.0f, 0.5f, 1.0f);
      display.enabled = !value;
      if (value) {
	noiseCanvas.ConfigureComputation(() => offsetGenerator(), (verts, indices, block) => {
	   //TODO 
	});
      } else {
	display.configureComputation();
      }
    }
  }

  private VoxelBlock<Voxel> block;
  public GameObject displayedObject;
  public GameObject DisplayedObject {
    get {
      return displayedObject;
    }
    set {
      displayedObject = value;
      block = (VoxelBlock<Voxel>)displayedObject.GetComponent<BlockInfo>().Block;
    }
  }
  private int layerIndex = 0;

  public void Update() {
    if (Input.GetKeyUp(KeyCode.Escape)) {
      Active = false;
    }
  }

  public void Start() {
    canvas = GetComponent<Canvas>();
    texture = GetComponentInChildren<LayerTexture>();
    display = UnityEngine.Object.FindObjectOfType<VertexDisplay>();
    NoiseNodeEditor editor = UnityEngine.Object.FindObjectOfType<NoiseNodeEditor>();
    noiseCanvas = editor.GetCanvas() as NoiseCanvasType;
    Active = false;
  }

  public void changeLayerIndex(int amount) {
    layerIndex += amount;
    layerIndex = Math.Max(Math.Min(layerIndex, block.Height - 1), 0);
    texture.Layer = block.Layers[layerIndex];
  }

}
