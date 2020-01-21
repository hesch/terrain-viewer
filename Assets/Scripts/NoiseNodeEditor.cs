using NodeEditorFramework;
using NodeEditorFramework.Standard;
using NodeEditorFramework.Utilities;
using UnityEngine;

public class NoiseNodeEditor : MonoBehaviour
{
    public NodeCanvas canvas;
    public string loadSceneName;
    private NodeEditorUserCache canvasCache;
    private NodeEditorInterface editorInterface;

    private Rect rootRect = new Rect(0, 0, 1000, 500);
    private Rect canvasRect = new Rect(50, 50, 900, 400);

    private RectTransform rt;

    public NodeCanvas GetCanvas()
    {
        return canvasCache.nodeCanvas;
    }

    public void Awake()
    {
        rt = transform as RectTransform;
        NormalReInit();
    }

    private void Update()
    {
        NodeEditor.Update();
    }

    private void NormalReInit()
    {
        NodeEditor.ReInit(false);
        AssureSetup();
        if (canvasCache.nodeCanvas)
            canvasCache.nodeCanvas.Validate();
    }

    private void AssureSetup()
    {
        if (canvasCache == null)
        { // Create cache and load startup-canvas
            canvasCache = new NodeEditorUserCache();
            if (canvas != null)
                canvasCache.SetCanvas(NodeEditorSaveManager.CreateWorkingCopy(canvas));
            else if (!string.IsNullOrEmpty(loadSceneName))
                canvasCache.LoadSceneNodeCanvas(loadSceneName);
        }
        canvasCache.AssureCanvas();
        if (editorInterface == null)
        { // Setup editor interface
            editorInterface = new NodeEditorInterface();
            editorInterface.canvasCache = canvasCache;
        }
    }

    private void OnGUI()
    {
        float inset = 0.0f;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector3 topLeft = corners[1];
        topLeft.y = Screen.height - topLeft.y;
        rootRect = new Rect(topLeft.x, topLeft.y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        canvasRect = new Rect(rootRect.x + inset, rootRect.y + inset, rootRect.width - inset * 2, rootRect.height - inset * 2);

        // Initiation
        NodeEditor.checkInit(true);
        if (NodeEditor.InitiationError)
        {
            GUILayout.Label("Node Editor Initiation failed! Check console for more information!");
            return;
        }
        AssureSetup();

        // ROOT: Start Overlay GUI for popups
        OverlayGUI.StartOverlayGUI("RTNodeEditor");

        // Set various nested groups
        GUI.BeginGroup(rootRect, GUI.skin.box);

        // Begin Node Editor GUI and set canvas rect
        NodeEditorGUI.StartNodeGUI(false);
        canvasCache.editorState.canvasRect = new Rect(canvasRect.x, canvasRect.y + editorInterface.toolbarHeight, canvasRect.width, canvasRect.height - editorInterface.toolbarHeight);

        try
        { // Perform drawing with error-handling
            NodeEditor.DrawCanvas(canvasCache.nodeCanvas, canvasCache.editorState);
        }
        catch (UnityException e)
        { // On exceptions in drawing flush the canvas to avoid locking the UI
            canvasCache.NewNodeCanvas();
            NodeEditor.ReInit(true);
            Debug.LogError("Unloaded Canvas due to exception in Draw!");
            Debug.LogException(e);
        }

        // Draw Interface
        editorInterface.DrawToolbarGUI(canvasRect);
        editorInterface.DrawModalPanel();

        // End Node Editor GUI
        NodeEditorGUI.EndNodeGUI();

        // End various nested groups
        GUI.EndGroup();

        // END ROOT: End Overlay GUI and draw popups
        OverlayGUI.EndOverlayGUI();
    }
}

