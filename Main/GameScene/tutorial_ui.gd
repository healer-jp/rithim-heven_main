class_name TutorialUiGd
extends Control

@export var tutorial_texts: Array[String] = ["カラテ家です。", "とりあえずやってみよう！"]
var tutorial_texts_2: Array[String] = ["なんとなくわかったかな？", "じゃあ本番いってみよう！"]
var text_label: RichTextLabel
var arrow: Label
var timer: Timer
var game_manager: GameManagerGd
var tutorial_num := 0
var text_index := 0
var current_text := ""
var typing := false
var finished := false

func _ready() -> void:
	text_label = get_node("Panel/Content/RichTextLabel")
	arrow = get_node("Panel/Content/Label")
	timer = get_node("Timer")
	game_manager = get_node("../../GameManager")
	timer.timeout.connect(_on_timer_timeout)
	game_manager.state_changed.connect(_on_state_changed)
	arrow.hide()
	hide()
	_on_state_changed(game_manager.state)

func _on_state_changed(new_state: int) -> void:
	if new_state == GameManagerGd.State.TUTORIAL_TEXT:
		if tutorial_num == 0:
			show(); _animate_panel(); text_index = 0; _show_text(tutorial_texts[text_index]); tutorial_num = 1
		elif tutorial_num == 1:
			show(); _animate_panel(); text_index = 0; _show_text(tutorial_texts_2[text_index]); tutorial_num = 2
	else: hide()

func _show_text(text: String) -> void:
	current_text = text
	text_label.text = text
	text_label.visible_characters = 0
	typing = true
	finished = false
	arrow.hide()
	timer.start()

func _on_timer_timeout() -> void:
	if text_label.visible_characters < current_text.length():
		text_label.visible_characters += 1
		return
	timer.stop(); typing = false; finished = true; arrow.show()

func _unhandled_input(event: InputEvent) -> void:
	if not visible or not _is_advance_input(event): return
	if typing:
		timer.stop(); text_label.visible_characters = current_text.length(); typing = false; finished = true; arrow.show()
	elif finished: _next_text()
	get_viewport().set_input_as_handled()

func _next_text() -> void:
	text_index += 1
	var texts := tutorial_texts if tutorial_num == 1 else tutorial_texts_2
	if text_index >= texts.size():
		hide()
		if tutorial_num == 1: game_manager.start_tutorial_play()
		elif tutorial_num == 2:
			game_manager.start_game()
			tutorial_num = 0
		return
	_show_text(texts[text_index])

func _process(_delta: float) -> void:
	if finished: arrow.visible = int(Time.get_ticks_msec() / 300) % 2 == 0

func _animate_panel() -> void:
	var panel := get_node("Panel") as Control
	panel.modulate.a = 0.0
	panel.position.y += 28.0
	var tween := create_tween().set_parallel(true).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	tween.tween_property(panel, "modulate:a", 1.0, 0.25)
	tween.tween_property(panel, "position:y", panel.position.y - 28.0, 0.32)

static func _is_advance_input(event: InputEvent) -> bool:
	if event.is_action_pressed("rhythm") or event.is_action_pressed("ui_accept"): return true
	if event is InputEventMouseButton: return event.button_index == MOUSE_BUTTON_LEFT and event.pressed
	return event is InputEventScreenTouch and event.pressed
