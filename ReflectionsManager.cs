using System;
using System.Collections.Generic;
using System.Reflection;

namespace ControllerSupport {
	public class ReflectionsManager {

		public static MethodInfo GetMethod(object obj, string methodName) {
			MethodInfo methodInfo = null;
			try {
				methodInfo = obj.GetType ().GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			} 
			catch {
				Console.WriteLine ("ControllerSupport: ReflectionsManager: GetMethod: Error: couldn't retreive method with name: " + obj.GetType().ToString() + "." + methodName);
			}
			return methodInfo;
		}

		public static Object GetValue(object obj, string variableName) {
			Object value = null;
			try {
				value = obj.GetType().GetField(variableName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue (obj);
			} 
			catch {
				Console.WriteLine ("ControllerSupport: ReflectionsManager: GetValue: Error: couldn't retreive value with name: " + obj.GetType().ToString() + "." + variableName);
			}
			return value;
		}

		public static void SetValue<Value>(object obj, string variableName, Value value) {
			try {
				obj.GetType().GetField(variableName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue (obj, value);
			} 
			catch {
				Console.WriteLine ("ControllerSupport: ReflectionsManager: SetValue: Error: couldn't set value with name: " + obj.GetType().ToString() + "." + variableName);
			}

		}
	}
}

