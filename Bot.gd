class_name Bot
extends Node


func get_bid(game: Game, player: int) -> Card.Suit:
	print("thinking...")
	var _j = 0
	for i in range(30_000_000):
		_j = i
	return Card.Suit.NONE
	
# Assumes the player's hand cards have already been marked as eligible/ineligible.
func get_discards(game: Game, player: int) -> Array[Card]:
	print("thinking...")
	var hand = game.players[player].hand

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
	
func get_play_monte_carlo(game: Game, p_id: int) -> Card:
	print("bot: ", p_id)
	var player = game.players[p_id] as Player
	var best_card = null
	var card_ids: Array[int]
	var id_scores: Array[int]
	
	# Gather the player's card ids.
	for card in player.hand:
		if card.eligible == 1:
			card_ids.append(card.id)
			best_card = card
	if card_ids.size() == 1:
		return best_card
		
	# Gather the cards that might playable by other players.
	var card_stock: Array[Card]
	card_stock.append_array(game.deck.duplicate(true)) # should be zero cards
	card_stock.append_array(game.nest.duplicate(true))
	for p in range(game.player_count):
		if p == p_id:
			continue
		card_stock.append_array(game.players[p].hand.duplicate(true))
	print("card_stock size: ", card_stock.size())
	
	var simulations := 1000
	
	for i in range(simulations):
		print("sim: ", i)
		# make copy of game and card stock for this simulation...
		var game_copy = game.make_copy()
		var card_stock_copy = card_stock.duplicate(true)
		card_stock_copy.shuffle()
			
		# Play each eligible player card in turn
		for id in card_ids:
			print("id: ", id)
			print("sim player: ", game_copy.active_player)
			game_copy.play_card(id)
			# Assign the available cards to the other players' hands.
			for p in range(game_copy.player_count):
				if p == p_id:
					continue
				game_copy.players[p].hand.clear()
				for c in range(player.hand.size()):
					game_copy.players[p].hand.append(card_stock_copy.pop_back())
					
			
			
			while game_copy.state != Game.State.HAND_OVER:
				while game_copy.state != Game.State.AWARDING_TRICK:
					while game_copy.state == Game.State.WAITING_FOR_PLAY:
						game_copy.mark_cards_eligible_for_play()
						var cards = game_copy.players[game_copy.active_player].hand
						for card in cards:
							if card.eligible == 1:
								print("player: ", game_copy.active_player)
								game_copy.play_card(card.id)

								game_copy.check_state()
					game_copy.award_trick()
					game_copy.check_state()
				if game_copy.state == Game.State.PREPARING_FOR_NEW_TRICK:
					game_copy.prepare_for_new_trick()
	
			# Award points
			if player == 0 || player == 2:
				id_scores.push_back(game_copy.we_points - game_copy.they_points)
			else:
				id_scores.push_back(game_copy.they_points - game_copy.we_points)
	
	# Determine the highest scoring id...
	var highest_score = -1000
	for i in range(id_scores.size()):
		if id_scores[i] > highest_score:
			highest_score = id_scores[i]
			best_card = player.hand[i]
			
	print("highest score: ", highest_score)
	return best_card
	
