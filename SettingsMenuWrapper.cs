using System;
using System.Reflection;
using UnityEngine;

namespace ControllerSupport {
	public class SettingsMenuWrapper {
		private GUISkin settingsSkin;
		private GUISkin regularUI;
		private ConfigManager configManager;
		private bool usePS3;

		public SettingsMenuWrapper (SettingsMenu settingsMenu, ConfigManager configManager) {
			this.configManager = configManager;
			usePS3 = configManager.UsingPS3 ();
			settingsSkin = (GUISkin)typeof(SettingsMenu).GetField ("settingsSkin", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (settingsMenu);
			regularUI = (GUISkin)typeof(SettingsMenu).GetField ("regularUI", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (settingsMenu);
		}

		public void OnGUI() {
			GUI.depth = 21;
			GUI.skin = settingsSkin;

			Rect rect = new Rect ((float)Screen.width * 0.5f - (float)Screen.height * 0.25f, (float)Screen.height * 0.7f, (float)Screen.height * 0.5f, (float)Screen.height * 0.2f);
			new ScrollsFrame (rect).AddNinePatch (ScrollsFrame.Border.DARK_CURVED, NinePatch.Patches.CENTER).Draw ();

			GUI.skin.label.fontSize = Screen.height / 32;
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			float num = (float)Screen.width * 0.5f;
			float num2 = (float)Screen.height * 0.72f;
			GUI.skin = regularUI;
			int fontSize = GUI.skin.label.fontSize;
			GUI.skin.label.fontSize = Screen.height / 32;
			GUI.Label (new Rect (num - num2 / 2f, rect.y + (float)Screen.height * 0.018f, num2, (float)Screen.height * 0.05f), "ControllerSupport");

			float height = .08f;
			GUI.skin.label.fontSize = fontSize;
			TextAnchor alignment = GUI.skin.label.alignment;
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;
			GUI.skin.label.fontSize = Screen.height / 40;
			GUI.Label (new Rect (num - (float)Screen.height * 0.21f, rect.y + (float)Screen.height * height, (float)Screen.height * 0.2f, (float)Screen.height * 0.04f), "Xbox 360");
			GUI.Label (new Rect (num + (float)Screen.height * 0.125f, rect.y + (float)Screen.height * height, (float)Screen.height * 0.2f, (float)Screen.height * 0.04f), "PS3");
			GUI.skin.label.alignment = alignment;

			Rect position = new Rect (num - (float)Screen.height * 0.12f, rect.y + (float)Screen.height * height, (float)Screen.height * 0.04f, (float)Screen.height * 0.04f + 2f);
			Rect position2 = new Rect (num + (float)Screen.height * 0.17f, rect.y + (float)Screen.height * height, (float)Screen.height * 0.04f, (float)Screen.height * 0.04f + 2f);
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
		}
	}
}

