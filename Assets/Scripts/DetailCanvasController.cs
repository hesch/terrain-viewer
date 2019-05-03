using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NodeEditorFramework;

public class DetailCanvasController : MonoBehaviour {
  public Material selectionMaterial;
  public Text layerSliderText;
  public Slider layerSlider;
  public Camera camera;
  private Canvas canvas;

  private Vector3 previousCameraPosition;
  private Quaternion previousCameraOrientation;

  private GameObject layerSelection;

  private LayerTexture texture;
  private VertexDisplay display;
  private TerrainPreviewController terrainPreviewController;
  private NoiseCanvasType noiseCanvas;

  public bool Active {
    get {
      return canvas.enabled;
    }
    set {
      canvas.enabled = value;
      camera.rect = value ? new Rect(0.5f, 0.5f, 0.5f, 0.5f) : new Rect(0.5f, 0.0f, 0.5f, 1.0f);
      terrainPreviewController.enabled = !value;
      layerSelection.SetActive(value);
      if (value) {
	display.hideAllBut(displayedObject);
	noiseCanvas.ConfigureComputation(() => offsetGenerator(), (verts, indices, block) => {
	    display.PushNewMeshForOffset(verts, indices, block);
	});
	previousCameraPosition = camera.transform.position;
	previousCameraOrientation = camera.transform.localRotation;
	camera.transform.position = displayedObject.transform.position - new Vector3(100.0f, 100.0f, 100.0f);
	camera.transform.LookAt(displayedObject.transform.position); 
      } else {
	display.clearHideState();
	terrainPreviewController.configureComputation();
	camera.transform.position = previousCameraPosition;
	camera.transform.localRotation = previousCameraOrientation;
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
      if (Active && block.Offset != value.GetComponent<BlockInfo>().Block.Offset) {
	display.hideAllBut(value);
      }
      displayedObject = value;
      block = (VoxelBlock<Voxel>)displayedObject.GetComponent<BlockInfo>().Block;
      Debug.Log("displayedObject: " + block.Offset);
      layerSlider.maxValue = block.Height;
      layerSelection.transform.localScale = new Vector3(block.Width, 1.0f, block.Length);
      setLayerIndex(layerIndex);
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

    display.addMeshAddedDelegate(gameObject => {
	if (DisplayedObject == null || gameObject.GetComponent<BlockInfo>().Block.Offset == block.Offset)
	  DisplayedObject = gameObject;
    });

    layerSelection = GameObject.CreatePrimitive(PrimitiveType.Cube);
    layerSelection.name = "Layer Selection";
    layerSelection.transform.parent = display.transform;
    layerSelection.GetComponent<Renderer>().material = selectionMaterial;
    layerSelection.SetActive(false);
    Destroy(layerSelection.GetComponent<BoxCollider>());
  }

  public void setLayerIndex(float index) {
    layerIndex = (int)index;
    layerIndex = Math.Max(Math.Min(layerIndex, block.Height - 1), 0);
    texture.Layer = block.Layers[layerIndex];
    layerSliderText.text = "" + layerIndex;
    layerSelection.transform.localPosition = displayedObject.transform.localPosition + layerSelection.transform.localScale/2;
    layerSelection.transform.localPosition += new Vector3(0.0f, layerIndex, 0.0f);
  }

  private IEnumerable<(int, int)> offsetGenerator() {
    yield return (block.Offset.x, block.Offset.y);
  }
}
