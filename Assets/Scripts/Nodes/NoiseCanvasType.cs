using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Standard;

using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

[NodeCanvasType("Noise Canvas")]
public class NoiseCanvasType : NodeCanvas
{
    private Func<IEnumerable<(int, int)>> offsetGenerator;
    private Action<ComputeBuffer, ComputeBuffer, ComputeBuffer, VoxelBlock<Voxel>> callback;
    private CancellationTokenSource tokenSource;
    private Task task;
    private Stopwatch stopwatch = new Stopwatch();
    private Stopwatch nodeStopwatch = new Stopwatch();
    private StreamWriter file;

    public override string canvasName { get { return "Noise"; } }

    private string rootNodeID { get { return "VoxelInput"; } }
    // TODO: make this more generic
    public VoxelInputNode rootNode;

    protected override void OnCreate()
    {
        Traversal = new NoiseTraversal(this);
        rootNode = Node.Create(rootNodeID, Vector2.zero) as VoxelInputNode;
    }

    private void OnEnable()
    {

        if (Traversal == null)
            Traversal = new NoiseTraversal(this);
        // Register to other callbacks
        //NodeEditorCallbacks.OnDeleteNode += CheckDeleteNode;
    }

    protected override void ValidateSelf()
    {
        if (Traversal == null)
            Traversal = new NoiseTraversal(this);
        if (rootNode == null && (rootNode = nodes.Find((Node n) => n.GetID == rootNodeID) as VoxelInputNode) == null)
            rootNode = Node.Create(rootNodeID, Vector2.zero) as VoxelInputNode;
    }

    public override bool CanAddNode(string nodeID)
    {
        //Debug.Log ("Check can add node " + nodeID);
        if (nodeID == rootNodeID)
            return !nodes.Exists((Node n) => n.GetID == rootNodeID);
        return true;
    }

    public void ConfigureComputation(Func<IEnumerable<(int, int)>> offsetGenerator, Action<ComputeBuffer, ComputeBuffer, ComputeBuffer, VoxelBlock<Voxel>> callback)
    {
        this.offsetGenerator = offsetGenerator;
        this.callback = callback;
        Traversal.OnChange(null);
    }

    public void StartComputation(List<Node> cache)
    {
        tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;
        Action taskAction = () =>
        {
            token.ThrowIfCancellationRequested();
            if (offsetGenerator == null)
            {
                return;
            }
            var offsets = offsetGenerator();
            var performanceString = "";
            foreach ((int, int) tuple in offsets)
            {
                stopwatch.Restart();
                foreach (Node n in cache)
                {
                    //UnityEngine.Debug.Log(String.Format("Calculating Node {0}, out: {1}, n is VertexNode {2}", n.Title, n.isOutput(), n is VertexNode));
                    if (n.isInput() && n is VoxelInputNode)
                    {
                        VoxelInputNode n2 = n as VoxelInputNode;
                        n2.Offset = new Vector2Int(tuple.Item1, tuple.Item2);
                    }
                    nodeStopwatch.Restart();
                    n.Calculate();
                    nodeStopwatch.Stop();
                    performanceString += String.Format("\n\tNode {0} took\t{1}ms", n.Title, nodeStopwatch.ElapsedMilliseconds);

                    if (n.isOutput() && n is VertexNode)
                    {
                        VertexNode vertexNode = n as VertexNode;
                        callback(vertexNode.Vertices, vertexNode.Indices, vertexNode.Normals, vertexNode.Block);
                    }
                    token.ThrowIfCancellationRequested();
                }
                stopwatch.Stop();
                performanceString = String.Format("Calculation of block({0}, {1}) took\t{2}ms{3}", tuple.Item1, tuple.Item2, stopwatch.ElapsedMilliseconds, performanceString);
                if (file == null)
                {
                    file = new StreamWriter("performance.txt");
                }
                file.WriteLine(performanceString);
                file.Flush();
                performanceString = "";
            }
        };
        //taskAction();
        task = Task.Factory.StartNew(taskAction, token);
    }

    public void StopComputation()
    {
        if (tokenSource == null)
            return;
        tokenSource.Cancel();
        try
        {
            if (task != null)
            {
                task.Wait();
            }
        }
        finally
        {
            tokenSource.Dispose();
            tokenSource = null;
        }
    }
}
