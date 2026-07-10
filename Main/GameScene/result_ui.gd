class_name ResultUiGd
extends Control

var title_label: Label
var score_label: Label
var manager: GameManagerGd
var score_manager: ScoreManagerGd

func _ready() -> void:
	manager = get_node("../../GameManager")
	score_manager = get_node("../../GamePlay/ScoreManager")
	title_label = get_node("Panel/ResultTitleLabel")
	score_label = get_node("Panel/ResultScoreLabel")
	manager.state_changed.connect(_on_state_changed)

func _on_state_changed(new_state: int) -> void:
	if new_state == GameManagerGd.State.RESULT: _show_result()
	else: hide()

func _show_result() -> void:
	visible = true
	match score_manager.get_evaluation():
		0: title_label.text = "failed"; score_label.text = "がんばりましょう"
		1: title_label.text = "so so"; score_label.text = "まあまあできてた"
		2: title_label.text = "great"; score_label.text = "すごくできてた"
		3: title_label.text = "perfect"; score_label.text = "最高！"
