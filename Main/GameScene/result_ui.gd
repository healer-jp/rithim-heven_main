class_name ResultUiGd
extends Control

var manager: GameManagerGd

func _ready() -> void:
	manager = get_node("../../GameManager")
	manager.state_changed.connect(_on_state_changed)

func _on_state_changed(new_state: int) -> void:
	if new_state == GameManagerGd.State.RESULT: _show_result()
	else: hide()

func _show_result() -> void:
	visible = true
	call_deferred("_animate_result")

func _animate_result() -> void:
	var panel := get_node("Panel") as Control
	var content := get_node("Panel/Content") as Control
	panel.scale = Vector2(0.82, 0.82)
	panel.modulate.a = 0.0
	for child in content.get_children():
		if child is CanvasItem: child.modulate.a = 0.0
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.tween_property(panel, "scale", Vector2.ONE, 0.38)
	tween.parallel().tween_property(panel, "modulate:a", 1.0, 0.22)
	tween.set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	for child in content.get_children():
		if child is CanvasItem: tween.tween_property(child, "modulate:a", 1.0, 0.12)
	var guide := get_node("Panel/Content/ResultGuidePanel") as Control
	tween.tween_callback(_start_guide_pulse.bind(guide))

func _start_guide_pulse(guide: Control) -> void:
	var pulse := create_tween().set_loops().set_trans(Tween.TRANS_SINE)
	pulse.tween_property(guide, "modulate:a", 0.55, 0.65)
	pulse.tween_property(guide, "modulate:a", 1.0, 0.65)
