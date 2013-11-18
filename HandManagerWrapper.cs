using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace ControllerSupport {
	public class HandManagerWrapper {
		private HandManager handManager;
		private MethodInfo getCardViewByIdMethodInfo = null;
		private long activeCardID = -1L;
		private bool nextCardReserved = false;

		public HandManagerWrapper (HandManager handManager){
			Initialize (handManager);
		}

		public void Validate (HandManager handManager) {
			if (this.handManager == null) {
				Initialize (handManager);
			}
		}

		private void Initialize(HandManager handManager) {
			this.handManager = handManager;
			getCardViewByIdMethodInfo = ReflectionsManager.GetMethod (handManager, "GetCardViewById");
			activeCardID = -1L;
			nextCardReserved = false;
		}

		public void SelectNextCard(int displacement) {
			// If a next card was previously reserved and we are pressing right, ignore this call.
			// This is to prevent the following: 
			// We have cards 1 2 3 4 in hand.
			// We play 2, 3 gets reserved and set as active (since otherwise we lose our index)
			// If we would not reserve and we press right, we immediatly jump to card 4 which feels unnatural.
			// Similarely if the displacement is -1 we DO want to ignore the reservation and select card 1.
			Console.WriteLine ("A");
			if (displacement == 1 && nextCardReserved) {
				nextCardReserved = false;
				return;
			}
			Console.WriteLine ("B");
			CardView nextCard = GetCardViewById(GetNextCard(displacement));
			Console.WriteLine ("C");
			// Select the first card in hand if we had no previously selected card.
			if (nextCard == null) {
				List<CardView> cardViews = handManager.GetCardViewsInHand ();
				if (cardViews.Count > 0) {
					nextCard =  cardViews[0];
				}
			}
			Console.WriteLine ("D");
			// If we have a valid card to select, save it as the active card.
			if (nextCard != null) {
				activeCardID = nextCard.getCardInfo ().getId ();
			}
			Console.WriteLine ("E");
		}
		private long GetNextCard(int displacement) {
			CardView nextCard = null;
			long nextCardID = -1L;
			// Check if we had a previously selected card.
			if (activeCardID != -1L) {
				nextCard = GetCardViewById (activeCardID);
				// Check if that card still exists.
				if (nextCard != null) {
					// If so, look it up in our hand and select the next one.
					List<CardView> cardsInHand = GetCardViewsInHand ();
					for (int i = 0; i < cardsInHand.Count; i++) {
						if (cardsInHand [i].getCardInfo ().getId () == activeCardID) {
							int nextCardIndex = i + displacement;
							if (nextCardIndex < 0) {
								nextCardIndex += cardsInHand.Count;
							}
							if (nextCardIndex >= cardsInHand.Count) {
								nextCardIndex -= cardsInHand.Count;
							}
							nextCard = cardsInHand[nextCardIndex];
							nextCardID = nextCard.getCardInfo ().getId ();
							break;
						}
					}
				} 
			}
			return nextCardID;
		}

		public void MagnifySelected() {
			CardView magnifiedCard = (CardView)ReflectionsManager.GetValue (handManager, "magnifiedCard");
			// If a magnifiedCard already exists, destroy it. Else check if we have a valid selected card and then magnify it.
			if (magnifiedCard != null ) {
				ReflectionsManager.GetMethod (handManager, "DestroyMagnified").Invoke (handManager, new object[] {});
				handManager.DeselectCard ();
			} else if (handManager.GetSelectedCard () != null) {
				handManager.MagnifySelected ();
			}
		}

		public void UseActiveCard(string action, ResourceType[] resTypes = null) {
			CardActivator cardActivator = (CardActivator)ReflectionsManager.GetValue (handManager, "cardActivator");
			if (cardActivator != null) {
				bool allowAction = true;
				if (action == "growth" || action == "order" || action == "energy" || action == "decay") {
					allowAction = CanSacrificeForResource (action, resTypes);
				}
				if (allowAction) {
					// Try to reserve the next card.
					ReserveNextCard ();
					// Use the currently active card.
					ReflectionsManager.GetMethod (cardActivator, "iconClicked").Invoke (cardActivator, new object[] { action });
				}
			}
		}

		public void ReserveNextCard () {
			// Safe the next card id while we still have our card in hand.
			long nextCardID = GetNextCard (1);
			// Make sure it is valid.
			if (nextCardID != -1L) {
				// Safety check to see if the next card id still is the same card (aka we only have 1 card in hand).
				if (nextCardID == GetCardViewById (activeCardID).getCardInfo ().getId ()) {
					nextCardID = -1L;
				} else {
					// Reserve the next card, so it gets selected the next time the user attempts to move through his scrolls.
					nextCardReserved = true;
					activeCardID = nextCardID;
				}
			}
		}

		public void DeselectCard() {
			handManager.DeselectCard ();
		}

		public CardView GetActiveCard() {
			return GetCardViewById (activeCardID);
		}

		public bool DoesSelectedSpellHaveCastButton() {
			CardView selectedCard = handManager.GetSelectedCard ();
			// Check if we have a valid selected card.
			if (selectedCard != null) {
				CardActivator cardActivator = (CardActivator)ReflectionsManager.GetValue (handManager, "cardActivator");
				GameObject playIcon = (GameObject)ReflectionsManager.GetValue (cardActivator, "playIcon");
				if (playIcon != null) {
					return true;
				}
			}
			return false;
		}

		public bool CompareSelectedCardToActiveCard() {
			// Check if the active card id matches with the currently selected card.
			return CompareSelectedCardToID (activeCardID);
		}

		public bool IsSelectedCardPlayableOnBoard() {
			// Check if the selected card is a creature/enchantment/structure or a spell that needs targeting on the board (aka doesn't have a cast button).
			CardType.Kind type = GetSelectedCardType ();
			if (type == CardType.Kind.CREATURE || 
				type == CardType.Kind.ENCHANTMENT || 
				type == CardType.Kind.STRUCTURE || 
				(type == CardType.Kind.SPELL && !DoesSelectedSpellHaveCastButton ())) {
				return true;
			}
			return false;
		}

		public void SetCardsGrayedOut(bool shouldGrayOut) {
			handManager.SetCardsGrayedOut (shouldGrayOut);
		}

		private bool CanSacrificeForResource (string resource, ResourceType[] resTypes) {
			foreach (ResourceType type in resTypes) {
				switch (resource) {
				case "growth":
					if (type == ResourceType.GROWTH) {
						return true;
					}
					break;
				case "order":
					if (type == ResourceType.ORDER) {
						return true;
					}
					break;
				case "energy":
					if (type == ResourceType.ENERGY) {
						return true;
					}
					break;
				case "decay":
					if (type == ResourceType.DECAY) {
						return true;
					}
					break;
				}
			}
			return false;
		}

		private CardType.Kind GetSelectedCardType() {
			return handManager.GetSelectedCard ().getCardInfo ().getPieceKind ();
		}

		private bool CompareSelectedCardToID(long id) {
			CardView selectedCard = handManager.GetSelectedCard ();
			if (selectedCard == null) {
				return false;
			}
			return (selectedCard.getCardInfo ().getId () == id);
		}

		private CardView GetCardViewById (long id) {
			return (CardView)getCardViewByIdMethodInfo.Invoke (handManager, new object[] { id });
		}

		private List<CardView> GetCardViewsInHand() {
			return handManager.GetCardViewsInHand ();
		}
	}
}

