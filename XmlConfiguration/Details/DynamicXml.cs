using System;
using System.Collections;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace XmlConfiguration.Details
{
	class DynamicXml : DynamicConverter
	{
		public DynamicXml(XElement el)
		{
			if (el == null)
				throw new ArgumentNullException();

			element_ = el;
			Value = element_.Value;
		}

		// Access to XML subnodes or attributes.
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			// is it a subelement?
			var subelement = Element.Element(binder.Name);
			if (subelement != null)
			{
				result = new DynamicXml(subelement) { Converter = Converter };
				return true;
			}

			// if not a subelement, try attributes
			var attribute = Element.Attribute(binder.Name);
			if (attribute != null)
			{
				result = new DynamicConverter(attribute.Value) { Converter = Converter };
				return true;
			}

			// name not found
			result = new Missing(binder.Name);
			return true;
		}

		public override string ToString()
		{
			return Element.Value;
		}

		// Support for type conversions. Basic numerical types are covered in base class.
		// IEnumerable is also supported for nodes to traverse direct descendants.
		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			if (binder.Type.FullName == "System.Collections.IEnumerable")
			{
				result = GetDescendants(this, false, Converter);
				return true;
			}
			else
				return base.TryConvert(binder, out result);
		}

		// Access by index to elements in document order.
		// Useful for repeated elements.
		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			if (indexes.Length == 1 && indexes[0] is int)
			{
				var index = (int)indexes[0];
				if (index >= 0)
				{
					// this is slow...
					var el = element_.Elements().Skip(index).FirstOrDefault();

					if (el != null)
						result = new DynamicXml(el) { Converter = Converter };
					else
						result = new Missing($"{index}. element");

					return true;
				}
				else
				{
					// negative index throws exception;
					// alternatively access from the end of collection can be considered
					throw new IndexOutOfRangeException();
				}
			}

			return base.TryGetIndex(binder, indexes, out result);
		}

		public XElement Element { get { return element_; } }

		// Traversal support for nodes.
		public static IEnumerable GetDescendants(DynamicXml element, bool deep, ConfigurationConverterFunc converter)
		{
			if (element == null)
				return empty_;

			var elements = deep ? element.Element.Descendants() : element.Element.Elements();

			return elements.Select(
				d => new ConfigurationElement(d, new DynamicXml(d) { Converter = converter }));
		}

		// Traversal support for node attributes.
		public static IEnumerable GetAttributes(DynamicXml element, ConfigurationConverterFunc converter)
		{
			if (element == null)
				return empty_;

			return element.Element.Attributes().Select(
				a => new ConfigurationAttribute(a, new DynamicConverter { Value = a.Value, Converter = converter }));
		}

		XElement element_;
		static readonly Array empty_ = new object[0];
	}
}
