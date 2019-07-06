using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vertex CurrentVertex { get; set; }

    private Map Map => CurrentVertex.Map;

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

    private Stack<Vertex> Linking = new Stack<Vertex>();

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
                        Linking.Clear();

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
                AddPointToLine();

            if (nextVertex.IsLinked)
                DestroySection();
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

        Linking.Push(CurrentVertex);
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

        var first = Linking.Last();

        var nextNode = Map.Outline.Find(Linking.Peek());

        int failSafeCounter = 0;

        while (nextNode.Value != first)
        {
            Linking.Push(nextNode.Value);

            nextNode = Map.GetNextVertexNode(nextNode);

            failSafeCounter++;

            if (failSafeCounter > 10000)
                throw new System.StackOverflowException("Infinite loop in DestroySection");
        }

        Map.SetOutline(Linking);

        CurrentLine = null;
        Linking.Clear();
    }

    private void Reset()
    {
        if (Linking.Count > 0)
            SetCurrentVertex(Linking.Last());

        Destroy(CurrentLine.gameObject);

        CurrentLine = null;
        Linking.Clear();
    }
}
