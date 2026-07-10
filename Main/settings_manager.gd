extends Node

const SETTINGS_PATH := "user://settings.cfg"
const BGM_BUS := "BGM"
const SE_BUS := "SE"

var bgm_volume := 1.0
var se_volume := 1.0
var bgm_muted := false
var se_muted := false
var input_offset_ms := 0

func _ready() -> void:
	load_settings()
	apply_audio_settings()

func set_bgm_volume(value: float) -> void:
	bgm_volume = clampf(value, 0.0, 1.0)
	apply_audio_settings()
	save_settings()

func set_se_volume(value: float) -> void:
	se_volume = clampf(value, 0.0, 1.0)
	apply_audio_settings()
	save_settings()

func set_bgm_muted(value: bool) -> void:
	bgm_muted = value
	apply_audio_settings()
	save_settings()

func set_se_muted(value: bool) -> void:
	se_muted = value
	apply_audio_settings()
	save_settings()

func set_input_offset_ms(value: int) -> void:
	input_offset_ms = clampi(value, -200, 200)
	save_settings()

func reset_input_offset() -> void:
	set_input_offset_ms(0)

func apply_audio_settings() -> void:
	_set_bus(BGM_BUS, bgm_volume, bgm_muted)
	_set_bus(SE_BUS, se_volume, se_muted)

func save_settings() -> void:
	var config := ConfigFile.new()
	config.set_value("audio", "bgm_volume", bgm_volume)
	config.set_value("audio", "se_volume", se_volume)
	config.set_value("audio", "bgm_muted", bgm_muted)
	config.set_value("audio", "se_muted", se_muted)
	config.set_value("timing", "input_offset_ms", input_offset_ms)
	var error := config.save(SETTINGS_PATH)
	if error != OK:
		push_warning("設定を保存できませんでした: %s" % error_string(error))

func load_settings() -> void:
	var config := ConfigFile.new()
	if config.load(SETTINGS_PATH) != OK:
		return
	bgm_volume = clampf(float(config.get_value("audio", "bgm_volume", 1.0)), 0.0, 1.0)
	se_volume = clampf(float(config.get_value("audio", "se_volume", 1.0)), 0.0, 1.0)
	bgm_muted = bool(config.get_value("audio", "bgm_muted", false))
	se_muted = bool(config.get_value("audio", "se_muted", false))
	input_offset_ms = clampi(int(config.get_value("timing", "input_offset_ms", 0)), -200, 200)

func _set_bus(bus_name: String, linear_volume: float, muted: bool) -> void:
	var bus_index := AudioServer.get_bus_index(bus_name)
	if bus_index < 0:
		push_warning("Audio bus not found: " + bus_name)
		return
	AudioServer.set_bus_volume_db(bus_index, linear_to_db(maxf(linear_volume, 0.0001)))
	AudioServer.set_bus_mute(bus_index, muted or linear_volume <= 0.0)
