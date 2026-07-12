class_name SettingsUiGd
extends Control

const TEST_SE := preload("res://imported/SE1_nc246084_【スマブラSP】_ジャストガード音_高音質.wav")

@onready var panel: Control = $Dimmer/Panel
@onready var dimmer: Control = $Dimmer
@onready var bgm_slider: HSlider = $Dimmer/Panel/Content/BgmRow/BgmSlider
@onready var bgm_value: Label = $Dimmer/Panel/Content/BgmRow/BgmValue
@onready var bgm_mute: CheckButton = $Dimmer/Panel/Content/BgmMute
@onready var se_slider: HSlider = $Dimmer/Panel/Content/SeRow/SeSlider
@onready var se_value: Label = $Dimmer/Panel/Content/SeRow/SeValue
@onready var se_mute: CheckButton = $Dimmer/Panel/Content/SeActions/SeMute
@onready var offset_value: Label = $Dimmer/Panel/Content/OffsetRow/OffsetValue
@onready var test_player: AudioStreamPlayer = $TestSePlayer
@onready var audio_manager: AudioManagerGd = get_node_or_null("../../GamePlay/AudioManager") as AudioManagerGd

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	dimmer.hide()
	$GearButton.pressed.connect(open_settings)
	bgm_slider.value_changed.connect(_on_bgm_changed)
	se_slider.value_changed.connect(_on_se_changed)
	bgm_mute.toggled.connect(SettingsManager.set_bgm_muted)
	se_mute.toggled.connect(SettingsManager.set_se_muted)
	$Dimmer/Panel/Content/OffsetRow/MinusButton.pressed.connect(_change_offset.bind(-5))
	$Dimmer/Panel/Content/OffsetRow/PlusButton.pressed.connect(_change_offset.bind(5))
	$Dimmer/Panel/Content/OffsetActions/ResetButton.pressed.connect(_reset_offset)
	$Dimmer/Panel/Content/SeActions/TestButton.pressed.connect(_play_test_se)
	$Dimmer/Panel/Content/CloseButton.pressed.connect(close_settings)
	test_player.stream = TEST_SE
	test_player.bus = SettingsManager.SE_BUS

func open_settings() -> void:
	_sync_controls()
	dimmer.show()
	$GearButton.hide()
	panel.scale = Vector2(0.88, 0.88)
	panel.modulate.a = 0.0
	if audio_manager != null: audio_manager.pause_timeline()
	get_tree().paused = true
	var tween := create_tween().set_parallel(true).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.tween_property(panel, "scale", Vector2.ONE, 0.25)
	tween.tween_property(panel, "modulate:a", 1.0, 0.18)

func close_settings() -> void:
	if not dimmer.visible:
		return
	dimmer.hide()
	$GearButton.show()
	if audio_manager != null: audio_manager.resume_timeline()
	get_tree().paused = false

func _sync_controls() -> void:
	bgm_slider.set_value_no_signal(SettingsManager.bgm_volume * 100.0)
	se_slider.set_value_no_signal(SettingsManager.se_volume * 100.0)
	bgm_mute.set_pressed_no_signal(SettingsManager.bgm_muted)
	se_mute.set_pressed_no_signal(SettingsManager.se_muted)
	_update_labels()

func _on_bgm_changed(value: float) -> void:
	SettingsManager.set_bgm_volume(value / 100.0)
	_update_labels()

func _on_se_changed(value: float) -> void:
	SettingsManager.set_se_volume(value / 100.0)
	_update_labels()

func _change_offset(amount: int) -> void:
	SettingsManager.set_input_offset_ms(SettingsManager.input_offset_ms + amount)
	_update_labels()

func _reset_offset() -> void:
	SettingsManager.reset_input_offset()
	_update_labels()

func _play_test_se() -> void:
	test_player.stop()
	test_player.play()

func _update_labels() -> void:
	bgm_value.text = "%d%%" % roundi(bgm_slider.value)
	se_value.text = "%d%%" % roundi(se_slider.value)
	offset_value.text = "%+d ms" % SettingsManager.input_offset_ms
