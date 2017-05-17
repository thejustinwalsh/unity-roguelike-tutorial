using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
	public int playerDamage;

	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;
	
	private Animator animator;
	private Transform target;
	private bool skipMove;

	protected override void Start ()
	{
		animator = GetComponent<Animator>();
		target = GameObject.FindWithTag("Player").transform;

		GameManager.instance.AddEnemyToList(this);
		base.Start();
	}

	public bool MoveEnemy()
	{
		int xDir = 0, yDir = 0;
		if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon) {
			yDir = target.position.y > transform.position.y ? 1 : -1;
		}
		else {
			xDir = target.position.x > transform.position.x ? 1 : -1;
		}

		return AttemptMove(xDir, yDir);
	}

	protected override bool AttemptMove(int xDir, int yDir)
	{
		if (skipMove) { skipMove = false; return false; }
		skipMove = true;

		return base.AttemptMove(xDir, yDir);
	}

	protected override void OnCantMove(Transform transform)
	{
		Player hitPlayer = transform.GetComponent<Player>();
		if (hitPlayer) {
			SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);

			hitPlayer.LoseFood(playerDamage);
			animator.SetTrigger("EnemyAttack");
		}
	}
}
