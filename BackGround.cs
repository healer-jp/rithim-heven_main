using Godot;

public partial class BackGround : AnimatedSprite2D
{
	public override void _Ready()
	{
		Texture2D texture = LoadTextureFromFile("res://assets/background.png");
		if (texture != null)
		{
			var frames = new SpriteFrames();
			frames.AddAnimation("BackGround");
			frames.AddFrame("BackGround", texture);
			frames.SetAnimationLoopMode("BackGround", SpriteFrames.LoopMode.Linear);
			SpriteFrames = frames;
			Animation = "BackGround";
			Play("BackGround");
		}
	}

	private static Texture2D LoadTextureFromFile(string path)
	{
		Image image = Image.LoadFromFile(ProjectSettings.GlobalizePath(path));
		return image == null || image.IsEmpty() ? null : ImageTexture.CreateFromImage(image);
	}
}
