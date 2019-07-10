using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class Line : MonoBehaviour
{
    private LineRenderer LineRenderer;

    private EdgeCollider2D EdgeCollider;

    [SerializeField]
    private Material PlacedLineMaterial = null;

    private void Awake()
    {
        LineRenderer = GetComponent<LineRenderer>();
        EdgeCollider = GetComponent<EdgeCollider2D>();
    }

    public void AddPoint(Vector2 coordinates)
    {
        LineRenderer.positionCount++;
        LineRenderer.SetPosition(LineRenderer.positionCount - 1, coordinates);

        Vector3[] positions = new Vector3[LineRenderer.positionCount];
        LineRenderer.GetPositions(positions);

        EdgeCollider.points = positions.Select(x => new Vector2(x.x, x.y)).ToArray();
    }

    public void PlaceLine()
    {
        LineRenderer.material = PlacedLineMaterial;
        LineRenderer.gameObject.layer = 11; // LinePlaced

        EdgeCollider.points = new Vector2[0];
    }
}
