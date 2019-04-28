using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NodeEditorFramework;

public class VertexDisplay : MonoBehaviour
{
  public Material m_material;
  public Material selectionMaterial;

  public Text blockPositon;

  private GameObject selection;
  private GameObject selectedObject;
  private IVoxelBlock selectedBlock;

  private Dictionary<Vector2Int, GameObject> Meshes = new Dictionary<Vector2Int, GameObject>();
  private static ConcurrentQueue<(List<Vector3>, List<int>, IVoxelBlock)> meshQueue = new ConcurrentQueue<(List<Vector3>, List<int>, IVoxelBlock)>();

  private bool gridlinesVisible = true;

  private DetailCanvasController detailController;
  private NoiseCanvasType noiseCanvas;

  public static void PushNewMeshForOffset(List<Vector3> meshVertices, List<int> meshIndices, IVoxelBlock block) {
    meshQueue.Enqueue((meshVertices, meshIndices, block));
  }

  public void Start() {
    selection = GameObject.CreatePrimitive(PrimitiveType.Cube);
    selection.name = "Selection";
    selection.transform.parent = transform;
    selection.GetComponent<Renderer>().material = selectionMaterial;
    Destroy(selection.GetComponent<BoxCollider>());

    detailController = UnityEngine.Object.FindObjectOfType<DetailCanvasController>();
    NoiseNodeEditor editor = UnityEngine.Object.FindObjectOfType<NoiseNodeEditor>();
    noiseCanvas = editor.GetCanvas() as NoiseCanvasType;
    configureComputation();
  }

  public void Update() {
    TryAddBlock();
  }

  public void configureComputation() {
    noiseCanvas.ConfigureComputation(() => Spiral(6), (verts, indices, block) => PushNewMeshForOffset(verts, indices, block));
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
	detailController.Active = true;
	detailController.DisplayedObject = selected;
	selection.SetActive(false);
      }
    }
  }

  public void setGridlinesVisible(bool visible) {
    gridlinesVisible = visible;
    foreach(GameObject mesh in Meshes.Values) {
      mesh.GetComponent<LineRenderer>().enabled = visible;
    }
  } 

  private void TryAddBlock() {
    (List<Vector3>, List<int>, IVoxelBlock) tuple;
    if(meshQueue.TryDequeue(out tuple)) {
      var (vertices, indices, block) = tuple;
      
      if(Meshes.ContainsKey(block.Offset)) {
	  GameObject oldMesh = Meshes[block.Offset];
	  Destroy(oldMesh);
      }

      GameObject go = BlockConverter.BlockToGameObject(vertices, indices, block, m_material, handleSelection);
      go.transform.parent = transform;
      go.GetComponent<LineRenderer>().enabled = gridlinesVisible;

      Meshes[block.Offset] = go;
      if (detailController.Active && detailController.DisplayedObject.GetComponent<BlockInfo>().Block.Offset == block.Offset) {
	detailController.displayedObject = go;
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
