class_name Bot
extends Node


func get_bid(_game: Game, _player: int) -> Card.Suit:
	print("thinking...")
	var _j = 0
	for i in range(30_000_000):
		_j = i
	return Card.Suit.NONE
	
# Assumes the player's hand cards have already been marked as eligible/ineligible.
func get_discards(game: Game, player: int) -> Array[Card]:
	print("thinking...")
	var _hand = game.players[player].hand

	var _j = 0
	for i in range(100_000_000):
		_j = i
	return []

# Assumes the player's hand cards have already been marked as eligible/ineligible.
func get_play(game: Game, player: int) -> Card:
	print("thinking...")
	var _j = 0
	for i in range(100_000_000):
		_j = i
		
	var hand = game.players[player].hand
	for card in hand:
		if card.eligible:
			return card
	return null
	
func get_play_monte_carlo(game: Game, p_id: int) -> int:
	print("bot: ", p_id)
	var monte_player = game.players[p_id] as Player
	var best_id := -1
	var playable_ids: Array[int] = []
	
	# Gather the player's card ids.
	for card in monte_player.hand:
		if card.eligible == 1:
			playable_ids.append(card.id)
			best_id = card.id
	if playable_ids.size() == 1:
		return best_id
		
	# Gather the cards that might playable by other players.
	var hidden_cards: Array[Card] = []
	hidden_cards.append_array(game.deck.duplicate(true)) # should be zero cards
	hidden_cards.append_array(game.nest.duplicate(true))
	for p in range(game.player_count):
		if p == p_id:
			continue
		hidden_cards.append_array(game.players[p].hand.duplicate(true))
	
	var id_scores: Array[int] = []
	for _i in range(playable_ids.size()):
		id_scores.push_back(0)
	
	var simulations := 1000
	
	for i in range(simulations):
		# make copy of game and card stock for this simulation...
		var sim_game = game.make_copy()
		var hidden_cards_copy = hidden_cards.duplicate(true)
		hidden_cards_copy.shuffle()
		
		# Assign the available cards to the other players' hands.
		for p in range(sim_game.player_count):
			if p == p_id:
				continue
			var hand = sim_game.players[p].hand
			var size = hand.size()
			hand.clear()
			for c in range(size):
				hand.append(hidden_cards_copy.pop_back())
					
		# Play each eligible player card in turn
		for playable_idx in range(playable_ids.size()):
			var id = playable_ids[playable_idx]
			var monte_game = sim_game.make_copy()
			monte_game.play_card(id)
			
			while !monte_game.hand_completed():
				while !monte_game.trick_completed():
					monte_game.mark_cards_eligible_for_play()
					var cards = monte_game.players[monte_game.active_player].hand
					cards.shuffle()
					for card in cards:
						if card.eligible == 1:
							monte_game.play_card(card.id)
							break

				monte_game.award_trick()
				monte_game.prepare_for_new_trick()
					
			# Award points
			monte_game.tally_hand_score()
			if p_id == 0 || p_id == 2:
				id_scores[playable_idx] += (monte_game.we_points - monte_game.they_points)
			else:
				id_scores[playable_idx] += (monte_game.they_points - monte_game.we_points)
	
	# Determine the highest scoring id...
	var highest_score = -10000000
	for i in range(id_scores.size()):
		if id_scores[i] > highest_score:
			highest_score = id_scores[i]
			best_id = playable_ids[i]
			
	print("highest score: ", highest_score)
	return best_id
	
