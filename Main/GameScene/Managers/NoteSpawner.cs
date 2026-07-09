using Godot;
using System;

public partial class NoteSpawner : Node
{
	private GameManager manager;
	private ChartManager chartmanager;
	private Node2D container;
	[Export]
	private PackedScene noteScene;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		manager = GetNode<GameManager>("../../GameManager");
		chartmanager = GetNode<ChartManager>("../ChartManager");
		container = GetNode<Node2D>("../NoteContainer");
		manager.StateChanged += OnStateChanged;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(manager.State == GameStates.TUTORIAL_PLAY || manager.State == GameStates.PLAYING)
		{
			if(chartmanager.createNote < chartmanager.chart[0].Length){
			if(chartmanager.chart[0][chartmanager.createNote] - 1000f * (60f / (float)chartmanager.bpm) <= chartmanager.GetTime()){
				Notes note = noteScene.Instantiate<Notes>();
				note.noteType = chartmanager.chart[1][chartmanager.createNote];
				note.noteId = chartmanager.createNote;
				container.AddChild(note);
				chartmanager.createNote++;
			}
			}
		}
	}

	private void OnStateChanged(byte state)
	{
		if(state == (byte)GameStates.TUTORIAL_PLAY || state == (byte)GameStates.PLAYING)
			ClearNotes();
	}

	private void ClearNotes()
	{
		foreach (Node child in container.GetChildren())
			child.QueueFree();
	}
}
