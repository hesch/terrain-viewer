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

    if (selectedObject) {
      IVoxelBlock block = selectedObject.GetComponent<BlockInfo>().Block;
      blockPositon.text = String.Format("Block position: {0}", block.Offset.ToString());
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

      if(vertices.Count > UInt16.MaxValue) {
	Debug.LogError("too many vertices");
	return;
      }

      Mesh mesh = new Mesh();
      mesh.SetVertices(vertices);
      mesh.SetTriangles(indices, 0);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();

      GameObject go = new GameObject(String.Format("Block({0}, {1})", block.Offset.x, block.Offset.y));
      go.transform.parent = transform;
      go.AddComponent<MeshFilter>();
      go.AddComponent<MeshRenderer>();
      go.AddComponent<BoxCollider>();
      go.AddComponent<BlockInfo>();
      go.GetComponent<Renderer>().material = m_material;
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
