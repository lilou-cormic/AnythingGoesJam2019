using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FacingRotation))]
public class Player : MonoBehaviour
{
    private static Player _instance;

    public Vertex CurrentVertex { get; set; }

    private Map Map => CurrentVertex.Map;

    private FacingRotation FacingRotation;

    private float _movementCooldown = 0.1f;

    private float _freezeTimeLeft = 0f;

    private Line CurrentLine = null;

    private List<Vertex> Linking = new List<Vertex>();

    public static event Action PlayerMoved;

    private bool _isDying = false;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        FacingRotation = GetComponent<FacingRotation>();
    }

    private void Update()
    {
        if (_isDying)
            return;

        if (_freezeTimeLeft > 0f)
        {
            _freezeTimeLeft -= Time.deltaTime;
        }
        else
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            if (vertical > 0)
                FacingRotation.FacingDirection = Vector2Int.up;
            else if (horizontal > 0)
                FacingRotation.FacingDirection = Vector2Int.right;
            else if (vertical < 0)
                FacingRotation.FacingDirection = Vector2Int.down;
            else if (horizontal < 0)
                FacingRotation.FacingDirection = Vector2Int.left;
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

                transform.position = CurrentVertex.Coordinates;
                SetCurrentVertex(nextVertex);

                if (Linking.Contains(nextVertex))
                {
                    AddPointToLine();

                    Die();
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

        if (Vector2.Distance(transform.position, CurrentVertex.Coordinates) < Mathf.Epsilon)
            transform.position = CurrentVertex.Coordinates;
        else
            transform.position = Vector2.Lerp(transform.position, CurrentVertex.Coordinates, 20f * Time.deltaTime);
    }

    private void SetCurrentVertex(Vertex nextVertex)
    {
        CurrentVertex = nextVertex;
        //transform.position = CurrentVertex.Coordinates;

        if (!_isDying)
            PlayerMoved?.Invoke();
    }

    private void AddPointToLine()
    {
        CurrentLine.AddPoint(CurrentVertex.Coordinates);

        Linking.Add(CurrentVertex);
    }

    private Vertex GetNextVertex()
    {
        Vector2Int nextIndex = CurrentVertex.Index + FacingRotation.FacingDirection;

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

        CurrentLine.PlaceLine();

        CurrentLine = null;
        Linking.Clear();
    }

    private void Die()
    {
        _isDying = true;

        transform.position = CurrentVertex.Coordinates;

        StartCoroutine(ExplodeAndDie());
    }

    private IEnumerator ExplodeAndDie()
    {
        if (Linking.Count > 0)
        {
            for (int i = 0; i < Linking.Count; i++)
            {
                ExplosionManager.SpawnExplosion(Linking[i].Coordinates);

                yield return new WaitForSeconds(0.07f);
            }

            SetCurrentVertex(Linking.First());
        }
        else
        {
            ExplosionManager.SpawnExplosion(transform.position);

            yield return new WaitForSeconds(0.1f);
        }

        if (CurrentLine != null)
            Destroy(CurrentLine.gameObject);

        CurrentLine = null;
        Linking.Clear();

        _isDying = false;
    }

    public static void KillPlayer()
    {
        _instance.Die();
    }
}
