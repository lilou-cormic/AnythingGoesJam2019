using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Cell CurrentCell { get; set; }

    public Vector3Int FacingDirection { get; set; }

    [SerializeField]
    public GameObject ExplosionPrefab = null;

    public bool CanMove()
    {
        Map map = CurrentCell.Map;

        Vector3Int nextIndex = CurrentCell.Index + FacingDirection;

        Cell nextCell = map.TryGetCell(nextIndex.x, nextIndex.y);

        return nextCell != null && !nextCell.IsDestroyed;
    }

    public void Die()
    {
        var explosion = Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
        Destroy(explosion, 2f);

        Destroy(gameObject);
    }
}
