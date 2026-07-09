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

	private ulong lasttime;
	
	public override void _Ready()
	{
		lasttime = 0;
		chart1 = Score.GetScore(100,Score.score1);
		chart2 = Score.GetScore(105,Score.score2);
		chart3 = Score.GetScore(120,Score.score3);
		manager = GetNode<GameManager>("../../GameManager");
		manager.StateChanged += OnStateChanged;
	}
	public int GetTime()
	{
		if(manager.State == GameStates.TUTORIAL_PLAY || manager.State == GameStates.PLAYING){
			return (int)(Time.GetTicksMsec() - lasttime);
		}else return 0;
	}
	private void OnStateChanged(byte state)
	{
		if(state == (byte)GameStates.TUTORIAL_PLAY || state == (byte)GameStates.PLAYING)
		{
			lasttime = Time.GetTicksMsec();
			judgeNote = 0;
			createNote = 0;
		}
		if(state == (byte)GameStates.TUTORIAL_PLAY)
		{
			bpm = 100;
			chart = chart1;
		}
		if(state == (byte)GameStates.PLAYING)
		{
			bpm = 105;
			chart = chart2;
		}
	}
}
