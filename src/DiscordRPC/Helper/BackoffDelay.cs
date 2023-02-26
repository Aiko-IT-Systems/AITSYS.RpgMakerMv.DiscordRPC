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

namespace DiscordRPC.Helper
{

	internal class BackoffDelay
	{
		/// <summary>
		/// The maximum time the backoff can reach
		/// </summary>
		public int Maximum { get; private set; }

		/// <summary>
		/// The minimum time the backoff can start at
		/// </summary>
		public int Minimum { get; private set; }

		/// <summary>
		/// The current time of the backoff
		/// </summary>
		public int Current { get; private set; }

		/// <summary>
		/// The current number of failures
		/// </summary>
		public int Fails { get; private set; }

		/// <summary>
		/// The random generator
		/// </summary>
		public Random Random { get; set; }

		private BackoffDelay() { }
		public BackoffDelay(int min, int max) : this(min, max, new Random()) { }
		public BackoffDelay(int min, int max, Random random)
		{
			this.Minimum = min;
			this.Maximum = max;

			this.Current = min;
			this.Fails = 0;
			this.Random = random;
		}

		/// <summary>
		/// Resets the backoff
		/// </summary>
		public void Reset()
		{
			this.Fails = 0;
			this.Current = this.Minimum;
		}

		public int NextDelay()
		{
			//Increment the failures
			this.Fails++;

			double diff = (this.Maximum - this.Minimum) / 100f;
			this.Current = (int)Math.Floor(diff * this.Fails) + this.Minimum;


			return Math.Min(Math.Max(this.Current, this.Minimum), this.Maximum);
		}
	}
}
