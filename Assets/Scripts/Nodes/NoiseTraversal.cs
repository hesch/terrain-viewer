﻿using NodeEditorFramework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NoiseTraversal : NodeCanvasTraversal
{
  // A list of Nodes from which calculation originates -> Call StartCalculation
  public List<Node> workList;

  public List<Node> cache = new List<Node>();

  public NoiseTraversal (NodeCanvas canvas) : base(canvas) {}

  /// <summary>
  /// Recalculate from every node regarded as an input node
  /// </summary>
  public override void TraverseAll () 
  {
    PopulateCache();
    if(cache.Any()) {
      cache.ForEach(n => n.Calculate());
      return;
    }
  }

  /// <summary>
  /// Recalculate from the specified node
  /// </summary>
  public override void OnChange (Node node) 
  {
        Debug.Log("OnChange called");
    (nodeCanvas as NoiseCanvasType).StopComputation();
    //repopulate cache on change
    cache = new List<Node>();
    PopulateCache();
    (nodeCanvas as NoiseCanvasType).StartComputation(cache);
  }

  private void PopulateCache() {
    workList = new List<Node> ();
    for (int i = 0; i < nodeCanvas.nodes.Count; i++) 
    {
      Node node = nodeCanvas.nodes[i];
      if (node.isInput ())
      { // Add all Inputs
	node.ClearCalculation ();
	workList.Add (node);
      }
    }
    StartCalculation ();
  }

  private void StartCalculation () 
  {
    if (workList == null || workList.Count == 0)
      return;

    bool limitReached = false;
    while (!limitReached)
    { // Runs until the whole workList is calculated thoroughly or no further calculation is possible
      limitReached = true;
      for (int workCnt = 0; workCnt < workList.Count; workCnt++)
      { // Iteratively check workList
	if (ContinueCalculation (workList[workCnt]))
	  limitReached = false;
      }
    }
    if (workList.Count > 0)
    {
      Debug.LogError("Did not complete calculation! " + workList.Count + " nodes block calculation from advancing!");
      foreach (Node node in workList)
	Debug.LogError("" + node.name + " blocks calculation!");
    }
  }

  private bool ContinueCalculation (Node node) 
  {
    if (node.calculated && !node.AllowRecursion)
    { // Already calulated
      workList.Remove (node);
      return true;
    }
    if (node.ancestorsCalculated ())
    { // Calculation was successful
      node.calculated = true;
      cache.Add(node);
      workList.Remove (node);
      if (node.ContinueCalculation)
      { // Continue with children
	for (int i = 0; i < node.outputPorts.Count; i++)
	{
	  ConnectionPort outPort = node.outputPorts[i];
	  for (int t = 0; t < outPort.connections.Count; t++)
	    ContinueCalculation(outPort.connections[t].body);
	}
      }
      return true;
    }
    else if (!workList.Contains (node)) 
    { // Calculation failed, record to calculate later on
      workList.Add (node);
    }
    return false;
  }
}

