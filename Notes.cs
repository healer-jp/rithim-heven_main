using Godot;

public partial class Notes : AnimatedSprite2D
{
	private enum NoteMotion
	{
		Flying,
		Bouncing,
		Falling,
		Done,
	}

	private const float ArcHeight = 200f;
	private const float PerfectBounceDistance = 420f;
	private const float GoodBounceDistance = 300f;
	private const float PerfectBounceHeight = 240f;
	private const float GoodBounceHeight = 160f;
	private const float BounceDuration = 0.35f;
	private const float FallDuration = 0.42f;

	private JudgeManager judge;
	private ChartManager chart;
	private Node2D karateman;
	private NoteMotion motion = NoteMotion.Flying;
	private Vector2 spawnPosition;
	private Vector2 hitPosition;
	private Vector2 groundPosition;
	private Vector2 motionStart;
	private Vector2 motionControl;
	private Vector2 motionEnd;
	private float motionElapsed;
	private float motionDuration = 1f;
	private bool resolved;

	public int noteType;
	public int noteId;

	public override void _Ready()
	{
		judge = GetNodeOrNull<JudgeManager>("../JudgeManager") ?? GetNodeOrNull<JudgeManager>("../../JudgeManager");
		chart = GetNodeOrNull<ChartManager>("../ChartManager") ?? GetNodeOrNull<ChartManager>("../../ChartManager");
		karateman = GetTree().CurrentScene?.GetNodeOrNull<Node2D>("Karateman");

		if (judge != null)
			judge.NoteResolved += OnNoteResolved;

		SpriteFrames = BuildSpriteFrames(noteType);
		Animation = GetBaseAnimationName(noteType);
		Play(Animation);

		Scale = new Vector2(0.15f, 0.15f);
		ZIndex = 30;
		SetupPositions();
		GlobalPosition = spawnPosition;
	}

	public override void _Process(double delta)
	{
		Rotation += (float)delta * 5f;

		if (motion == NoteMotion.Flying)
		{
			UpdateFlyingPosition();
			return;
		}

		if (motion == NoteMotion.Bouncing || motion == NoteMotion.Falling)
			UpdateResolvedMotion((float)delta);
	}

	public override void _ExitTree()
	{
		if (judge != null)
			judge.NoteResolved -= OnNoteResolved;
	}

	private void OnNoteResolved(int id, int type)
	{
		if (id != noteId || resolved)
			return;

		resolved = true;

		if (type == 0)
		{
			PlayBreakAnimation();
			StartBounce(PerfectBounceDistance, PerfectBounceHeight);
		}
		else if (type == 1)
		{
			StartBounce(GoodBounceDistance, GoodBounceHeight);
		}
		else if (type == 2)
		{
			StartFallAfterBodyHit();
		}
	}

	private void SetupPositions()
	{
		Vector2 viewportSize = GetViewportRect().Size;
		hitPosition = karateman != null
			? karateman.GlobalPosition + new Vector2(60f, -25f)
			: new Vector2(viewportSize.X * 0.8f, viewportSize.Y * 0.4f);

		if(chart.chart[1][noteId] == 4){
			spawnPosition = hitPosition + new Vector2(50f,-20f);
		groundPosition = hitPosition + new Vector2(20f, viewportSize.Y * 0.22f);
		}
		else
		{
			spawnPosition = new Vector2(viewportSize.X,viewportSize.Y/2);
			groundPosition = hitPosition + new Vector2(20f, viewportSize.Y * 0.22f);
		
		}
	}

	private void UpdateFlyingPosition()
	{
		if (chart == null || chart.chart == null || chart.chart[0].Length <= noteId)
			return;

		int bpm = chart.bpm > 0 ? chart.bpm : 120;
		float duration = 60000f / bpm;
		float hitTime = chart.chart[0][noteId];
		float now = chart.GetTime();
		float t = Mathf.Clamp((now - (hitTime - duration)) / duration, 0f, 1f);

		GlobalPosition = QuadraticBezier(
			spawnPosition,
			GetArcControl(spawnPosition, hitPosition, ArcHeight),
			hitPosition,
			t
			
		);
	}

	private void StartBounce(float distance, float height)
	{
		motion = NoteMotion.Bouncing;
		motionStart = GlobalPosition;
		motionEnd = GlobalPosition + new Vector2(-distance, 80f);
		motionControl = GetArcControl(motionStart, motionEnd, height);
		motionDuration = BounceDuration;
		motionElapsed = 0f;
	}

	private void StartFallAfterBodyHit()
	{
		GlobalPosition = hitPosition;
		motion = NoteMotion.Falling;
		motionStart = hitPosition;
		motionEnd = groundPosition;
		motionControl = hitPosition + new Vector2(60f, 20f);
		motionDuration = FallDuration;
		motionElapsed = 0f;
		Rotation = 0f;
	}

	private void UpdateResolvedMotion(float delta)
	{
		motionElapsed += delta;
		float t = Mathf.Clamp(motionElapsed / motionDuration, 0f, 1f);
		GlobalPosition = QuadraticBezier(motionStart, motionControl, motionEnd, t);

		if (motion == NoteMotion.Falling)
			Rotation = Mathf.Lerp(0f, Mathf.Pi * 0.5f, t);

		if (t < 1f)
			return;

		motion = NoteMotion.Done;
		QueueFree();
	}

	private void PlayBreakAnimation()
	{
		if (SpriteFrames != null && SpriteFrames.HasAnimation("break"))
			Play("break");
	}

	private static SpriteFrames BuildSpriteFrames(int type)
	{
		var frames = new SpriteFrames();
		string baseAnimation = GetBaseAnimationName(type);
		frames.AddAnimation(baseAnimation);
		frames.SetAnimationLoopMode(baseAnimation, SpriteFrames.LoopMode.Linear);
		frames.SetAnimationSpeed(baseAnimation, 5f);
		AddTextureFrame(frames, baseAnimation, GetBaseTexturePath(type));

		frames.AddAnimation("break");
		frames.SetAnimationLoopMode("break", SpriteFrames.LoopMode.None);
		frames.SetAnimationSpeed("break", 10f);
		AddTextureFrame(frames, "break", GetBrokenTexturePath(type));
		AddTextureFrame(frames, "break", "res://imported/爆発エフェクト(透過後).png");
		AddTextureFrame(frames, "break", "res://imported/星(透過後).png");
		return frames;
	}

	private static void AddTextureFrame(SpriteFrames frames, string animationName, string path)
	{
		Texture2D texture = LoadTextureFromFile(path);
		if (texture != null)
			frames.AddFrame(animationName, texture);
	}

	private static string GetBaseAnimationName(int type)
	{
		return type switch
		{
			2 => "bulb",
			3 => "barrel",
			4 => "bomb",
			_ => "normal",
		};
	}

	private static string GetBaseTexturePath(int type)
	{
		return type switch
		{
			2 => "res://imported/電球(透過後).png",
			3 => "res://imported/樽(透過後).png",
			4 => "res://imported/爆弾(透過後).png",
			_ => "res://imported/植木鉢(透過後).png",
		};
	}

	private static string GetBrokenTexturePath(int type)
	{
		return type switch
		{
			2 => "res://imported/破損した電球(透過後).png",
			3 => "res://imported/樽の破片(透過後).png",
			4 => "res://imported/爆発エフェクト(透過後).png",
			_ => "res://imported/星(透過後).png",
		};
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

	private static Texture2D LoadTextureFromFile(string path)
	{
		Image image = Image.LoadFromFile(ProjectSettings.GlobalizePath(path));
		return image == null || image.IsEmpty() ? null : ImageTexture.CreateFromImage(image);
	}
}
