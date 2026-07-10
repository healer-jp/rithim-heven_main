class_name GameSceneGd
extends Node2D

signal get_loading

const BPM := 120
const PERFECT_WINDOW_MS := 80
const GOOD_WINDOW_MS := 180
const SONG_END_DELAY_MS := 900
const THROW_LEAD_MS := 800
const RESULT_RETRY_LOCK_MS := 700

var game_manager: GameManagerGd
var audio_manager: AudioManagerGd
var chart_manager: ChartManagerGd
var judge_manager: JudgeManagerGd
var world_character: Node2D
var visual_layer: Node2D
var screen_character: Sprite2D
var play_ui: Control
var result_ui: Control
var score_label: Label
var score_panel: Control
var prompt_label: Label
var judge_label: Label
var guide_panel: Control
var result_title_label: Label
var result_score_label: Label
var result_breakdown_label: Label
var result_rank_label: Label
var result_guide_label: Label
var clear_score_label: Label
var master_comment_label: RichTextLabel
var character_texture: Texture2D
var throw_texture: Texture2D
var active_throw: Sprite2D
var active_throw_note_index := -1
var chart: Array = []
var current_note_index := 0
var result_shown_at := 0
var is_playing := false
var score_tween: Tween
var judge_tween: Tween
var guide_tween: Tween

func _ready() -> void:
	print("GameScene Ready")
	game_manager = get_node("GameManager")
	audio_manager = get_node_or_null("GamePlay/AudioManager")
	chart_manager = get_node_or_null("GamePlay/ChartManager")
	judge_manager = get_node_or_null("GamePlay/JudgeManager")
	world_character = get_node_or_null("Karateman")
	game_manager.state_changed.connect(_on_state_changed)
	game_manager.score_changed.connect(_on_score_changed)
	game_manager.result_ready.connect(_on_result_ready)
	if judge_manager != null: judge_manager.judged.connect(_on_judged)
	character_texture = _load_texture_from_file("res://assets/character.png")
	throw_texture = _load_texture_from_file("res://assets/bulb.png")
	_bind_ui()
	_ensure_screen_visuals()
	_apply_responsive_layout()
	get_loading.emit()
	_show_play_ui(false)
	_show_result_ui(false)
	game_manager.start_tutorial()

func _process(_delta: float) -> void:
	_apply_responsive_layout()
	if not is_playing or chart.is_empty() or chart[0].is_empty(): return
	var elapsed := _get_song_elapsed_ms()
	_update_prompt(elapsed)
	var last_note_time: int = chart[0][-1]
	var resolved_note_index := current_note_index if chart_manager == null else chart_manager.judge_note
	if resolved_note_index >= chart[0].size() and chart_manager.get_time() >= last_note_time + SONG_END_DELAY_MS:
		_finish_song()

func _unhandled_input(event: InputEvent) -> void:
	if game_manager.state == GameManagerGd.State.RESULT and _is_rhythm_input(event) and Time.get_ticks_msec() - result_shown_at >= RESULT_RETRY_LOCK_MS:
		_start_song(true)
		get_viewport().set_input_as_handled()

func _bind_ui() -> void:
	play_ui = get_node_or_null("CanvasLayer/PlayUI")
	result_ui = get_node_or_null("CanvasLayer/ResultUI")
	score_panel = get_node_or_null("CanvasLayer/PlayUI/ScorePanel")
	score_label = get_node_or_null("CanvasLayer/PlayUI/ScorePanel/Margin/Row/ScoreLabel")
	prompt_label = get_node_or_null("CanvasLayer/PlayUI/PromptLabel")
	judge_label = get_node_or_null("CanvasLayer/PlayUI/JudgeLabel")
	guide_panel = get_node_or_null("CanvasLayer/PlayUI/GuidePanel")
	result_title_label = get_node_or_null("CanvasLayer/ResultUI/Panel/Content/ResultTitleLabel")
	result_score_label = get_node_or_null("CanvasLayer/ResultUI/Panel/Content/ResultScoreLabel")
	result_breakdown_label = get_node_or_null("CanvasLayer/ResultUI/Panel/Content/ResultBreakdownLabel")
	result_rank_label = get_node_or_null("CanvasLayer/ResultUI/Panel/Content/ResultRankLabel")
	clear_score_label = get_node_or_null("CanvasLayer/ResultUI/Panel/Content/ClearScoreLabel")
	master_comment_label = get_node_or_null("CanvasLayer/ResultUI/Panel/Content/MasterCommentLabel")
	result_guide_label = get_node_or_null("CanvasLayer/ResultUI/Panel/Content/ResultGuidePanel/ResultGuideLabel")

func _ensure_screen_visuals() -> void:
	if world_character != null: world_character.visible = true

func _apply_responsive_layout() -> void:
	var size := get_viewport_rect().size
	if world_character != null: world_character.position = Vector2(size.x * 0.5, size.y * 0.63)
	if screen_character != null: screen_character.position = Vector2(size.x * 0.5, size.y * 0.63)

func _get_throw_spawn_position() -> Vector2:
	var size := get_viewport_rect().size
	return Vector2(size.x * 0.9, size.y * 0.3)

func _get_throw_target_position() -> Vector2:
	if screen_character != null: return screen_character.position + Vector2(0, -90)
	var size := get_viewport_rect().size
	return Vector2(size.x * 0.5, size.y * 0.55)

func _get_throw_arc_offset() -> Vector2:
	return Vector2(0, -get_viewport_rect().size.y * 0.12)

func _load_chart() -> void:
	chart = ScoreData.get_score(BPM, ScoreData.SCORE_1)
	current_note_index = 0

func _on_state_changed(new_state: int) -> void:
	if new_state == GameManagerGd.State.TUTORIAL_PLAY: _start_song(false)
	elif new_state == GameManagerGd.State.PLAYING and not is_playing: _start_song(true)

func _start_song(enter_playing: bool) -> void:
	var selected_bpm := 105 if enter_playing else 100
	var selected_score := ScoreData.SCORE_2 if enter_playing else ScoreData.SCORE_1
	if chart_manager != null:
		chart_manager.prepare_chart(selected_bpm, selected_score)
		chart = chart_manager.chart
	else: _load_chart()
	_remove_throw_visual()
	_apply_responsive_layout()
	if audio_manager != null: audio_manager.play_song()
	current_note_index = 0
	is_playing = true
	if enter_playing: game_manager.start_game(chart[0].size())
	_show_play_ui(true)
	_show_result_ui(false)
	_set_judge_text("START")
	_update_prompt(0)

func _judge_input() -> void:
	if current_note_index >= chart[0].size(): return
	var elapsed := _get_song_elapsed_ms()
	var difference: int = elapsed - chart[0][current_note_index]
	var absolute_difference := absi(difference)
	if absolute_difference <= PERFECT_WINDOW_MS:
		game_manager.add_perfect(); _set_judge_text("PERFECT"); current_note_index += 1; _remove_throw_visual()
	elif absolute_difference <= GOOD_WINDOW_MS:
		game_manager.add_good(); _set_judge_text("GOOD"); current_note_index += 1; _remove_throw_visual()
	elif difference > GOOD_WINDOW_MS:
		game_manager.add_miss(); _set_judge_text("MISS"); current_note_index += 1; _remove_throw_visual()
	else: _set_judge_text("EARLY")

func _resolve_expired_notes(elapsed: int) -> void:
	while current_note_index < chart[0].size() and elapsed - chart[0][current_note_index] > GOOD_WINDOW_MS:
		game_manager.add_miss(); _set_judge_text("MISS"); current_note_index += 1; _remove_throw_visual()

func _update_throw_visual(elapsed: int) -> void:
	if current_note_index >= chart[0].size() or throw_texture == null:
		_remove_throw_visual(); return
	var note_time: int = chart[0][current_note_index]
	var until_note := note_time - elapsed
	if until_note > THROW_LEAD_MS or until_note < -GOOD_WINDOW_MS:
		_remove_throw_visual(); return
	if active_throw == null or active_throw_note_index != current_note_index: _create_throw_visual(current_note_index)
	var spawn := _get_throw_spawn_position()
	var target := _get_throw_target_position()
	var weight := clampf(float(THROW_LEAD_MS - until_note) / float(THROW_LEAD_MS), 0.0, 1.0)
	active_throw.position = _quadratic_bezier(spawn, (spawn + target) * 0.5 + _get_throw_arc_offset(), target, weight)
	active_throw.rotation = weight * TAU

func _create_throw_visual(note_index: int) -> void:
	_remove_throw_visual()
	active_throw = Sprite2D.new()
	active_throw.texture = throw_texture
	active_throw.scale = Vector2(0.14, 0.14)
	active_throw.z_index = 30
	active_throw_note_index = note_index
	(visual_layer if visual_layer != null else self).add_child(active_throw)

func _remove_throw_visual() -> void:
	if active_throw != null:
		active_throw.queue_free()
		active_throw = null
	active_throw_note_index = -1

func _update_prompt(_elapsed: int) -> void:
	if prompt_label == null: return
	var next_note_index := current_note_index if chart_manager == null else chart_manager.judge_note
	if next_note_index >= chart[0].size():
		prompt_label.text = "Finish!"
	else: prompt_label.text = ""

func _finish_song() -> void:
	print("finish")
	var finished_state := game_manager.state
	is_playing = false
	if audio_manager != null: audio_manager.stop_song()
	_remove_throw_visual()
	_show_play_ui(false)
	if finished_state == GameManagerGd.State.TUTORIAL_PLAY: game_manager.start_tutorial()
	else: game_manager.show_result()

func _on_score_changed(new_score: int, _perfect: int, _good: int, _miss: int) -> void:
	if score_label != null: score_label.text = "%06d" % new_score
	if score_panel != null:
		if score_tween != null: score_tween.kill()
		score_panel.pivot_offset = score_panel.size * 0.5
		score_panel.scale = Vector2(1.08, 1.08)
		score_tween = create_tween().set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		score_tween.tween_property(score_panel, "scale", Vector2.ONE, 0.22)

func _on_result_ready(new_score: int, perfect: int, good: int, miss: int, rank: String, cleared: bool) -> void:
	result_shown_at = Time.get_ticks_msec()
	_show_result_ui(true)
	if result_title_label != null:
		result_title_label.text = "CLEAR!" if cleared else "FAILED"
		result_title_label.modulate = Color("ffd34e") if cleared else Color("ff5349")
	if result_score_label != null: result_score_label.text = "SCORE                 %06d" % new_score
	if result_breakdown_label != null: result_breakdown_label.text = "PERFECT   %d\nGOOD      %d\nMISS      %d" % [perfect, good, miss]
	if result_rank_label != null:
		result_rank_label.text = rank
		result_rank_label.modulate = {"S": Color("ffd34e"), "A": Color("ff6b4a"), "B": Color("5ec8ff"), "C": Color("70d68b")}.get(rank, Color("b7bdc8"))
	if clear_score_label != null: clear_score_label.text = "CLEAR SCORE          %06d" % game_manager.clear_score
	if master_comment_label != null:
		master_comment_label.text = {
			"D": "キホンができてない。やりなおし。",
			"C": "う〜ん･･･ まぁまぁ、かな。",
			"B": "キホンはできてるけど、つづけざまは ニガテみたい。",
			"A": "オウヨウできてる!しかも連続パンチがキマッてた。",
			"S": "カンペキだ！動きに少しもムダがない!!",
		}.get(rank, "")
	if result_guide_label != null: result_guide_label.text = "SPACE / TAP   RETRY"

func _on_judged(type: int) -> void:
	_set_judge_text({0: "PERFECT", 1: "GOOD", 2: "MISS", 3: "EARLY"}.get(type, ""))
	if guide_panel != null:
		if guide_tween != null: guide_tween.kill()
		guide_panel.scale = Vector2(0.92, 0.92)
		guide_tween = create_tween().set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		guide_tween.tween_property(guide_panel, "scale", Vector2.ONE, 0.18)

func _show_play_ui(value: bool) -> void:
	if play_ui != null: play_ui.visible = value

func _show_result_ui(value: bool) -> void:
	if result_ui != null: result_ui.visible = value

func _set_judge_text(text: String) -> void:
	if judge_label == null: return
	judge_label.text = text
	var colors := {"PERFECT": Color("ffd34e"), "GOOD": Color("63d8ff"), "MISS": Color("ff5349"), "EARLY": Color("d892ff"), "START": Color("fff2b2")}
	judge_label.modulate = colors.get(text, Color.WHITE)
	if judge_tween != null: judge_tween.kill()
	judge_label.scale = Vector2(1.35, 1.35)
	judge_label.position.y = 175.0
	judge_label.modulate.a = 1.0
	judge_tween = create_tween().set_parallel(true).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	judge_tween.tween_property(judge_label, "scale", Vector2.ONE, 0.22)
	judge_tween.tween_property(judge_label, "position:y", 158.0, 0.5)
	if text not in ["START", "READY"]: judge_tween.tween_property(judge_label, "modulate:a", 0.0, 0.55).set_delay(0.18)

func _get_song_elapsed_ms() -> int:
	return 0 if chart_manager == null else chart_manager.get_time()

static func _load_texture_from_file(path: String) -> Texture2D:
	var image := Image.load_from_file(ProjectSettings.globalize_path(path))
	return null if image == null or image.is_empty() else ImageTexture.create_from_image(image)

static func _quadratic_bezier(p0: Vector2, p1: Vector2, p2: Vector2, weight: float) -> Vector2:
	var inverse := 1.0 - weight
	return inverse * inverse * p0 + 2.0 * inverse * weight * p1 + weight * weight * p2

static func _is_rhythm_input(event: InputEvent) -> bool:
	if event.is_action_pressed("rhythm"): return true
	if event is InputEventMouseButton: return event.button_index == MOUSE_BUTTON_LEFT and event.pressed
	return event is InputEventScreenTouch and event.pressed
