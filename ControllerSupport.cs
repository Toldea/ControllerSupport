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
		private ControllerKeyBindings controllerBindings;
		private const float axisDelay = .2f;
		private float axisDeltaTime = 1000.0f;

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
					// hook handleMessage in battlemode for adding input methods.
					scrollsTypes["BattleMode"].Methods.GetMethod("handleInput")[0],
					scrollsTypes["EndGameScreen"].Methods.GetMethod("OnGUI")[0],
					scrollsTypes["LobbyMenu"].Methods.GetMethod("menuGUI")[0],
				};
			}
			catch {
				return new MethodDefinition[] { };
			}
		}

		public override bool BeforeInvoke(InvocationInfo info, out object returnValue) {
			if (info.targetMethod.Equals("handleInput")) {
				if (battleMode == null) {
					battleMode = new BattleModeWrapper((BattleMode)info.target);
					handManager = new HandManagerWrapper(battleMode.GetHandManager());
				}
				HandleBattleModeControls ();
			} else if (info.targetMethod.Equals("OnGUI")) {
				if (endGameScreen == null) {
					endGameScreen = new EndGameScreenWrapper ((EndGameScreen)info.target);
				}
				HandleEndGameScreenControls();
			} else if (info.targetMethod.Equals("menuGUI")) {
				/*
				if (Input.GetKeyUp(controllerBindings.RB)) {
					Console.WriteLine("ControllerSupport: Pressing RB in menuGUI :D");
					LobbyMenu menu = App.LobbyMenu;
					if (menu != null) {
						typeof(LobbyMenu).GetField ("_sceneToLoad", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (menu, "_Lobby");
						App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
						menu.GetType().GetMethod("fadeOutScene", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(menu, new object[]{});
					}
				}*/
			}
			returnValue = null;
			return false;
		}
		public override void AfterInvoke(InvocationInfo info, ref object returnValue) {
			return;
		}

		private void HandleBattleModeControls() {
			// If the end screen is linked and it 'active', disable battle mode controls.
			if (endGameScreen != null && endGameScreen.isInited ()) {
				return;
			}
			// Update the Axis delta time. (Used to control how often axis input is registered)
			axisDeltaTime += Time.deltaTime;

			// Check if the Modifier Key is down
			if (Input.GetKey(controllerBindings.LB)) {
				// Sacrifice for Growth
				if (Input.GetKeyUp (controllerBindings.A)) {
					HandleInput ("SacGrowth");
				}
				// Sacrifice for Order
				if (Input.GetKeyUp (controllerBindings.X)) {
					HandleInput ("SacOrder");
				}
				// Sacrifice for Energy
				if (Input.GetKeyUp (controllerBindings.Y)) {
					HandleInput ("SacEnergy");
				}
				// Sacrifice for Scrolls
				if (Input.GetKeyUp (controllerBindings.B)) {
					HandleInput ("Cycle");
				}
			} else {
				// End Turn
				if (Input.GetKeyUp (controllerBindings.Y)) {
					HandleInput ("EndTurn");
				}
				// Show Menu
				if (Input.GetKeyUp (controllerBindings.START)) {
					HandleInput ("ShowMenu");
				}
				// Toggle Show Stats
				if (Input.GetKeyUp (controllerBindings.RIGHT_STICK_CLICK)) {
					HandleInput ("ToggleUnitStats");
				}
				// Magnify Selected Card
				if (Input.GetKeyUp (controllerBindings.RB)) {
					HandleInput ("MagnifySelected");
				}
				// Accept
				if (Input.GetKeyUp (controllerBindings.A)) {
					HandleInput ("Accept");
				}
				// Cancel
				if (Input.GetKeyUp (controllerBindings.B)) {
					HandleInput ("Cancel");
				}
				// Right
				if (axisDeltaTime > axisDelay && Input.GetAxis (controllerBindings.LEFT_STICK_HORIZONTAL_AXIS) > .5f) {
					axisDeltaTime = .0f;
					HandleInput ("Right");
				}
				// Left
				if (axisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_HORIZONTAL_AXIS) < -0.5f) {
					axisDeltaTime = .0f;
					HandleInput ("Left");
				}
				// Up
				if (axisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) > .5f) {
					axisDeltaTime = .0f;
					HandleInput ("Up");
				}
				// Down
				if (axisDeltaTime > axisDelay && Input.GetAxis(controllerBindings.LEFT_STICK_VERTICAL_AXIS) < -0.5f) {
					axisDeltaTime = .0f;
					HandleInput ("Down");
				}

				// Control Board (Xbox:Left Stick Click)
				if (Input.GetKeyDown (controllerBindings.LEFT_STICK_CLICK)) {
					HandleInput ("ControlBoard");
				}

			}

			// Windows Specific Controller Controls
			if (OsSpec.getOS () == OSType.Windows) {}

			// OSX Specific Controller DPAD Controls (as they are buttons in OSX and an axis in Windows).
			if (OsSpec.getOS () == OSType.OSX) {
				// Right
				if (Input.GetKeyDown (controllerBindings.DPAD_RIGHT)) {
					HandleInput ("Right");
				}
				// Left
				if (Input.GetKeyDown (controllerBindings.DPAD_LEFT)) {
					HandleInput ("Left");
				}
				// Up
				if (Input.GetKeyDown (controllerBindings.DPAD_UP)) {
					HandleInput ("Up");
				}
				// Down
				if (Input.GetKeyDown (controllerBindings.DPAD_DOWN)) {
					HandleInput ("Down");
				}
			}
		}

		private void HandleEndGameScreenControls() {
			// Check to see if the end screen is already 'inited'.
			// This because the object itself is created at the start of the battle, and otherwise we would be able to instantly end the game.
			if (endGameScreen.isInited ()) {
				if (Input.GetKeyDown (controllerBindings.A)) {
					endGameScreen.ExitScreen ();
				}
			}
		}

		private void HandleInput(String inputType) {
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
							Console.WriteLine ("ControllerSupport: Can play currently selected card on board!");
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
					// Deselect the selected card if controlling the hand.
					handManager.DeselectCard ();
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
				/*if (controlHand) {
					battleMode.TakeControlOfBoard ();
				} else */if (controlBoard) {
					battleMode.MoveOnBoard (0, -1);
				}
				break;
			case "Down":
				/*if (controlHand) {
					battleMode.TakeControlOfBoard ();
				} else */if (controlBoard) {
					battleMode.MoveOnBoard (0, 1);
				}
				break;

			case "ControlBoard":
				if (controlHand) {
					battleMode.TakeControlOfBoard ();
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

