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

#if NETSTANDARD1_1_OR_GREATER
#define USE_RUNTIME_INFO
#endif

using System;

using DiscordRPC.Logging;
#if USE_RUNTIME_INFO
using System.Runtime.InteropServices;
#endif

namespace DiscordRPC.Registry
{
	internal class UriSchemeRegister
	{
		/// <summary>
		/// The ID of the Discord App to register
		/// </summary>
		public string ApplicationID { get; set; }

		/// <summary>
		/// Optional Steam App ID to register. If given a value, then the game will launch through steam instead of Discord.
		/// </summary>
		public string SteamAppID { get; set; }

		/// <summary>
		/// Is this register using steam?
		/// </summary>
		public bool UsingSteamApp => !string.IsNullOrEmpty(this.SteamAppID) && this.SteamAppID != "";

		/// <summary>
		/// The full executable path of the application.
		/// </summary>
		public string ExecutablePath { get; set; }

		private readonly ILogger _logger;
		public UriSchemeRegister(ILogger logger, string applicationID, string steamAppID = null, string executable = null)
		{
			this._logger = logger;
			this.ApplicationID = applicationID.Trim();
			this.SteamAppID = steamAppID?.Trim();
			this.ExecutablePath = executable ?? GetApplicationLocation();
		}

		/// <summary>
		/// Registers the URI scheme, using the correct creator for the correct platform
		/// </summary>
		public bool RegisterUriScheme()
		{
			//Get the creator
			IUriSchemeCreator creator = null;
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32Windows:
				case PlatformID.Win32S:
				case PlatformID.Win32NT:
				case PlatformID.WinCE:
					this._logger.Trace("Creating Windows Scheme Creator");
					creator = new WindowsUriSchemeCreator(this._logger);
					break;

				case PlatformID.Unix:
#if USE_RUNTIME_INFO
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        _logger.Trace("Creating MacOSX Scheme Creator");
                        creator = new MacUriSchemeCreator(_logger);
                    }
                    else
                    {
#endif
					this._logger.Trace("Creating Unix Scheme Creator");
					creator = new UnixUriSchemeCreator(this._logger);
#if USE_RUNTIME_INFO
                    }
#endif
					break;

#if !USE_RUNTIME_INFO
				case PlatformID.MacOSX:
					this._logger.Trace("Creating MacOSX Scheme Creator");
					creator = new MacUriSchemeCreator(this._logger);
					break;
#endif

				default:
					this._logger.Error("Unkown Platform: {0}", Environment.OSVersion.Platform);
					throw new PlatformNotSupportedException("Platform does not support registration.");
			}

			//Regiser the app
			if (creator.RegisterUriScheme(this))
			{
				this._logger.Info("URI scheme registered.");
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the FileName for the currently executing application
		/// </summary>
		/// <returns></returns>
		public static string GetApplicationLocation() => Environment.ProcessPath;
	}
}
