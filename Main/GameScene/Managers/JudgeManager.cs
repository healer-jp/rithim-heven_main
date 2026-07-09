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
			if(chartmanager.judgeNote < chartmanager.chart[0].Length){
			time = chartmanager.GetTime();
			if(Input.IsActionJustPressed("rhythm"))
			{
				if(Math.Abs(chartmanager.chart[0][chartmanager.judgeNote] - time) < P_RANGE)
				{
					Judge(0);
					return;
				}else if(Math.Abs(chartmanager.chart[0][chartmanager.judgeNote] - time) < J_RANGE)
				{
					Judge(1);
					return;
				}else{
					
					EmitSignal(SignalName.Judged,3);
					return;
				}
				
			}
			if(chartmanager.chart[0][chartmanager.judgeNote] + J_RANGE < time){
				GD.Print("aaa");
				Judge(2);
				return;
			}
			
			}
			else {
				if(manager.State == GameStates.TUTORIAL_PLAY)manager.StartTutorial();
				if(manager.State == GameStates.PLAYING)manager.ShowResult();
			}
			
		}
	}
	private void Judge(int judgeType)
	{
		GD.Print($"Judge: note={chartmanager.judgeNote}, type={judgeType}, frame={Engine.GetProcessFrames()}");
		EmitSignal(SignalName.Judged,judgeType);
		EmitSignal(SignalName.NoteResolved,chartmanager.judgeNote,judgeType);
		EmitSignal(SignalName.RemoveNote,chartmanager.judgeNote);
		chartmanager.judgeNote++;
	}
}
