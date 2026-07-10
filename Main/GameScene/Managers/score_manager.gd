class_name ScoreManagerGd
extends Node

var manager: GameManagerGd
var chart_manager: ChartManagerGd
var judge: JudgeManagerGd
var perfect := 0
var bad := 0
var miss := 0

func _ready() -> void:
	manager = get_node("../../GameManager")
	judge = get_node("../JudgeManager")
	chart_manager = get_node("../ChartManager")
	judge.judged.connect(_on_judged)
	manager.state_changed.connect(_on_state_changed)

func _on_judged(type: int) -> void:
	if type == 0: perfect += 1
	elif type == 1: bad += 1
	elif type == 2: miss += 1

func _on_state_changed(new_state: int) -> void:
	if new_state == GameManagerGd.State.RESULT:
		print("perfect:%d bad:%d miss:%d" % [perfect, bad, miss])

func get_evaluation() -> int:
	var full_score: int = chart_manager.chart[0].size() * 8
	var current_score: int = perfect * 8 + bad * 4
	if full_score == current_score: return 3
	if (full_score / 8) * 7 <= current_score: return 2
	if full_score / 2 <= current_score: return 1
	return 0
