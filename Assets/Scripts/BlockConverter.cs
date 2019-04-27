using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;

public class BlockConverter {

  public static GameObject BlockToGameObject(List<Vector3> vertices, List<int> indices, IVoxelBlock block, Material material, Action<PointerEventData, GameObject> clickCallback) {
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
      go.AddComponent<MeshFilter>();
      go.AddComponent<MeshRenderer>();
      go.AddComponent<BoxCollider>();
      go.AddComponent<BlockInfo>();
      go.AddComponent<EventTrigger>();

      go.GetComponent<Renderer>().materials = Enumerable.Range(0, triangles.Count()).Select(i => material).ToArray();
      go.GetComponent<MeshFilter>().mesh = mesh;
      go.GetComponent<BlockInfo>().Block = block;
      go.transform.localPosition = new Vector3((block.Offset.x - 0.5f)*block.Width, -block.Height / 2, (block.Offset.y - 0.5f)*block.Length);
      go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
      go.GetComponent<BoxCollider>().center = new Vector3(block.Width/2, block.Height/2, block.Length/2);
      go.GetComponent<BoxCollider>().size = new Vector3(block.Width, block.Height, block.Length);

      EventTrigger trigger = go.GetComponent<EventTrigger>();
      EventTrigger.Entry entry = new EventTrigger.Entry();
      entry.eventID = EventTriggerType.PointerClick;
      entry.callback.AddListener((data) => { clickCallback((PointerEventData)data, go); });
      trigger.triggers.Add(entry);

      go.AddComponent<LineRenderer>();
      LineRenderer boundingBoxRenderer = go.GetComponent<LineRenderer>();
      Vector3[] points = getBoundingBoxPoints(block);
      boundingBoxRenderer.positionCount = points.Length;
      boundingBoxRenderer.useWorldSpace = false;
      boundingBoxRenderer.SetPositions(points);

      return go;
  }

  private static List<(List<int>, int)> splitIndices(List<int> indices, int limit) {
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

  
  private static Vector3[] getBoundingBoxPoints(IVoxelBlock block) {
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
