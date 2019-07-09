using UnityEngine;

public class FacingRotation : MonoBehaviour
{
    private Vector2Int _FacingDirection;
    public Vector2Int FacingDirection
    {
        get { return _FacingDirection; }

        set
        {
            _FacingDirection = value;

            float rotation = 0f;

            if (FacingDirection == Vector2Int.up)
                rotation = 90f;
            else if (FacingDirection == Vector2Int.right)
                rotation = 0f;
            else if (FacingDirection == Vector2Int.down)
                rotation = 270f;
            else if (FacingDirection == Vector2Int.left)
                rotation = 180f;

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            }
        }
    }

    private void Start()
    {
        FacingDirection = Vector2Int.right;
    }

}
