// ------------------------------------------------------------------------
// 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org/>
// 
// ------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace FIXViewer
{
	public class Reader
	{

		public static Message Read(System.IO.TextReader reader, StringBuilder stringBuilder, char delimiter)
		{
			MessageField field8 = ReadTagValuePair(reader, stringBuilder, delimiter, 8);
			string version = field8.Value;
			MessageField field9 = ReadTagValuePair(reader, stringBuilder, delimiter, 9);
			MessageField field35 = ReadTagValuePair(reader, stringBuilder, delimiter, 35);
			List<MessageField> fields = new List<MessageField>();
			MessageField field49 = null;
			MessageField field56 = null;
			MessageField field34 = null;
			MessageField field52 = null;
			while (reader.Peek() != -1)
			{
				MessageField field = ReadTagValuePair(reader, stringBuilder, delimiter);
				switch(field.Tag)
				{
					case 8: throw new System.Exception("Invalid message."); //TODO make this a FIXException
					case 10:
						if (field34 == null || field49 == null || field52 == null || field56 == null)
							throw new System.Exception("Invalid message."); //TODO make this a FIXException
						return new Message(
							version, 
							Int32.Parse(field9.Value), 
							field35.Value, 
							field49.Value, 
							field56.Value, 
							Int32.Parse(field34.Value), 
							ParseTimestamp(field52.Value), 
							Int32.Parse(field.Value),
							new List<MessageField>(fields.ToArray()));
					case 34: field34 = field; break;
					case 49: field49 = field; break;
					case 52: field52 = field; break;
					case 56: field56 = field; break;
					default: fields.Add(field); break;
				}
			}
			//TODO make this a FIXException
			throw new System.Exception("Invalid message.");
		}

		public static Message Read(string s, StringBuilder stringBuilder, char delimiter)
		{
			System.IO.StringReader reader = new System.IO.StringReader(s);
			Message message = Read(reader, stringBuilder, delimiter);
			reader.Close();
			return message;
		}

		private static MessageField ReadTagValuePair(System.IO.TextReader reader, StringBuilder stringBuilder, char delimiter, int expectedTag)
		{
			int tag = Int32.Parse(ReadString(reader, stringBuilder, '='));
			MessageField field = new MessageField(tag, ReadString(reader, stringBuilder, delimiter));
			if (field.Tag != expectedTag)
				throw new FIXException(expectedTag, field.Tag);
			return field;
		}

		public class FIXException : System.Exception
		{
			public FIXException(int expectedTag, int actualTag)
				: base("Invalid tag.  Expected " + expectedTag + " but was " + actualTag + ".")
			{
			}
		}

		public static MessageField ReadTagValuePair(System.IO.TextReader reader, StringBuilder stringBuilder, char delimiter)
		{
			return new MessageField(Int32.Parse(ReadString(reader, stringBuilder, '=')), ReadString(reader, stringBuilder, delimiter));
		}

		private static string ReadString(System.IO.TextReader reader, StringBuilder stringBuilder, char delimiter)
		{
			stringBuilder.Length = 0;
			while (reader.Peek() != -1)
			{
				char ch = (char)reader.Read();
				if (ch == delimiter)
					return stringBuilder.ToString();
				else
					stringBuilder.Append(ch);
			}
			//TODO make this a FIXException
			throw new System.Exception("Unterminated field: " + stringBuilder.ToString());
		}

		public static DateTime ParseTimestamp(string s)
		{
			return DateTime.ParseExact(
				s, 
				(s.IndexOf('.') == -1) ? "yyyyMMdd-HH:mm:ss" : "yyyyMMdd-HH:mm:ss.fff", 
				System.Globalization.CultureInfo.InvariantCulture);
		}

	}
}
