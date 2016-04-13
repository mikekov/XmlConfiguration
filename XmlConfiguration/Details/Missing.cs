using System;
using System.Collections;
using System.Dynamic;

namespace XmlConfiguration.Details
{
	// Object representing missing branch in XML configuration.
	// Missing branches can be constucted, but cannot be read from.
	// Branches can be tested for existance with IXmlConfiguration.Exists call.
	class Missing : DynamicObject
	{
		public Missing(string name)
		{
			name_ = name;
		}

		string name_;

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = new Missing(binder.Name);
			return true;
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			result = null;
			return true;
		}

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			result = null;

			if (binder.Type.FullName == "System.Collections.IEnumerable")
				result = (IEnumerable)empty_;
			else if (binder.Type.IsValueType && binder.Type.Name != "Nullable`1")
				throw new MissingConfigurationValueException(name_);

			return true;
		}

		static readonly Array empty_ = new object[0];
	}
}
