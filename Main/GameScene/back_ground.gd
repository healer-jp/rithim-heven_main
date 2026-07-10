class_name BackGroundGd
extends AnimatedSprite2D

func _ready() -> void:
	var texture := load("res://assets/background.png") as Texture2D
	if texture == null: return
	var frames := SpriteFrames.new()
	frames.add_animation("BackGround")
	frames.add_frame("BackGround", texture)
	frames.set_animation_loop("BackGround", true)
	sprite_frames = frames
	animation = "BackGround"
	play("BackGround")
