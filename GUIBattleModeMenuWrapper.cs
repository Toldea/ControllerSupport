using System;
using System.Reflection;
using UnityEngine;

namespace ControllerSupport {
	public class GUIBattleModeMenuWrapper {
		private GUIBattleModeMenu battleModeMenu;
		private MethodInfo upMethodInfo;
		private MethodInfo downMethodInfo;
		private MethodInfo getNumMenuButtonsMethodInfo;
		private bool highlightingControlScheme = false;

		public GUIBattleModeMenuWrapper (GUIBattleModeMenu battleModeMenu) {
			Initialize (battleModeMenu);
		}
		public void Initialize(GUIBattleModeMenu battleModeMenu) {
			this.battleModeMenu = battleModeMenu;
			upMethodInfo = battleModeMenu.GetType ().GetMethod ("up", BindingFlags.NonPublic | BindingFlags.Instance);
			downMethodInfo = battleModeMenu.GetType ().GetMethod ("down", BindingFlags.NonPublic | BindingFlags.Instance);
			getNumMenuButtonsMethodInfo = battleModeMenu.GetType ().GetMethod ("getNumMenuButtons", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public void HandleInput(string inputType) {
			switch (inputType) {
			case "Accept":
				typeof(GUIBattleModeMenu).GetField ("menuPendingConfirm", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (battleModeMenu, true);
				break;
			case "Cancel":
				battleModeMenu.toggleMenu ();
				ResetHighlight ();
				break;
			case "Up":
				if (GetMenuState() == EMenuState.MAIN || GetMenuState() == EMenuState.CONTROL_SCHEME) {
					if (!highlightingControlScheme) {
						// Check if we are about to go over the regular last item in the normal menu, if so highlight our custom control scheme button instead.
						int menuCurrentSelection = (int)typeof(GUIBattleModeMenu).GetField ("menuCurrentSelection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleModeMenu);
						if (menuCurrentSelection == 0) {
							typeof(GUIBattleModeMenu).GetField ("menuCurrentSelection", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (battleModeMenu, -1);
							highlightingControlScheme = true;
						} else {
							upMethodInfo.Invoke (battleModeMenu, new object[] { });
						}
					} else {
						typeof(GUIBattleModeMenu).GetField ("menuCurrentSelection", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (battleModeMenu, getNumMenuButtonsMethodInfo.Invoke(battleModeMenu, new object[]{}));
						upMethodInfo.Invoke (battleModeMenu, new object[] { });
						highlightingControlScheme = false;
					}
				} else {
					upMethodInfo.Invoke (battleModeMenu, new object[] { });
				}
				break;
			case "Down":
				if (GetMenuState() == EMenuState.MAIN || GetMenuState() == EMenuState.CONTROL_SCHEME) {
					if (!highlightingControlScheme) {
						// Check if we are about to go over the regular last item in the normal menu, if so highlight our custom control scheme button instead.
						int menuCurrentSelection = (int)typeof(GUIBattleModeMenu).GetField ("menuCurrentSelection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleModeMenu);
						if (menuCurrentSelection + 1 == (int)getNumMenuButtonsMethodInfo.Invoke(battleModeMenu, new object[]{})) {
							typeof(GUIBattleModeMenu).GetField ("menuCurrentSelection", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (battleModeMenu, -1);
							highlightingControlScheme = true;
						} else {
							downMethodInfo.Invoke (battleModeMenu, new object[] { });
						}
					} else {
						downMethodInfo.Invoke (battleModeMenu, new object[] { });
						highlightingControlScheme = false;
					}
				} else {
					downMethodInfo.Invoke (battleModeMenu, new object[] { });
				}
				break;
			}
		}

		private void ResetHighlight() {
			typeof(GUIBattleModeMenu).GetField ("menuCurrentSelection", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (battleModeMenu, 0);
			highlightingControlScheme = false;
		}

		public EMenuState GetMenuState() {
			if (highlightingControlScheme) {
				return EMenuState.CONTROL_SCHEME;
			} else {
				return (EMenuState)typeof(GUIBattleModeMenu).GetField ("menuState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (battleModeMenu);
			}
		}

		public enum EMenuState {
			NONE,
			MAIN,
			QUIT,
			HELP,
			SETTINGS,
			CONTROL_SCHEME
		}
	}
}

