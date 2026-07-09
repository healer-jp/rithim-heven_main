using Godot;
using System;

public partial class Charactor : AnimatedSprite2D
{
	private ulong last;
	private AnimatedSprite2D anim;
	[Signal]
	public delegate void HitEffectEventHandler(int effectType);//エフェクトが出るときのシグナル effectType=1でヒット　effectType=2 でかすり　effectType=3で失敗（仮）
	
	[Signal]
	public delegate void CreateNoteEventHandler(int noteType);//球が発射されるときのシグナル　noteType=1で通常 noteType=2で電球 noteType=3で樽 noteType=4で爆弾
	private int[][] score;
	private int e;
	private int time;
	private int note_a;//連続防止用1
	private int hantei_a;//連続防止用2
	private readonly int H_RANGE = 100;//判定の幅
	private readonly int P_RANGE = 10;//成功判定の幅
	private int bpm;
	[Export]private int exbpm;
	[Export]private int bpm1;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bpm = bpm1;
		Score.bpm = bpm;
		note_a = 0;
		hantei_a = 0;
		score = Score.GetScore(bpm,Score.score1);
		last = 0;
		anim = GetNode<AnimatedSprite2D>("Charactor");
		anim.AnimationFinished += IdleAnimation;
		anim.Play("taiki");
		
		
	}
	private void StopScore(){
		last = 0;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		if(last != 0){
			time = (int)(Time.GetTicksMsec() - last);
			if(score[0][e] <= time + (60 / bpm)){
				if(note_a == 0){
					EmitSignal(SignalName.CreateNote,score[1][e]);
					note_a = 1;
				}
			}
			if(Input.IsActionJustPressed("rhythm")){
				
				if(hantei_a == 0){
					if(score[0][e] - P_RANGE <= time &&  time <= score[0][e] + P_RANGE)
					{
						EmitSignal(SignalName.HitEffect,1);
						anim.Play("attack1");
						hantei_a = 1;
					}else if(score[0][e] - H_RANGE <= time && time <= score[0][e] + H_RANGE)
					{
						EmitSignal(SignalName.HitEffect,2);
						anim.Play("attack2");
						hantei_a = 1;
					}
				}
			}
				if(time >= score[0][e] + H_RANGE){
					e += 1;
					if(hantei_a == 0){
						EmitSignal(SignalName.HitEffect,3);
						anim.Play("failed");
					}
					note_a = 0;
					hantei_a = 0;
					
				}
		}
	}
	private void IdleAnimation(){
		anim.Play("taiki");
	}
	private void OnStartGame(){
		e = 0;
		last = (Time.GetTicksMsec());
	}
}
