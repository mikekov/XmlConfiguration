using System;
using System.Dynamic;

namespace XmlConfiguration.Details
{
	class DynamicConverter : DynamicObject
	{
		public DynamicConverter()
		{
			Value = null;
		}

		public DynamicConverter(string value)
		{
			Value = value;
		}

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			if (binder.Type.Name == "Nullable`1")
			{
				if (Value == string.Empty)
				{
					// missing value; nullable type can handle that
					result = null;
					return true;
				}
				// stripping 'nullable' part and passing base type to conversion routine
				return Convert(binder.Type.GetGenericArguments()[0], out result);
			}
			else
				return Convert(binder.Type, out result);
		}

		// Type conversions supported "out-of-the-box".
		bool Convert(Type type, out object result)
		{
			// try user-defined converter first, if any
			if (Converter != null && Converter(Value, type, out result))
				return true;

			if (type.FullName == "System.String")
				result = Value;
			else if (type.FullName == "System.TimeSpan")
				result = TimeSpan.Parse(Value);
			else
				result = System.Convert.ChangeType(Value, type);

			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return base.TryGetMember(binder, out result);
		}

		public string Value { get; internal set; }

		public ConfigurationConverterFunc Converter { get; set; }
	}
}
