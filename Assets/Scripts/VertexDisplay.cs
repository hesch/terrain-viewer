using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class VertexDisplay : MonoBehaviour
{
  public Material m_material;
  public Material selectionMaterial;
  public Camera raycastCamera;

  public Text blockPositon;

  private GameObject selection;
  private GameObject selectedObject;

  private Dictionary<Vector2Int, GameObject> Meshes = new Dictionary<Vector2Int, GameObject>();
  private static ConcurrentQueue<(List<Vector3>, List<int>, IVoxelBlock)> meshQueue = new ConcurrentQueue<(List<Vector3>, List<int>, IVoxelBlock)>();

  private bool gridlinesVisible = true;
  private bool examineMode = false;

  public static void PushNewMeshForOffset(List<Vector3> meshVertices, List<int> meshIndices, IVoxelBlock block) {
    meshQueue.Enqueue((meshVertices, meshIndices, block));
  }

  public void Start() {
    selection = GameObject.CreatePrimitive(PrimitiveType.Cube);
    selection.name = "Selection";
    selection.transform.parent = transform;
    selection.GetComponent<Renderer>().material = selectionMaterial;
    Destroy(selection.GetComponent<BoxCollider>());
  }


  public void Update() {
    TryAddBlock();
    if (selectedObject && examineMode) {

    } else {
      handleSelection();
    }
  }

  private void handleSelection() {
    if (Input.GetMouseButtonUp(0)) {  
      RaycastHit hit;
      Ray r = raycastCamera.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(r, out hit) && hit.collider.gameObject.GetComponent<BlockInfo>()) {
	IVoxelBlock block = hit.collider.gameObject.GetComponent<BlockInfo>().Block;
	selection.transform.localScale = new Vector3(block.Width, block.Height, block.Length);
	selection.transform.localPosition = hit.collider.transform.localPosition + selection.transform.localScale/2;
	selectedObject = hit.collider.gameObject;
	selection.SetActive(true);
      } else {
	selection.SetActive(false);
      }
    }

    if (selectedObject) {
      IVoxelBlock block = selectedObject.GetComponent<BlockInfo>().Block;
      blockPositon.text = String.Format("Block position: {0}", block.Offset.ToString());
    }
  }

  public void setExamineMode(bool enabled) {
    examineMode = enabled;
    foreach(GameObject mesh in Meshes.Values) {
      if (mesh == selectedObject) continue;
      mesh.SetActive(!enabled);
    }
  }

  public void setGridlinesVisible(bool visible) {
    gridlinesVisible = visible;
    foreach(GameObject mesh in Meshes.Values) {
      mesh.GetComponent<LineRenderer>().enabled = visible;
    }
  } 

  private List<(List<int>, int)> splitIndices(List<int> indices, int limit) {
    List<(List<int>, int)> resultList = new List<(List<int>, int)>();
    List<int> indicesLeft = indices;
    int currentOffset = 0;

    while(indicesLeft.Any()) {
      int offset = indicesLeft.Min(i => i);
      indicesLeft = indicesLeft.Select(i => i - offset).ToList();
      currentOffset += offset;

      List<int> belowLimit = new List<int>();
      List<int> aboveLimit = new List<int>();
      for(int i = 0; i < indicesLeft.Count(); i += 3) {
	List<int> addToList = indicesLeft[i] < limit || indicesLeft[i+1] < limit || indicesLeft[i+2] < limit
	  ? belowLimit
	  : aboveLimit;

	addToList.Add(indicesLeft[i]);
	addToList.Add(indicesLeft[i+1]);
	addToList.Add(indicesLeft[i+2]);
      }

      indicesLeft = aboveLimit;
      resultList.Add((belowLimit, currentOffset));
    }

    return resultList;
  }


  private void TryAddBlock() {
    (List<Vector3>, List<int>, IVoxelBlock) tuple;
    if(meshQueue.TryDequeue(out tuple)) {
      var (vertices, indices, block) = tuple;
      
      if(Meshes.ContainsKey(block.Offset)) {
	  GameObject oldMesh = Meshes[block.Offset];
	  Destroy(oldMesh);
      }

      int limit = UInt16.MaxValue;

      List<(List<int>, int)> triangles = splitIndices(indices, limit);
      Mesh mesh = new Mesh();
      mesh.SetVertices(vertices);
      mesh.subMeshCount = triangles.Count();
      for(int i = 0; i < triangles.Count(); i++) {
	mesh.SetTriangles(triangles[i].Item1, i, true, triangles[i].Item2);
      }
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();

      GameObject go = new GameObject(String.Format("Block({0}, {1})", block.Offset.x, block.Offset.y));
      go.transform.parent = transform;
      go.AddComponent<MeshFilter>();
      go.AddComponent<MeshRenderer>();
      go.AddComponent<BoxCollider>();
      go.AddComponent<BlockInfo>();
      go.GetComponent<Renderer>().materials = Enumerable.Range(0, triangles.Count()).Select(i => m_material).ToArray();
      go.GetComponent<MeshFilter>().mesh = mesh;
      go.GetComponent<BlockInfo>().Block = block;
      go.transform.localPosition = new Vector3((block.Offset.x - 0.5f)*block.Width, -block.Height / 2, (block.Offset.y - 0.5f)*block.Length);
      go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
      go.GetComponent<BoxCollider>().center = new Vector3(block.Width/2, block.Height/2, block.Length/2);
      go.GetComponent<BoxCollider>().size = new Vector3(block.Width, block.Height, block.Length);

      go.AddComponent<LineRenderer>();
      LineRenderer boundingBoxRenderer = go.GetComponent<LineRenderer>();
      Vector3[] points = getBoundingBoxPoints(block);
      boundingBoxRenderer.positionCount = points.Length;
      boundingBoxRenderer.useWorldSpace = false;
      boundingBoxRenderer.SetPositions(points);
      boundingBoxRenderer.enabled = gridlinesVisible;

      Meshes[block.Offset] = go;
    }
  }

  private Vector3[] getBoundingBoxPoints(IVoxelBlock block) {
    return new Vector3[] {
      new Vector3( 0,  0,  0),
      new Vector3( 0,  1,  0),
      new Vector3( 1,  1,  0),
      new Vector3( 1,  0,  0),
      new Vector3( 0,  0,  0),
      new Vector3( 0,  0,  1),
      new Vector3( 0,  1,  1),
      new Vector3( 0,  1,  0),
      new Vector3( 1,  1,  0),
      new Vector3( 1,  1,  1),
      new Vector3( 0,  1,  1),
      new Vector3( 0,  0,  1),
      new Vector3( 1,  0,  1),
      new Vector3( 1,  0,  0),
      new Vector3( 1,  1,  0),
      new Vector3( 1,  1,  1),
      new Vector3( 1,  0,  1)
    }.Select(p => {
	p.x *= block.Width;
	p.y *= block.Height;
	p.z *= block.Length;
	return p;
    }).ToArray();
  }
}
