using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Cell CurrentCell { get; set; }

    public Vector2Int FacingDirection { get; set; }

    public bool CanMove()
    {
        Map map = CurrentCell.Map;

        Vector2Int nextIndex = CurrentCell.Index + FacingDirection;

        Cell nextCell = map.TryGetCell(nextIndex.x, nextIndex.y);

        return nextCell != null && !nextCell.IsDestroyed;
    }
}
