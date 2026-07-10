using Godot;
using System;

public partial class ResultUi : Control
{
	private Label titleLabel;
	private Label scoreLabel;
	private GameManager manager;
	private ScoreManager scoremanager;
	public override void _Ready()
	{
		manager = GetNode<GameManager>("../../GameManager");
		scoremanager = GetNode<ScoreManager>("../../GamePlay/ScoreManager");
		titleLabel = GetNode<Label>("Panel/ResultTitleLabel");
		scoreLabel = GetNode<Label>("Panel/ResultScoreLabel");
		
		manager.StateChanged += OnStateChanged;
	}
	private void OnStateChanged(byte state)
	{
		if(state == (byte)GameStates.RESULT)ShowResult();
		else HideResult();
	}
	private void ShowResult()
	{
		Visible = true;
		switch(scoremanager.GetEvaluation()){
			case 0:
				titleLabel.Text = "failed";
				scoreLabel.Text = "がんばりましょう";
				break;
			case 1:
				titleLabel.Text = "so so";
				scoreLabel.Text = "まあまあできてた";
				break;
			case 2:
				titleLabel.Text = "great";
				scoreLabel.Text = "すごくできてた";
				break;
			case 3:
				titleLabel.Text = "perfect";
				scoreLabel.Text = "最高！";
				break;
			//テキストは仮
		}
		
	}
	private void HideResult()
	{
		Visible = false;
	}
}
