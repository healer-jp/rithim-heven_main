using Godot;

public partial class FlyingObjects : Node2D
{
	private enum Phase
	{
		Approach,
		AtTarget,
		Bounce,
		MissToTarget,
	}

	private class FlyingBulb
	{
		public AnimatedSprite2D Sprite;
		public Phase Phase;
		public Vector2 Start;
		public Vector2 Control;
		public Vector2 End;
		public float Duration;
		public float Elapsed;
		public Vector2 TargetPosition;
	}

	[Export] public NodePath CharacterPath { get; set; } = new NodePath("../Charactor");
	[Export] public Vector2 SpawnOffset { get; set; } = new Vector2(400, -80);
	[Export] public float ArcHeight { get; set; } = 120f;
	[Export] public float BounceArcHeightPerfect { get; set; } = 180f;
	[Export] public float BounceArcHeightGood { get; set; } = 100f;
	[Export] public float BounceDistancePerfect { get; set; } = 350f;
	[Export] public float BounceDistanceGood { get; set; } = 200f;
	[Export] public float BounceDuration { get; set; } = 0.4f;

	private FlyingBulb _activeBulb;
	private Node2D _character;

	public override void _Ready()
	{
		_character = GetNodeOrNull<Node2D>(CharacterPath);
	}

	public override void _Process(double delta)
	{
		if (_activeBulb == null)
			return;

		var bulb = _activeBulb;
		bulb.Elapsed += (float)delta;

		if (bulb.Phase is Phase.Approach or Phase.MissToTarget or Phase.Bounce)
		{
			float t = Mathf.Clamp(bulb.Elapsed / bulb.Duration, 0f, 1f);
			bulb.Sprite.GlobalPosition = QuadraticBezier(bulb.Start, bulb.Control, bulb.End, t);

			if (t >= 1f)
			{
				switch (bulb.Phase)
				{
					case Phase.Approach:
						bulb.Phase = Phase.AtTarget;
						bulb.Sprite.GlobalPosition = bulb.End;
						break;
					case Phase.MissToTarget:
					case Phase.Bounce:
						RemoveBulb();
						break;
				}
			}
		}
	}

	public void OnCreateNote(int noteType)
	{
		if (noteType != 2 || _character == null)
			return;

		RemoveBulb();

		int bpm = Score.bpm > 0 ? Score.bpm : 120;
		float flyDuration = 60f / bpm;

		Vector2 targetPosition = _character.GlobalPosition;
		Vector2 spawnPosition = targetPosition + SpawnOffset;

		var sprite = CreateBulbSprite();
		AddChild(sprite);

		_activeBulb = new FlyingBulb
		{
			Sprite = sprite,
			Phase = Phase.Approach,
			Start = spawnPosition,
			End = targetPosition,
			Control = GetArcControl(spawnPosition, targetPosition, ArcHeight),
			Duration = flyDuration,
			Elapsed = 0f,
			TargetPosition = targetPosition,
		};
		sprite.GlobalPosition = spawnPosition;
	}

	public void OnHitEffect(int effectType)
	{
		if (_activeBulb == null)
			return;

		var bulb = _activeBulb;

		switch (effectType)
		{
			case 1:
				StartBounce(bulb, BounceDistancePerfect, BounceArcHeightPerfect);
				break;
			case 2:
				StartBounce(bulb, BounceDistanceGood, BounceArcHeightGood);
				break;
			case 3:
				if (bulb.Phase == Phase.AtTarget)
				{
					RemoveBulb();
				}
				else
				{
					bulb.Phase = Phase.MissToTarget;
					bulb.Start = bulb.Sprite.GlobalPosition;
					bulb.End = bulb.TargetPosition;
					bulb.Control = GetArcControl(bulb.Start, bulb.End, ArcHeight * 0.5f);
					float remaining = 1f - Mathf.Clamp(bulb.Elapsed / bulb.Duration, 0f, 1f);
					bulb.Duration = Mathf.Max(remaining * bulb.Duration, 0.05f);
					bulb.Elapsed = 0f;
				}
				break;
		}
	}

	private void StartBounce(FlyingBulb bulb, float distance, float arcHeight)
	{
		Vector2 currentPosition = bulb.Sprite.GlobalPosition;
		Vector2 bounceEnd = currentPosition + Vector2.Left * distance;

		bulb.Phase = Phase.Bounce;
		bulb.Start = currentPosition;
		bulb.End = bounceEnd;
		bulb.Control = GetArcControl(currentPosition, bounceEnd, arcHeight);
		bulb.Duration = BounceDuration;
		bulb.Elapsed = 0f;
	}

	private static Vector2 GetArcControl(Vector2 start, Vector2 end, float arcHeight)
	{
		Vector2 midpoint = (start + end) * 0.5f;
		return midpoint + Vector2.Up * arcHeight;
	}

	private static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
	{
		float u = 1f - t;
		return u * u * p0 + 2f * u * t * p1 + t * t * p2;
	}

	private AnimatedSprite2D CreateBulbSprite()
	{
		var spriteFrames = new SpriteFrames();
		spriteFrames.AddAnimation("bulb");
		spriteFrames.SetAnimationLoopMode("bulb", SpriteFrames.LoopMode.Linear);
		spriteFrames.SetAnimationSpeed("bulb", 5f);

		Texture2D bulbTexture = LoadTextureFromFile("res://assets/bulb.png");
		if (bulbTexture != null)
			spriteFrames.AddFrame("bulb", bulbTexture);

		var sprite = new AnimatedSprite2D
		{
			SpriteFrames = spriteFrames,
			Animation = "bulb",
			Scale = new Vector2(0.28f, 0.28f),
			ZIndex = 30,
		};
		sprite.Play("bulb");
		return sprite;
	}

	private static Texture2D LoadTextureFromFile(string path)
	{
		Image image = Image.LoadFromFile(ProjectSettings.GlobalizePath(path));
		return image == null || image.IsEmpty() ? null : ImageTexture.CreateFromImage(image);
	}
	private void RemoveBulb()
	{
		if (_activeBulb == null)
			return;

		_activeBulb.Sprite.QueueFree();
		_activeBulb = null;
	}
}
