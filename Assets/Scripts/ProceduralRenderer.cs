using UnityEngine;

public class ProceduralRenderer : MonoBehaviour
{
    public int numVertices = 0;

    void OnPostRender()
    {
        Debug.Log("post render: " + numVertices);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, numVertices, 1);
    }
}