class_name NotesGd
extends AnimatedSprite2D

enum Motion { FLYING, BOUNCING, FALLING, DONE }
const ARC_HEIGHT := 200.0
const PERFECT_BOUNCE_DISTANCE := 420.0
const GOOD_BOUNCE_DISTANCE := 300.0
const PERFECT_BOUNCE_HEIGHT := 240.0
const GOOD_BOUNCE_HEIGHT := 160.0
const BOUNCE_DURATION := 0.35
const FALL_DURATION := 0.42

var judge: JudgeManagerGd
var chart: ChartManagerGd
var karateman: Node2D
var motion := Motion.FLYING
var spawn_position: Vector2
var hit_position: Vector2
var ground_position: Vector2
var motion_start: Vector2
var motion_control: Vector2
var motion_end: Vector2
var motion_elapsed := 0.0
var motion_duration := 1.0
var resolved := false
var note_type := 0
var note_id := 0

func _ready() -> void:
	judge = get_node_or_null("../JudgeManager") as JudgeManagerGd
	if judge == null: judge = get_node_or_null("../../JudgeManager") as JudgeManagerGd
	chart = get_node_or_null("../ChartManager") as ChartManagerGd
	if chart == null: chart = get_node_or_null("../../ChartManager") as ChartManagerGd
	var current_scene := get_tree().current_scene
	karateman = null if current_scene == null else current_scene.get_node_or_null("Karateman") as Node2D
	if judge != null: judge.note_resolved.connect(_on_note_resolved)
	sprite_frames = _build_sprite_frames(note_type)
	animation = _get_base_animation_name(note_type)
	play(animation)
	scale = Vector2(0.15, 0.15)
	z_index = 30
	_setup_positions()
	global_position = spawn_position

func _process(delta: float) -> void:
	rotation += delta * 5.0
	if motion == Motion.FLYING: _update_flying_position()
	elif motion in [Motion.BOUNCING, Motion.FALLING]: _update_resolved_motion(delta)

func _on_note_resolved(id: int, type: int) -> void:
	if id != note_id or resolved: return
	resolved = true
	match type:
		0:
			_play_break_animation()
			_start_bounce(PERFECT_BOUNCE_DISTANCE, PERFECT_BOUNCE_HEIGHT)
		1: _start_bounce(GOOD_BOUNCE_DISTANCE, GOOD_BOUNCE_HEIGHT)
		2: _start_fall_after_body_hit()

func _setup_positions() -> void:
	var viewport_size := get_viewport_rect().size
	hit_position = karateman.global_position + Vector2(80, -60) if karateman != null else Vector2(viewport_size.x * 0.8, viewport_size.y * 0.4)
	if chart.chart[1][note_id] == 4:
		spawn_position = hit_position + Vector2(50, -20)
	else: spawn_position = Vector2(viewport_size.x, viewport_size.y / 2.0)
	ground_position = hit_position + Vector2(20, viewport_size.y * 0.22)

func _update_flying_position() -> void:
	if chart == null or chart.chart.is_empty() or chart.chart[0].size() <= note_id: return
	var current_bpm := chart.bpm if chart.bpm > 0 else 120
	var duration := 60000.0 / float(current_bpm)
	var hit_time: float = chart.chart[0][note_id]
	var now := float(chart.get_time())
	var weight := clampf((now - (hit_time - duration)) / duration, 0.0, 1.0)
	global_position = _quadratic_bezier(spawn_position, _get_arc_control(spawn_position, hit_position, ARC_HEIGHT), hit_position, weight)

func _start_bounce(distance: float, height: float) -> void:
	motion = Motion.BOUNCING
	motion_start = global_position
	motion_end = global_position + Vector2(-distance, 80)
	motion_control = _get_arc_control(motion_start, motion_end, height)
	motion_duration = BOUNCE_DURATION
	motion_elapsed = 0.0

func _start_fall_after_body_hit() -> void:
	global_position = hit_position
	motion = Motion.FALLING
	motion_start = hit_position
	motion_end = ground_position
	motion_control = hit_position + Vector2(60, 20)
	motion_duration = FALL_DURATION
	motion_elapsed = 0.0
	rotation = 0.0

func _update_resolved_motion(delta: float) -> void:
	motion_elapsed += delta
	var weight := clampf(motion_elapsed / motion_duration, 0.0, 1.0)
	global_position = _quadratic_bezier(motion_start, motion_control, motion_end, weight)
	if motion == Motion.FALLING: rotation = lerpf(0.0, PI * 0.5, weight)
	if weight >= 1.0:
		motion = Motion.DONE
		queue_free()

func _play_break_animation() -> void:
	if sprite_frames != null and sprite_frames.has_animation("break"): play("break")

static func _build_sprite_frames(type: int) -> SpriteFrames:
	var frames := SpriteFrames.new()
	var base_animation := _get_base_animation_name(type)
	frames.add_animation(base_animation)
	frames.set_animation_loop(base_animation, true)
	frames.set_animation_speed(base_animation, 5.0)
	_add_texture_frame(frames, base_animation, _get_base_texture_path(type))
	frames.add_animation("break")
	frames.set_animation_loop("break", false)
	frames.set_animation_speed("break", 10.0)
	_add_texture_frame(frames, "break", _get_broken_texture_path(type))
	_add_texture_frame(frames, "break", "res://imported/爆発エフェクト(透過後).png")
	_add_texture_frame(frames, "break", "res://imported/星(透過後).png")
	return frames

static func _add_texture_frame(frames: SpriteFrames, animation_name: String, path: String) -> void:
	var texture := _load_texture_from_file(path)
	if texture != null: frames.add_frame(animation_name, texture)

static func _get_base_animation_name(type: int) -> String:
	return {2: "bulb", 3: "barrel", 4: "bomb"}.get(type, "normal")

static func _get_base_texture_path(type: int) -> String:
	return {2: "res://imported/電球(透過後).png", 3: "res://imported/樽(透過後).png", 4: "res://imported/爆弾(透過後).png"}.get(type, "res://imported/植木鉢(透過後).png")

static func _get_broken_texture_path(type: int) -> String:
	return {2: "res://imported/破損した電球(透過後).png", 3: "res://imported/樽の破片(透過後).png", 4: "res://imported/爆発エフェクト(透過後).png"}.get(type, "res://imported/星(透過後).png")

static func _get_arc_control(start: Vector2, end: Vector2, height: float) -> Vector2:
	return (start + end) * 0.5 + Vector2.UP * height

static func _quadratic_bezier(p0: Vector2, p1: Vector2, p2: Vector2, weight: float) -> Vector2:
	var inverse := 1.0 - weight
	return inverse * inverse * p0 + 2.0 * inverse * weight * p1 + weight * weight * p2

static func _load_texture_from_file(path: String) -> Texture2D:
	return load(path) as Texture2D
