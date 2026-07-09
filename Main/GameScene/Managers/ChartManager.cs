using Godot;

public partial class ChartManager : Node
{
	public int[][] chart;
	public int bpm;
	
	public int judgeNote;
	public int createNote;
	
	private int[][] chart1;
	private int[][] chart2;
	private int[][] chart3;
	private GameManager manager;
	private AudioManager audioManager;
	
	public override void _Ready()
	{
		chart1 = Score.GetScore(100,Score.score1);
		chart2 = Score.GetScore(105,Score.score2);
		chart3 = Score.GetScore(120,Score.score3);
		PrepareChart(100, Score.score1);
		manager = GetNode<GameManager>("../../GameManager");
		audioManager = GetNodeOrNull<AudioManager>("../AudioManager");
		manager.StateChanged += OnStateChanged;
	}

	public int NoteCount => chart == null ? 0 : chart[0].Length;

	public int GetTime()
	{
		if(manager.State == GameStates.TUTORIAL_PLAY || manager.State == GameStates.PLAYING){
			return audioManager == null ? 0 : audioManager.GetSongTimeMs();
		}else return 0;
	}

	public void PrepareChart(int selectedBpm, int[][] sourceChart)
	{
		bpm = selectedBpm;
		chart = Score.GetScore(selectedBpm, sourceChart);
		judgeNote = 0;
		createNote = 0;
	}

	private void OnStateChanged(byte state)
	{
		if(state == (byte)GameStates.TUTORIAL_PLAY || state == (byte)GameStates.PLAYING)
		{
			judgeNote = 0;
			createNote = 0;
		}
		if(state == (byte)GameStates.TUTORIAL_PLAY)
		{
			PrepareChart(100, Score.score1);
		}
		if(state == (byte)GameStates.PLAYING)
		{
			if (chart == null)
				PrepareChart(105, Score.score2);
		}
	}
}
