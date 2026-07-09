using Godot;

public enum GameStates : byte
{
	TUTORIAL_TEXT = 0,
	TUTORIAL_PLAY = 1,
	PLAYING = 2,
	RESULT = 3,
	PAUSE = 4
}

public partial class GameManager : Node
{
	[Signal]
	public delegate void StateChangedEventHandler(byte state);

	[Signal]
	public delegate void ScoreChangedEventHandler(int score, int perfect, int good, int miss);

	[Signal]
	public delegate void ResultReadyEventHandler(int score, int perfect, int good, int miss, string rank, bool cleared);

	[Export] public int PerfectScore { get; set; } = 1000;
	[Export] public int GoodScore { get; set; } = 500;
	[Export(PropertyHint.Range, "0,1,0.01")] public float ClearRate { get; set; } = 0.6f;

	private GameStates _state = GameStates.PAUSE;

	public int Score { get; private set; }
	public int PerfectCount { get; private set; }
	public int GoodCount { get; private set; }
	public int MissCount { get; private set; }
	public int MaxScore { get; private set; }
	public int ClearScore => Mathf.CeilToInt(MaxScore * ClearRate);

	public GameStates State
	{
		get => _state;
		set
		{
			if (_state == value)
				return;

			_state = value;
			GD.Print($"GameState : {_state}");
			EmitSignal(SignalName.StateChanged, (byte)_state);
		}
	}

	public void StartTutorial()
	{
		State = GameStates.TUTORIAL_TEXT;
	}

	public void StartTutorialPlay()
	{
		State = GameStates.TUTORIAL_PLAY;
	}

	public void StartGame()
	{
		State = GameStates.PLAYING;
	}

	public void AddPerfect()
	{
		PerfectCount++;
		Score += PerfectScore;
		EmitScoreChanged();
	}

	public void AddGood()
	{
		GoodCount++;
		Score += GoodScore;
		EmitScoreChanged();
	}

	public void AddMiss()
	{
		MissCount++;
		EmitScoreChanged();
	}

	public void ShowResult()
	{
		State = GameStates.RESULT;
		EmitSignal(SignalName.ResultReady, Score, PerfectCount, GoodCount, MissCount, GetRank(), IsCleared());
	}

	public void PauseGame()
	{
		State = GameStates.PAUSE;
	}

	private void ResetResult(int noteCount)
	{
		Score = 0;
		PerfectCount = 0;
		GoodCount = 0;
		MissCount = 0;
		MaxScore = noteCount * PerfectScore;
		EmitScoreChanged();
	}

	private void EmitScoreChanged()
	{
		EmitSignal(SignalName.ScoreChanged, Score, PerfectCount, GoodCount, MissCount);
	}

	private bool IsCleared()
	{
		return MaxScore > 0 && Score >= ClearScore;
	}

	private string GetRank()
	{
		if (MaxScore <= 0)
			return "D";

		float rate = (float)Score / MaxScore;
		if (rate >= 0.9f)
			return "S";
		if (rate >= 0.75f)
			return "A";
		if (rate >= 0.6f)
			return "B";
		if (rate >= 0.4f)
			return "C";
		return "D";
	}
}
