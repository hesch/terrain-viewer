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
    tokenSource = new CancellationTokenSource();
    CancellationToken token = tokenSource.Token;
    Action taskAction = () => {
      token.ThrowIfCancellationRequested();
      foreach((int, int) tuple in Spiral(6)) {
	  rootNode.Offset = new Vector2Int(tuple.Item1, tuple.Item2);
	  cache.ForEach(n => {
	      if (n.isInput() && n is VoxelInputNode) {
		VoxelInputNode n2 = n as VoxelInputNode;
		n2.Offset = rootNode.Offset;
	      }
	      n.Calculate();
	      token.ThrowIfCancellationRequested();
	      });
	}
    };
    task = Task.Factory.StartNew(taskAction,token);
  }

  public void StopComputation() {
    if(tokenSource == null)
      return;
    tokenSource.Cancel();
    try {
      task.Wait();
    } catch(AggregateException _) {
    } finally {
      tokenSource.Dispose();
      tokenSource = null;
    }
  }

  private IEnumerable<(int, int)> Spiral(int radius) {
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
