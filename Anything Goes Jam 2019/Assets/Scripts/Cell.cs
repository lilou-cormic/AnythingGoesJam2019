using UnityEngine;

public class Cell
{
    public Map Map { get; }

    public Vector2Int Index { get; }

    public Vector2 Coordinates => new Vector2(Index.x + 0.5f, Index.y + 0.5f);

    public Vertex[,] Vertices { get; }

    public bool IsDestroyed { get; set; }

    public Cell(Map map, int colIndex, int rowIndex, Vertex bottomLeftVertex, Vertex bottomRightVertex, Vertex topLeftVertex, Vertex topRightVertex)
    {
        Map = map;

        Index = new Vector2Int(colIndex, rowIndex);

        Vertices = new Vertex[2, 2];
        Vertices[0, 1] = topLeftVertex; Vertices[1, 1] = topRightVertex;
        Vertices[0, 0] = bottomLeftVertex; Vertices[1, 0] = bottomRightVertex;
    }
}
