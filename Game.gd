class_name Game
extends Node

# This controls whether cards are face up, among possibly other things.
var TESTING = false

enum State {
	INIT,
	STARTING,
	PREPARING_FOR_NEW_HAND,
	DEALING_INITIAL,
	DEALING_TO_NEST, 
	DEALING_ONE_CARD_EACH,
	DEALING_OUT,
	BIDDING, 
	WAITING_FOR_BID,
	HANDLING_BID,
	MOVING_NEST_TO_HAND,
	DISCARDING,
	WAITING_FOR_DISCARDS,
	STARTING_HAND,
	STARTING_NO_TRUMP,
	PREPARING_FOR_NEW_TRICK,
	PLAYING,
	WAITING_FOR_PLAY,
	AWARDING_TRICK,
	HAND_OVER,
	GAME_OVER
}
enum Action {
	CHECK_STATE,
	PREPARE_FOR_NEW_HAND,
	DEAL_CARD, 
	DEAL_TO_NEST, 
	GET_BID,
	HIDE_BIDS,
	START_HAND,
	START_NO_TRUMP,
	MOVE_NEST_TO_HAND,
	GET_DISCARDS,
	PREPARE_FOR_NEW_TRICK,
	GET_PLAY,
	AWARD_NEST,
	AWARD_TRICK,
	AWARD_LAST_TRICK_BONUS,
	TALLY_HAND_SCORE,
	PAUSE_TINY, 
	PAUSE
}

signal card_created(card)
signal active_player_updated(player, state)
signal dealer_updated(dealer)
signal deck_updated(cards)
signal hand_updated(player, cards, is_bot)
signal card_eligibility_updated(eligible_ids, is_bot)
signal nest_exchange_updated(cards)
signal get_bid(player, is_bot)
signal display_bid(player, bid)
signal hide_bids
signal trump_suit_updated(suit)
signal get_discards(player, is_bot, hand_cards, eligible_ids)
signal nest_aside_updated(cards)
signal get_play(player, is_bot, eligible_ids)
signal trick_updated(cards)
signal trick_awarded(player, cards)
signal points_updated(we_hand, they_hand, we_total, they_total)
signal joker_updated(card)
signal last_trick_winner(trick_winner, nest_points, bonus)
signal hand_result(maker, made_bid)

var player_count: int = 4
var deck: Array[Card] = []
var nest: Array[Card] = []
var players: Array[Player] = []
var trick: Array[Card] = []
var state: State = State.INIT
var joker_ids: Array[int] = []

# These are set in prepare_for_new_hand()
var active_player: int
var dealer := randi_range(0, player_count - 1)
var cards_dealt: int
var cards_to_deal: int
var cards_to_deal_total: int
var maker: int # player number
var pass_count: int # when equal to player_count, bid round is over
var trump_suit:int # will be assigned a Card.Suit
var jokers_played_count: int
var tricks_played: int

# These is set in prepare_for_new_trick()
var lead_card: Card = null
var winning_card: Card = null
var trick_winner: int # player

var we_points := 0
var they_points := 0
var hand_point_req := 80
var nest_bonus := 10

var view_exists := true
var pause_time := 0.0
var think_time := 0.0
var actions: Array[Action] = []

func make_copy() -> Game:
	var resource = preload("res://game.tscn")
	var copy = resource.instantiate() as Game
	copy.view_exists = false
	
	copy.player_count = self.player_count
	copy.deck += self.deck.duplicate(true)
	copy.nest += self.nest.duplicate(true)
	
	for p in range(self.players.size()):
		copy.players.push_back(self.players[p].make_copy())
	
	copy.trick += self.trick.duplicate(true)
	copy.state = self.state
	copy.joker_ids += self.joker_ids.duplicate(true)
	
	copy.active_player = self.active_player
	copy.cards_dealt = self.cards_dealt
	copy.cards_to_deal = self.cards_to_deal
	copy.cards_to_deal_total = self.cards_to_deal_total
	copy.maker = self.maker
	copy.trump_suit = self.trump_suit
	copy.jokers_played_count = self.jokers_played_count
	copy.tricks_played = self.tricks_played
	copy.hand_point_req = self.hand_point_req
	copy.nest_bonus = self.nest_bonus
	
	if self.lead_card != null:
		copy.lead_card = self.lead_card.make_copy()
	
	copy.trick_winner = self.trick_winner
	
	if self.winning_card != null:
		copy.winning_card = self.winning_card.make_copy()
	
	copy.hand_point_req = self.hand_point_req
	return copy

# new/_init funcs are not automatically called when a scene is instantiated.
func setup(_player_count: int) -> void:
	self.player_count = _player_count
	for i in range(self.player_count):
		var player = Player.new()
		player.is_bot = i != 0
		self.players.push_back(player)
	self.create_cards()
	
func player_is_bot() -> bool:
	return self.players[self.active_player].is_bot

func add_card_to_deck(card: Card):
	card.face_up = false
	self.deck.push_back(card)
	
func find_card_in_deck(id: int) -> Card:
	for card in self.deck:
		if card.id == id:
			return card
	return null
	
func find_card_in_hand(id: int, player: int) -> Card:
	for card in self.players[player].hand:
		if card.id == id:
			return card
	return null
	
func find_card_in_nest(id) -> Card:
	for card in self.nest:
		if card.id == id:
			return card
	return null
	
func on_state_entered(state: State) -> void:
	match self.state:
		State.DEALING_INITIAL:
			self.cards_to_deal = self.player_count * 5
		State.DEALING_TO_NEST:
			self.deal_to_nest(2)
		State.DEALING_ONE_CARD_EACH:
			self.cards_to_deal = self.player_count
		State.DEALING_OUT:
			self.cards_to_deal = self.cards_to_deal_total - self.cards_dealt
		State.BIDDING:
			self.pass_count = 0
		State.MOVING_NEST_TO_HAND:
			self.move_nest_to_hand()
		State.DISCARDING:
			if self.player_is_bot():
				self.pause_time = 1.0
		_:
			pass
	
func on_state_exited(state: State) -> void:
	match self.state:
		State.BIDDING:
			emit_signal("hide_bids")
		State.MOVING_NEST_TO_HAND:
			
		_:
			pass
	
func change_state(new_state: State) -> void:
	self.on_state_exited(self.state)
	self.state = new_state
	self.on_state_entered(self.state)
	
func process_state(time_delta: float):
	self.pause_time = max(self.pause_time - time_delta, 0.0)
	if pause_time > 0.0:
		return

	match self.state:
		State.INIT:
			pass # next state set by start()
		State.STARTING:
			self.change_state(State.PREPARING_FOR_NEW_HAND)
		State.PREPARING_FOR_NEW_HAND:
			self.change_state(State.DEALING_INITIAL)
		State.DEALING_INITIAL:
			if self.cards_to_deal == 0:
				self.change_state(State.DEALING_TO_NEST)
			else:
				self.deal_card(self.active_player)
				if self.view:
					self.pause_time = 0.5
		State.DEALING_TO_NEST:
			self.change_state(State.BIDDING)
		State.DEALING_ONE_CARD_EACH:
			if self.cards_to_deal == 0:
				self.change_state(State.BIDDING)
			else:
				self.deal_card(self.active_player)
				if self.view:
					self.pause_time = 0.5
		
		State.BIDDING:
			pass
			if self.maker == -1:
				if self.pass_count == self.player_count:
					if self.cards_dealt < self.cards_to_deal_total:
						#self.active_player = self.dealer
						#self.advance_player()
						self.change_state(State.DEALING_ONE_CARD_EACH)
					else:
						self.change_state(State.STARTING_NO_TRUMP)
				else: # Continue bidding
					self.advance_player()
					emit_signal("get_bid", self.active_player, self.player_is_bot())
			else: # We have a maker.
				if self.cards_dealt < self.cards_to_deal_total:
					# Finish dealing the rest of the cards.
					self.change_state(State.DEALING_OUT)
				else:
					self.change_state(State.MOVING_NEST_TO_HAND)
					
		State.MOVING_NEST_TO_HAND:
			self.state = State.DISCARDING
		State.DISCARDING:
			self.state = State.WAITING_FOR_DISCARDS
		State.WAITING_FOR_DISCARDS:
			pass # next state set by discards_done()
		State.STARTING_HAND:
			self.state = State.PREPARING_FOR_NEW_TRICK
		State.STARTING_NO_TRUMP:
			self.state = State.PREPARING_FOR_NEW_TRICK
		State.PREPARING_FOR_NEW_TRICK:
			self.state = State.PLAYING
		State.PLAYING:
			self.state = State.WAITING_FOR_PLAY
		State.WAITING_FOR_PLAY:
			if self.trick_completed():
				self.state = State.AWARDING_TRICK
			else:
				self.state = State.PLAYING
		State.AWARDING_TRICK:
			if self.hand_completed():
				self.state = State.HAND_OVER
			else:
				self.state = State.PREPARING_FOR_NEW_TRICK
		State.HAND_OVER:
			print("hand over")
		State.GAME_OVER:
			pass
	
	
func check_state():
	var current_state = self.state
	match self.state:
		State.INIT:
			pass # next state set by start()
		State.STARTING:
			self.state = State.PREPARING_FOR_NEW_HAND
		State.PREPARING_FOR_NEW_HAND:
			self.state = State.DEALING
		State.DEALING:
			if self.cards_dealt == 20:
				self.state = State.DEALING_TO_NEST
			elif self.cards_to_deal == 0:
				if self.maker == -1:
					self.state = State.BIDDING
				else:
					# We just finished dealing out the rest of the cards after a bid.
					self.state = State.MOVING_NEST_TO_HAND
		State.DEALING_TO_NEST:
			self.state = State.BIDDING
		State.BIDDING:
			self.state = State.WAITING_FOR_BID
		State.WAITING_FOR_BID:
			if self.maker == -1:
				if self.pass_count == self.player_count:
					self.pass_count = 0
					if self.cards_dealt < self.cards_to_deal_total:
						self.state = State.DEALING
						self.cards_to_deal = 4
						self.active_player = self.dealer
						self.advance_player()
					else:
						self.state = State.STARTING_NO_TRUMP
				else:
					self.advance_player()
					self.state = State.BIDDING
			else: # We have a maker.
				if self.cards_dealt < self.cards_to_deal_total:
					# Finish dealing the rest of the cards.
					self.state = State.DEALING
					self.cards_to_deal = self.cards_to_deal_total - self.cards_dealt
				else:
					self.state = State.MOVING_NEST_TO_HAND
		State.MOVING_NEST_TO_HAND:
			self.state = State.DISCARDING
		State.DISCARDING:
			self.state = State.WAITING_FOR_DISCARDS
		State.WAITING_FOR_DISCARDS:
			pass # next state set by discards_done()
		State.STARTING_HAND:
			self.state = State.PREPARING_FOR_NEW_TRICK
		State.STARTING_NO_TRUMP:
			self.state = State.PREPARING_FOR_NEW_TRICK
		State.PREPARING_FOR_NEW_TRICK:
			self.state = State.PLAYING
		State.PLAYING:
			self.state = State.WAITING_FOR_PLAY
		State.WAITING_FOR_PLAY:
			if self.trick_completed():
				self.state = State.AWARDING_TRICK
			else:
				self.state = State.PLAYING
		State.AWARDING_TRICK:
			if self.hand_completed():
				self.state = State.HAND_OVER
			else:
				self.state = State.PREPARING_FOR_NEW_TRICK
		State.HAND_OVER:
			print("hand over")
		State.GAME_OVER:
			pass
			
	if current_state != self.state:
		print("New state: %s" % [State.keys()[self.state]])
		self.add_actions()

func add_actions():
	var new_actions = []
	match self.state:
		State.INIT:
			pass
		State.STARTING:
			pass
		State.PREPARING_FOR_NEW_HAND:
			new_actions = [Action.PREPARE_FOR_NEW_HAND, Action.PAUSE]
		State.DEALING:
			new_actions = [Action.HIDE_BIDS]
			for i in range(self.cards_to_deal):
				new_actions.push_back(Action.DEAL_CARD)
				new_actions.push_back(Action.PAUSE_TINY)
		State.DEALING_TO_NEST:
			new_actions = [Action.PAUSE, Action.DEAL_TO_NEST]
		State.BIDDING:
			new_actions = [Action.GET_BID]
		State.WAITING_FOR_BID:
			pass
		State.MOVING_NEST_TO_HAND:
			new_actions = [Action.HIDE_BIDS, Action.MOVE_NEST_TO_HAND]
			if self.player_is_bot():
				new_actions.push_back(Action.PAUSE)
		State.DISCARDING:
			new_actions = [Action.GET_DISCARDS]
		State.WAITING_FOR_DISCARDS:
			pass
		State.STARTING_HAND:
			new_actions = [Action.START_HAND]
		State.STARTING_NO_TRUMP:
			new_actions = [Action.HIDE_BIDS, Action.START_NO_TRUMP]
		State.PREPARING_FOR_NEW_TRICK:
			new_actions = [Action.PREPARE_FOR_NEW_TRICK]
		State.PLAYING:
			new_actions = [Action.GET_PLAY]
		State.WAITING_FOR_PLAY:
			pass
		State.AWARDING_TRICK:
			new_actions = [Action.PAUSE, Action.AWARD_TRICK]
		State.HAND_OVER:
			new_actions = [Action.AWARD_NEST, Action.AWARD_LAST_TRICK_BONUS, Action.TALLY_HAND_SCORE]
		State.GAME_OVER:
			pass
			
	if !new_actions.is_empty():
		for a in new_actions:
			self.actions.push_back(a)
		self.actions.push_back(Action.CHECK_STATE)
		

func process_actions(time_delta: float):
	self.think_time += time_delta
	
	if self.actions.is_empty():
		return
		
	if self.pause_time > 0.0:
		self.pause_time -= time_delta
		if self.pause_time <= 0.0:
			self.pause_time = 0.0
		return
		
	var action = self.actions.pop_front()
	match action:
		Action.CHECK_STATE:
			self.check_state()
		Action.PREPARE_FOR_NEW_HAND:
			self.prepare_for_new_hand()
		Action.DEAL_CARD:
			self.deal_card(self.active_player)
			self.advance_player()
		Action.DEAL_TO_NEST:
			self.deal_to_nest(2)
		Action.GET_BID:
			emit_signal("get_bid", self.active_player, self.player_is_bot())
		Action.HIDE_BIDS:
			emit_signal("hide_bids")
		Action.START_HAND:
			self.start_hand()
		Action.START_NO_TRUMP:
			self.start_no_trump_hand()
		Action.MOVE_NEST_TO_HAND:
			self.move_nest_to_hand()
		Action.GET_DISCARDS:
			var is_bot = self.players[self.maker].is_bot
			var hand = self.players[self.maker].hand
			var id_dict = self.get_eligible_discards()
			if self.view_exists:
				emit_signal("card_eligibility_updated", id_dict, is_bot)
			self.think_time = 0.0
			emit_signal("get_discards", self.maker, is_bot, hand, id_dict)
		Action.PREPARE_FOR_NEW_TRICK:
			self.prepare_for_new_trick()
		Action.GET_PLAY:
			var is_bot = self.players[self.active_player].is_bot
			var id_dict = self.get_eligible_play_cards()
			if self.view_exists:
				emit_signal("card_eligibility_updated", id_dict, is_bot)
			self.think_time = 0.0
			emit_signal("get_play", self.active_player, is_bot, id_dict)
		Action.AWARD_NEST:
			self.award_nest()
		Action.AWARD_TRICK:
			self.award_trick()
		Action.AWARD_LAST_TRICK_BONUS:
			self.award_last_trick_bonus()
		Action.TALLY_HAND_SCORE:
			self.tally_hand_score()
		Action.PAUSE_TINY:
			if self.view_exists:
				self.pause_time = 0.1
		Action.PAUSE:
			if self.view_exists:
				self.pause_time = 1.0
					

func create_cards():
	var id: int = 0
	
	for suit in [Card.Suit.CLUB, Card.Suit.DIAMOND, Card.Suit.HEART, Card.Suit.SPADE]:
		# Base cards. Note that 6 is skipped.
		for rank in [5, 7, 8, 9, 10, 11, 12, 13, 14]:
		
			var points: int
			match rank:
				5:
					points = 5
				10:
					points = 10
				14:
					points = 15
				_:
					points = 0
			self.create_card(id, suit, rank as float, points)
			id += 1
	# Jokers
	for i in range(2):
		self.create_card(id, Card.Suit.JOKER, 15.0, 0)
		self.joker_ids.push_back(id)
		id += 1
		
	if self.view_exists:
		emit_signal("deck_updated", self.deck)
		
func create_card(id: int, suit: Card.Suit, rank: float, points: int):
	var card = Card.new(id, suit, rank, points)
	self.add_card_to_deck(card)
	emit_signal("card_created", card)
	
func advance_player():
	self.active_player = (self.active_player + 1) % self.player_count
	if self.view_exists:
		emit_signal("active_player_updated", self.active_player, self.state)
		
func advance_dealer():
	self.dealer = (self.dealer + 1) % self.player_count
	if self.view_exists:
		emit_signal("dealer_updated", self.dealer)
	
func start():
	self.state = State.STARTING
	self.check_state()
		
func prepare_for_new_hand() -> void:
	# **** Gather cards to deck first, then...
	self.deck.shuffle()
	
	self.advance_dealer()
	self.active_player = self.dealer
	self.advance_player()
	
	if self.view_exists:
		emit_signal("deck_updated", self.deck)
		emit_signal("active_player_updated", self.active_player, self.state)
		
	self.cards_dealt = 0
	self.cards_to_deal = 20
	self.cards_to_deal_total = 36
	self.maker = -1 # player number
	self.pass_count = 0 # when equal to player_count, bid round is over
	self.trump_suit = -1 # will be assigned a Card.Suit
	self.jokers_played_count = 0
	self.tricks_played = 0
	
	# Restore jokers
	for id in self.joker_ids:
		var card = self.find_card_in_deck(id)
		card.points = 0
		card.rank = 15.0
		if self.view_exists:
			emit_signal("joker_updated", card)
		
	for player in self.players:
		player.reset_for_new_hand()
	
func deal_card(p: int):
	self.cards_dealt += 1
	self.cards_to_deal -= 1
	var card = self.deck.pop_back() as Card
	var player = self.players[p]
	if !player.is_bot || TESTING:
		card.face_up = true
	else:
		card.face_up = false
	player.hand.push_back(card)
	if !player.is_bot:
		player.sort_hand()
	
	if self.view_exists:
		emit_signal("hand_updated", p, player.hand, player.is_bot)

func deal_to_nest(count: int):
	for i in count:
		var card = self.deck.pop_back()
		card.face_up = false
		self.nest.push_back(card)
		
	if self.view_exists:
		emit_signal("nest_aside_updated", self.nest)

func make_bid(bid: Card.Suit):
	#print("player: " + str(self.active_player) + " bid made: ", bid)
	match bid:
		Card.Suit.NONE: # pass
			self.pass_count += 1
		_:
			self.maker = self.active_player
			self.trump_suit = bid
			if self.view_exists:
				emit_signal("trump_suit_updated", bid)
			
	if self.view_exists:
		emit_signal("display_bid", self.active_player, bid)
		#if self.player_is_bot():
		self.actions.push_back(Action.PAUSE)
	self.check_state()

func start_hand(): # not a no-trump hand
	print("Starting hand.")
	self.active_player = self.maker
	self.advance_player()
	if self.view_exists:
		emit_signal("nest_aside_updated", self.nest)

func start_no_trump_hand():
	print("Starting no-trump hand.")
	self.active_player = self.dealer
	self.advance_player()
	if self.view_exists:
		emit_signal("nest_aside_updated", self.nest)
	
func move_nest_to_hand():
	var player = self.players[self.maker]
	for i in self.nest.size():
		var card = self.nest.pop_back()
		if !player.is_bot || TESTING:
			card.face_up = true
		else:
			card.face_up = false
		player.hand.push_back(card)
		
	if !player.is_bot:
		player.sort_hand()
		
	if self.view_exists:
		emit_signal("hand_updated", self.maker, player.hand, player.is_bot)
		
func turn_off_eligibility(_player: int) -> void:
	var player = self.players[_player]
	var hand = player.hand
	
	for card in hand:
		card.set_eligibility(-1)
				
	if self.view_exists:
		emit_signal("hand updated", _player, hand, player.is_bot)

func get_eligible_discards() -> Dictionary:
	var player = self.players[self.maker]
	var eligible_ids: Array[int] = []
	var ineligible_ids: Array[int] = []
	for card: Card in player.hand:
		match card.suit:
			Card.Suit.JOKER:
				ineligible_ids.push_back(card.id)
			_:
				eligible_ids.push_back(card.id)	
	return {1 : eligible_ids, 0: ineligible_ids}
		
func get_eligible_play_cards() -> Dictionary:
	var player = self.players[self.active_player]
	
	var has_lead_suit = false
	
	if self.lead_card != null:
		for card: Card in player.hand:
			if card.suit == self.lead_card.suit:
				has_lead_suit = true
				break
			if self.lead_card.suit == self.trump_suit && card.suit == Card.Suit.JOKER:
				has_lead_suit = true
				break
			if self.lead_card.suit == Card.Suit.JOKER && card.suit == self.trump_suit:
				has_lead_suit = true
				break
	
	var eligible_ids: Array[int] = []
	var ineligible_ids: Array[int] = []
	
	for card: Card in player.hand:
		# First card of trick?
		if self.lead_card == null || !has_lead_suit:
			eligible_ids.push_back(card.id)
			continue
		
		# Cards matching lead suit are always eligible.
		if card.suit == self.lead_card.suit:
			eligible_ids.push_back(card.id)
			continue
			
		# Lead card is trump and this card is Joker?
		if self.lead_card.suit == self.trump_suit && card.suit == Card.Suit.JOKER:
			eligible_ids.push_back(card.id)
			continue
		
		# If Joker is lead, trump suited cards are eligible.
		if self.lead_card.suit == Card.Suit.JOKER && card.suit == self.trump_suit:
			eligible_ids.push_back(card.id)
			continue
			
		ineligible_ids.push_back(card.id)
	
	return {1 : eligible_ids, 0: ineligible_ids}
	#if self.view_exists:
		#emit_signal("hand_updated", self.active_player, player.hand, false) #player.is_bot

func eligible_card_pressed(id) -> void:
	match self.state:
		State.WAITING_FOR_DISCARDS:
			var hand_card = self.find_card_in_hand(id, self.active_player)
			if hand_card != null && self.nest.size() < 2:
				self.move_card_to_nest(id)
			else:
				var nest_card = self.find_card_in_nest(id)
				if nest_card != null:
					self.move_nest_card_to_hand(id, self.active_player)
					
		State.WAITING_FOR_PLAY:
			var hand_card = self.find_card_in_hand(id, self.active_player)
			if hand_card != null:
				self.play_card(id)
				self.check_state()
				
		_:
			print("no dice")

func move_card_to_nest(card_id: int) -> void:
	var player = self.players[self.maker]
	
	for i in range(player.hand.size()):
		var card = player.hand[i]
		if card.id == card_id:
			player.hand.remove_at(i)
			self.nest.push_back(card)
			
			if self.view_exists:
				emit_signal("hand_updated", self.maker, player.hand, player.is_bot)
				emit_signal("nest_exchange_updated", self.nest)
			break

func move_nest_card_to_hand(id: int, p_id: int) -> void:
	var player = self.players[p_id]
	
	for i in range(self.nest.size()):
		var card = self.nest[i]
		if card.id == id:
			self.nest.remove_at(i)
			player.hand.push_back(card)
			break
			
	if !player.is_bot:
		player.sort_hand()
		
	if self.view_exists:
		emit_signal("hand_updated", p_id, player.hand, player.is_bot)
		emit_signal("nest_exchange_updated", self.nest)

# There are two discards in the nest, and discarding is over.
func discards_done() -> void:
	for i in range(self.nest.size()):
		var card = self.nest[i]
		card.face_up = false
	
	self.state = State.STARTING_HAND
	self.add_actions()
		
func prepare_for_new_trick() -> void:
	self.lead_card = null
	self.winning_card = null
	self.trick_winner = -1
	self.trick.clear()
	for _i in self.player_count:
		self.trick.push_back(null)
		
func is_trump(suit: Card.Suit) -> bool:
	return suit == self.trump_suit || suit == Card.Suit.JOKER

func play_card(card_id: int) -> void:
	# Need a copy since self.active_player changes below before signals are emitted.
	var p_id = self.active_player
	var player = self.players[p_id]
	
	var card: Card = null
	for i in range(player.hand.size()):
		if player.hand[i].id == card_id:
			card = player.hand[i]
			player.hand.remove_at(i)
			break
	
	self.trick[self.active_player] = card
	
	# If this is the first joker played, change the second one.
	if card.suit == Card.Suit.JOKER:
		self.jokers_played_count += 1
		if self.jokers_played_count == 1:
			self.update_second_joker()
			
	# For Bot use: determine if the card played signals the player is out of the lead suit.
	var out_of_lead_suit := true
	if self.lead_card != null:
		if self.is_trump(card.suit) && self.is_trump(self.lead_card.suit):
			out_of_lead_suit = false
		elif card.suit == self.lead_card.suit:
			out_of_lead_suit = false
		if out_of_lead_suit:
			player.set_out_of_suit(self.lead_card.suit)
	
	# first card played in trick
	if self.lead_card == null:
		self.lead_card = card
		self.winning_card = card
		self.trick_winner = p_id
	
	# playing a trump card
	elif self.is_trump(card.suit):
		# winning card is also trump
		if self.is_trump(self.winning_card.suit):
			if card.rank > self.winning_card.rank:
				self.winning_card = card
				self.trick_winner = p_id
		# first trump card played
		else:
			self.winning_card = card
			self.trick_winner = p_id
		
	# card suit is not trump
	elif card.suit == self.winning_card.suit && card.rank > self.winning_card.rank:
		self.winning_card = card
		self.trick_winner = p_id
	
	# end card comparison
	
	var yet_to_play = self.trick.count(null)
	if yet_to_play > 0:
		self.advance_player()
	
	if self.think_time < 1.0:
		self.actions.push_back(Action.PAUSE_TINY)
		
	if self.view_exists:
		card.face_up = true
		emit_signal("hand_updated", p_id, player.hand, player.is_bot)
		emit_signal("trick_updated", self.trick)
	
	
func update_second_joker() -> void:
	var found := false
	for player: Player in self.players:
		for card: Card in player.hand:
			if card.suit == Card.Suit.JOKER:
				found = true
				card.points = 20
				card.rank = 0.0
				if self.view_exists:
					emit_signal("joker_updated", card)
				break
		if found:
			break
			
func trick_completed() -> bool:
	return self.trick.count(null) == 0
	
func award_nest() -> void:
	for card in self.nest:
		card.face_up = true
	if self.view_exists:
		# Perhaps a new signal is needed to not only show nest but display the points etc.
		emit_signal("nest_aside_updated", self.nest)

func award_trick() -> void:
	var player = self.players[self.trick_winner]
	player.take_trick(self.trick)
	self.active_player = self.trick_winner
	
	self.tricks_played += 1
	if self.view_exists:
		emit_signal("active_player_updated", self.active_player, self.state)
		emit_signal("trick_awarded", self.trick_winner, self.trick)
		var we_hand = self.players[0].hand_points + self.players[2].hand_points
		var they_hand = self.players[1].hand_points + self.players[3].hand_points
		emit_signal("points_updated", we_hand, they_hand, self.we_points, self.they_points)

func hand_completed() -> bool:
	return self.tricks_played == 9
	
func award_last_trick_bonus() -> void:
	var nest_points = 0
	for card: Card in self.nest:
		# Very rare scenario: a no-trump hand and both Jokers are in the nest.
		if self.jokers_played_count == 0 && card.suit == Card.Suit.JOKER:
			self.jokers_played_count += 1
			card.points = 20
			if self.view_exists:
				emit_signal("joker_updated", card)
				
		nest_points += card.points
	
	self.players[self.trick_winner].hand_points += nest_points + self.nest_bonus
	
	var we_hand = self.players[0].hand_points + self.players[2].hand_points
	var they_hand = self.players[1].hand_points + self.players[3].hand_points
	
	if self.view_exists:
		emit_signal("last_trick_winner", self.trick_winner, nest_points, self.nest_bonus)
		emit_signal("points_updated", we_hand, they_hand, self.we_points, self.they_points)
		
func tally_hand_score() -> void:
	var we_hand = self.players[0].hand_points + self.players[2].hand_points
	var they_hand = self.players[1].hand_points + self.players[3].hand_points
	
	var made_bid = false
	
	if self.maker == 0 || self.maker == 2:
		# Add nest points to score...
		if we_hand >= self.hand_point_req:
			self.we_points += we_hand
			made_bid = true
		self.they_points += they_hand
		
	if self.maker == 1 || self.maker == 3:
		if they_hand >= self.hand_point_req:
			self.they_points += they_hand
			made_bid = true
		self.we_points += we_hand
		
	if self.maker == -1: # no trump hand
		self.we_points += we_hand
		self.they_points += they_hand

	if self.view_exists:
		emit_signal("hand_result", self.maker, made_bid)
		emit_signal("points_updated", we_hand, they_hand, self.we_points, self.they_points)
