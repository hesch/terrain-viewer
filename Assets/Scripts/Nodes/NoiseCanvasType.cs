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
  private Func<IEnumerable<(int, int)>> offsetGenerator;
  private Action<List<Vector3>, List<int>, List<Vector3>, VoxelBlock<Voxel>> callback;
  private CancellationTokenSource tokenSource;
  private Task task;

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

  public void ConfigureComputation(Func<IEnumerable<(int, int)>> offsetGenerator, Action<List<Vector3>, List<int>, List<Vector3>, VoxelBlock<Voxel>> callback) {
    this.offsetGenerator = offsetGenerator;
    this.callback = callback;
    Traversal.OnChange(null);
  }

  public void StartComputation(List<Node> cache) {
    tokenSource = new CancellationTokenSource();
    CancellationToken token = tokenSource.Token;
    Action taskAction = () => {
      token.ThrowIfCancellationRequested();
      var offsets = offsetGenerator();
      foreach((int, int) tuple in offsets) {
	  cache.ForEach(n => {
	      if (n.isInput() && n is VoxelInputNode) {
		VoxelInputNode n2 = n as VoxelInputNode;
		n2.Offset = new Vector2Int(tuple.Item1, tuple.Item2);
	      }
	      n.Calculate();
	      if (n.isOutput() && n is VertexNode) {
		VertexNode vertexNode = n as VertexNode;
		callback(vertexNode.Vertices, vertexNode.Indices, vertexNode.Normals, vertexNode.Block);
	      }
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
}
