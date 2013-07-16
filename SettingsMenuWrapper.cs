using System;
using System.Reflection;
using UnityEngine;

namespace ControllerSupport {
	public class SettingsMenuWrapper {
		private GUISkin settingsSkin;
		private GUISkin regularUI;
		private ConfigManager configManager;
		private ConfigGUI configGUI;
		private OSType osType;
		private bool usePS3;

		public SettingsMenuWrapper (ConfigManager configManager, ConfigGUI configGUI) {
			this.configManager = configManager;
			this.configGUI = configGUI;
			usePS3 = configManager.UsingPS3 ();
			osType = OsSpec.getOS ();
			settingsSkin = (GUISkin)Resources.Load ("_GUISkins/Settings");
			regularUI = (GUISkin)Resources.Load ("_GUISkins/RegularUI");
		}

		public void OnGUI() {
			GUI.depth = 21;
			GUI.skin = settingsSkin;

			// Background frame.
			//Rect rect = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.25f, (float)Screen.height * 0.7f, (float)Screen.height * 0.5f, (float)Screen.height * 0.2f);
			Rect rect = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.25f, (float)Screen.height * 0.65f, (float)Screen.height * 0.5f, (float)Screen.height * 0.2f);
			new ScrollsFrame (rect).AddNinePatch (ScrollsFrame.Border.DARK_CURVED, NinePatch.Patches.CENTER).Draw ();

			// Header
			GUI.skin.label.fontSize = Screen.height / 32;
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			float num = (float)Screen.width * 0.5f;
			float num2 = (float)Screen.height * 0.72f;
			GUI.skin = regularUI;
			int fontSize = GUI.skin.label.fontSize;
			GUI.skin.label.fontSize = Screen.height / 32;
			GUI.Label (new Rect (num - num2 / 2f, rect.y + (float)Screen.height * 0.018f, num2, (float)Screen.height * 0.05f), "ControllerSupport");

			float height = .09f;

			// OSX only Xbox or PS3 controller choice setting.
			if (osType == OSType.OSX) {
				height -= .02f;

				TextAnchor alignment = GUI.skin.label.alignment;
				GUI.skin.label.alignment = TextAnchor.MiddleLeft;
				GUI.skin.label.fontSize = Screen.height / 40;
				GUI.Label (new Rect (num - (float)Screen.height * 0.16f, rect.y + (float)Screen.height * height, (float)Screen.height * 0.2f, (float)Screen.height * 0.04f), "Xbox 360");
				GUI.Label (new Rect (num + (float)Screen.height * 0.075f, rect.y + (float)Screen.height * height, (float)Screen.height * 0.2f, (float)Screen.height * 0.04f), "PS3");
				GUI.skin.label.alignment = alignment;

				Rect position = new Rect (num - (float)Screen.height * 0.07f, rect.y + (float)Screen.height * height, (float)Screen.height * 0.04f, (float)Screen.height * 0.04f + 2f);
				Rect position2 = new Rect (num + (float)Screen.height * 0.12f, rect.y + (float)Screen.height * height, (float)Screen.height * 0.04f, (float)Screen.height * 0.04f + 2f);
				if (GUI.Button (position, string.Empty)) {
					App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
					usePS3 = false;
					configManager.SetUsePS3 (usePS3);
				}
				if (GUI.Button (position2, string.Empty)) {
					App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
					usePS3 = true;
					configManager.SetUsePS3 (usePS3);
				}
				if (!usePS3) {
					GUI.DrawTexture (position, ResourceManager.LoadTexture ("Arena/scroll_browser_button_cb_checked"));
					GUI.DrawTexture (position2, ResourceManager.LoadTexture ("Arena/scroll_browser_button_cb"));
				} else {
					GUI.DrawTexture (position, ResourceManager.LoadTexture ("Arena/scroll_browser_button_cb"));
					GUI.DrawTexture (position2, ResourceManager.LoadTexture ("Arena/scroll_browser_button_cb_checked"));
				}

				height += .055f;
			}

			int fontSize2 = GUI.skin.button.fontSize;
			GUI.skin.button.fontSize = Screen.height / 36;
			if (GUI.Button (new Rect (num - (float)Screen.height * 0.1f, rect.y + (float)Screen.height * height, (float)Screen.height * 0.2f, (float)Screen.height * 0.05f), "View Controls")) {
				App.AudioScript.PlaySFX ("Sounds/hyperduck/UI/ui_button_click");
				configGUI.ShowControlScheme ();
			}

			GUI.skin.label.fontSize = fontSize;
		}
	}
}

