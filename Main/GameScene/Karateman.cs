using Godot;
using System;

public partial class Karateman : Node2D
{
	private const float PunchDuration = 0.2f;
	private const float HurtDuration = 0.2f;

	private AnimatedSprite2D anim;
	private JudgeManager judge;
	private float actionTimer;

	public override void _Ready()
	{
	
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		judge = GetNodeOrNull<JudgeManager>("../GamePlay/JudgeManager");
		if (judge != null)
			judge.Judged += OnJudged;

		Idling();
	}

	public override void _Process(double delta)
	{
		if (actionTimer <= 0f)
			return;

		actionTimer -= (float)delta;
		if (actionTimer <= 0f)
			Idling();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("rhythm"))
			Punch();
	}

	public override void _ExitTree()
	{
		if (judge != null)
			judge.Judged -= OnJudged;
	}

	private void OnJudged(int type)
	{
		if (type == 2)
			Hurt();
		else
			Punch();
	}

	private void Punch()
	{
		if (anim == null)
			return;

		actionTimer = PunchDuration;
		anim.Play("punch");
	}

	private void Hurt()
	{
		if (anim == null)
			return;

		actionTimer = HurtDuration;
		anim.Play("hurt");
	}

	private void Idling()
	{
		if (anim != null)
			anim.Play("idoling");

		actionTimer = 0f;
	}
}
