class_name Bot
extends Node


func get_bid(game: Game, p_id: int) -> Card.Suit:
	print("thinking...")
	var hand = game.players[p_id].hand
	var suits := [Card.Suit.CLUB, Card.Suit.DIAMOND, Card.Suit.HEART, Card.Suit.SPADE]
	var suit_scores = [0, 0, 0, 0]
	
	var best_score = 0
	var best_suit: Card.Suit = Card.Suit.NONE
	for idx in range(4):
		var score = score_suit(hand, suits[idx])
		#print("P:" + str(p_id) + " suit: " + str(suits[idx]) + ", score: " + str(score))
		if score > best_score:
			best_score = score
			best_suit = suits[idx]
			
	print("P:" + str(p_id) + " best suit: " + str(best_suit) + ", score: " + str(best_score))
	
	best_score += score_jokers(hand)
	# See if score exceeds threshold needed, depending on hand size.
	#              0  1  2  3  4   5   6   7   8   9
	var pts_req = [0, 0, 0, 0, 0, 30, 34, 38, 42, 48]
	if best_score >= pts_req[hand.size()]:
		return best_suit
	return Card.Suit.NONE

# Score the values of the given suit. Trump suit not considered.
func score_suit(hand: Array[Card], suit: Card.Suit) -> int:
	var score := 0
	#              5  6  7  8  9 10  J  Q   K   A
	var values := [3, 0, 4, 5, 6, 7, 8, 9, 10, 15]
	for card in hand:
		if card.suit == suit:
			var idx := card.rank as int - 5
			score += values[idx]
	return score
	
func score_jokers(hand: Array[Card]) -> int:
	var count := 0
	for card in hand:
		if card.suit == Card.Suit.JOKER:
			count += 1
	match count:
		1:
			return 10
		2:
			return 20
		_:
			return 0
		
func get_discards(game: Game, p_id: int, _eligible_ids: Array[int]) -> Array[Card]:
	print("thinking...")
	var _hand = game.players[p_id].hand

	var _j = 0
	for i in range(100_000_000):
		_j = i
	return []


func get_play(_game: Game, _p_id: int, _eligible_ids: Array[int]) -> int:
	print("thinking...")
	var _j = 0
	for i in range(100_000_000):
		_j = i
	return _eligible_ids[0]
	
func get_play_monte_carlo(game: Game, p_id: int, id_dict: Dictionary) -> int:
	print("bot: ", p_id)
	#var monte_player = game.players[p_id] as Player
	var eligible_ids = id_dict[1] as Array[int]
	if eligible_ids.size() == 1:
		return eligible_ids[0]
		
	var best_id := -1
		
	# Gather the cards that might playable by other players.
	var hidden_cards: Array[Card] = []
	hidden_cards.append_array(game.deck.duplicate(true)) # should be zero cards
	hidden_cards.append_array(game.nest.duplicate(true))
	for p in range(game.player_count):
		if p == p_id:
			continue
		hidden_cards.append_array(game.players[p].hand.duplicate(true))
	
	var id_scores: Array[int] = []
	for _i in range(eligible_ids.size()):
		id_scores.push_back(0)
	
	var simulations := 500
	var start_time = Time.get_ticks_msec()
	var rng := RandomNumberGenerator.new()

	
	for i in range(simulations):
		# make copy of game and card stock for this simulation...
		var sim_game = game.make_copy() as Game
		var hidden_cards_copy = hidden_cards.duplicate(true)
		hidden_cards_copy.shuffle()
		
		# Assign the available cards to the other players' hands.
		for p in range(sim_game.player_count):
			if p == p_id:
				continue
			var player = sim_game.players[p] as Player
			var hand = player.hand as Array[Card]
			var orig_hand_size = hand.size()
			hand.clear()
			
			var hidden_idx = hidden_cards_copy.size() as int
			while hand.size() < orig_hand_size:
				hidden_idx -= 1
				if hidden_idx < 0:
					print("OUT OF HIDDEN CARDS TO ASSIGN!")
				else:
					var card = hidden_cards_copy[hidden_idx] as Card
					if !player.is_out_of_suit(card.suit):
						hand.push_back(card)
					
		# Play each eligible player card in turn
		for idx in range(eligible_ids.size()):
			var id = eligible_ids[idx] as int
			var monte_game = sim_game.make_copy() as Game
			monte_game.play_card(id)
			
			while !monte_game.hand_completed():
				while !monte_game.trick_completed():
					var other_dict = monte_game.get_eligible_play_cards() as Dictionary
					var other_ids = other_dict[1] as Array[int]
					var rnd_idx = rng.randi_range(0, other_ids.size()-1) as int
					monte_game.play_card(other_ids[rnd_idx])
				monte_game.award_trick()
				monte_game.prepare_for_new_trick()
					
			# Award points
			monte_game.tally_hand_score()
			if p_id == 0 || p_id == 2:
				id_scores[idx] += (monte_game.we_points - monte_game.they_points)
			else:
				id_scores[idx] += (monte_game.they_points - monte_game.we_points)
	
	# Determine the highest scoring id...
	var highest_score := -10000000
	for i in range(id_scores.size()):
		if id_scores[i] > highest_score:
			highest_score = id_scores[i]
			best_id = eligible_ids[i]
	
	var time_delta = Time.get_ticks_msec() - start_time
	print("time per eligible id in msec: ", time_delta / eligible_ids.size())
	print("highest score: ", highest_score)
	return best_id
	
