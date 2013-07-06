using System;
using System.Reflection;
using System.Collections.Generic;
using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;

namespace ControllerSupport
{
	public class ControllerSupport : BaseMod {
		private BattleModeWrapper battleMode = null;
		private HandManagerWrapper handManager = null;
		private EndGameScreenWrapper endGameScreen = null;
		private LobbyMenuWrapper lobbyMenu = null;
		private LoginWrapper login = null;
		private ControllerKeyBindings controllerBindings;
		private const float axisDelay = .2f;
		private float battleModeAxisDeltaTime = 1000.0f;
		private float lobbyMenuAxisDeltaTime = 1000.0f;

		//initialize everything here, Game is loaded at this point
		public ControllerSupport () {
			Console.WriteLine("Loaded mod ControllerSupport");
			controllerBindings = new ControllerKeyBindings ();
		}

		public static string GetName () {
			return "ControllerSupport";
		}

		public static int GetVersion () {
			return 1;
		}

		public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version) {
			try {
				return new MethodDefinition[] { 
					scrollsTypes["BattleMode"].Methods.GetMethod("handleInput")[0],
					scrollsTypes["EndGameScreen"].Methods.GetMethod("OnGUI")[0],
					scrollsTypes["LobbyMenu"].Methods.GetMethod("Update")[0],
					scrollsTypes["LobbyMenu"].Methods.GetMethod("OnGUI")[0],
					scrollsTypes["Login"].Methods.GetMethod("OnGUI")[0],
				};
			}
			catch {
				return new MethodDefinition[] { };
			}
		}

		public override bool BeforeInvoke(InvocationInfo info, out object returnValue) {
			if (info.target.GetType () == typeof(BattleMode) && info.targetMethod.Equals ("handleInput")) {
				if (info.target.GetType () == typeof(BattleMode) && battleMode == null) {
					battleMode = new BattleModeWrapper ((BattleMode)info.target);
					handManager = new HandManagerWrapper (battleMode.GetHandManager());
				}
				battleMode.Validate ((BattleMode)info.target);
				handManager.Validate (battleMode.GetHandManager());
				HandleBattleModeControls ();
			} else if (info.target.GetType () == typeof(EndGameScreen) && info.targetMethod.Equals ("OnGUI")) {
				if (endGameScreen == null) {
					endGameScreen = new EndGameScreenWrapper ((EndGameScreen)info.target);
				}
				endGameScreen.Validate((EndGameScreen)info.target);
				HandleEndGameScreenControls ();
			} else if (info.target.GetType () == typeof(LobbyMenu) && info.targetMethod.Equals ("Update")) {
				if (lobbyMenu == null) {
					lobbyMenu = new LobbyMenuWrapper ();
				}
				HandleLobbyMenuControls ();
			} else if (info.target.GetType () == typeof(Login) && info.targetMethod.Equals ("OnGUI")) {
				if (login == null) {
					login = new LoginWrapper ((Login)info.target);
				}
				HandleLoginControls ();
			}
			returnValue = null;
			return false;
		}
		public override void AfterInvoke(InvocationInfo info, ref object returnValue) {
			if (info.target.GetType () == typeof(LobbyMenu) && info.targetMethod.Equals ("OnGUI")) {
				lobbyMenu.OnGUI ();
			}
			return;
		}

		private void HandleBattleModeControls() {
			// If the end screen is linked and it 'active', disable battle mode controls.
			if (endGameScreen.isActive ()) {
				return;
			}

			// Update the Axis delta time. (Used to control how often axis input is registered)
			battleModeAxisDeltaTime += Time.deltaTime;

			// Check if the Modifier Key is down
			if (Input.GetKey (controllerBindings.LB)) {
				// Sacrifice for Growth
				if (Input.GetKeyUp (controllerBindings.A)) {
					HandleBattleModeInput ("SacGrowth");
				}
				// Sacrifice for Order
				if (Input.GetKeyUp (controllerBindings.X)) {
					HandleBattleModeInput ("SacOrder");
				}
				// Sacrifice for Energy
				if (Input.GetKeyUp (controllerBindings.Y)) {
					HandleBattleModeInput ("SacEnergy");
				}
				// Sacrifice for Scrolls
				if (Input.GetKeyUp (controllerBindings.B)) {
					HandleBattleModeInput ("Cycle");
				}
			} else if (Input.GetKey(controllerBindings.BACK)) {
				// Hotkeys for sending some basic chat messages.
				if (Input.GetKeyUp (controllerBindings.A)) {
					battleMode.SendChatMessage("Hello and good luck.");
				}
				if (Input.GetKeyUp (controllerBindings.B)) {
					battleMode.SendChatMessage ("Good Game.");
				}
				if (Input.GetKeyUp (controllerBindings.X)) {
					battleMode.SendChatMessage ("Nice play!");
				}
				if (Input.GetKeyUp (controllerBindings.Y)) {
					battleMode.SendChatMessage (":)");
				}
			} else {
				// End Turn
				if (Input.GetKeyUp (controllerBindings.Y)) {
					HandleBattleModeInput ("EndTurn");
				}
				// Show Menu
				if (Input.GetKeyUp (controllerBindings.START)) {
					HandleBattleModeInput ("ShowMenu");
				}
				// Toggle Show Stats
				if (Input.GetKeyUp (controllerBindings.RIGHT_STICK_CLICK)) {
					HandleBattleModeInput ("ToggleUnitStats");
				}
				// Magnify Selected Card
				if (Input.GetKeyUp (controllerBindings.RB)) {
					HandleBattleModeInput ("MagnifySelected");
				}
				// Accept
				if (Input.GetKeyUp (controllerBindings.A)) {
					HandleBattleModeInput ("Accept");
				}
				// Cancel
				if (Input.GetKeyUp (controllerBindings.B)) {
					HandleBattleModeInput ("Cancel");
				}
				// Right
				if (battleModeAxisDeltaTime > axisDelay && Input.GetAxis (controllerBindings.LEFT_STICK_HORIZONTAL_AXIS) > .5f) {
					battleModeAxisDeltaTime = .0f;
					HandleBattleModeInput ("Right");
				}
				// Left
				if (battleModeAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_HORIZONTAL_AXIS) < -0.5f) {
					battleModeAxisDeltaTime = .0f;
					HandleBattleModeInput ("Left");
				}
				// Up
				if (battleModeAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) > .5f) {
					battleModeAxisDeltaTime = .0f;
					HandleBattleModeInput ("Up");
				}
				// Down
				if (battleModeAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) < -0.5f) {
					battleModeAxisDeltaTime = .0f;
					HandleBattleModeInput ("Down");
				}

				// Control Board (Xbox:Left Stick Click)
				if (Input.GetKeyDown (controllerBindings.LEFT_STICK_CLICK)) {
					HandleBattleModeInput ("ControlBoard");
				}
			}

			// Windows Specific Controller Controls
			if (OsSpec.getOS () == OSType.Windows) {}

			// OSX Specific Controller DPAD Controls (as they are buttons in OSX and an axis in Windows).
			if (OsSpec.getOS () == OSType.OSX) {
				// Right
				if (Input.GetKeyDown (controllerBindings.DPAD_RIGHT)) {
					HandleBattleModeInput ("Right");
				}
				// Left
				if (Input.GetKeyDown (controllerBindings.DPAD_LEFT)) {
					HandleBattleModeInput ("Left");
				}
				// Up
				if (Input.GetKeyDown (controllerBindings.DPAD_UP)) {
					HandleBattleModeInput ("Up");
				}
				// Down
				if (Input.GetKeyDown (controllerBindings.DPAD_DOWN)) {
					HandleBattleModeInput ("Down");
				}
			}
		}

		private void HandleEndGameScreenControls() {
			// Check to see if the end screen is already 'inited'.
			// This because the object itself is created at the start of the battle, and otherwise we would be able to instantly end the game.
			if (endGameScreen.isActive ()) {
				if (Input.GetKeyDown (controllerBindings.A)) {
					endGameScreen.ExitScreen ();
					Console.WriteLine ("ControllerSupport: Exiting EndGameScreen!");
				}
			}
		}

		private void HandleLobbyMenuControls () {
			// Update the Axis delta time. (Used to control how often axis input is registered)
			lobbyMenuAxisDeltaTime += Time.deltaTime;

			// Button Input
			if (Input.GetKeyUp(controllerBindings.RB)) {
				lobbyMenu.HandleInput ("NextScene");
			}
			if (Input.GetKeyUp(controllerBindings.LB)) {
				lobbyMenu.HandleInput ("PreviousScene");
			}
			if (Input.GetKeyUp (controllerBindings.A)) {
				lobbyMenu.HandleInput ("Accept");
			}
			if (Input.GetKeyUp (controllerBindings.B)) {
				lobbyMenu.HandleInput ("Cancel");
			}

			// Left Stick Directional Input
			if (lobbyMenuAxisDeltaTime > axisDelay && Input.GetAxis (controllerBindings.LEFT_STICK_HORIZONTAL_AXIS) > .5f) {
				lobbyMenuAxisDeltaTime = .0f;
				lobbyMenu.HandleInput ("Right");
			}
			if (lobbyMenuAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_HORIZONTAL_AXIS) < -0.5f) {
				lobbyMenuAxisDeltaTime = .0f;
				lobbyMenu.HandleInput ("Left");
			}
			if (lobbyMenuAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) > .5f) {
				lobbyMenuAxisDeltaTime = .0f;
				lobbyMenu.HandleInput ("Up");
			}
			if (lobbyMenuAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) < -0.5f) {
				lobbyMenuAxisDeltaTime = .0f;
				lobbyMenu.HandleInput ("Down");
			}

			// OSX Specific Controller DPAD Controls (as they are buttons in OSX and an axis in Windows).
			if (OsSpec.getOS () == OSType.OSX) {
				if (Input.GetKeyDown (controllerBindings.DPAD_RIGHT)) {
					lobbyMenu.HandleInput ("Right");
				}
				if (Input.GetKeyDown (controllerBindings.DPAD_LEFT)) {
					lobbyMenu.HandleInput ("Left");
				}
				if (Input.GetKeyDown (controllerBindings.DPAD_UP)) {
					lobbyMenu.HandleInput ("Up");
				}
				if (Input.GetKeyDown (controllerBindings.DPAD_DOWN)) {
					lobbyMenu.HandleInput ("Down");
				}
			}
		}

		private void HandleLoginControls () {
			if (Input.GetKeyUp (controllerBindings.A) || Input.GetKeyUp (controllerBindings.START)) {
				login.Login ();
			}
		}

		private void HandleBattleModeInput(String inputType) {
			bool controlBoard = battleMode.InControlOfBoard ();
			bool controlHand = !controlBoard;

			switch (inputType) {
			case "EndTurn":
				battleMode.EndTurnPressed ();
				// Always take control over the hand again on end turn.
				if (controlBoard) {
					battleMode.TakeControlOfHand ();
				}
				break;
			
			case "ToggleUnitStats":
				battleMode.ToggleUnitStats ();
				break;
			
			case "Accept":
				if (controlHand) {
					// Check if we are already selecting the currently active card.
					if (handManager.CompareSelectedCardToActiveCard ()) {
						// Check if we can directly cast the selected card (aka it has a 'cast' button in game).
						if (handManager.DoesSelectedSpellHaveCastButton ()) {
							handManager.UseActiveCard ("play");
						} else if (handManager.IsSelectedCardPlayableOnBoard ()) { // Check if the scroll is playable on board.
							battleMode.TakeControlOfBoard ();
						}
					} else {
						// Select the currently active card.
						battleMode.CardClicked (handManager.GetActiveCard (), 0);
					}
				} else if (controlBoard) {
					battleMode.TileClicked ();
					//battleMode.TakeControlOfHand (); <- this can only happen when we actually play something, else you cant move units
				}
				break;
			case "Cancel":
				if (controlHand) {
					// Deselect the selected card and all tiles if controlling the hand.
					handManager.DeselectCard ();
					battleMode.DeselectAllTiles ();
				} else if (controlBoard) {
					// If a unit is selected, unselect all units. Else go back to controlling the hand.
					if (battleMode.UnitSelectedOnBoard ()) {
						battleMode.DeselectAllTiles ();
					} else {
						battleMode.TakeControlOfHand ();
					}
				}
				break;
			case "Right":
				if (controlHand) {
					handManager.SelectNextCard (1);
					battleMode.CardClicked (handManager.GetActiveCard (), 0);
				} else if (controlBoard) {
					battleMode.MoveOnBoard (-1, 0);
				}
				break;
			case "Left":
				if (controlHand) {
					handManager.SelectNextCard(-1);
					battleMode.CardClicked (handManager.GetActiveCard (), 0);
				} else if (controlBoard) {
					battleMode.MoveOnBoard (1, 0);
				}
				break;
			case "Up":
				if (controlBoard) {
					battleMode.MoveOnBoard (0, -1);
				}
				break;
			case "Down":
				if (controlBoard) {
					battleMode.MoveOnBoard (0, 1);
				}
				break;

			case "ControlBoard":
				if (controlHand) {
					battleMode.TakeControlOfBoard ();
					battleMode.DeselectAllTiles ();
					handManager.DeselectCard ();
				}
				break;
			
			case "SacGrowth":
				handManager.UseActiveCard ("growth");
				break;
			case "SacOrder":
				handManager.UseActiveCard ("order");
				break;
			case "SacEnergy":
				handManager.UseActiveCard ("energy");
				break;
			case "Cycle":
				handManager.UseActiveCard ("cycle");
				break;
			
			case "MagnifySelected":
				handManager.MagnifySelected ();
				break;
			
			case "ShowMenu":
				battleMode.ShowMenu ();
				break;
			default:
				break;
			}
		}


	}
}

