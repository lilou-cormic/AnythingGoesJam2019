using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public Map Map { get; }

    public Vector2Int Index { get; }

    public Vector2 Coordinates => Index;

    public bool IsDestroyed { get; set; }

    public bool IsLinked { get; set; }

    public Vertex(Map map, int x, int y)
    {
        Map = map;

        Index = new Vector2Int(x, y);
    }

    public Cell[] GetNeighboringCells()
    {
        List<Cell> cells = new List<Cell>();

        for (int i = Index.x - 1; i <= Index.x; i++)
        {
            for (int j = Index.y - 1; j <= Index.y; j++)
            {
                Cell cell = Map.TryGetCell(i, j);
                if (cell != null)
                    cells.Add(cell);
            }
        }

        return cells.ToArray();
    }
}
