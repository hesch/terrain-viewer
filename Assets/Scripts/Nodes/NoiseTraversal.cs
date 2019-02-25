using NodeEditorFramework;

public class NoiseTraversal : NodeCanvasTraversal
{
  NoiseCanvasType Canvas;

  public NoiseTraversal (NoiseCanvasType canvas) : base(canvas)
  {
    Canvas = canvas;
  }

  /// <summary>
  /// Traverse the canvas and evaluate it
  /// </summary>
  public override void TraverseAll () 
  {
    Input3DNode rootNode = Canvas.rootNode;
    rootNode.Calculate ();
    
    //Debug.Log ("RootNode is " + rootNode);
  }
}

