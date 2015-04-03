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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FIXViewer
{
	public partial class FormMain : Form
	{
		private StringBuilder _stringBuilder = new StringBuilder();
		private string[] _args;
		private char _delimiter;
		private List<Message> _messages = new List<Message>();
		private int _position = 0;
		private Dictionary<string, Specification> _specifications = new Dictionary<string, Specification>();

		public FormMain() { InitializeComponent(); }

		public FormMain(string[] args) : this() { _args = args; }

		private void FormMain_Load(object sender, EventArgs e)
		{
			if (_args != null)
			{
				switch (_args.Length)
				{
					case 0: break;
					case 1: FileOpen(_args[0]); break;
					default: throw new System.Exception("Too many arguments supplied.");
				}
			}
		}

		private void FileOpen()
		{
			Cursor = Cursors.WaitCursor;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == DialogResult.OK)
				FileOpen(openFileDialog.FileName);
			Cursor = Cursors.Default;
		}

		private void FileOpen(string path)
		{
			if (!System.IO.File.Exists(path))
				return;
			ReadAllMessages(path, _messages);
			_position = 0;
			string version = _messages[0].Version;
			if (!_specifications.ContainsKey(version))
				_specifications.Add(version, new Specification(version));
			if (_messages.Count == 0)
				this.toolStripButtonNext.Enabled = false;
			else
			{
				textBox.Text = Writer.Write(_messages[_position], _delimiter);
				if (_position == (_messages.Count - 1))
					toolStripButtonNext.Enabled = false;
			}
		}

		private void ReadAllMessages(string path, List<Message> messages)
		{
			System.IO.Stream stream = System.IO.File.OpenRead(path);
			_delimiter = ReadDelimiter(stream);
			System.IO.StreamReader reader = new StreamReader(stream);
			messages.Clear();
			while (reader.Peek() != -1)
				messages.Add(Reader.Read(reader, _stringBuilder, _delimiter));
			reader.Close();
		}

		private void Start()
		{
			if (_messages.Count > 0)
			{
				_position = 0;
				textBox.Text = Writer.Write(_messages[_position], _delimiter);
				if (!toolStripButtonNext.Enabled)
					toolStripButtonNext.Enabled = true;
			}
		}

		private void End()
		{
			if (_messages.Count > 0)
			{
				_position = _messages.Count - 1;
				textBox.Text = Writer.Write(_messages[_position], _delimiter);
			}
			if (_messages.Count > 0 && _position == (_messages.Count - 1))
			    toolStripButtonNext.Enabled = false;
		}

		private void Previous()
		{
			if (_messages.Count > 0 && _position > 0)
			{
				_position--;
				textBox.Text = Writer.Write(_messages[_position], _delimiter);
				if (!toolStripButtonNext.Enabled)
					toolStripButtonNext.Enabled = true;
			}
		}

		private void Next()
		{
			if (_messages.Count > 0 && _position < (_messages.Count - 1))
			{
				_position++;
				textBox.Text = Writer.Write(_messages[_position], _delimiter);
			}
			if (_messages.Count > 0 && _position == (_messages.Count - 1))
				toolStripButtonNext.Enabled = false;
		}

		private void textBox_TextChanged(object sender, EventArgs e)
		{
			Message message = _messages[_position];
			Specification specification = _specifications[message.Version];

			listView.Items.Clear();
			listView.Items.Add(new ListViewItem(new string[] { "8", "BeginString", message.Version }));
			listView.Items.Add(new ListViewItem(new string[] { "9", "BodyLength", message.BodyLength.ToString() }));
			listView.Items.Add(new ListViewItem(new string[] { "35", "MsgType", message.MessageType }));
			listView.Items.Add(new ListViewItem(new string[] { "49", "SenderCompID", message.SenderCompID }));
			listView.Items.Add(new ListViewItem(new string[] { "56", "TargetCompID", message.TargetCompID }));
			listView.Items.Add(new ListViewItem(new string[] { "34", "MsgSeqNum", message.MessageSequenceNumber.ToString() }));
			listView.Items.Add(new ListViewItem(new string[] { "52", "SendingTime", message.SendingTime.ToString("yyyyMMdd-HH:mm:ss.fff") }));
			for(int i = 0; i < message.Fields.Count; i++)
			{
				MessageField field = message.Fields[i];
				listView.Items.Add(new ListViewItem(new string[] { field.Tag.ToString(), specification.FieldName(field.Tag), field.Value }));
			}
			listView.Items.Add(new ListViewItem(new string[] { "10", "CheckSum", message.CheckSum.ToString("000") }));

			for (int i = 0; i < listView.Columns.Count - 1; i++)
				listView.Columns[i].Width = -1;
			listView.Columns[listView.Columns.Count - 1].Width = -2;
		}

		private void toolStripButtonOpen_Click(object sender, EventArgs e) { FileOpen(); }

		private void toolStripButtonNext_Click(object sender, EventArgs e) { Next(); }

		private void FormMain_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.O) { e.Handled = true; }
			else if (e.Control && e.KeyCode == Keys.Left) { e.Handled = true; Start(); }
			else if (e.KeyCode == Keys.Left) { e.Handled = true; Previous(); }
			else if (e.Control && e.KeyCode == Keys.Right) { e.Handled = true; End(); }
			else if (e.KeyCode == Keys.Right) { e.Handled = true; Next(); }
		}

		private void FormMain_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.O) { FileOpen(); }
		}

		private static char ReadDelimiter(System.IO.Stream stream)
		{
			System.IO.StreamReader reader = new StreamReader(stream);
			char[] charBuffer = new char[24];
			reader.Read(charBuffer, 0, 24);
			string s = new string(charBuffer);
			s = s.Substring(s.IndexOf("9=") - 1);
			stream.Position = 0;
			return s[0];
		}

	}
}