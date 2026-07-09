using Godot;

public partial class GameScene : Node2D
{
	[Signal]
	public delegate void GetLoadingEventHandler();

	private const int Bpm = 120;
	private const int PerfectWindowMs = 80;
	private const int GoodWindowMs = 180;
	private const int SongEndDelayMs = 900;
	private const int ChartStartDelayMs = 1000;
	private const int ThrowLeadMs = 800;
	private const ulong ResultRetryLockMs = 700;

	private GameManager gameManager;
	private Node2D worldCharacter;
	private Node2D visualLayer;
	private Sprite2D screenCharacter;
	private Control playUI;
	private Control resultUI;
	private Label scoreLabel;
	private Label promptLabel;
	private Label judgeLabel;
	private Label resultTitleLabel;
	private Label resultScoreLabel;
	private Label resultBreakdownLabel;
	private Label resultRankLabel;
	private Label resultGuideLabel;

	private Texture2D characterTexture;
	private Texture2D throwTexture;
	private Sprite2D activeThrow;
	private int activeThrowNoteIndex = -1;
	private int[][] chart;
	private int currentNoteIndex;
	private ulong songStartedAt;
	private ulong resultShownAt;
	private bool isPlaying;

	public override void _Ready()
	{
		GD.Print("GameScene Ready");

		gameManager = GetNode<GameManager>("GameManager");
		worldCharacter = GetNodeOrNull<Node2D>("Karateman");
		gameManager.StateChanged += OnStateChanged;
		gameManager.ScoreChanged += OnScoreChanged;
		gameManager.ResultReady += OnResultReady;

		characterTexture = LoadTextureFromFile("res://assets/character.png");
		throwTexture = LoadTextureFromFile("res://assets/bulb.png");

		BindUi();
		EnsureScreenVisuals();
		ApplyResponsiveLayout();
		LoadChart();
		EmitSignal(SignalName.GetLoading);
		ShowPlayUi(false);
		ShowResultUi(false);
		gameManager.StartTutorial();
	}

	public override void _Process(double delta)
	{
		ApplyResponsiveLayout();

		if (!isPlaying || chart[0].Length == 0)
			return;

		int elapsed = GetSongElapsedMs();
		ResolveExpiredNotes(elapsed);
		UpdatePrompt(elapsed);

		int lastNoteTime = chart[0][chart[0].Length - 1];
		if (currentNoteIndex >= chart[0].Length && elapsed >= lastNoteTime + SongEndDelayMs)
			FinishSong();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (isPlaying && IsRhythmInput(@event))
		{
			JudgeInput();
			GetViewport().SetInputAsHandled();
			return;
		}

		/*if (gameManager.State == GameStates.RESULT && IsRhythmInput(@event) && Time.GetTicksMsec() - resultShownAt >= ResultRetryLockMs)
		{
			StartSong();
			GetViewport().SetInputAsHandled();
		}*/
	}

	private void BindUi()
	{
		playUI = GetNodeOrNull<Control>("CanvasLayer/PlayUI");
		resultUI = GetNodeOrNull<Control>("CanvasLayer/ResultUI");
		scoreLabel = GetNodeOrNull<Label>("CanvasLayer/PlayUI/ScoreLabel");
		promptLabel = GetNodeOrNull<Label>("CanvasLayer/PlayUI/PromptLabel");
		judgeLabel = GetNodeOrNull<Label>("CanvasLayer/PlayUI/JudgeLabel");
		resultTitleLabel = GetNodeOrNull<Label>("CanvasLayer/ResultUI/Panel/ResultTitleLabel");
		resultScoreLabel = GetNodeOrNull<Label>("CanvasLayer/ResultUI/Panel/ResultScoreLabel");
		//resultBreakdownLabel = GetNodeOrNull<Label>("CanvasLayer/ResultUI/Panel/ResultBreakdownLabel");
		//resultRankLabel = GetNodeOrNull<Label>("CanvasLayer/ResultUI/Panel/ResultRankLabel");
		//resultGuideLabel = GetNodeOrNull<Label>("CanvasLayer/ResultUI/Panel/ResultGuideLabel");
	}

	private void EnsureScreenVisuals()
	{
		if (worldCharacter != null)
			worldCharacter.Visible = true;
	}

	private void ApplyResponsiveLayout()
	{
		Vector2 size = GetViewportRect().Size;
		/*if (worldCharacter != null)
			worldCharacter.Position = new Vector2(size.X * 0.5f, size.Y * 0.63f);*/
		if (screenCharacter != null)
			screenCharacter.Position = new Vector2(size.X * 0.5f, size.Y * 0.63f);
	}

	private Vector2 GetThrowSpawnPosition()
	{
		Vector2 size = GetViewportRect().Size;
		return new Vector2(size.X * 0.9f, size.Y * 0.3f);
	}

	private Vector2 GetThrowTargetPosition()
	{
		if (screenCharacter != null)
			return screenCharacter.Position + new Vector2(0, -90);

		Vector2 size = GetViewportRect().Size;
		return new Vector2(size.X * 0.5f, size.Y * 0.55f);
	}

	private Vector2 GetThrowArcOffset()
	{
		return new Vector2(0, -GetViewportRect().Size.Y * 0.12f);
	}

	private void LoadChart()
	{
		Score.bpm = Bpm;
		chart = Score.GetScore(Bpm, Score.score1);
		for (int i = 0; i < chart[0].Length; i++)
			chart[0][i] += ChartStartDelayMs;
		currentNoteIndex = 0;
	}

private void OnStateChanged(byte state)
	{
		if (state == (byte)GameStates.TUTORIAL_PLAY)
			StartSong();
	}

	private void StartSong()
	{
		LoadChart();
		RemoveThrowVisual();
		ApplyResponsiveLayout();
		//gameManager.StartGame();
		currentNoteIndex = 0;
		songStartedAt = Time.GetTicksMsec();
		isPlaying = true;
		ShowPlayUi(true);
		ShowResultUi(false);
		SetJudgeText("START");
		UpdatePrompt(0);
	}

	private void JudgeInput()
	{
		if (currentNoteIndex >= chart[0].Length)
			return;

		int elapsed = GetSongElapsedMs();
		int diff = elapsed - chart[0][currentNoteIndex];
		int absDiff = Mathf.Abs(diff);

		if (absDiff <= PerfectWindowMs)
		{
			gameManager.AddPerfect();
			//SetJudgeText("PERFECT");
			currentNoteIndex++;
			RemoveThrowVisual();
		}
		else if (absDiff <= GoodWindowMs)
		{
			gameManager.AddGood();
			//SetJudgeText("GOOD");
			currentNoteIndex++;
			RemoveThrowVisual();
		}
		else if (diff > GoodWindowMs)
		{
			gameManager.AddMiss();
		//	SetJudgeText("MISS");
			currentNoteIndex++;
			RemoveThrowVisual();
		}
		else
		{
		//	SetJudgeText("EARLY");
		}
	}

	private void ResolveExpiredNotes(int elapsed)
	{
		while (currentNoteIndex < chart[0].Length && elapsed - chart[0][currentNoteIndex] > GoodWindowMs)
		{
			gameManager.AddMiss();
			SetJudgeText("MISS");
			currentNoteIndex++;
			RemoveThrowVisual();
		}
	}

	private void UpdateThrowVisual(int elapsed)
	{
		if (currentNoteIndex >= chart[0].Length || throwTexture == null)
		{
			RemoveThrowVisual();
			return;
		}

		int noteTime = chart[0][currentNoteIndex];
		int untilNote = noteTime - elapsed;
		if (untilNote > ThrowLeadMs || untilNote < -GoodWindowMs)
		{
			RemoveThrowVisual();
			return;
		}

		if (activeThrow == null || activeThrowNoteIndex != currentNoteIndex)
			CreateThrowVisual(currentNoteIndex);

		Vector2 spawn = GetThrowSpawnPosition();
		Vector2 target = GetThrowTargetPosition();
		float t = Mathf.Clamp((ThrowLeadMs - untilNote) / (float)ThrowLeadMs, 0f, 1f);
		activeThrow.Position = QuadraticBezier(spawn, (spawn + target) * 0.5f + GetThrowArcOffset(), target, t);
		activeThrow.Rotation = t * Mathf.Tau;
	}

	private void CreateThrowVisual(int noteIndex)
	{
		RemoveThrowVisual();
		activeThrow = new Sprite2D
		{
			Texture = throwTexture,
			Scale = new Vector2(0.14f, 0.14f),
			ZIndex = 30,
		};
		activeThrowNoteIndex = noteIndex;
		(visualLayer ?? this).AddChild(activeThrow);
	}

	private void RemoveThrowVisual()
	{
		if (activeThrow != null)
		{
			activeThrow.QueueFree();
			activeThrow = null;
		}
		activeThrowNoteIndex = -1;
	}

	private void UpdatePrompt(int elapsed)
	{
		if (promptLabel == null)
			return;

		if (currentNoteIndex >= chart[0].Length)
		{
			//promptLabel.Text = "Finish!";
			return;
		}

		int diff = chart[0][currentNoteIndex] - elapsed;
		//if (Mathf.Abs(diff) <= GoodWindowMs)
			//promptLabel.Text = "HIT!";
		//else if (diff > 0)
			//promptLabel.Text = $"Next: {diff / 1000.0f:0.0}s";
		//else
			//promptLabel.Text = "...";
	}

	private void FinishSong()
	{
		isPlaying = false;
		RemoveThrowVisual();
		ShowPlayUi(false);
		//gameManager.ShowResult();
	}

	private void OnScoreChanged(int score, int perfect, int good, int miss)
	{
		if (scoreLabel != null)
			scoreLabel.Text = $"Score: {score}";
	}

	private void OnResultReady(int score, int perfect, int good, int miss, string rank, bool cleared)
	{
		resultShownAt = Time.GetTicksMsec();
		//ShowResultUi(true);
		/*if (resultTitleLabel != null)
			resultTitleLabel.Text = cleared ? "CLEAR" : "FAILED";
		if (resultScoreLabel != null)
			resultScoreLabel.Text = $"Score: {score} / {gameManager.MaxScore}";
		if (resultBreakdownLabel != null)
			resultBreakdownLabel.Text = $"Perfect: {perfect}   Good: {good}   Miss: {miss}";
		if (resultRankLabel != null)
			resultRankLabel.Text = $"Rank: {rank}   Clear Score: {gameManager.ClearScore}";
		if (resultGuideLabel != null)
			resultGuideLabel.Text = "Space / Tap: Retry";*/
	}

	private void ShowPlayUi(bool visible)
	{
		if (playUI != null)
			playUI.Visible = visible;
	}

	private void ShowResultUi(bool visible)
	{
		if (resultUI != null)
			resultUI.Visible = visible;
	}

	private void SetJudgeText(string text)
	{
		if (judgeLabel != null)
			judgeLabel.Text = text;
	}

	private int GetSongElapsedMs()
	{
		return (int)(Time.GetTicksMsec() - songStartedAt);
	}

	private static Texture2D LoadTextureFromFile(string path)
	{
		Image image = Image.LoadFromFile(ProjectSettings.GlobalizePath(path));
		return image == null || image.IsEmpty() ? null : ImageTexture.CreateFromImage(image);
	}

	private static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
	{
		float u = 1f - t;
		return u * u * p0 + 2f * u * t * p1 + t * t * p2;
	}

	private static bool IsRhythmInput(InputEvent @event)
	{
		if (@event.IsActionPressed("rhythm"))
			return true;

		if (@event is InputEventMouseButton mouseButton)
			return mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed;

		return @event is InputEventScreenTouch screenTouch && screenTouch.Pressed;
	}
}
