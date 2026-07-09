using Godot;
using System;

public partial class Game : Node2D
{
	public override void _Ready()
	{
		var character = GetNode<Charactor>("Charactor");
		var flyingObjects = GetNode<FlyingObjects>("FlyingObjects");

		character.CreateNote += flyingObjects.OnCreateNote;
		character.HitEffect += flyingObjects.OnHitEffect;
	}
}
