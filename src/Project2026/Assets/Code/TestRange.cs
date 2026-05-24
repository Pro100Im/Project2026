using UnityEngine;

public class TestRange : MonoBehaviour
{
    public float CellSize = 0.4f;
    public float Range = 6f;
    public Vector3 AnchorPoint = new Vector3(0, -0.6f, 0);

    public bool ShowOnlySelected = true;

    private void OnDrawGizmos()
    {
        if (ShowOnlySelected) 
            return;

        DrawRange();
    }

    private void OnDrawGizmosSelected()
    {
        DrawRange();
    }

    private void DrawRange()
    {
        Gizmos.color = new Color(1.2f, 0f, 0f, 0.6f);

        Gizmos.DrawWireSphere(transform.position + AnchorPoint, Range * CellSize);

        Gizmos.color = new Color(1.2f, 0f, 0f, 0.2f);
        DrawFlatDisk(transform.position + AnchorPoint, Range * CellSize);
    }

    private void DrawFlatDisk(Vector3 center, float radius)
    {
        var oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.identity, new Vector3(1, 0.01f, 1));
        Gizmos.DrawSphere(Vector3.zero, radius);
        Gizmos.matrix = oldMatrix;
    }
}
