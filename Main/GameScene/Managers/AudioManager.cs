using Godot;

public partial class AudioManager : Node
{
	private const string MusicPath = "res://imported/Mixdown.wav";
	private const string PerfectSePath = "res://imported/SE1_nc246084_【スマブラSP】_ジャストガード音_高音質.wav";
	private const string GoodSePath = "res://imported/SE3_nc389984_【効果音】__手を叩く・はたく・ハイタッチ.wav";
	private const string MissSePath = "res://imported/SE5_nc478542_破壊音.mp3";
	private const string BombMissSePath = "res://imported/SE4_bomb_explosion.mp3";
	private const string EarlySePath = "res://imported/SE6_重いパンチ3.mp3";
	private const int BombNoteType = 4;

	private AudioStreamPlayer2D player;
	private AudioStreamPlayer perfectSe;
	private AudioStreamPlayer goodSe;
	private AudioStreamPlayer missSe;
	private AudioStreamPlayer bombMissSe;
	private AudioStreamPlayer earlySe;
	private JudgeManager judgeManager;

	public bool IsPlaying => player != null && player.Playing;

	public override void _Ready()
	{
		player = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		if (player != null)
			player.Stream = LoadAudio(MusicPath);

		perfectSe = CreateSePlayer(PerfectSePath);
		goodSe = CreateSePlayer(GoodSePath);
		missSe = CreateSePlayer(MissSePath);
		bombMissSe = CreateSePlayer(BombMissSePath);
		earlySe = CreateSePlayer(EarlySePath);

		judgeManager = GetNodeOrNull<JudgeManager>("../JudgeManager");
		if (judgeManager != null)
			judgeManager.KaratemanAction += OnKaratemanAction;
	}

	public override void _ExitTree()
	{
		if (judgeManager != null)
			judgeManager.KaratemanAction -= OnKaratemanAction;
	}

	public void PlaySong()
	{
		if (player == null)
			return;

		player.Stop();
		player.Play();
	}

	public void StopSong()
	{
		if (player != null)
			player.Stop();
	}

	public int GetSongTimeMs()
	{
		if (player == null || !player.Playing)
			return 0;

		double playbackSeconds = player.GetPlaybackPosition();
		double mixedSeconds = playbackSeconds + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency();
		return Mathf.Max(0, Mathf.RoundToInt((float)(mixedSeconds * 1000.0)));
	}

	private void OnKaratemanAction(int judgeType, int noteType, bool isKick, bool isInput)
	{
		switch (judgeType)
		{
			case 0:
				PlaySe(perfectSe);
				break;
			case 1:
				PlaySe(goodSe);
				break;
			case 2:
				PlaySe(noteType == BombNoteType ? bombMissSe : missSe);
				break;
			case 3:
				PlaySe(earlySe);
				break;
		}
	}

	private AudioStreamPlayer CreateSePlayer(string path)
	{
		var sePlayer = new AudioStreamPlayer
		{
			Stream = LoadAudio(path),
			VolumeDb = -10f,
		};
		AddChild(sePlayer);
		return sePlayer;
	}

	private static void PlaySe(AudioStreamPlayer sePlayer)
	{
		if (sePlayer == null || sePlayer.Stream == null)
			return;

		sePlayer.Stop();
		sePlayer.Play();
	}

	private static AudioStream LoadAudio(string path)
	{
		AudioStream stream = ResourceLoader.Load<AudioStream>(path);
		if (stream == null)
			GD.PushWarning($"Audio file could not be loaded: {path}");

		return stream;
	}
}
