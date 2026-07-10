using Godot;
using System;

public partial class JudgeManager : Node
{
	public const int P_RANGE = 70;
	public const int J_RANGE = 160;
	private GameManager manager;
	private ChartManager chartmanager;
	private int time;
	private int kickUsedNoteIndex = -1;
	private int kickLockedNoteIndex = -1;
	[Signal]
	public delegate void JudgedEventHandler(int type);
	[Signal]
	public delegate void RemoveNoteEventHandler(int id);
	[Signal]
	public delegate void NoteResolvedEventHandler(int id, int type);
	[Signal]
	public delegate void KaratemanActionEventHandler(int judgeType, int noteType, bool isKick, bool isInput);

	public override void _Ready()
	{
		manager = GetNode<GameManager>("../../GameManager");
		chartmanager = GetNode<ChartManager>("../ChartManager");
		manager.StateChanged += OnStateChanged;
	}

	public override void _Process(double delta)
	{
		if(manager.State == GameStates.TUTORIAL_PLAY || manager.State == GameStates.PLAYING)
		{
			if(chartmanager.chart != null && chartmanager.judgeNote < chartmanager.chart[0].Length){
			time = chartmanager.GetTime();
			if(Input.IsActionJustPressed("rhythm"))
			{
				if (IsInputLocked())
					return;

				int diff = time - chartmanager.chart[0][chartmanager.judgeNote];
				int absDiff = Math.Abs(diff);
				bool isKick = IsKickWindow();
				if(absDiff <= P_RANGE)
				{
					Judge(0, true, isKick);
					return;
				}else if(absDiff <= J_RANGE)
				{
					Judge(1, true, isKick);
					return;
				}else if(diff > J_RANGE)
				{
					Judge(2, true, false);
					return;
				}else{
					EmitSignal(SignalName.Judged,3);
					EmitKaratemanAction(3, isKick, true);
					if (isKick)
						kickLockedNoteIndex = chartmanager.judgeNote;
					return;
				}
				
			}
			if(chartmanager.chart[0][chartmanager.judgeNote] + J_RANGE < time){
				Judge(2, false, false);
				return;
			}
			
			}
			
		}
	}

	private bool IsInputLocked()
	{
		if (kickLockedNoteIndex != chartmanager.judgeNote)
			return false;

		int noteEndTime = chartmanager.chart[0][chartmanager.judgeNote] + J_RANGE;
		if (time <= noteEndTime)
			return true;

		kickLockedNoteIndex = -1;
		return false;
	}

	private void OnStateChanged(byte state)
	{
		if(state == (byte)GameStates.TUTORIAL_PLAY || state == (byte)GameStates.PLAYING)
		{
			kickUsedNoteIndex = -1;
			kickLockedNoteIndex = -1;
		}
	}

	private bool IsKickWindow()
	{
		int noteIndex = chartmanager.judgeNote;
		if (chartmanager.chart == null || noteIndex <= 0 || noteIndex >= chartmanager.chart[0].Length)
			return false;
		if (kickUsedNoteIndex == noteIndex)
			return false;
		if (chartmanager.chart[1][noteIndex - 1] != 3 || chartmanager.chart[1][noteIndex] != 4)
			return false;

		int start = chartmanager.chart[0][noteIndex - 1] + J_RANGE;
		int end = chartmanager.chart[0][noteIndex] + J_RANGE;
		return time >= start && time <= end;
	}

	private void Judge(int judgeType, bool isInput, bool isKick)
	{
		GD.Print($"Judge: note={chartmanager.judgeNote}, type={judgeType}, frame={Engine.GetProcessFrames()}");
		EmitKaratemanAction(judgeType, isKick, isInput);

		if (judgeType == 0)
			manager.AddPerfect();
		else if (judgeType == 1)
			manager.AddGood();
		else if (judgeType == 2)
			manager.AddMiss();

		EmitSignal(SignalName.Judged,judgeType);
		EmitSignal(SignalName.NoteResolved,chartmanager.judgeNote,judgeType);
		EmitSignal(SignalName.RemoveNote,chartmanager.judgeNote);
		if (kickLockedNoteIndex == chartmanager.judgeNote)
			kickLockedNoteIndex = -1;
		chartmanager.judgeNote++;
	}

	private void EmitKaratemanAction(int judgeType, bool isKick, bool isInput)
	{
		int noteType = chartmanager.chart == null || chartmanager.judgeNote >= chartmanager.chart[1].Length
			? 0
			: chartmanager.chart[1][chartmanager.judgeNote];
		if (isKick)
			kickUsedNoteIndex = chartmanager.judgeNote;

		EmitSignal(SignalName.KaratemanAction, judgeType, noteType, isKick, isInput);
	}
}
