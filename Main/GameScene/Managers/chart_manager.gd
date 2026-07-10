class_name ChartManagerGd
extends Node

var chart: Array = []
var bpm := 0
var judge_note := 0
var create_note := 0
var manager: GameManagerGd
var audio_manager: AudioManagerGd

var note_count: int:
	get: return 0 if chart.is_empty() else chart[0].size()

func _ready() -> void:
	prepare_chart(100, ScoreData.SCORE_1)
	manager = get_node("../../GameManager")
	audio_manager = get_node_or_null("../AudioManager")
	manager.state_changed.connect(_on_state_changed)

func get_time() -> int:
	if manager.state in [GameManagerGd.State.TUTORIAL_PLAY, GameManagerGd.State.PLAYING]:
		return 0 if audio_manager == null else audio_manager.get_song_time_ms()
	return 0

func prepare_chart(selected_bpm: int, source_chart: Array) -> void:
	bpm = selected_bpm
	chart = ScoreData.get_score(selected_bpm, source_chart)
	judge_note = 0
	create_note = 0

func _on_state_changed(new_state: int) -> void:
	if new_state in [GameManagerGd.State.TUTORIAL_PLAY, GameManagerGd.State.PLAYING]:
		judge_note = 0
		create_note = 0
	if new_state == GameManagerGd.State.TUTORIAL_PLAY:
		prepare_chart(100, ScoreData.SCORE_1)
	elif new_state == GameManagerGd.State.PLAYING and chart.is_empty():
		prepare_chart(105, ScoreData.SCORE_2)
