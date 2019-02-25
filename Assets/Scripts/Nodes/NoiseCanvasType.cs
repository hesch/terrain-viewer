using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Standard;

[NodeCanvasType("Noise Canvas")]
public class NoiseCanvasType : NodeCanvas
{
  public override string canvasName { get { return "Noise"; } }

  private string rootNodeID { get { return "Input3D"; } }
  public Input3DNode rootNode;

  protected override void OnCreate () 
  {
    Traversal = new NoiseTraversal (this);
    rootNode = Node.Create (rootNodeID, Vector2.zero) as Input3DNode;
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
    if (rootNode == null && (rootNode = nodes.Find ((Node n) => n.GetID == rootNodeID) as Input3DNode) == null)
      rootNode = Node.Create (rootNodeID, Vector2.zero) as Input3DNode;
  }

  public override bool CanAddNode (string nodeID)
  {
    //Debug.Log ("Check can add node " + nodeID);
    if (nodeID == rootNodeID)
      return !nodes.Exists ((Node n) => n.GetID == rootNodeID);
    return true;
  }

}
