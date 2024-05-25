class_name View
extends CanvasLayer

signal bid_made(bid)

var player_count = 4
var center: Vector2

var bid_panel: Panel
var bid_button: Button

var discard_panel: Panel
var done_button: Button
var discard_outlines: Array[Sprite2D]

var trump_suit: Sprite2D
var play_outline: Sprite2D
var j = 0

# Called when the node enters the scene tree for the first time.
func _ready():
	# Get view size
	self.center = Vector2(500.0, 500.0)
	self.bid_panel = get_node("GUI/BidPanel") as Panel
	self.bid_panel.visible = false
	
	self.discard_panel = get_node("GUI/DiscardPanel") as Panel
	self.done_button = get_node("GUI/DiscardPanel/DoneButton") as Button
	
	var discard_outline1 = get_node("GUI/DiscardOutline1") as Sprite2D
	discard_outline1.position = self.nest_exchange_position(0, 2)
	self.discard_outlines.push_back(discard_outline1)
	var discard_outline2 = get_node("GUI/DiscardOutline2") as Sprite2D
	discard_outline2.position = self.nest_exchange_position(1, 2)
	self.discard_outlines.push_back(discard_outline2)
	for outline in self.discard_outlines:
		outline.visible = false
		
	self.trump_suit = get_node("GUI/TrumpSuit") as Sprite2D
	self.trump_suit.visible = false
	
	self.play_outline = get_node("GUI/PlayOutline") as Sprite2D
	self.play_outline.visible = false

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	pass
	
func find_card_node(id: int) -> CardNode:
	return find_child(str(id), true, false) as CardNode
	
func player_radians_from_center(player: int) -> float:
	return player as float * PI * 2.0 / self.player_count as float + PI / 2.0
		
func player_rotation(player: int) -> float:
	return player as float * PI * 2.0 / self.player_count as float
		
func position_from(start_pos: Vector2, radians: float, magnitude: float) -> Vector2:
	return Vector2(start_pos.x + cos(radians) * magnitude, start_pos.y + sin(radians) * magnitude)
	
func active_player_position(player: int) -> Vector2:
	var distance_from_center = 470.0
	var rad = self.player_radians_from_center(player)
	var pos = self.position_from(self.center, rad, distance_from_center)
	return pos
	
func deck_position(card_idx: int) -> Vector2:
	var pos = Vector2(100.0, 120.0)
	pos.x += card_idx * 2
	return pos
	
func nest_exchange_position(card_idx: int, card_count: int) -> Vector2:
	var x_spacing = 130.0;
	var pos = self.center
	pos.x -= (card_count - 1) as float * x_spacing / 2.0;
	pos.x += card_idx as float * x_spacing
	return pos
	
func nest_aside_position(card_idx: int, card_count: int) -> Vector2:
	var x_spacing = 24.0;
	var pos = Vector2(100.0, 110.0)
	pos.x -= (card_count - 1) as float * x_spacing / 2.0;
	pos.x += card_idx as float * x_spacing
	return pos
	
func hand_card_position(player: int, is_bot: bool, card_idx: int, card_count: int) -> Vector2:
	var distance_from_center = 340.0
	
	var max_width = 900.0
	if is_bot:
		max_width = 300.0

	var max_spacing = 85.0

	var computed_width = max_width / card_count;
	var x_spacing = min(max_spacing, computed_width)

	var x_offset = (card_count - 1) * -x_spacing / 2.0;
	x_offset += card_idx * x_spacing;

	var radians = self.player_radians_from_center(player)
	var pos = self.position_from(self.center, radians, distance_from_center)

	var angle = self.player_rotation(player)
	pos.x += x_offset * cos(angle)
	pos.y += x_offset * sin(angle)
	return pos
	
func play_position(player: int) -> Vector2:
	var distance_from_center = 150.0
	var rad = self.player_radians_from_center(player)
	var pos = self.position_from(self.center, rad, distance_from_center)
	return pos
	
func trick_awarded_position(player: int) -> Vector2:
	var distance_from_center = 600.0
	var rad = self.player_radians_from_center(player)
	var pos = self.position_from(self.center, rad, distance_from_center)
	return pos
		
func tween_card_position(card_node: Node, new_pos: Vector2, duration: float):
	var tween = get_tree().create_tween() as Tween
	tween.tween_property(card_node, "position", new_pos, duration).from_current()

func tween_card_rotation(card_node: Node, new_rot: float, duration: float):
	var tween = get_tree().create_tween() as Tween
	tween.tween_property(card_node, "rotation", new_rot, duration).from_current()
	
func create_card_node(card: Card) -> CardNode:
	var resource = preload("res://card_node.tscn")
	var card_node = resource.instantiate() as CardNode
	card_node.setup(card)
	card_node.position = self.deck_position(card.id)
	card_node.z_index = card.id
	card_node.set_face_up(card.face_up)
	$Cards.add_child(card_node)
	return card_node
	
func _on_active_player_updated(player: int, game_state: Game.State):
	$GUI/ActivePlayer.visible = true
	$GUI/ActivePlayer.position = self.active_player_position(player)
	if game_state == Game.State.WAITING_FOR_PLAY:
		self.update_play_outline(true, player)
	
func _on_hand_updated(player: int, cards: Array[Card], is_bot: bool):
	for i in cards.size():
		var card = cards[i] as Card
		var card_node = self.find_card_node(card.id)
		card_node.z_index = i
		card_node.set_face_up(!is_bot)
		
		var new_pos = self.hand_card_position(player, is_bot, i, cards.size())
		self.tween_card_position(card_node, new_pos, 0.5)
		self.tween_card_rotation(card_node, self.player_rotation(player), 0.2)
		
func _on_nest_exchange_updated(cards: Array[Card]):
	for i in cards.size():
		var card = cards[i]
		var card_node = self.find_card_node(card.id)
		card_node.z_index = i
	
		# Hard-coding nest size (2) here so that both discards go to the proper
		# outline position.
		var new_pos = self.nest_exchange_position(i, 2)
		self.tween_card_position(card_node, new_pos, 0.5)
		self.tween_card_rotation(card_node, 0.0, 0.2)
		
		self.done_button.disabled = cards.size() != 2

func _on_nest_aside_updated(cards: Array[Card]):
	for i in cards.size():
		var card = cards[i]
		var card_node = self.find_card_node(card.id) as CardNode
		card_node.z_index = i
		card_node.set_face_up(card.face_up)
	
		var new_pos = self.nest_aside_position(i, cards.size())
		self.tween_card_position(card_node, new_pos, 0.5)
		self.tween_card_rotation(card_node, 0.0, 0.2)
		
func get_bid():
	self.bid_panel.visible = true
	#self.bid_button.disabled = false
	
# The suit buttons and the pass button pass in an int for the suit.
func _on_bid_button_pressed(bid: Card.Suit) -> void:
	print(bid)
	self.bid_panel.visible = false
	emit_signal("bid_made", bid)
	
func _on_trump_suit_updated(suit: Card.Suit) -> void:
	match suit:
		-1:
			self.trump_suit.visible = false
		_:
			self.trump_suit.visible = true
			self.trump_suit.set_suit(suit)
	
func get_discards(eligible_cards):
	self.discard_panel.visible = true
	self.done_button.disabled = true
	
	for outline in self.discard_outlines:
		outline.visible = true
	for i in eligible_cards.size():
		var card = eligible_cards[i]
		var card_node = self.find_card_node(card.id)
		card_node.set_eligibility(card.eligible)
	
func discards_done():
	self.discard_panel.visible = false
	for outline in self.discard_outlines:
		outline.visible = false
		
func get_play(player: int, eligible_cards: Array[Card]) -> void:
	self.update_play_outline(true, player)
	for i in eligible_cards.size():
		var card = eligible_cards[i]
		var card_node = self.find_card_node(card.id)
		card_node.set_eligibility(card.eligible)
		
func update_play_outline(_visible: bool, player: int):
	self.play_outline.visible = _visible
	if visible:
		self.play_outline.position = self.play_position(player)
		self.play_outline.rotation = self.player_rotation(player)
		
func update_trick(cards: Array[Card]) -> void:
	for player in cards.size():
		if cards[player] != null:
			var card = cards[player] as Card
			var card_node = self.find_card_node(card.id)
			# TO-DO: Have z_index depend on order of card play.
			card_node.z_index = 100
			card_node.set_face_up(true)
			
			var new_pos = self.play_position(player)
			self.tween_card_position(card_node, new_pos, 0.5)
			self.tween_card_rotation(card_node, self.player_rotation(player), 0.2)

func update_awarded_trick(player: int, cards: Array[Card]) -> void:
	for card in cards:
		var card_node = self.find_card_node(card.id)
		card_node.set_face_up(false)
		
		var new_pos = self.trick_awarded_position(player)
		self.tween_card_position(card_node, new_pos, 0.5)
		self.tween_card_rotation(card_node, self.player_rotation(player), 0.2)
		
func _on_points_updated(we_hand: int, they_hand: int, we_total: int, they_total: int) -> void:
	$GUI/Score/HBoxContainer/WeColumn/HandPoints.text = str(we_hand)
	$GUI/Score/HBoxContainer/TheyColumn/HandPoints.text = str(they_hand)
	$GUI/Score/HBoxContainer/WeColumn/TotalPoints.text = str(we_total)
	$GUI/Score/HBoxContainer/TheyColumn/TotalPoints.text = str(they_total)
