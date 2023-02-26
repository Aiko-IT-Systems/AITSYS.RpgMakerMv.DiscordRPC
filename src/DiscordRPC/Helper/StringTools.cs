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
using System.Linq;
using System.Text;

namespace DiscordRPC.Helper
{
	/// <summary>
	/// Collectin of helpful string extensions
	/// </summary>
	public static class StringTools
	{
		/// <summary>
		/// Will return null if the string is whitespace, otherwise it will return the string. 
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <returns>Null if the string is empty, otherwise the string</returns>
		public static string GetNullOrString(this string str) => str.Length == 0 || string.IsNullOrEmpty(str.Trim()) ? null : str;

		/// <summary>
		/// Does the string fit within the given amount of bytes? Uses UTF8 encoding.
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <param name="bytes">The maximum number of bytes the string can take up</param>
		/// <returns>True if the string fits within the number of bytes</returns>
		public static bool WithinLength(this string str, int bytes) => str.WithinLength(bytes, Encoding.UTF8);

		/// <summary>
		/// Does the string fit within the given amount of bytes?
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <param name="bytes">The maximum number of bytes the string can take up</param>
		/// <param name="encoding">The encoding to count the bytes with</param>
		/// <returns>True if the string fits within the number of bytes</returns>
		public static bool WithinLength(this string str, int bytes, Encoding encoding) => encoding.GetByteCount(str) <= bytes;


		/// <summary>
		/// Converts the string into UpperCamelCase (Pascal Case).
		/// </summary>
		/// <param name="str">The string to convert</param>
		/// <returns></returns>
		public static string ToCamelCase(this string str)
		{
			return str?.ToLowerInvariant()
				.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => char.ToUpper(s[0]) + s[1..])
				.Aggregate(string.Empty, (s1, s2) => s1 + s2);
		}

		/// <summary>
		/// Converts the string into UPPER_SNAKE_CASE
		/// </summary>
		/// <param name="str">The string to convert</param>
		/// <returns></returns>
		public static string ToSnakeCase(this string str)
		{
			if (str == null) return null;
			var concat = string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()).ToArray());
			return concat.ToUpperInvariant();
		}
	}
}
