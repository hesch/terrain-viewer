using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Standard;

using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

[NodeCanvasType("Noise Canvas")]
public class NoiseCanvasType : NodeCanvas
{
  public override string canvasName { get { return "Noise"; } }

  private string rootNodeID { get { return "VoxelInput"; } }
  // TODO: make this more generic
  public VoxelInputNode rootNode;

  protected override void OnCreate () 
  {
    Traversal = new NoiseTraversal (this);
    rootNode = Node.Create (rootNodeID, Vector2.zero) as VoxelInputNode;
  }

  private void OnEnable () 
  {
    if (Traversal == null)
      Traversal = new NoiseTraversal (this);
    // Register to other callbacks
    //NodeEditorCallbacks.OnDeleteNode += CheckDeleteNode;
  }

  protected override void ValidateSelf ()
  {
    if (Traversal == null)
      Traversal = new NoiseTraversal (this);
    if (rootNode == null && (rootNode = nodes.Find ((Node n) => n.GetID == rootNodeID) as VoxelInputNode) == null)
      rootNode = Node.Create (rootNodeID, Vector2.zero) as VoxelInputNode;
  }

  public override bool CanAddNode (string nodeID)
  {
    //Debug.Log ("Check can add node " + nodeID);
    if (nodeID == rootNodeID)
      return !nodes.Exists ((Node n) => n.GetID == rootNodeID);
    return true;
  }

  private CancellationTokenSource tokenSource;
  private Task task;

  public void StartComputation(List<Node> cache) {
    Debug.Log("starting task");
    tokenSource = new CancellationTokenSource();
    CancellationToken token = tokenSource.Token;
    Action taskAction = () => {
      token.ThrowIfCancellationRequested();
      for(int x = 0; x < 2; x++) {
	for(int y = 0; y < 2; y++) {
	  Debug.Log("rendering meshes at offset: " + x + ", " + y);
	  rootNode.OffsetX = x;
	  rootNode.OffsetY = y;
	  Debug.Log("Nodes in cache: " + cache.Count);
	  cache.ForEach(n => {
	      Debug.Log("starting thread calculation");
	      Debug.Log("calculating node" + n.GetID);
	      n.Calculate();
	      Debug.Log("calculating node end");
	      token.ThrowIfCancellationRequested();
	      Debug.Log("throw node end");
	      });
	  Debug.Log("after mesh rendering");
	}
      }
    };
    task = Task.Factory.StartNew(taskAction,token);
    Debug.Log("Task started");
  }

  public void StopComputation() {
    if(tokenSource == null)
      return;
    Debug.Log("stopping task");
    tokenSource.Cancel();
    try {
      task.Wait();
    } catch(AggregateException _) {
    } finally {
      tokenSource.Dispose();
    }
  }
}
