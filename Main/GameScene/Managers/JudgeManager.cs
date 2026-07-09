using Godot;
using System;

public partial class JudgeManager : Node
{
	public const int P_RANGE = 100;
	public const int J_RANGE = 160;
	private GameManager manager;
	private ChartManager chartmanager;
	private int time;
	[Signal]
	public delegate void JudgedEventHandler(int type);
	[Signal]
	public delegate void RemoveNoteEventHandler(int id);
	[Signal]
	public delegate void NoteResolvedEventHandler(int id, int type);

	public override void _Ready()
	{
		manager = GetNode<GameManager>("../../GameManager");
		chartmanager = GetNode<ChartManager>("../ChartManager");
	}

	public override void _Process(double delta)
	{
		if(manager.State == GameStates.TUTORIAL_PLAY || manager.State == GameStates.PLAYING)
		{
			if(chartmanager.chart != null && chartmanager.judgeNote < chartmanager.chart[0].Length){
			time = chartmanager.GetTime();
			if(Input.IsActionJustPressed("rhythm"))
			{
				int diff = time - chartmanager.chart[0][chartmanager.judgeNote];
				int absDiff = Math.Abs(diff);
				if(absDiff <= P_RANGE)
				{
					Judge(0);
					return;
				}else if(absDiff <= J_RANGE)
				{
					Judge(1);
					return;
				}else if(diff > J_RANGE)
				{
					Judge(2);
					return;
				}else{
					
					EmitSignal(SignalName.Judged,3);
					return;
				}
				
			}
			if(chartmanager.chart[0][chartmanager.judgeNote] + J_RANGE < time){
				Judge(2);
				return;
			}
			
			}
			
		}
	}
	private void Judge(int judgeType)
	{
		GD.Print($"Judge: note={chartmanager.judgeNote}, type={judgeType}, frame={Engine.GetProcessFrames()}");
		if (judgeType == 0)
			manager.AddPerfect();
		else if (judgeType == 1)
			manager.AddGood();
		else if (judgeType == 2)
			manager.AddMiss();

		EmitSignal(SignalName.Judged,judgeType);
		EmitSignal(SignalName.NoteResolved,chartmanager.judgeNote,judgeType);
		EmitSignal(SignalName.RemoveNote,chartmanager.judgeNote);
		chartmanager.judgeNote++;
	}
}
