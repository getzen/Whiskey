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
		card_stock.shuffle()
		
		# Assign the available cards to the other players' hands.
		for p in range(game.player_count):
			if p == p_id:
				continue
			for c in range(player.hand.size()):
				game.players[p].hand.append(card_stock.pop_back())
	
		# Play each eligible player card in turn
		for id in card_ids:
			game.play_card(id)
			while game.state != Game.State.HAND_OVER:
				while game.state != Game.State.AWARDING_TRICK:
					game.mark_cards_eligible_for_play()
					var cards = game.players[game.active_player].hand
				game.prepare_for_new_trick()
			# scoring...
				
	return best_card
	
