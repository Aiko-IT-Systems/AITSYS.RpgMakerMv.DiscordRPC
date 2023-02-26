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
using System.Reflection;

using Newtonsoft.Json;

namespace DiscordRPC.Converters
{
	/// <summary>
	/// Converts enums with the <see cref="EnumValueAttribute"/> into Json friendly terms. 
	/// </summary>
	internal class EnumSnakeCaseConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => objectType.IsEnum;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			=> reader.Value == null ? null : this.TryParseEnum(objectType, (string)reader.Value, out var val) ? val : existingValue;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var enumtype = value.GetType();
			var name = Enum.GetName(enumtype, value);

			//Get each member and look for hte correct one
			var members = enumtype.GetMembers(BindingFlags.Public | BindingFlags.Static);
			foreach (var m in members)
			{
				if (m.Name.Equals(name))
				{
					var attributes = m.GetCustomAttributes(typeof(EnumValueAttribute), true);
					if (attributes.Length > 0)
					{
						name = ((EnumValueAttribute)attributes[0]).Value;
					}
				}
			}

			writer.WriteValue(name);
		}


		public bool TryParseEnum(Type enumType, string str, out object obj)
		{
			//Make sure the string isn;t null
			if (str == null)
			{
				obj = null;
				return false;
			}

			//Get the real type
			var type = enumType;
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				type = type.GetGenericArguments().First();

			//Make sure its actually a enum
			if (!type.IsEnum)
			{
				obj = null;
				return false;
			}


			//Get each member and look for hte correct one
			var members = type.GetMembers(BindingFlags.Public | BindingFlags.Static);
			foreach (var m in members)
			{
				var attributes = m.GetCustomAttributes(typeof(EnumValueAttribute), true);
				foreach (var a in attributes)
				{
					var enumval = (EnumValueAttribute)a;
					if (str.Equals(enumval.Value))
					{
						obj = Enum.Parse(type, m.Name, ignoreCase: true);

						return true;
					}
				}
			}

			//We failed
			obj = null;
			return false;
		}

	}
}
