extends Node

# Establish these as field since they are so commonly accessed. Using the
# dollar sign ($Game) doesn't type cast the object.
var game: Game
var view: View

enum BotKind {
	None,
	Bid,
	Discard,
	Play
}
var bot_kind := BotKind.None 
var bot_thread: Thread

# Called when the node enters the scene tree for the first time.
func _ready():
	print("Main.ready")
	
	self.view = get_node('View')
	self.view.bid_made.connect(self._on_bid_made)
	self.view.card_pressed.connect(self._on_card_pressed)
	
	# *** Since Game does not use:
	# await get_tree().create_timer(0.1).timeout
	# any longer, Game does not need to be a node nor added to scene tree.
	var resource = preload("res://game.tscn")
	self.game = resource.instantiate() as Game
	
	self.game.active_player_updated.connect(self.view._on_active_player_updated)
	self.game.card_created.connect(self._on_card_created)
	self.game.hand_updated.connect(self.view._on_hand_updated)
	self.game.nest_exchange_updated.connect(self.view._on_nest_exchange_updated)
	self.game.get_bid.connect(self._on_get_bid)
	self.game.trump_suit_updated.connect(self.view._on_trump_suit_updated)
	self.game.get_discards.connect(self._on_get_discards)
	self.game.nest_aside_updated.connect(self.view._on_nest_aside_updated)
	self.game.get_play.connect(self._on_get_play)
	self.game.trick_updated.connect(self._on_trick_updated)
	self.game.trick_awarded.connect(self._on_trick_awarded)
	self.game.points_updated.connect(self.view._on_points_updated)
	self.game.joker_updated.connect(self.view._on_joker_updated)
	self.game.last_trick_winner.connect(self.view._on_last_trick_winner)
	self.game.hand_result.connect(self.view._on_hand_result)
	self.game.setup(4)
	

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	match self.bot_kind:
		BotKind.None:
			pass
		BotKind.Bid:
			if !self.bot_thread.is_alive():
				var bid = self.bot_thread.wait_to_finish()
				self.bot_thread = null
				print("Bot bid: ", bid)
				self.bot_kind = BotKind.None
				self.game.make_bid(bid)
		BotKind.Discard:
			if !self.bot_thread.is_alive():
				var discards = self.bot_thread.wait_to_finish()
				self.bot_thread = null
				print("Bot discards: ", discards)
				self.bot_kind = BotKind.None
				self.game.make_discards(discards)
		BotKind.Play:
			if !self.bot_thread.is_alive():
				var card_id = self.bot_thread.wait_to_finish()
				self.bot_thread = null
				print("Bot plays id: ", card_id)
				self.bot_kind = BotKind.None
				self.game.play_card(card_id)
				self.game.check_state()
				
	self.game.process_actions(delta)
	
func _on_card_created(card: Card):
	var _node = self.view.create_card_node(card)
	
func _on_start_button_pressed() -> void:
	$View/GUI/StartButton.visible = false
	self.game.start()

# signal sent by View
func _on_card_pressed(id: int):
	var card_node = self.view.find_card_node(id) as CardNode
	if card_node.eligible == 1:
		self.game.eligible_card_pressed(id)
		
	
func _on_get_bid(player: int, is_bot: bool):
	print("main.on_get_bid")
	if is_bot:
		var bot := Bot.new()
		self.bot_kind = BotKind.Bid
		var game_copy = self.game.make_copy()
		self.bot_thread = Thread.new()
		self.bot_thread.start(bot.get_bid.bind(game_copy, player))
	else:
		self.view.get_bid()

func _on_bid_made(bid: Card.Suit) -> void:
	self.game.make_bid(bid)
	#self.do_play_loop()
	
func _on_get_discards(player: int, is_bot: bool, eligible_cards: Array[Card]):
	print("main._on_get_discards")
	if is_bot:
		var bot := Bot.new()	
		self.bot_kind = BotKind.Discard	
		var game_copy = self.game.make_copy()
		self.bot_thread = Thread.new()
		self.bot_thread.start(bot.get_discards.bind(game_copy, player))
	else:
		self.view.get_discards(eligible_cards)

func _on_done_button_pressed() -> void:
	self.view.discards_done()
	self.game.discards_done()
	#self.do_play_loop()
	
func _on_get_play(player: int, is_bot: bool, eligible_cards: Array[Card]):
	print("main._on_get_play")
	if is_bot:
		var bot := Bot.new()
		self.bot_kind = BotKind.Play
		var game_copy = self.game.make_copy()
		self.bot_thread = Thread.new()
		#self.bot_thread.start(bot.get_play.bind(game_copy, player))
		self.bot_thread.start(bot.get_play_monte_carlo.bind(game_copy, player))
	else:
		self.view.get_play(player, eligible_cards)
		
func _on_trick_updated(cards: Array[Card]) -> void:
	self.view.update_trick(cards)
	
func _on_trick_awarded(player: int, cards: Array[Card]) -> void:
	self.view.update_awarded_trick(player, cards)
	

