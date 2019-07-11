using System.Collections;
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
    private Line LinePrefab = null;

    public LinkedList<Vertex> Outline { get; private set; }

    public List<Enemy> Enemies { get; private set; }

    public float CellDestroyedPct { get; private set; }

    [SerializeField]
    public TMPro.TextMeshProUGUI CellDestroyedPctText = null;

    public static int Lives { get; private set; } = 3;

    [SerializeField]
    public TMPro.TextMeshProUGUI LivesText = null;

    [SerializeField]
    private AudioClip SectionDestroyedSound = null;

    private void Awake()
    {
        Outline = new LinkedList<Vertex>();
        Enemies = new List<Enemy>();

        _cells = new Cell[CellColCount, CellRowCount];
        _vertices = new Vertex[CellColCount + 1, CellRowCount + 1];

        Player.PlayerDied += Player_PlayerDied;
    }

    private void OnDestroy()
    {
        Player.PlayerDied -= Player_PlayerDied;
    }

    private void Start()
    {
        LivesText.text = $"Lives: {Lives}";

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

        Line line = GetLine();
        line.AddPoint(new Vector3(0, 0));
        line.AddPoint(new Vector3(verticesColCount - 1, 0));
        line.AddPoint(new Vector3(verticesColCount - 1, verticesRowCount - 1));
        line.AddPoint(new Vector3(0, verticesRowCount - 1));
        line.AddPoint(new Vector3(0, 0));
        line.PlaceLine();

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

        SpawnPlayer();

        SpawnEnemies();
    }

    private void SpawnPlayer()
    {
        Player player = Instantiate(PlayerPrefeb);
        player.CurrentVertex = _vertices[_vertices.GetLength(0) / 2, 0];
        player.transform.position = new Vector3(player.CurrentVertex.Index.x, player.CurrentVertex.Index.y);
    }

    private void SpawnEnemies()
    {
        int colSplit = 1 + LevelNumberManager.Level;
        int rowSplit = 2 + (LevelNumberManager.Level > 0 ? (LevelNumberManager.Level / 4) : 0);

        for (int i = 0; i < colSplit; i++)
        {
            for (int j = 0; j < rowSplit; j++)
            {
                if (j != 0 || i != 0)
                    SpawnEnemy(i, j, colSplit, rowSplit);
            }
        }
    }

    private void SpawnEnemy(int i, int j, float colSplit, float rowSplit)
    {
        i = Mathf.Min(Mathf.FloorToInt(Random.Range(i * CellColCount / colSplit, (i + 1) * CellColCount / colSplit)), CellColCount - 1);
        j = Mathf.Min(Mathf.FloorToInt(Random.Range(j * CellRowCount / rowSplit, (j + 1) * CellRowCount / rowSplit)), CellRowCount - 1);

        Enemy enemy = Instantiate(EnemyPrefab);
        enemy.CurrentCell = _cells[i, j];
        enemy.transform.position = enemy.CurrentCell.Coordinates;

        Enemies.Add(enemy);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            DebugPrint();

        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene("Main");
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

    public Line GetLine()
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
        SoundPlayer.Play(SectionDestroyedSound);

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

        if (CellDestroyedPct >= 85)
            Win();
    }

    private void Win()
    {
        ScoreManager.AddPoints(Mathf.FloorToInt(CellDestroyedPct * ((2 * 60) - Time.timeSinceLevelLoad)));
        ScoreManager.SetHighScore();

        foreach (var cell in _cells)
        {
            if (cell.IsDestroyed)
                continue;

            DestroyCell(cell);
        }

        StartCoroutine(ExplodeMap());
    }

    private void Lose()
    {
        SceneManager.LoadScene("GameOver");

        //Hack Lives
        Lives = 3;
        LevelNumberManager.GoToFirstLevel();
    }

    private IEnumerator ExplodeMap()
    {
        float stopTime = Time.timeSinceLevelLoad + 2.5f;

        while (Time.timeSinceLevelLoad < stopTime)
        {
            ExplosionManager.SpawnExplosion(_cells[Random.Range(0, CellColCount), Random.Range(0, CellRowCount)].Coordinates);

            yield return new WaitForSeconds(0.1f);
        }

        LevelNumberManager.GoToNextLevel();

        SceneManager.LoadScene("Main");
    }

    public LinkedListNode<Vertex> GetNextVertexNode(LinkedListNode<Vertex> vertexNode)
    {
        return vertexNode.Next ?? Outline.First;
    }

    public LinkedListNode<Vertex> GetPreviousVertexNode(LinkedListNode<Vertex> vertexNode)
    {
        return vertexNode.Previous ?? Outline.Last;
    }

    private void Player_PlayerDied()
    {
        Lives--;

        LivesText.text = $"Lives: {Lives}";

        if (Lives <= 0)
            Lose();
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
}
