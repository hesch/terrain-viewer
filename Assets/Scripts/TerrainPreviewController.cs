﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using NodeEditorFramework;

public class TerrainPreviewController : MonoBehaviour
{
  public Material selectionMaterial;

  public Text blockPositon;

  private GameObject selection;
  private GameObject selectedObject;
  private IVoxelBlock selectedBlock;

  private DetailCanvasController detailController;
  private NoiseCanvasType noiseCanvas;
  private VertexDisplay vertexDisplay;

  public void Start() {
    selection = GameObject.CreatePrimitive(PrimitiveType.Cube);
    selection.name = "Selection";
    selection.transform.parent = transform;
    selection.GetComponent<Renderer>().material = selectionMaterial;
    Destroy(selection.GetComponent<BoxCollider>());

    detailController = UnityEngine.Object.FindObjectOfType<DetailCanvasController>();
    vertexDisplay = UnityEngine.Object.FindObjectOfType<VertexDisplay>();
    NoiseNodeEditor editor = UnityEngine.Object.FindObjectOfType<NoiseNodeEditor>();
    noiseCanvas = editor.GetCanvas() as NoiseCanvasType;
    configureComputation();

    vertexDisplay.addMeshEventDelegate(handleSelection);
  }

  public void configureComputation() {
    noiseCanvas.ConfigureComputation(() => Spiral(6), (verts, indices, block) => vertexDisplay.PushNewMeshForOffset(verts, indices, block));
  }

  private void handleSelection(PointerEventData eventData, GameObject selected) {
    if (!enabled) {
      return;
    }

    if (eventData.button == PointerEventData.InputButton.Left) {
      if (eventData.clickCount == 1) {
	selectedObject = selected;
	selectedBlock = selected.GetComponent<BlockInfo>().Block;

	selection.transform.localScale = new Vector3(selectedBlock.Width, selectedBlock.Height, selectedBlock.Length);
	selection.transform.localPosition = selectedObject.transform.localPosition + selection.transform.localScale/2;
	
	selection.SetActive(true);
	blockPositon.text = String.Format("Block position: {0}", selectedBlock.Offset.ToString());
      } else if (eventData.clickCount == 2) {
	detailController.DisplayedObject = selected;
	detailController.Active = true;
	selection.SetActive(false);
      }
    }
  }

  public IEnumerable<(int, int)> Spiral(int radius) {
    int x = 0;
    int y = 0;
    int dx = 0;
    int dy = -1;
    int maxI = radius * radius;
    for(int i=0; i < maxI; i++) {
        if ((-radius/2 < x && x <= radius/2) && (-radius/2 < y && y <= radius/2))
            yield return (x, y);
        if (x == y || (x < 0 && x == -y) || (x > 0 && x == 1-y)) {
	  int t = dx;
            dx = -dy;
	    dy = t;
	}
        x += dx;
	y += dy;
    }
  }
}
