class_name KaratemanGd
extends Node2D

const PUNCH_DURATION := 0.2
const HURT_DURATION := 0.2
var anim: AnimatedSprite2D
var judge: JudgeManagerGd
var action_timer := 0.0

func _ready() -> void:
	anim = get_node("AnimatedSprite2D")
	judge = get_node_or_null("../GamePlay/JudgeManager")
	if judge != null: judge.karateman_action.connect(_on_karateman_action)
	_idling()

func _process(delta: float) -> void:
	if action_timer <= 0.0: return
	action_timer -= delta
	if action_timer <= 0.0: _idling()

func _on_karateman_action(judge_type: int, _note_type: int, is_kick: bool, is_input: bool) -> void:
	if is_kick: _kick()
	elif judge_type == 2 and not is_input: _hurt()
	else: _punch()

func _punch() -> void:
	if anim == null: return
	action_timer = PUNCH_DURATION
	anim.play("punch")

func _kick() -> void:
	if anim == null: return
	action_timer = PUNCH_DURATION
	anim.play("kick")

func _hurt() -> void:
	if anim == null: return
	action_timer = HURT_DURATION
	anim.play("hurt")

func _idling() -> void:
	if anim != null: anim.play("idoling")
	action_timer = 0.0
