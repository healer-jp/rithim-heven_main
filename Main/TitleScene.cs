using Godot;

public partial class TitleScene : Control
{
	public override void _Ready()
	{
		TextureRect textureRect = GetNodeOrNull<TextureRect>("TextureRect");
		if (textureRect != null)
			textureRect.Texture = LoadTextureFromFile("res://assets/title.png");
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("rhythm"))
			GetTree().ChangeSceneToFile("res://Main/select_scene.tscn");
	}

	private static Texture2D LoadTextureFromFile(string path)
	{
		Image image = Image.LoadFromFile(ProjectSettings.GlobalizePath(path));
		return image == null || image.IsEmpty() ? null : ImageTexture.CreateFromImage(image);
	}
}
