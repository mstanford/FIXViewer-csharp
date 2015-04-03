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
	public class Writer
	{

		public static void Write(Message message, char delimiter, System.IO.TextWriter writer)
		{
			string sendingTime = message.SendingTime.ToString("yyyyMMdd-HH:mm:ss.fff");

			int checkSum = 1066 + ((int)delimiter * 7) + Sum(message.Version.ToString()) + Sum(message.MessageType) + Sum(message.SenderCompID) + Sum(message.TargetCompID) + Sum(message.MessageSequenceNumber.ToString()) + Sum(sendingTime);
			int bodyLength = 41 + message.MessageType.Length + message.SenderCompID.Length + message.TargetCompID.Length + message.MessageSequenceNumber.ToString().Length;
			for (int i = 0; i < message.Fields.Count; i++)
			{
				MessageField field = message.Fields[i];
				string tag = field.Tag.ToString();
				bodyLength += tag.Length + field.Value.Length + 2;
				checkSum += Sum(tag) + Sum(field.Value) + (int)'=' + (int)delimiter;
			}
			checkSum += Sum(bodyLength.ToString());

			writer.Write("8=");
			writer.Write(message.Version);
			writer.Write(delimiter);

			writer.Write("9=");
			writer.Write(bodyLength);
			writer.Write(delimiter);

			writer.Write("35=");
			writer.Write(message.MessageType);
			writer.Write(delimiter);

			writer.Write("34=");
			writer.Write(message.MessageSequenceNumber);
			writer.Write(delimiter);

			writer.Write("49=");
			writer.Write(message.SenderCompID);
			writer.Write(delimiter);

			writer.Write("56=");
			writer.Write(message.TargetCompID);
			writer.Write(delimiter);

			writer.Write("52=");
			writer.Write(sendingTime);
			writer.Write(delimiter);

			for (int i = 0; i < message.Fields.Count; i++)
			{
				MessageField field = message.Fields[i];
				writer.Write(field.Tag);
				writer.Write('=');
				writer.Write(field.Value);
				writer.Write(delimiter);
			}

			writer.Write("10=");
			writer.Write((checkSum % 256).ToString("000"));
			writer.Write(delimiter);

			writer.Flush();
		}

		public static string Write(Message message, char delimiter)
		{
			System.IO.TextWriter writer = new System.IO.StringWriter();
			Write(message, delimiter, writer);
			return writer.ToString();
		}

		private static int Sum(string s)
		{
			int sum = 0;
			char[] ach = s.ToCharArray();
			for (int i = 0; i < ach.Length; i++)
				sum += ach[i];
			return sum;
		}

	}
}
