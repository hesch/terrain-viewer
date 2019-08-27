using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

public class BlockConverter {

  public static GameObject BlockToGameObject(List<Vector3> vertices, List<int> indices, List<Vector3> normals, IVoxelBlock block, Material material, Action<PointerEventData, GameObject> clickCallback) {
      int limit = UInt16.MaxValue;

        List<(List<int>, int)> triangles;
        if (vertices.Count() > limit)
        {
            Stopwatch sw = Stopwatch.StartNew();
            triangles = splitIndices(indices, limit);
            sw.Stop();
            UnityEngine.Debug.LogFormat("splitIndices took {0}ms", sw.ElapsedMilliseconds);
        }
        else
        {
            triangles = new List<(List<int>, int)> { (indices, 0) };
        }
      Mesh mesh = new Mesh();
      mesh.SetVertices(vertices);
      mesh.SetNormals(normals);
      mesh.subMeshCount = triangles.Count();
      for(int i = 0; i < triangles.Count(); i++) {
	mesh.SetTriangles(triangles[i].Item1, i, true, triangles[i].Item2);
      }
      mesh.RecalculateBounds();

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

  public static List<(List<int>, int)> splitIndices(List<int> indices, int limit) {
    List<(List<int>, int)> resultList = new List<(List<int>, int)>();
    List<int> indicesLeft = indices;
    int currentOffset = 0;

    while(indicesLeft.Any()) {

      List<int> belowLimit = new List<int>();
      List<int> aboveLimit = new List<int>();
      int currentLimit = currentOffset + limit;
      int nextOffset = currentLimit;
      int i1, i2, i3;
      int indexCount = indicesLeft.Count();
      for(int i = 0; i < indexCount; i += 3) {
	i1 = indicesLeft[i];
	i2 = indicesLeft[i+1];
	i3 = indicesLeft[i+2];
	List<int> addToList;
	if (i1 < currentLimit && i2 < currentLimit && i3 < currentLimit) {
	  addToList = belowLimit;
	  i1 -= currentOffset;
	  i2 -= currentOffset;
	  i3 -= currentOffset;
	} else if (i1 > currentLimit && i2 > currentLimit && i3 > currentLimit) {
	  addToList = aboveLimit;
	} else {
	  addToList = aboveLimit;
	  int minIndex = Math.Min(i1, Math.Min(i2, i3));
	  if (minIndex < nextOffset) {
	    nextOffset = minIndex;
	  }
	}

	addToList.Add(i1);
	addToList.Add(i2);
	addToList.Add(i3);
      }

      indicesLeft = aboveLimit;
      resultList.Add((belowLimit, currentOffset));
      currentOffset = nextOffset;
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
