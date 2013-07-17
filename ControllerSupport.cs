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
		private PopupsWrapper popups = null;
		private SettingsMenuWrapper settingsMenu = null;
		private GUIBattleModeMenuWrapper battleModeMenu = null;
		private ControllerKeyBindings controllerBindings;
		private ConfigManager configManager = null;
		private ConfigGUI configGUI = null;
		private const float axisDelay = .2f;
		private float battleModeAxisDeltaTime = 1000.0f;
		private float lobbyMenuAxisDeltaTime = 1000.0f;
		private float popupsAxisDeltaTime = 1000.0f;
		private float battleModeMenuAxisDeltaTime = 1000.0f;
		//private Tile selectedTile = null;

		//initialize everything here, Game is loaded at this point
		public ControllerSupport () {
			Console.WriteLine("Loaded mod ControllerSupport");
			controllerBindings = new ControllerKeyBindings ();
			configManager = new ConfigManager (this.OwnFolder (), controllerBindings);
			controllerBindings.SetUsePS3 (configManager.UsingPS3 ());
			popups = new PopupsWrapper (App.Popups);
		}

		private void ShowConfigGUI() {
			configGUI = new ConfigGUI (configManager, GetVersion ());
			configManager.SetVersion (GetVersion ());
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
					scrollsTypes["BattleMode"].Methods.GetMethod("OnGUI")[0],
					scrollsTypes["EndGameScreen"].Methods.GetMethod("OnGUI")[0],
					scrollsTypes["LobbyMenu"].Methods.GetMethod("Update")[0],
					scrollsTypes["LobbyMenu"].Methods.GetMethod("OnGUI")[0],
					scrollsTypes["LobbyMenu"].Methods.GetMethod("fadeOutScene")[0],
					scrollsTypes["Login"].Methods.GetMethod("OnGUI")[0],
					scrollsTypes["Popups"].Methods.GetMethod("OnGUI")[0],
					scrollsTypes["SettingsMenu"].Methods.GetMethod("OnGUI")[0],
					//scrollsTypes["Tile"].Methods.GetMethod("FixedUpdate")[0],
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
					handManager = new HandManagerWrapper (battleMode.GetHandManager ());
					battleModeMenu = new GUIBattleModeMenuWrapper (battleMode.GetMenu());
				}
				if (settingsMenu == null) {
					settingsMenu = new SettingsMenuWrapper (configManager, configGUI);
				}
				if (battleMode.Validate ((BattleMode)info.target)) {
					battleModeMenu.Initialize (battleMode.GetMenu ());
				}
				handManager.Validate (battleMode.GetHandManager ());

				if (popups.IsShowingPopup ()) {
					HandlePopupsControls ();
				} else {
					HandleBattleModeControls ();
					if (battleMode.ShowingMenu()) {
						HandleSettingsMenuControls ();
						HandleBattleModeMenuControls ();
					}
				}
				// Cache the currently selected tile (used to display a custom tinted hover indicator).
				//if (battleMode.InControlOfBoard ()) {
				//	selectedTile = battleMode.GetTile ();
				//}
			} else if (info.target.GetType () == typeof(EndGameScreen) && info.targetMethod.Equals ("OnGUI")) {
				if (endGameScreen == null) {
					endGameScreen = new EndGameScreenWrapper ((EndGameScreen)info.target);
				}
				endGameScreen.Validate ((EndGameScreen)info.target);
				HandleEndGameScreenControls ();
			} else if (info.target.GetType () == typeof(LobbyMenu) && info.targetMethod.Equals ("Update")) {
				if (lobbyMenu == null) {
					lobbyMenu = new LobbyMenuWrapper ();
					if (configGUI == null) {
						ShowConfigGUI ();
					}
				}
				// Let popups leech of LobbyMenu's update function.
				if (popups.IsShowingPopup ()) {
					HandlePopupsControls ();
				} else {
					HandleLobbyMenuControls ();
					if (lobbyMenu.GetCurrentSceneName () == "_Settings") {
						HandleSettingsMenuControls ();
					}
				}
			} else if (info.target.GetType () == typeof(LobbyMenu) && info.targetMethod.Equals ("fadeOutScene")) {
				if (lobbyMenu == null) {
					lobbyMenu = new LobbyMenuWrapper ();
				}
				lobbyMenu.RegisterCurrentScene ();
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
			} else if (info.target.GetType () == typeof(Popups) && info.targetMethod.Equals ("OnGUI")) {
				popups.OnGUI ();
			}/* else if (info.target.GetType () == typeof(Tile) && info.targetMethod.Equals ("FixedUpdate")) {
				// Custom, more clearly visable tile hover color.
				if (battleMode.InControlOfBoard() && selectedTile != null && selectedTile.Equals(info.target)) {
					Tile.SelectionType markerType = (Tile.SelectionType)typeof(Tile).GetField ("markerType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (selectedTile);
					if (markerType == Tile.SelectionType.Hover) {
						GameObject referenceTile = (GameObject)typeof(Tile).GetField ("referenceTile", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (selectedTile);
						referenceTile.renderer.material.color = new Color(.3f, 1f, .3f, .6f);
					}
				}
			}*/
			else if (info.target.GetType () == typeof(SettingsMenu) && info.targetMethod.Equals ("OnGUI")) {
				if (settingsMenu == null) {
					settingsMenu = new SettingsMenuWrapper (configManager, configGUI);
				}
				if (lobbyMenu.GetCurrentSceneName () == "_Settings") {
					settingsMenu.OnGUI (true);
				}
			} else if (info.target.GetType () == typeof(BattleMode) && info.targetMethod.Equals ("OnGUI")) {
				if (battleMode.ShowingMenu () && battleModeMenu.GetMenuState() != GUIBattleModeMenuWrapper.EMenuState.HELP) {
					if (settingsMenu == null) {
						settingsMenu = new SettingsMenuWrapper (configManager, configGUI);
					}
					if (!popups.IsShowingPopup ()) {
						bool drawHighlight = (battleModeMenu.GetMenuState() == GUIBattleModeMenuWrapper.EMenuState.CONTROL_SCHEME);
						settingsMenu.OnGUI (drawHighlight);
					}
				}
			}
			return;
		}

		private void HandleBattleModeControls() {
			// If the end screen is linked and it 'active', disable battle mode controls.
			if (endGameScreen.isActive ()) {
				return;
			}
			// Toggle Show Menu
			if (Input.GetKeyUp (controllerBindings.START)) {
				HandleBattleModeInput ("ShowMenu");
			}
			// Also disable battle mode controls when the menu is showing.
			if (battleMode.ShowingMenu()) {
				return;
			}

			// Update the Axis delta time. (Used to control how often axis input is registered)
			battleModeAxisDeltaTime += Time.deltaTime;

			// Check if the Modifier Key is down
			if (Input.GetKey (controllerBindings.LB)) {
				// Make sure we can't 'sac' when we ended our turn.
				if (!battleMode.TurnEnded ()) {
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
				}
			} else if (Input.GetKey (controllerBindings.BACK)) {
				if (Input.GetKeyUp (controllerBindings.A)) {
					battleMode.SendChatMessage ("Hello and good luck.");
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
				// Reshow the chat
				if (Input.GetKeyUp (controllerBindings.BACK)) {
					battleMode.ShowChat ();
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
				// Cycle through played cards
				if (Input.GetKeyUp(controllerBindings.X)) {
					battleMode.CycleShowRecentlyPlayedCards ();
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
				// Set the lobbyMenu back to the lobby screen, as the game will always put you back there after a game.
				lobbyMenu.SetCurrentScene ("_Lobby");

				if (Input.GetKeyDown (controllerBindings.A)) {
					endGameScreen.ExitScreen ();
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
			if (Input.GetKeyUp (controllerBindings.START)) {
				lobbyMenu.HandleInput ("Start");
			}
			if (Input.GetKeyUp (controllerBindings.Y)) {
				lobbyMenu.HandleInput ("ShowInvite");
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

		private void HandlePopupsControls () {
			// Update the Axis delta time. (Used to control how often axis input is registered)
			popupsAxisDeltaTime += Time.deltaTime;

			// Button Input
			if (Input.GetKeyUp (controllerBindings.A)) {
				popups.HandleInput ("Accept");
			}
			if (Input.GetKeyUp (controllerBindings.B)) {
				popups.HandleInput ("Cancel");
			}

			// Left Stick Directional Input
			if (popupsAxisDeltaTime > axisDelay && Input.GetAxis (controllerBindings.LEFT_STICK_HORIZONTAL_AXIS) > .5f) {
				popupsAxisDeltaTime = .0f;
				popups.HandleInput ("Right");
			}
			if (popupsAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_HORIZONTAL_AXIS) < -0.5f) {
				popupsAxisDeltaTime = .0f;
				popups.HandleInput ("Left");
			}
			if (popupsAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) > .5f) {
				popupsAxisDeltaTime = .0f;
				popups.HandleInput ("Up");
			}
			if (popupsAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) < -0.5f) {
				popupsAxisDeltaTime = .0f;
				popups.HandleInput ("Down");
			}

			// OSX Specific Controller DPAD Controls (as they are buttons in OSX and an axis in Windows).
			if (OsSpec.getOS () == OSType.OSX) {
				if (Input.GetKeyDown (controllerBindings.DPAD_RIGHT)) {
					popups.HandleInput ("Right");
				}
				if (Input.GetKeyDown (controllerBindings.DPAD_LEFT)) {
					popups.HandleInput ("Left");
				}
				if (Input.GetKeyDown (controllerBindings.DPAD_UP)) {
					popups.HandleInput ("Up");
				}
				if (Input.GetKeyDown (controllerBindings.DPAD_DOWN)) {
					popups.HandleInput ("Down");
				}
			}
		}

		private void HandleBattleModeMenuControls () {
			// Update the Axis delta time. (Used to control how often axis input is registered)
			battleModeMenuAxisDeltaTime += Time.deltaTime;
			if (battleMode.ShowingMenu ()) {
				// Button Input
				if (Input.GetKeyUp (controllerBindings.A)) {
					battleModeMenu.HandleInput ("Accept");
				}
				if (Input.GetKeyUp (controllerBindings.B)) {
					battleModeMenu.HandleInput ("Cancel");
				}

				if (battleModeMenuAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) > .5f) {
					battleModeMenuAxisDeltaTime = .0f;
					battleModeMenu.HandleInput ("Up");
				}
				if (battleModeMenuAxisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) < -0.5f) {
					battleModeMenuAxisDeltaTime = .0f;
					battleModeMenu.HandleInput ("Down");
				}

				// OSX Specific Controller DPAD Controls (as they are buttons in OSX and an axis in Windows).
				if (OsSpec.getOS () == OSType.OSX) {
					if (Input.GetKeyDown (controllerBindings.DPAD_UP)) {
						battleModeMenu.HandleInput ("Up");
					}
					if (Input.GetKeyDown (controllerBindings.DPAD_DOWN)) {
						battleModeMenu.HandleInput ("Down");
					}
				}
			}
		}

		private void HandleSettingsMenuControls () {
			if (Input.GetKeyUp (controllerBindings.A)) {
				settingsMenu.HandleInput ("Accept");
			}
		}

		private void HandleBattleModeInput(String inputType) {
			bool controlBoard = battleMode.InControlOfBoard ();
			bool controlHand = !controlBoard;

			switch (inputType) {
			case "EndTurn":
				if (!battleMode.TurnEnded ()) {
					battleMode.EndTurnPressed ();
					// Always take control over the hand again on end turn.
					if (controlBoard) {
						battleMode.TakeControlOfHand ();
					}
				}
				break;
			
			case "ToggleUnitStats":
				battleMode.ToggleUnitStats ();
				break;
			
			case "Accept":
				if (controlHand) {
					// Make sure it is still our turn.
					if (!battleMode.TurnEnded()) {
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
					} 
				} else if (controlBoard) {
					battleMode.TileClicked (handManager);
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
				handManager.UseActiveCard ("growth", battleMode.GetResourceTypes());
				break;
			case "SacOrder":
				handManager.UseActiveCard ("order", battleMode.GetResourceTypes());
				break;
			case "SacEnergy":
				handManager.UseActiveCard ("energy", battleMode.GetResourceTypes());
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

