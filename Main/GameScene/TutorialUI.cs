using Godot;

public partial class TutorialUI : Control
{
	private RichTextLabel textLabel;
	private Label arrow;
	private Timer timer;
	private GameManager gameManager;
	private int tutorialNum;
	[Export]
	private string[] tutorialTexts =
	{
		"カラテ家です。",
		"とりあえずやってみよう！"
	};
	private string[] tutorialTexts2 =
	{
		"なんとなくわかったかな？",
		"じゃあ本番いってみよう！"
	};

	private int textIndex;
	private string currentText = "";
	private bool typing;
	private bool finished;

	public override void _Ready()
	{
		tutorialNum = 0;
		textLabel = GetNode<RichTextLabel>("Panel/RichTextLabel");
		arrow = GetNode<Label>("Panel/Label");
		timer = GetNode<Timer>("Timer");
		gameManager = GetNode<GameManager>("../../GameManager");

		timer.Timeout += OnTimerTimeout;
		gameManager.StateChanged += OnStateChanged;
		arrow.Hide();
		Hide();
		OnStateChanged((byte)gameManager.State);
	}

	private void OnStateChanged(byte state)
	{
		if (state == (byte)GameStates.TUTORIAL_TEXT)
		{
			if(tutorialNum == 0){
			Show();
			textIndex = 0;
			ShowText(tutorialTexts[textIndex]);
			tutorialNum = 1;
			}else if(tutorialNum == 1){
				Show();
			textIndex = 0;
			ShowText(tutorialTexts2[textIndex]);
			tutorialNum = 2;
			}
		}
		else
		{
			Hide();
		}
	}

	private void ShowText(string text)
	{
		currentText = text;
		textLabel.Text = text;
		textLabel.VisibleCharacters = 0;
		typing = true;
		finished = false;
		arrow.Hide();
		timer.Start();
	}

	private void OnTimerTimeout()
	{
		if (textLabel.VisibleCharacters < currentText.Length)
		{
			textLabel.VisibleCharacters++;
			return;
		}

		timer.Stop();
		typing = false;
		finished = true;
		arrow.Show();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!Visible || !IsAdvanceInput(@event))
			return;

		if (typing)
		{
			timer.Stop();
			textLabel.VisibleCharacters = currentText.Length;
			typing = false;
			finished = true;
			arrow.Show();
		}
		else if (finished)
		{
			NextText();
		}

		GetViewport().SetInputAsHandled();
	}

	private void NextText()
	{
		textIndex++;

		if (textIndex >= (tutorialNum == 1 ? tutorialTexts.Length : tutorialTexts2.Length))
		{
			Hide();
			if(tutorialNum == 1)gameManager.StartTutorialPlay();
			if(tutorialNum == 2){gameManager.StartGame();
			tutorialNum = 0;}
			return;
		}

		ShowText(tutorialNum == 1 ? tutorialTexts[textIndex] : tutorialTexts2[textIndex]);
	}

	public override void _Process(double delta)
	{
		if (finished)
			arrow.Visible = (Time.GetTicksMsec() / 300) % 2 == 0;
	}

	private static bool IsAdvanceInput(InputEvent @event)
	{
		if (@event.IsActionPressed("rhythm") || @event.IsActionPressed("ui_accept"))
			return true;

		if (@event is InputEventMouseButton mouseButton)
			return mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed;

		return @event is InputEventScreenTouch screenTouch && screenTouch.Pressed;
	}
}
