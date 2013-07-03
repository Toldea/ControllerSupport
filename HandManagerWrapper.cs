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
			Console.WriteLine ("ControllerSupport: Creating HandManager Wrapper.");
			this.handManager = handManager;
			getCardViewByIdMethodInfo = handManager.GetType().GetMethod("GetCardViewById", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public void SelectNextCard(int displacement) {
			// If a next card was previously reserved, ignore this call.
			if (nextCardReserved) {
				nextCardReserved = false;
				return;
			}
			CardView nextCard = GetCardViewById(GetNextCard(displacement));
			// Select the first card in hand if we had no previously selected card.
			if (nextCard == null) {
				nextCard = handManager.GetCardViewsInHand () [0];
			}
			// If we have a valid card to select, save it as the active card.
			if (nextCard != null) {
				activeCardID = nextCard.getCardInfo ().getId ();
			}
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
			CardView magnifiedCard = (CardView)typeof(HandManager).GetField ("magnifiedCard", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (handManager);
			// If a magnifiedCard already exists, destroy it. Else check if we have a valid selected card and then magnify it.
			if (magnifiedCard != null ) {
				handManager.GetType ().GetMethod ("DestroyMagnified", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (handManager, new object[] {});
			} else if (handManager.GetSelectedCard () != null) {
				handManager.MagnifySelected ();
			}
		}

		public void UseActiveCard(string action) {
			CardActivator cardActivator = (CardActivator)typeof(HandManager).GetField ("cardActivator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (handManager);
			if (cardActivator != null) {
				// Safe the next card id while we still have our card in hand.
				long nextCardID = GetNextCard (1);
				// Safety check to see if the next card id still is the same card (aka we only have 1 card in hand).
				if (nextCardID == GetCardViewById (activeCardID).getCardInfo ().getId ()) {
					nextCardID = -1L;
				} else {
					// Reserve the next card, so it gets selected the next time the user attempts to move through his scrolls.
					nextCardReserved = true;
				}
				// Use the currently active card.
				cardActivator.GetType ().GetMethod ("iconClicked", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (cardActivator, new object[] { action });
				// If available set the next card as active.
				if (nextCardID != -1L) {
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
				CardActivator cardActivator = (CardActivator)typeof(HandManager).GetField ("cardActivator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (handManager);
				GameObject playIcon = (GameObject)typeof(CardActivator).GetField ("playIcon", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (cardActivator);
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

