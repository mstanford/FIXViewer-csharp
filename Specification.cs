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
	public class Specification
	{
		public readonly string Version;
		public List<FieldDef> Fields = new List<FieldDef>();
		public List<MessageFieldDef> Header = new List<MessageFieldDef>();
		public Dictionary<string, MessageDef> Messages = new Dictionary<string, MessageDef>();

		public Specification(string version)
			: this(version, typeof(Specification).Assembly.GetManifestResourceStream(typeof(Specification).Namespace + ".spec." + version + ".xml"))
		{
			Version = version;
		}

		public Specification(string version, System.IO.Stream stream)
		{
			Version = version;

			System.Xml.XmlDocument document = new System.Xml.XmlDocument();
			document.Load(stream);
			stream.Close();

			Dictionary<string, FieldDef> fieldsByName = new Dictionary<string, FieldDef>();
			List<MessageFieldDef> trailer = new List<MessageFieldDef>();
			foreach (System.Xml.XmlNode node in document.DocumentElement.ChildNodes)
			{
				if (node is System.Xml.XmlElement)
				{
					System.Xml.XmlElement element = (System.Xml.XmlElement)node;
					switch (element.Name)
					{
						case "fields":
							foreach (System.Xml.XmlNode node2 in element.ChildNodes)
							{
								if (node2 is System.Xml.XmlElement)
								{
									System.Xml.XmlElement element2 = (System.Xml.XmlElement)node2;
									switch (element2.Name)
									{
										case "field":
											FieldDef field = new FieldDef(element2);
											Fields.Add(field);
											fieldsByName.Add(field.Name, field);
											break;
									}
								}
							}
							break;
					}
				}
			}
			foreach (System.Xml.XmlNode node in document.DocumentElement.ChildNodes)
			{
				if (node is System.Xml.XmlElement)
				{
					System.Xml.XmlElement element = (System.Xml.XmlElement)node;
					switch (element.Name)
					{
						case "header":
							foreach (System.Xml.XmlNode node2 in element.ChildNodes)
							{
								if (node2 is System.Xml.XmlElement)
								{
									System.Xml.XmlElement element2 = (System.Xml.XmlElement)node2;
									switch (element2.Name)
									{
										case "field": Header.Add(new MessageFieldDef(element2, fieldsByName)); break;
										//default: throw new System.Exception("Invalid header.");
									}
								}
							}
							break;
						case "trailer":
							foreach (System.Xml.XmlNode node2 in element.ChildNodes)
							{
								if (node2 is System.Xml.XmlElement)
								{
									System.Xml.XmlElement element2 = (System.Xml.XmlElement)node2;
									switch (element2.Name)
									{
										case "field": trailer.Add(new MessageFieldDef(element2, fieldsByName)); break;
										//default: throw new System.Exception("Invalid trailer.");
									}
								}
							}
							break;
					}
				}
			}
			foreach (System.Xml.XmlNode node in document.DocumentElement.ChildNodes)
			{
				if (node is System.Xml.XmlElement)
				{
					System.Xml.XmlElement element = (System.Xml.XmlElement)node;
					switch (element.Name)
					{
						case "messages":
							foreach (System.Xml.XmlNode node2 in element.ChildNodes)
							{
								if (node2 is System.Xml.XmlElement)
								{
									System.Xml.XmlElement element2 = (System.Xml.XmlElement)node2;
									switch (element2.Name)
									{
										case "message": MessageDef message = new MessageDef(element2, Header, trailer, fieldsByName); Messages.Add(message.MsgType, message); break;
										default: throw new System.Exception("Invalid message.");
									}
								}
							}
							break;
						case "components": break;
					}
				}
			}
		}

		public string FieldName(int tag)
		{
			foreach (FieldDef field in Fields)
				if (field.Number.Equals(tag.ToString()))
					return field.Name;
			return "";
		}

		public class FieldDef
		{
			public readonly string Number;
			public readonly string Name;
			public readonly string Type;
			public readonly FieldValueDef[] Values;

			public FieldDef(System.Xml.XmlElement element)
			{
				List<FieldValueDef> values = new List<FieldValueDef>();
				foreach (System.Xml.XmlAttribute attribute in element.Attributes)
				{
					switch (attribute.Name)
					{
						case "number": Number = attribute.Value; break;
						case "name": Name = attribute.Value; break;
						case "type": Type = attribute.Value; break;
					}
				}
				foreach (System.Xml.XmlNode node2 in element.ChildNodes)
				{
					if (node2 is System.Xml.XmlElement)
					{
						System.Xml.XmlElement element2 = (System.Xml.XmlElement)node2;
						switch (element2.Name)
						{
							case "value": values.Add(new FieldValueDef(element2)); break;
						}
					}
				}
				if (Number == null || Name == null || Type == null) throw new System.Exception("Invalid FIX specification.");
				Values = values.ToArray();
			}
		}

		public class FieldValueDef
		{
			public readonly string Enum;
			public readonly string Description;

			public FieldValueDef(System.Xml.XmlElement element)
			{
				foreach (System.Xml.XmlAttribute attribute in element.Attributes)
				{
					switch (attribute.Name)
					{
						case "enum": Enum = attribute.Value; break;
						case "description": Description = attribute.Value; break;
					}
				}
			}
		}

		public class MessageDef
		{
			public readonly string Name;
			public readonly string MsgCat;
			public readonly string MsgType;
			public readonly MessageFieldDef[] Fields;

			public MessageDef(System.Xml.XmlElement element, List<MessageFieldDef> header, List<MessageFieldDef> trailer, Dictionary<string, FieldDef> definitions)
			{
				foreach (System.Xml.XmlAttribute attribute in element.Attributes)
				{
					switch (attribute.Name)
					{
						case "name": Name = attribute.Value; break;
						case "msgcat": MsgCat = attribute.Value; break;
						case "msgtype": MsgType = attribute.Value; break;
					}
				}
				List<MessageFieldDef> fields = new List<MessageFieldDef>();
				foreach (MessageFieldDef field in header)
					fields.Add(field);
				foreach (System.Xml.XmlNode node2 in element.ChildNodes)
				{
					if (node2 is System.Xml.XmlElement)
					{
						System.Xml.XmlElement element2 = (System.Xml.XmlElement)node2;
						switch (element2.Name)
						{
							case "field":
							case "group": if (fields == null) fields = new List<MessageFieldDef>(); fields.Add(new MessageFieldDef(element2, definitions)); break;
							default: break;
						}
					}
				}
				foreach (MessageFieldDef field in trailer)
					fields.Add(field);
				if (Name == null || MsgCat == null || MsgType == null) throw new System.Exception("Invalid FIX specification.");
				Fields = fields.ToArray();
			}
		}

		public class MessageFieldDef
		{
			public readonly string Name;
			public readonly string Required;
			public readonly FieldDef Definition;
			public readonly MessageFieldDef[] Fields;

			public MessageFieldDef(System.Xml.XmlElement element, Dictionary<string, FieldDef> definitions)
			{
				foreach (System.Xml.XmlAttribute attribute in element.Attributes)
				{
					switch (attribute.Name)
					{
						case "name": Name = attribute.Value; Definition = definitions[Name]; break;
						case "required": Required = attribute.Value; break;
					}
				}
				List<MessageFieldDef> fields = null;
				foreach (System.Xml.XmlNode node2 in element.ChildNodes)
				{
					if (node2 is System.Xml.XmlElement)
					{
						System.Xml.XmlElement element2 = (System.Xml.XmlElement)node2;
						switch (element2.Name)
						{
							case "field":
							case "group": if (fields == null) fields = new List<MessageFieldDef>(); fields.Add(new MessageFieldDef(element2, definitions)); break;
							default: break;
						}
					}
				}
				if (Name == null || Required == null) throw new System.Exception("Invalid FIX specification.");
				if (fields != null) Fields = fields.ToArray();
			}
		}

	}
}
