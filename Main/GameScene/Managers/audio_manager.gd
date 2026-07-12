class_name AudioManagerGd
extends Node

const MUSIC_PATH := "res://imported/Mixdown.wav"
const TUTORIAL_PATH := "res://imported/tutorial.wav"
const PERFECT_SE_PATH := "res://imported/SE1_nc246084_【スマブラSP】_ジャストガード音_高音質.wav"
const GOOD_SE_PATH := "res://imported/SE3_nc389984_【効果音】__手を叩く・はたく・ハイタッチ.wav"
const MISS_SE_PATH := "res://imported/SE5_nc478542_破壊音.mp3"
const BOMB_MISS_SE_PATH := "res://imported/SE4_bomb_explosion.mp3"
const EARLY_SE_PATH := "res://imported/SE6_重いパンチ3.mp3"
const BOMB_NOTE_TYPE := 4

var music: AudioStream
var tutorial: AudioStream
var player: AudioStreamPlayer2D
var perfect_se: AudioStreamPlayer
var good_se: AudioStreamPlayer
var miss_se: AudioStreamPlayer
var bomb_miss_se: AudioStreamPlayer
var early_se: AudioStreamPlayer
var judge_manager: JudgeManagerGd
var manager: GameManagerGd
var playback_started_at_ms := 0
var last_song_time_ms := 0
var song_started := false
var timeline_paused_at_ms := -1

var is_playing: bool:
	get: return player != null and player.playing

func _ready() -> void:
	player = get_node_or_null("AudioStreamPlayer2D")
	if player != null: player.bus = SettingsManager.BGM_BUS
	manager = get_node("../../GameManager")
	music = _load_audio(MUSIC_PATH)
	tutorial = _load_audio(TUTORIAL_PATH)
	perfect_se = _create_se_player(PERFECT_SE_PATH)
	good_se = _create_se_player(GOOD_SE_PATH)
	good_se.volume_db = -8.0
	miss_se = _create_se_player(MISS_SE_PATH)
	bomb_miss_se = _create_se_player(BOMB_MISS_SE_PATH)
	bomb_miss_se.volume_db = -15.0
	early_se = _create_se_player(EARLY_SE_PATH)
	judge_manager = get_node_or_null("../JudgeManager")
	if judge_manager != null:
		judge_manager.karateman_action.connect(_on_karateman_action)

func play_song() -> void:
	if player == null: return
	player.stream = tutorial if manager.state == GameManagerGd.State.TUTORIAL_PLAY else music
	player.stop()
	playback_started_at_ms = Time.get_ticks_msec()
	last_song_time_ms = 0
	song_started = true
	timeline_paused_at_ms = -1
	player.play()

func stop_song() -> void:
	if player != null: player.stop()
	timeline_paused_at_ms = -1

func pause_timeline() -> void:
	if song_started and timeline_paused_at_ms < 0:
		timeline_paused_at_ms = Time.get_ticks_msec()

func resume_timeline() -> void:
	if timeline_paused_at_ms < 0: return
	playback_started_at_ms += Time.get_ticks_msec() - timeline_paused_at_ms
	timeline_paused_at_ms = -1

func get_song_time_ms() -> int:
	if player == null or not song_started: return last_song_time_ms
	var clock_now := timeline_paused_at_ms if timeline_paused_at_ms >= 0 else Time.get_ticks_msec()
	var monotonic_time := maxi(0, clock_now - playback_started_at_ms)
	var candidate_time := monotonic_time
	# Web sample playback does not share the desktop mixer's latency model. Using a
	# monotonic clock keeps judgement stable and time advancing after audio ends.
	if not OS.has_feature("web") and player.playing:
		var mixed_seconds := player.get_playback_position() + AudioServer.get_time_since_last_mix() - AudioServer.get_output_latency()
		candidate_time = maxi(0, roundi(mixed_seconds * 1000.0))
	last_song_time_ms = maxi(last_song_time_ms, candidate_time)
	return last_song_time_ms

func _on_karateman_action(judge_type: int, note_type: int, _is_kick: bool, _is_input: bool) -> void:
	match judge_type:
		0: _play_se(perfect_se)
		1: _play_se(good_se)
		2: _play_se(bomb_miss_se if note_type == BOMB_NOTE_TYPE else miss_se)
		3: _play_se(early_se)

func _create_se_player(path: String) -> AudioStreamPlayer:
	var se_player := AudioStreamPlayer.new()
	se_player.stream = _load_audio(path)
	se_player.volume_db = -20.0
	se_player.bus = SettingsManager.SE_BUS
	add_child(se_player)
	return se_player

static func _play_se(se_player: AudioStreamPlayer) -> void:
	if se_player == null or se_player.stream == null: return
	se_player.stop()
	se_player.play()

static func _load_audio(path: String) -> AudioStream:
	var stream := load(path) as AudioStream
	if stream == null: push_warning("Audio file could not be loaded: " + path)
	return stream
