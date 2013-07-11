using System;
using System.Reflection;

namespace ControllerSupport {
	public class GUIBattleModeMenuWrapper {
		private GUIBattleModeMenu battleModeMenu;
		private MethodInfo upMethodInfo;
		private MethodInfo downMethodInfo;
		public GUIBattleModeMenuWrapper (GUIBattleModeMenu battleModeMenu) {
			this.battleModeMenu = battleModeMenu;
			upMethodInfo = battleModeMenu.GetType ().GetMethod ("up", BindingFlags.NonPublic | BindingFlags.Instance);
			downMethodInfo = battleModeMenu.GetType ().GetMethod ("down", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public void HandleInput(string inputType) {
			switch (inputType) {
			case "Accept":
				typeof(GUIBattleModeMenu).GetField ("menuPendingConfirm", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (battleModeMenu, true);
				break;
			case "Cancel":
				battleModeMenu.toggleMenu ();
				break;
			case "Up":
				upMethodInfo.Invoke (battleModeMenu, new object[] { });
				break;
			case "Down":
				downMethodInfo.Invoke (battleModeMenu, new object[] { });
				break;
			}
		}
	}
}

