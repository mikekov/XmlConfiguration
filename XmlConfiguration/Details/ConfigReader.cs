using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XmlConfiguration.Details
{
	internal class ConfigReader : IXmlConfiguration
	{
		public dynamic Root
		{
			get { return cfg_; }
		}

		DynamicXml cfg_;

		public bool Exists(dynamic element)
		{
			if (element == null || element is Missing)
				return false;

			return true;
		}

		public IEnumerable GetElements(dynamic element, bool deep)
		{
			return DynamicXml.GetDescendants(element as DynamicXml, deep, Converter);
		}

		public IEnumerable GetAttributes(dynamic element)
		{
			return DynamicXml.GetAttributes(element as DynamicXml, Converter);
		}

		public ConfigurationConverterFunc Converter
		{
			get { return converter_; }
			set
			{
				converter_ = value;
				cfg_.Converter = converter_;
			}
		}

		ConfigurationConverterFunc converter_;

		public ConfigReader(string xml, bool path) // takes configuration xml file path or xml text
		{
			if (xml == null)
				throw new ArgumentNullException(nameof(xml)); //ArgumentException("Missing configuration: " + path);
			if (path && !File.Exists(xml))
				throw new ArgumentException($"Missing XML file: {path}.");

			cfg_ = new DynamicXml(path ? XElement.Load(xml) : XElement.Parse(xml));
		}

		public ConfigReader(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream)); //ArgumentException("Missing configuration: " + path);

			cfg_ = new DynamicXml(XElement.Load(stream));
		}
	}

	// wrapper for elements to return list of (Name, Value) pairs when iterating
	public class ConfigurationElement
	{
		public ConfigurationElement(XElement element, dynamic value)
		{
			Element = element;
			Value = value;
		}

		public string Name { get { return Element.Name.LocalName; } }

		public dynamic Value { get; private set; }

		public XElement Element { get; private set; }
	}

	// wrapper for element attributes to return list of (Name, Value) pairs when iterating
	public class ConfigurationAttribute
	{
		public ConfigurationAttribute(XAttribute attribute, dynamic value)
		{
			Attribute = attribute;
			Value = value;
		}

		public string Name { get { return Attribute.Name.LocalName; } }

		public dynamic Value { get; private set; }

		public XAttribute Attribute { get; private set; }
	}

}
