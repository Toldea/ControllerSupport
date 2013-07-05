using System;
using System.Reflection;

namespace ControllerSupport {
	public class LoginWrapper {
		private Login login;
		public LoginWrapper (Login login) {
			this.login = login;
		}
		public void Login() {
			login.GetType ().GetMethod ("login", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (login, new object[] { });
		}
	}
}

