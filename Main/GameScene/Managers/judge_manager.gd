class_name JudgeManagerGd
extends Node

signal judged(type: int)
signal remove_note(id: int)
signal note_resolved(id: int, type: int)
signal karateman_action(judge_type: int, note_type: int, is_kick: bool, is_input: bool)

const P_RANGE := 70
const J_RANGE := 160

var manager: GameManagerGd
var chart_manager: ChartManagerGd
var current_time := 0
var kick_used_note_index := -1
var kick_locked_note_index := -1

func _ready() -> void:
	manager = get_node("../../GameManager")
	chart_manager = get_node("../ChartManager")
	manager.state_changed.connect(_on_state_changed)

func _process(_delta: float) -> void:
	if manager.state not in [GameManagerGd.State.TUTORIAL_PLAY, GameManagerGd.State.PLAYING]: return
	if chart_manager.chart.is_empty() or chart_manager.judge_note >= chart_manager.chart[0].size(): return
	current_time = chart_manager.get_time() + SettingsManager.input_offset_ms
	if Input.is_action_just_pressed("rhythm"):
		_try_judge_input()
		return
	if int(chart_manager.chart[0][chart_manager.judge_note]) + J_RANGE < current_time:
		_judge(2, false, false)

func _unhandled_input(event: InputEvent) -> void:
	if manager.state not in [GameManagerGd.State.TUTORIAL_PLAY, GameManagerGd.State.PLAYING]: return
	if chart_manager.chart.is_empty() or chart_manager.judge_note >= chart_manager.chart[0].size(): return
	var pressed := false
	if event is InputEventScreenTouch:
		pressed = event.pressed
	elif event is InputEventMouseButton:
		pressed = event.button_index == MOUSE_BUTTON_LEFT and event.pressed
	if not pressed: return
	current_time = chart_manager.get_time() + SettingsManager.input_offset_ms
	_try_judge_input()
	get_viewport().set_input_as_handled()

func _try_judge_input() -> void:
	if _is_input_locked(): return
	var difference := current_time - int(chart_manager.chart[0][chart_manager.judge_note])
	var absolute_difference := absi(difference)
	var is_kick := _is_kick_window()
	if absolute_difference <= P_RANGE:
		_judge(0, true, is_kick)
	elif absolute_difference <= J_RANGE:
		_judge(1, true, is_kick)
	elif difference > J_RANGE:
		_judge(2, true, false)
	else:
		judged.emit(3)
		_emit_karateman_action(3, is_kick, true)
		if is_kick: kick_locked_note_index = chart_manager.judge_note

func _is_input_locked() -> bool:
	if kick_locked_note_index != chart_manager.judge_note: return false
	if current_time <= int(chart_manager.chart[0][chart_manager.judge_note]) + J_RANGE: return true
	kick_locked_note_index = -1
	return false

func _on_state_changed(new_state: int) -> void:
	if new_state in [GameManagerGd.State.TUTORIAL_PLAY, GameManagerGd.State.PLAYING]:
		kick_used_note_index = -1
		kick_locked_note_index = -1

func _is_kick_window() -> bool:
	var note_index := chart_manager.judge_note
	if chart_manager.chart.is_empty() or note_index <= 0 or note_index >= chart_manager.chart[0].size(): return false
	if kick_used_note_index == note_index: return false
	if chart_manager.chart[1][note_index - 1] != 3 or chart_manager.chart[1][note_index] != 4: return false
	var start := int(chart_manager.chart[0][note_index - 1]) + J_RANGE
	var end := int(chart_manager.chart[0][note_index]) + J_RANGE
	return current_time >= start and current_time <= end

func _judge(judge_type: int, is_input: bool, is_kick: bool) -> void:
	print("Judge: note=%d, type=%d, frame=%d" % [chart_manager.judge_note, judge_type, Engine.get_process_frames()])
	_emit_karateman_action(judge_type, is_kick, is_input)
	match judge_type:
		0: manager.add_perfect()
		1: manager.add_good()
		2: manager.add_miss()
	judged.emit(judge_type)
	note_resolved.emit(chart_manager.judge_note, judge_type)
	remove_note.emit(chart_manager.judge_note)
	if kick_locked_note_index == chart_manager.judge_note: kick_locked_note_index = -1
	chart_manager.judge_note += 1

func _emit_karateman_action(judge_type: int, is_kick: bool, is_input: bool) -> void:
	var note_type := 0
	if not chart_manager.chart.is_empty() and chart_manager.judge_note < chart_manager.chart[1].size():
		note_type = chart_manager.chart[1][chart_manager.judge_note]
	if is_kick: kick_used_note_index = chart_manager.judge_note
	karateman_action.emit(judge_type, note_type, is_kick, is_input)
