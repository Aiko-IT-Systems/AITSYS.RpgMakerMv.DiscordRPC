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

namespace DiscordRPC.Exceptions
{
	/// <summary>
	/// A StringOutOfRangeException is thrown when the length of a string exceeds the allowed limit.
	/// </summary>
	public class StringOutOfRangeException : Exception
	{
		/// <summary>
		/// Maximum length the string is allowed to be.
		/// </summary>
		public int MaximumLength { get; private set; }

		/// <summary>
		/// Minimum length the string is allowed to be.
		/// </summary>
		public int MinimumLength { get; private set; }

		/// <summary>
		/// Creates a new string out of range exception with a range of min to max and a custom message
		/// </summary>
		/// <param name="message">The custom message</param>
		/// <param name="min">Minimum length the string can be</param>
		/// <param name="max">Maximum length the string can be</param>
		internal StringOutOfRangeException(string message, int min, int max) : base(message)
		{
			this.MinimumLength = min;
			this.MaximumLength = max;
		}

		/// <summary>
		/// Creates a new sting out of range exception with a range of min to max
		/// </summary>
		/// <param name="minumum"></param>
		/// <param name="max"></param>
		internal StringOutOfRangeException(int minumum, int max)
			: this($"Length of string is out of range. Expected a value between {minumum} and {max}", minumum, max) { }

		/// <summary>
		/// Creates a new sting out of range exception with a range of 0 to max
		/// </summary>
		/// <param name="max"></param>
		internal StringOutOfRangeException(int max)
			: this($"Length of string is out of range. Expected a value with a maximum length of {max}", 0, max) { }
	}
}
