using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vertex CurrentVertex { get; set; }

    private Map Map => CurrentVertex.Map;

    [SerializeField]
    private Material PlacedLineMaterial = null;

    private Vector2Int _FacingDirection;
    public Vector2Int FacingDirection
    {
        get { return _FacingDirection; }

        set
        {
            _FacingDirection = value;

            float rotation = 0f;

            if (FacingDirection == Vector2Int.up)
                rotation = 90f;
            else if (FacingDirection == Vector2Int.right)
                rotation = 0f;
            else if (FacingDirection == Vector2Int.down)
                rotation = 270f;
            else if (FacingDirection == Vector2Int.left)
                rotation = 180f;

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            }
        }
    }

    private float _movementCooldown = 0.1f;

    private float _freezeTimeLeft = 0f;

    private LineRenderer CurrentLine = null;

    private List<Vertex> Linking = new List<Vertex>();

    private void Start()
    {
        FacingDirection = Vector2Int.right;
    }

    private void Update()
    {
        if (_freezeTimeLeft > 0f)
        {
            _freezeTimeLeft -= Time.deltaTime;
            return;
        }

        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        if (vertical > 0)
            FacingDirection = Vector2Int.up;
        else if (horizontal > 0)
            FacingDirection = Vector2Int.right;
        else if (vertical < 0)
            FacingDirection = Vector2Int.down;
        else if (horizontal < 0)
            FacingDirection = Vector2Int.left;
        else
            return;

        _freezeTimeLeft = _movementCooldown;

        Vertex nextVertex = GetNextVertex();

        if (nextVertex != null)
        {
            var vertexNode = Map.Outline.Find(CurrentVertex);

            if (vertexNode != null)
            {
                if (nextVertex != Map.GetNextVertexNode(vertexNode).Value && nextVertex != Map.GetPreviousVertexNode(vertexNode).Value)
                {
                    if (CurrentLine == null)
                    {
                        CurrentLine = Map.GetLine();
                        AddPointToLine();
                    }
                }
            }

            SetCurrentVertex(nextVertex);

            if (Linking.Contains(nextVertex))
            {
                Reset();
                return;
            }

            if (CurrentLine != null)
            {
                AddPointToLine();

                if (nextVertex.IsLinked)
                    DestroySection();
            }
        }
    }

    private void SetCurrentVertex(Vertex nextVertex)
    {
        CurrentVertex = nextVertex;
        transform.position = CurrentVertex.Coordinates;
    }

    private void AddPointToLine()
    {
        CurrentLine.positionCount++;
        CurrentLine.SetPosition(CurrentLine.positionCount - 1, CurrentVertex.Coordinates);

        Linking.Add(CurrentVertex);
    }

    private Vertex GetNextVertex()
    {
        Vector2Int nextIndex = CurrentVertex.Index + FacingDirection;

        Vertex nextVertex = Map.TryGetVertex(nextIndex.x, nextIndex.y);

        return (nextVertex != null && !nextVertex.IsDestroyed ? nextVertex : null);
    }

    private void DestroySection()
    {
        if (Linking.Count == 0)
            return;

        //TODO DestroySection

        foreach (var vertex in Linking)
        {
            vertex.IsLinked = true;
        }

        var first = Linking.First();

        List<Vertex> outline1 = new List<Vertex>(Linking);
        List<Vertex> outline2 = new List<Vertex>(Linking);

        var nextNode = Map.Outline.Find(Linking.Last());

        int failSafeCounter = 0;

        while (nextNode.Value != first)
        {
            nextNode = Map.GetNextVertexNode(nextNode);

            if (nextNode.Value != first)
                outline1.Add(nextNode.Value);

            failSafeCounter++;

            if (failSafeCounter > 10000)
                throw new System.StackOverflowException("Infinite loop in DestroySection");
        }

        nextNode = Map.Outline.Find(Linking.Last());

        failSafeCounter = 0;

        while (nextNode.Value != first)
        {
            nextNode = Map.GetPreviousVertexNode(nextNode);

            if (nextNode.Value != first)
                outline2.Add(nextNode.Value);

            failSafeCounter++;

            if (failSafeCounter > 10000)
                throw new System.StackOverflowException("Infinite loop in DestroySection");
        }

        Map.SetOutline(outline1, outline2);

        CurrentLine.material = PlacedLineMaterial;

        CurrentLine = null;
        Linking.Clear();
    }

    private void Reset()
    {
        if (Linking.Count > 0)
            SetCurrentVertex(Linking.First());

        Destroy(CurrentLine.gameObject);

        CurrentLine = null;
        Linking.Clear();
    }
}
