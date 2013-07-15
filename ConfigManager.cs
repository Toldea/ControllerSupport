using System;
using System.IO;
using System.Collections.Generic;
using JsonFx.Json;

namespace ControllerSupport {
	public class ConfigManager {
		private ControllerKeyBindings controllerBindings;
		private string modFolder;
		private string configPath;
		private Dictionary<String, object> config;

		public ConfigManager (string modFolder, ControllerKeyBindings controllerBindings) {
			this.modFolder = modFolder;
			this.controllerBindings = controllerBindings;
			configPath = modFolder + Path.DirectorySeparatorChar + "config";

			// Create a config folder.
			if (!Directory.Exists (configPath + Path.DirectorySeparatorChar)) {
				Directory.CreateDirectory(configPath + Path.DirectorySeparatorChar);
			}
			// Load the config.json file.
			LoadConfig ();
		}

		public String GetPathToFile(String file) {
			return String.Format("{0}/config/{1}/{2}", modFolder, Path.DirectorySeparatorChar, file);
		}

		private void LoadConfig() {
			String path = GetPathToFile ("config.json");
			if( File.Exists(path) ) {
				String data = File.ReadAllText(path);
				config = new JsonReader().Read<Dictionary<String, object>>(data);
			} else {
				config = new Dictionary<String, object>();
			}
		}
		public void WriteConfig() {
			String data = new JsonWriter().Write(config);
			File.WriteAllText(GetPathToFile("config.json"), data);
		}

		private void Remove(String key) {
			if( !config.ContainsKey(key)) {
				return;
			}
			config.Remove(key);
		}
		private void Add(String key, object value) {
			config[key] = value;
		}

		public bool IsNewInstall() {
			return (!config.ContainsKey ("mod_version"));
		}
		public bool IsNewVersion(int version) {
			return config.ContainsKey("mod_version") ? ((int)config["mod_version"] < version) : true;
		}
		public void SetVersion(int version) {
			Add ("mod_version", version);
			WriteConfig ();
		}

		public void SetUsePS3(bool usePS3) {
			Add ("use_ps3", usePS3);
			controllerBindings.SetUsePS3(usePS3);
			WriteConfig ();
		}
		public bool UsingPS3() {
			return (config.ContainsKey ("use_ps3") ? (bool)config["use_ps3"] : false);
		}
	}
}

