using System;
using System.Reflection;
using UnityEngine;

namespace ControllerSupport {
	public class BattleModeWrapper {
		private BattleMode battleMode;
		private HandManager handManager;
		private MethodInfo cardClickedMethodInfo;
		private MethodInfo toggleUnitStatsMethodInfo;
		private GUIBattleModeMenu gameMenu;
		private bool controlBoard = false;
		private int tileRow = 2;
		private int tileColumn = 1;

		public BattleModeWrapper (BattleMode battleMode) {
			Console.WriteLine ("ControllerSupport: Creating BattleMode Wrapper.");
			this.battleMode = battleMode;
			this.handManager = GetHandManager ();
			cardClickedMethodInfo = battleMode.GetType().GetMethod("cardClicked", BindingFlags.NonPublic | BindingFlags.Instance);
			toggleUnitStatsMethodInfo = battleMode.GetType().GetMethod("toggleUnitStats", BindingFlags.NonPublic | BindingFlags.Instance);
			gameMenu = (GUIBattleModeMenu)typeof(BattleMode).GetField ("menu", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
		}

		public void CardClicked(CardView cardView, int mouseButton) {
			cardClickedMethodInfo.Invoke (battleMode, new object[] { cardView, mouseButton });
		}

		public void EndTurnPressed() {
			battleMode.endturnPressed ();
		}

		public void ShowMenu() {
			gameMenu.toggleMenu ();
		}

		public void ToggleUnitStats() {
			toggleUnitStatsMethodInfo.Invoke (battleMode, new object[] { });
		}

		public HandManager GetHandManager() {
			return (HandManager)typeof(BattleMode).GetField ("handManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleMode);
		}

		public bool InControlOfBoard() {
			return controlBoard;
		}
		public bool InControlOfHand() {
			return !controlBoard;
		}
		public void TakeControlOfBoard() {
			controlBoard = true;
			TileOver ();
			handManager.SetCardsGrayedOut (true);
		}
		public void TakeControlOfHand() {
			controlBoard = false;
			TileOut ();
			handManager.SetCardsGrayedOut (false);
		}

		public void MoveOnBoard(int x, int y) {
			// Are we allowed to move on the right board (disabled for placing units / structures).
			bool allowRightBoardMovement = true;

			// Get the selected card.
			CardView card = handManager.GetSelectedCard ();
			if (card != null) {
				// Get the selected card type.
				CardType.Kind type = handManager.GetSelectedCard ().getCardInfo ().getPieceKind ();
				if (type == CardType.Kind.CREATURE || type == CardType.Kind.STRUCTURE) {
					allowRightBoardMovement = false;
				}
			}

			TileOut (); // Fade out the last tile.
			// Add the displacement.
			tileColumn += x;
			tileRow += y;
			// Make sure the row/column stays within the board.
			if (allowRightBoardMovement) {
				if (tileColumn < -3) tileColumn = -3;
			} else {
				if (tileColumn < 0) tileColumn = 0;
			}
			if (tileColumn > 2) tileColumn = 2;
			if (tileRow < 0) tileRow = 0;
			if (tileRow > 4) tileRow = 4;

			Console.WriteLine ("ControllerSupport: Moved to tile: [" + tileColumn + ", " + tileRow + "]");

			TileOver (); // Fade in the new tile.
		}

		public void TileClicked () {
			Tile t = GetTile ();
			if (t != null) {
				battleMode.tileClicked(t);
			}
		}

		private void TileOver() {
			Tile t = GetTile ();
			if (t != null) {
				battleMode.tileOver (t.gameObject, tileRow, tileColumn);
			}
		}
		private void TileOut() {
			Tile t = GetTile ();
			if (t != null) {
				battleMode.tileOut (t.gameObject, tileRow, tileColumn);
			}
		}

		private Tile GetTile() {
			// Get the colors for the left and right side.
			TileColor leftColor = (battleMode.isLeftColor(TileColor.black)) ? TileColor.black : TileColor.white;
			TileColor rightColor = (leftColor == TileColor.black) ? TileColor.white : TileColor.black;
			TileColor color = (tileColumn > -1) ? leftColor : rightColor;
			// Convert the tile column to the right format (2 1 0 - 0 1 2).
			int convertedColumn = tileColumn;
			if (color == rightColor) {
				convertedColumn = Math.Abs (tileColumn + 1);
			}
			return battleMode.getTile (color, tileRow, convertedColumn);
		}
	}
}

