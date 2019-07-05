using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Map : MonoBehaviour
{
    private Cell[,] _cells;

    private Vertex[,] _vertices;

    [SerializeField]
    private Tilemap BackgroundTileMap = null;

    [SerializeField]
    private Tile Cell1 = null;

    [SerializeField]
    private Tile Cell2 = null;

    [SerializeField]
    private Player PlayerPrefeb = null;

    [SerializeField]
    private LineRenderer LinePrefab = null;

    private void Awake()
    {
        _cells = new Cell[20, 10];
        _vertices = new Vertex[_cells.GetLength(0) + 1, _cells.GetLength(1) + 1];
    }

    private void Start()
    {
        int verticesColCount = _vertices.GetLength(0);
        int verticesRowRount = _vertices.GetLength(1);

        for (int i = 0; i < verticesColCount; i++)
        {
            for (int j = 0; j < verticesRowRount; j++)
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
        line.SetPosition(2, new Vector3(verticesColCount - 1, verticesRowRount - 1));
        line.SetPosition(3, new Vector3(0, verticesRowRount - 1));
        line.SetPosition(4, new Vector3(0, 0));

        for (int i = 0; i < verticesColCount; i++)
        {
            _vertices[i, 0].IsLinked = true;
            _vertices[i, verticesRowRount - 1].IsLinked = true;
        }

        for (int j = 0; j < verticesRowRount; j++)
        {
            _vertices[0, j].IsLinked = true;
            _vertices[verticesColCount - 1, j].IsLinked = true;
        }

        Player player = Instantiate(PlayerPrefeb);
        player.CurrentVertex = _vertices[0, 0];
        player.transform.position = new Vector3(player.CurrentVertex.Index.x, player.CurrentVertex.Index.y);
    }

    private void DestroyCell(int i, int j)
    {
        Cell cell = TryGetCell(i, j);

        if (cell == null)
            return;

        cell.IsDestroyed = true;

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
}
