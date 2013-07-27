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
			upMethodInfo = ReflectionsManager.GetMethod (battleModeMenu, "up");
			downMethodInfo = ReflectionsManager.GetMethod (battleModeMenu, "down");
			getNumMenuButtonsMethodInfo = ReflectionsManager.GetMethod (battleModeMenu, "getNumMenuButtons");
		}

		public void HandleInput(string inputType) {
			switch (inputType) {
			case "Accept":
				ReflectionsManager.SetValue (battleModeMenu, "menuPendingConfirm", true);
				break;
			case "Cancel":
				battleModeMenu.toggleMenu ();
				ResetHighlight ();
				break;
			case "Up":
				if (GetMenuState() == EMenuState.MAIN || GetMenuState() == EMenuState.CONTROL_SCHEME) {
					if (!highlightingControlScheme) {
						// Check if we are about to go over the regular last item in the normal menu, if so highlight our custom control scheme button instead.
						int menuCurrentSelection = (int)ReflectionsManager.GetValue (battleModeMenu, "menuCurrentSelection");
						if (menuCurrentSelection == 0) {
							ReflectionsManager.SetValue (battleModeMenu, "menuCurrentSelection", -1);
							highlightingControlScheme = true;
						} else {
							upMethodInfo.Invoke (battleModeMenu, new object[] { });
						}
					} else {
						ReflectionsManager.SetValue(battleModeMenu, "menuCurrentSelection", getNumMenuButtonsMethodInfo.Invoke(battleModeMenu, new object[]{}));
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
						int menuCurrentSelection = (int)ReflectionsManager.GetValue(battleModeMenu, "menuCurrentSelection");
						if (menuCurrentSelection + 1 == (int)getNumMenuButtonsMethodInfo.Invoke(battleModeMenu, new object[]{})) {
							ReflectionsManager.SetValue(battleModeMenu, "menuCurrentSelection", -1);
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
			ReflectionsManager.SetValue (battleModeMenu, "menuCurrentSelection", 0);
			highlightingControlScheme = false;
		}

		public EMenuState GetMenuState() {
			if (highlightingControlScheme) {
				return EMenuState.CONTROL_SCHEME;
			} else {
				return (EMenuState)ReflectionsManager.GetValue (battleModeMenu, "menuState");
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

