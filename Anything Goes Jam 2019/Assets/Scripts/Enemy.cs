using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FacingRotation))]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public Cell CurrentCell { get; set; }

    private Map Map => CurrentCell.Map;

    private Rigidbody2D rb;

    private FacingRotation FacingRotation;

    private bool _isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        FacingRotation = GetComponent<FacingRotation>();
        FacingRotation.FacingDirection = Vector2Int.zero;

        Player.PlayerMoved += Player_PlayerMoved;
    }

    private void OnDestroy()
    {
        Player.PlayerMoved -= Player_PlayerMoved;
    }

    private void Start()
    {
        ChangeDirection();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player.KillPlayer();
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
        CurrentCell = nextCell;
    }

    private void Update()
    {
        if (Vector2.Distance(rb.position, CurrentCell.Coordinates) < Mathf.Epsilon)
            rb.MovePosition(CurrentCell.Coordinates);
        else
            rb.MovePosition(Vector2.Lerp(rb.position, CurrentCell.Coordinates, 20f * Time.deltaTime));
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

        if (possibleDirections.Count > 0)
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
        if (_isDead)
            return;

        _isDead = true;

        ExplosionManager.SpawnExplosion(transform.position);

        Destroy(gameObject);
    }

    private void Player_PlayerMoved()
    {
        if (_isDead)
            return;

        Move();
    }
}
