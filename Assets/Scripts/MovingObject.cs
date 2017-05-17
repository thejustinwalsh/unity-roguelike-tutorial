using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
	public bool isMoving = false;

	public float moveTime = 0.1f;
	public LayerMask blockingLayer;

	private BoxCollider2D boxCollider;
	private Rigidbody2D rigidBody;
	private float inverseMoveTime;
	protected virtual void Start ()
	{
		boxCollider = GetComponent<BoxCollider2D>();
		rigidBody = GetComponent<Rigidbody2D>();
		inverseMoveTime = 1.0f / moveTime;
	}
	protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
	{
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2(xDir, yDir);

		boxCollider.enabled = false;
		hit = Physics2D.Linecast(start, end, blockingLayer);
		boxCollider.enabled = true;

		if (hit.transform == null) {
			StartCoroutine(SmoothMovement(end));
			return true;
		}

		return false;
	}

	protected IEnumerator SmoothMovement(Vector2 end)
	{
		isMoving = true;

		Vector2 distance = new Vector2(transform.position.x - end.x, transform.position.y - end.y);

		float sqrDistance = distance.sqrMagnitude;

		while (sqrDistance > float.Epsilon) {
			var newPosition = Vector3.MoveTowards(rigidBody.position, end, inverseMoveTime * Time.deltaTime);
			rigidBody.MovePosition(newPosition);

			distance = new Vector2(transform.position.x - end.x, transform.position.y - end.y);
			sqrDistance = distance.sqrMagnitude;

			yield return null;
		}

		isMoving = false;
	}

	protected virtual bool AttemptMove(int xDir, int yDir)
	{
		RaycastHit2D hit;
		bool canMove = Move(xDir, yDir, out hit);
		if (hit.transform == null) return true;

		if (!canMove) OnCantMove(hit.transform);
		return false;
	}

	protected abstract void OnCantMove(Transform transform);
}
