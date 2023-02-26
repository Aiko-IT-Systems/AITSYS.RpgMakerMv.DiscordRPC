// This file is part of an AITSYS project.
//
// Copyright (c) AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using DiscordRPC.Logging;

namespace DiscordRPC.Registry
{
	internal class WindowsUriSchemeCreator : IUriSchemeCreator
	{
		private readonly ILogger _logger;
		public WindowsUriSchemeCreator(ILogger logger)
		{
			this._logger = logger;
		}

		public bool RegisterUriScheme(UriSchemeRegister register)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
			{
				throw new PlatformNotSupportedException("URI schemes can only be registered on Windows");
			}

			//Prepare our location
			var location = register.ExecutablePath;
			if (location == null)
			{
				this._logger.Error("Failed to register application because the location was null.");
				return false;
			}

			//Prepare the Scheme, Friendly name, default icon and default command
			var scheme = $"discord-{register.ApplicationID}";
			var friendlyName = $"Run game {register.ApplicationID} protocol";
			var defaultIcon = location;
			var command = location;

			//We have a steam ID, so attempt to replce the command with a steam command
			if (register.UsingSteamApp)
			{
				//Try to get the steam location. If found, set the command to a run steam instead.
				var steam = this.GetSteamLocation();
				if (steam != null)
					command = string.Format("\"{0}\" steam://rungameid/{1}", steam, register.SteamAppID);

			}

			//Okay, now actually register it
			this.CreateUriScheme(scheme, friendlyName, defaultIcon, command);
			return true;
		}

		/// <summary>
		/// Creates the actual scheme
		/// </summary>
		/// <param name="scheme"></param>
		/// <param name="friendlyName"></param>
		/// <param name="defaultIcon"></param>
		/// <param name="command"></param>
		private void CreateUriScheme(string scheme, string friendlyName, string defaultIcon, string command)
		{
			using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Classes\\{scheme}"))
			{
				key.SetValue("", $"URL:{friendlyName}");
				key.SetValue("URL Protocol", "");

				using (var iconKey = key.CreateSubKey("DefaultIcon"))
					iconKey.SetValue("", defaultIcon);

				using var commandKey = key.CreateSubKey("shell\\open\\command");
				commandKey.SetValue("", command);
			}

			this._logger.Trace("Registered {0}, {1}, {2}", scheme, friendlyName, command);
		}

		/// <summary>
		/// Gets the current location of the steam client
		/// </summary>
		/// <returns></returns>
		public string GetSteamLocation()
		{
			using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
			return key == null ? null : key.GetValue("SteamExe") as string;
		}
	}
}
