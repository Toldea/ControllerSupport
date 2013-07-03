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

		//initialize everything here, Game is loaded at this point
		public ControllerSupport () {
			Console.WriteLine("Loaded mod ControllerSupport");
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
				// Xbox Controls : Windows
				if (OsSpec.getOS () == OSType.Windows) {
				}

				// Xbox Controls : OSX
				if (OsSpec.getOS () == OSType.OSX) {
					// Check if the Modifier Key is down (Xbox:LB)
					if (Input.GetKey("joystick button 13")) {
						// Sacrifice for Growth (Xbox:LB + A)
						if (Input.GetKeyUp ("joystick button 16")) {
							HandleInput ("SacGrowth");
						}
						// Sacrifice for Order (Xbox:LB + X)
						if (Input.GetKeyUp ("joystick button 18")) {
							HandleInput ("SacOrder");
						}
						// Sacrifice for Energy (Xbox:LB + Y)
						if (Input.GetKeyUp ("joystick button 19")) {
							HandleInput ("SacEnergy");
						}
						// Sacrifice for Scrolls (Xbox:LB + B)
						if (Input.GetKeyUp ("joystick button 17")) {
							HandleInput ("Cycle");
						}
					} else {
						// End Turn (Xbox:Y)
						if (Input.GetKeyUp ("joystick button 19")) {
							HandleInput ("EndTurn");
						}
						// Show Menu (Xbox:Start)
						if (Input.GetKeyUp ("joystick button 9")) {
							HandleInput ("ShowMenu");
						}
						// Toggle Show Stats (Xbox:Right Stick Click)
						if (Input.GetKeyUp ("joystick button 12")) {
							HandleInput ("ToggleUnitStats");
						}
						// Magnify Selected Card (Xbox:RT)
						if (Input.GetKeyUp ("joystick button 14")) {
							HandleInput ("MagnifySelected");
						}
						// Accept (Xbox: A)
						if (Input.GetKeyUp ("joystick button 16")) {
							HandleInput ("Accept");
						}
						// Cancel (Xbox: B)
						if (Input.GetKeyUp ("joystick button 17")) {
							HandleInput ("Cancel");
						}
						// Right (Xbox:DPadR / LeftStickR)
						if (Input.GetKeyDown ("joystick button 8")) {
							HandleInput ("Right");
						}
						// Left (Xbox:DPadL / LeftStickL)
						if (Input.GetKeyDown ("joystick button 7")) {
							HandleInput ("Left");
						}
						// Up (Xbox:DPadU / LeftStickU)
						if (Input.GetKeyDown ("joystick button 5")) {
							HandleInput ("Up");
						}
						// Down (Xbox:DPadD / LeftStickD)
						if (Input.GetKeyDown ("joystick button 6")) {
							HandleInput ("Down");
						}
						// Control Board (Xbox:Left Stick Click)
						if (Input.GetKeyDown ("joystick button 11")) {
							HandleInput ("ControlBoard");
						}
					}
				}

				// PC Controls

			}
			returnValue = null;
			return false;
		}
		public override void AfterInvoke(InvocationInfo info, ref object returnValue) {
			return;
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
					// If controlling the board, go back to controlling the hand.
					battleMode.TakeControlOfHand ();
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

