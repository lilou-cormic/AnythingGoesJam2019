using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    private Cell[,] _cells;

    private int CellColCount = 20;
    private int CellRowCount = 10;

    private Vertex[,] _vertices;

    [SerializeField]
    private PolygonCollider2D outlineCollider = null;

    [SerializeField]
    private PolygonCollider2D testCollider = null;

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
    private Enemy EnemyPrefab = null;

    [SerializeField]
    private LineRenderer LinePrefab = null;

    [SerializeField]
    private Material PlacedLineMaterial = null;

    public LinkedList<Vertex> Outline { get; private set; }

    public List<Enemy> Enemies { get; private set; }

    public float CellDestroyedPct { get; private set; }

    [SerializeField]
    public TMPro.TextMeshProUGUI CellDestroyedPctText = null;

    private void Awake()
    {
        Outline = new LinkedList<Vertex>();
        Enemies = new List<Enemy>();

        _cells = new Cell[CellColCount, CellRowCount];
        _vertices = new Vertex[CellColCount + 1, CellRowCount + 1];
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

        for (int i = 0; i < CellColCount; i++)
        {
            for (int j = 0; j < CellRowCount; j++)
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
        line.material = PlacedLineMaterial;

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

        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        int enemyCount = 3;
        int split = enemyCount + 1;
        int lineSplit = (split / 2);

        for (int i = 0; i < lineSplit; i++)
        {
            for (int j = 0; j < lineSplit; j++)
            {
                if (j != 0 || i != 0)
                    SpawnEnemy(i, j, lineSplit);
            }
        }
    }

    private void SpawnEnemy(int i, int j, int lineSplit)
    {
        Enemy enemy = Instantiate(EnemyPrefab);
        enemy.CurrentCell = _cells[Random.Range(i * CellColCount / lineSplit, (i + 1) * CellColCount / lineSplit), Random.Range(j * CellRowCount / lineSplit, (i + 1) * CellRowCount / lineSplit)];
        enemy.transform.position = enemy.CurrentCell.Coordinates;

        Enemies.Add(enemy);
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

        foreach (var enemy in Enemies.ToList())
        {
            if (enemy.CurrentCell == cell)
            {
                Enemies.Remove(enemy);
                enemy.Die();
            }

        }

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

        if (i >= 0 && i < CellColCount && j >= 0 && j < CellRowCount)
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

    public void SetOutline(IEnumerable<Vertex> outline1, IEnumerable<Vertex> outline2)
    {
        SetOutline(GetOutline(outline1, outline2));
    }

    private IEnumerable<Vertex> GetOutline(IEnumerable<Vertex> outline1, IEnumerable<Vertex> outline2)
    {
        testCollider.points = outline1.Select(x => x.Coordinates).ToArray();

        int count1 = Enemies.Count(x => testCollider.OverlapPoint(x.CurrentCell.Coordinates));

        testCollider.points = outline2.Select(x => x.Coordinates).ToArray();

        int count2 = Enemies.Count(x => testCollider.OverlapPoint(x.CurrentCell.Coordinates));

        if (count1 > count2)
            return outline1;

        if (count2 > count1)
            return outline2;

        testCollider.points = outline1.Select(x => x.Coordinates).ToArray();

        count1 = _cells.Cast<Cell>().Count(x => testCollider.OverlapPoint(x.Coordinates));

        testCollider.points = outline2.Select(x => x.Coordinates).ToArray();

        count2 = _cells.Cast<Cell>().Count(x => testCollider.OverlapPoint(x.Coordinates));

        if (count1 > count2)
            return outline1;

        if (count2 > count1)
            return outline2;

        return outline1;
    }

    private void SetOutline(IEnumerable<Vertex> outline)
    {
        Outline = new LinkedList<Vertex>(outline);

        outlineCollider.points = Outline.Select(x => x.Coordinates).ToArray();

        foreach (var cell in _cells)
        {
            if (cell.IsDestroyed)
                continue;

            if (!outlineCollider.OverlapPoint(cell.Coordinates))
                DestroyCell(cell);
        }

        CellDestroyedPct = (_cells.Cast<Cell>().Count(x => x.IsDestroyed) / (float)_cells.Length) * 100;

        CellDestroyedPctText.text = CellDestroyedPct.ToString("F1") + "%";

        if (CellDestroyedPct >= 80)
            Win();
    }

    private void Win()
    {
        foreach (var cell in _cells)
        {
            if (cell.IsDestroyed)
                continue;

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
        Debug.Log(string.Join(System.Environment.NewLine, Outline.Select(x => x.Coordinates)));

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
