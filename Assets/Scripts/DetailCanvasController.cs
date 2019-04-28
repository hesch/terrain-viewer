using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NodeEditorFramework;

public class DetailCanvasController : MonoBehaviour {
  private Canvas canvas;
  public Text layerSliderText;
  public Slider layerSlider;
  private LayerTexture texture;
  private VertexDisplay display;
  private TerrainPreviewController terrainPreviewController;
  private NoiseCanvasType noiseCanvas;
  public Camera camera;

  public bool Active {
    get {
      return canvas.enabled;
    }
    set {
      canvas.enabled = value;
      camera.rect = value ? new Rect(0.5f, 0.5f, 0.5f, 0.5f) : new Rect(0.5f, 0.0f, 0.5f, 1.0f);
      terrainPreviewController.enabled = !value;
      if (value) {
	display.hideAllBut(displayedObject);
	noiseCanvas.ConfigureComputation(() => offsetGenerator(), (verts, indices, block) => {
	    display.PushNewMeshForOffset(verts, indices, block);
	});
      } else {
	display.clearHideState();
	terrainPreviewController.configureComputation();
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
      layerSlider.maxValue = block.Height;
    }
  }
  private int layerIndex = 0;

  public void Update() {
    if (!Active) return;
    if (Input.GetKeyUp(KeyCode.Escape)) {
      Active = false;
    }
  }

  public void Start() {
    canvas = GetComponent<Canvas>();
    texture = GetComponentInChildren<LayerTexture>();
    display = UnityEngine.Object.FindObjectOfType<VertexDisplay>();
    terrainPreviewController = UnityEngine.Object.FindObjectOfType<TerrainPreviewController>();
    NoiseNodeEditor editor = UnityEngine.Object.FindObjectOfType<NoiseNodeEditor>();
    noiseCanvas = editor.GetCanvas() as NoiseCanvasType;
    canvas.enabled = false;

    display.addMeshAddedDelegate(gameObject => DisplayedObject = gameObject);
  }

  public void setLayerIndex(float index) {
    layerIndex = (int)index;
    layerIndex = Math.Max(Math.Min(layerIndex, block.Height - 1), 0);
    texture.Layer = block.Layers[layerIndex];
    layerSliderText.text = "" + layerIndex;
  }

  private IEnumerable<(int, int)> offsetGenerator() {
    while(true) yield return (block.Offset.x, block.Offset.y);
  }
}
