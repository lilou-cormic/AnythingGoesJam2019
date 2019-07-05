using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    public Vertex CurrentVertex { get; set; }

    private Map Map => CurrentVertex.Map;

    private Vector2Int _FacingDirection;
    public Vector2Int FacingDirection
    {
        get { return _FacingDirection; }

        set
        {
            _FacingDirection = value;

            if (FacingDirection == Vector2Int.up)
                rb.rotation = 0;
            else if (FacingDirection == Vector2Int.right)
                rb.rotation = 270;
            else if (FacingDirection == Vector2Int.down)
                rb.rotation = 180;
            else if (FacingDirection == Vector2Int.left)
                rb.rotation = 90;
        }
    }

    private float _movementCooldown = 0.1f;

    private float _freezeTimeLeft = 0f;

    private LineRenderer CurrentLine = null;

    private Queue<Vertex> Linking = new Queue<Vertex>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

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
            //TODO: BUG
            if (!CurrentVertex.IsLinked || !nextVertex.IsLinked)
            {
                if (CurrentLine == null)
                {
                    CurrentLine = Map.GetLine();
                    Linking.Clear();

                    AddPointToLine();
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
        rb.MovePosition(CurrentVertex.Coordinates);
    }

    private void AddPointToLine()
    {
        CurrentLine.positionCount++;
        CurrentLine.SetPosition(CurrentLine.positionCount - 1, CurrentVertex.Coordinates);

        Linking.Enqueue(CurrentVertex);
    }

    private Vertex GetNextVertex()
    {
        Vector2Int nextIndex = CurrentVertex.Index + FacingDirection;

        Vertex nextVertex = Map.TryGetVertex(nextIndex.x, nextIndex.y);

        return (nextVertex != null && !nextVertex.IsDestroyed ? nextVertex : null);
    }

    private void DestroySection()
    {
        //TODO DestroySection

        foreach (var vertex in Linking)
        {
            vertex.IsLinked = true;
        }

        CurrentLine = null;
        Linking.Clear();
    }

    private void Reset()
    {
        if (Linking.Count > 0)
            SetCurrentVertex(Linking.Dequeue());

        Destroy(CurrentLine.gameObject);

        CurrentLine = null;
        Linking.Clear();
    }
}
