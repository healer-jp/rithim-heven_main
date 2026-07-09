using Godot;
using System;

public partial class ScoreManager : Node
{
	private GameManager manager;
	private ChartManager chartmanager;
	private int perfect;
	private int bad;
	private int miss;
	private JudgeManager judge;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		manager = GetNode<GameManager>("../../GameManager");
		perfect = 0;
		bad = 0;
		miss = 0;
		judge = GetNode<JudgeManager>("../JudgeManager");
		chartmanager =  GetNode<ChartManager>("../ChartManager");
		judge.Judged += OnJudged;
		manager.StateChanged += OnStateChanged;
	}
	private void OnJudged(int type)
	{
		if(type == 0)perfect++;
		if(type == 1)bad++;
		if(type == 2)miss++;//3hakaraburi
	}
	private void OnStateChanged(byte state){
		if(state == (byte)GameStates.RESULT)
		{
			GD.Print($"perfect:{perfect} bad:{bad}miss:{miss}");
		}
	}
	public int GetEvaluation(){
		int fullscore = chartmanager.chart[0].Length * 8;
		int score = perfect * 8 + bad * 4;
		if(fullscore == score)return 3;
		else if(fullscore/8 * 7 <= score)return 2;
		else if(fullscore/2 <= score)return 1;
		else return 0;
	}//評価をえる 0で破門　1で平凡 2でハイスコア 3で最高
}
