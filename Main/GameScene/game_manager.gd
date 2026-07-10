class_name GameManagerGd
extends Node

signal state_changed(state: int)
signal score_changed(score: int, perfect: int, good: int, miss: int)
signal result_ready(score: int, perfect: int, good: int, miss: int, rank: String, cleared: bool)

enum State { TUTORIAL_TEXT, TUTORIAL_PLAY, PLAYING, RESULT, PAUSE }

@export var perfect_score := 1000
@export var good_score := 500
@export_range(0.0, 1.0, 0.01) var clear_rate := 0.6

var state: State = State.PAUSE:
	set(value):
		if state == value:
			return
		state = value
		print("GameState : ", State.keys()[state])
		state_changed.emit(state)

var score := 0
var perfect_count := 0
var good_count := 0
var miss_count := 0
var max_score := 0
var clear_score: int:
	get: return ceili(max_score * clear_rate)

func start_tutorial() -> void: state = State.TUTORIAL_TEXT
func start_tutorial_play() -> void: state = State.TUTORIAL_PLAY

func start_game(note_count: int = 0) -> void:
	_reset_result(note_count)
	state = State.PLAYING

func add_perfect() -> void:
	perfect_count += 1
	score += perfect_score
	_emit_score_changed()

func add_good() -> void:
	good_count += 1
	score += good_score
	_emit_score_changed()

func add_miss() -> void:
	miss_count += 1
	_emit_score_changed()

func show_result() -> void:
	state = State.RESULT
	result_ready.emit(score, perfect_count, good_count, miss_count, _get_rank(), _is_cleared())

func pause_game() -> void: state = State.PAUSE

func _reset_result(note_count: int) -> void:
	score = 0
	perfect_count = 0
	good_count = 0
	miss_count = 0
	max_score = note_count * perfect_score
	_emit_score_changed()

func _emit_score_changed() -> void:
	score_changed.emit(score, perfect_count, good_count, miss_count)

func _is_cleared() -> bool:
	return max_score > 0 and score >= clear_score

func _get_rank() -> String:
	if max_score <= 0: return "D"
	var rate := float(score) / float(max_score)
	if rate >= 0.9: return "S"
	if rate >= 0.75: return "A"
	if rate >= 0.6: return "B"
	if rate >= 0.4: return "C"
	return "D"
