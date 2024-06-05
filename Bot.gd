class_name Bot
extends Node


func get_bid(_game: Game, _p_id: int) -> Card.Suit:
	print("thinking...")
	var _j = 0
	for i in range(30_000_000):
		_j = i
	return Card.Suit.NONE
	
func get_discards(game: Game, p_id: int, eligible_ids: Array[int]) -> Array[Card]:
	print("thinking...")
	var _hand = game.players[p_id].hand

	var _j = 0
	for i in range(100_000_000):
		_j = i
	return []


func get_play(game: Game, _p_id: int, eligible_ids: Array[int]) -> int:
	print("thinking...")
	var _j = 0
	for i in range(100_000_000):
		_j = i
	return eligible_ids[0]
	
func get_play_monte_carlo(game: Game, p_id: int, eligible_ids: Array[int]) -> int:
	print("bot: ", p_id)
	var monte_player = game.players[p_id] as Player

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
	
	var simulations := 1000
	var start_time = Time.get_ticks_msec()
	var rng := RandomNumberGenerator.new()

	
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
		for idx in range(eligible_ids.size()):
			var id = eligible_ids[idx]
			var monte_game = sim_game.make_copy()
			monte_game.play_card(id)
			
			while !monte_game.hand_completed():
				while !monte_game.trick_completed():
					var other_ids = monte_game.get_eligible_play_cards() as Array[int]
					var rnd_idx = rng.randi_range(0, other_ids.size()-1)
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
	
