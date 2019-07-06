using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    private Cell[,] _cells;

    private Vertex[,] _vertices;

    [SerializeField]
    private PolygonCollider2D outlineCollider = null;

    [SerializeField]
    private Tilemap BackgroundTileMap = null;

    [SerializeField]
    private Tilemap DestoyedTileMap = null;

    [SerializeField]
    private Tile Cell1 = null;

    [SerializeField]
    private Tile Cell2 = null;

    [SerializeField]
    private Tile DestoyedCell = null;

    [SerializeField]
    private Player PlayerPrefeb = null;

    [SerializeField]
    private LineRenderer LinePrefab = null;

    public LinkedList<Vertex> Outline { get; private set; }

    private void Awake()
    {
        Outline = new LinkedList<Vertex>();

        _cells = new Cell[20, 10];
        _vertices = new Vertex[_cells.GetLength(0) + 1, _cells.GetLength(1) + 1];
    }

    private void Start()
    {
        int verticesColCount = _vertices.GetLength(0);
        int verticesRowCount = _vertices.GetLength(1);

        for (int i = 0; i < verticesColCount; i++)
        {
            for (int j = 0; j < verticesRowCount; j++)
            {
                _vertices[i, j] = new Vertex(this, i, j);
            }
        }

        for (int i = 0; i < _cells.GetLength(0); i++)
        {
            for (int j = 0; j < _cells.GetLength(1); j++)
            {
                _cells[i, j] = new Cell(this, i, j, _vertices[i, j], _vertices[i + 1, j], _vertices[i, j + 1], _vertices[i + 1, j + 1]);

                Tile cellTile = (i % 2 != j % 2 ? Cell1 : Cell2);

                BackgroundTileMap.SetTile(new Vector3Int(i, j, 0), cellTile);
            }
        }

        LineRenderer line = GetLine();
        line.positionCount = 5;
        line.SetPosition(0, new Vector3(0, 0));
        line.SetPosition(1, new Vector3(verticesColCount - 1, 0));
        line.SetPosition(2, new Vector3(verticesColCount - 1, verticesRowCount - 1));
        line.SetPosition(3, new Vector3(0, verticesRowCount - 1));
        line.SetPosition(4, new Vector3(0, 0));

        Outline.Clear();

        for (int i = 0; i < verticesColCount - 1; i++)
        {
            Vertex vertex = _vertices[i, 0];

            vertex.IsLinked = true;
            Outline.AddLast(vertex);
        }

        for (int j = 0; j < verticesRowCount - 1; j++)
        {
            Vertex vertex = _vertices[verticesColCount - 1, j];

            vertex.IsLinked = true;
            Outline.AddLast(vertex);
        }

        for (int i = verticesColCount - 1; i > 0; i--)
        {
            Vertex vertex = _vertices[i, verticesRowCount - 1];

            vertex.IsLinked = true;
            Outline.AddLast(vertex);
        }

        for (int j = verticesRowCount - 1; j > 0; j--)
        {
            Vertex vertex = _vertices[0, j];

            vertex.IsLinked = true;
            Outline.AddLast(vertex);
        }

        Player player = Instantiate(PlayerPrefeb);
        player.CurrentVertex = _vertices[0, 0];
        player.transform.position = new Vector3(player.CurrentVertex.Index.x, player.CurrentVertex.Index.y);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            DebugPrint();

        if (Input.GetKeyDown(KeyCode.R))
            DebugRestart();
    }

    private void DestroyCell(Cell cell)
    {
        cell.IsDestroyed = true;

        DestoyedTileMap.SetTile(cell.Index, DestoyedCell);

        foreach (var vertex in cell.Vertices)
        {
            if (vertex.IsDestroyed)
                continue;

            if (vertex.GetNeighboringCells().All(x => x.IsDestroyed))
                vertex.IsDestroyed = true;
        }
    }

    public Cell TryGetCell(int i, int j)
    {
        //Debug.Log($"TryGetCell({i}, {j})");

        if (i >= 0 && i < _cells.GetLength(0) && j >= 0 && j < _cells.GetLength(1))
            return _cells[i, j];

        return null;
    }

    public Vertex TryGetVertex(int i, int j)
    {
        //Debug.Log($"TryGetVertex({i}, {j})");

        if (i >= 0 && i < _vertices.GetLength(0) && j >= 0 && j < _vertices.GetLength(1))
            return _vertices[i, j];

        return null;
    }

    public LineRenderer GetLine()
    {
        return Instantiate(LinePrefab, transform);
    }

    public bool IsPointInside(Vector2 point)
    {
        return outlineCollider.OverlapPoint(point);
    }

    public void SetOutline(IEnumerable<Vertex> outline)
    {
        Outline = new LinkedList<Vertex>(outline);

        outlineCollider.points = Outline.Select(x => x.Coordinates).ToArray();

        foreach (var cell in _cells)
        {
            if (!IsPointInside(cell.Coordinates))
                DestroyCell(cell);
        }
    }

    public LinkedListNode<Vertex> GetNextVertexNode(LinkedListNode<Vertex> vertexNode)
    {
        return vertexNode.Next ?? Outline.First;
    }

    public LinkedListNode<Vertex> GetPreviousVertexNode(LinkedListNode<Vertex> vertexNode)
    {
        return vertexNode.Previous ?? Outline.Last;
    }

    private void DebugPrint()
    {
        string print = string.Empty;

        for (int j = _vertices.GetLength(1) - 1; j >= 0; j--)
        {
            for (int i = 0; i < _vertices.GetLength(0); i++)
            {
                Vertex vertex = _vertices[i, j];

                if (Outline.Contains(vertex))
                    print += "o";
                else
                    print += (vertex.IsDestroyed ? "x" : (vertex.IsLinked ? "*" : "."));
            }

            print += System.Environment.NewLine;
        }

        Debug.Log(print);
    }

    private void DebugRestart()
    {
        SceneManager.LoadScene("Main");
    }
}
