using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FacingRotation))]
public class Enemy : MonoBehaviour
{
    public Cell CurrentCell { get; set; }

    private Map Map => CurrentCell.Map;

    private FacingRotation FacingRotation;

    private void Awake()
    {
        FacingRotation = GetComponent<FacingRotation>();
        FacingRotation.FacingDirection = Vector2Int.zero;

        Player.PlayerMoved += Player_PlayerMoved;
    }

    private void Start()
    {
        ChangeDirection();
    }

    public void Move()
    {
        Vector3Int nextIndex = CurrentCell.Index + new Vector3Int(FacingRotation.FacingDirection.x, FacingRotation.FacingDirection.y, 0);

        Cell nextCell = Map.TryGetCell(nextIndex.x, nextIndex.y);

        if (nextCell != null && !nextCell.IsDestroyed && UnityEngine.Random.Range(0f, 100f) < 95f)
            SetCurrentCell(nextCell);
        else
            ChangeDirection();

        //TODO See if enemy collides with new barrier
    }

    private void SetCurrentCell(Cell nextCell)
    {
        //if (Physics2D.OverlapCircle(CurrentCell.Coordinates + (Vector2)FacingRotation.FacingDirection * 0.5f, 0.1f))
        //    Player.KillPlayer();

        CurrentCell = nextCell;
        transform.position = CurrentCell.Coordinates;
    }

    private void ChangeDirection()
    {
        List<Vector2Int> possibleDirections = new List<Vector2Int>();

        void AddIfValid(Vector2Int direction)
        {
            if (FacingRotation.FacingDirection != direction && CanMove(direction))
                possibleDirections.Add(direction);
        }

        AddIfValid(Vector2Int.up);
        AddIfValid(Vector2Int.right);
        AddIfValid(Vector2Int.down);
        AddIfValid(Vector2Int.left);

        FacingRotation.FacingDirection = possibleDirections[UnityEngine.Random.Range(0, possibleDirections.Count)];
    }

    private bool CanMove(Vector2Int direction)
    {
        Vector3Int nextIndex = CurrentCell.Index + new Vector3Int(direction.x, direction.y, 0);

        Cell nextCell = Map.TryGetCell(nextIndex.x, nextIndex.y);

        return nextCell != null && !nextCell.IsDestroyed;
    }

    public void Die()
    {
        Player.PlayerMoved -= Player_PlayerMoved;

        ExplosionManager.SpawnExplosion(transform.position);

        Destroy(gameObject);
    }

    private void Player_PlayerMoved()
    {
        Move();
    }
}
