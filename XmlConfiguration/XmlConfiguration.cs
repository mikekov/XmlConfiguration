using System;
using System.Collections;

namespace XmlConfiguration
{
	/// <summary>
	/// This is representation of an XML-based configuration.
	/// Use Root.YourElementName to access values.
	/// To create instance of XML configuration use ConfigurationReader.
	/// </summary>
	public interface IXmlConfiguration
	{
		/// <summary>
		/// Access to the XML settings.
		/// </summary>
		/// <returns>The configuration root element.</returns>
		dynamic Root { get; }

		/// <summary>
		/// Test if gven branch in XML settings exists.
		/// </summary>
		/// <param name="element">The configuration element.</param>
		/// <returns>'True' if branch is present in XML.</returns>
		bool Exists(dynamic element);

		/// <summary>
		/// Enumerator for all subelements of given configuration element.
		/// </summary>
		/// <param name="element">The configuration element.</param>
		/// <param name="deep">Flag to enumerate direct child elements (false - shallow)
		/// or recursively all descendant elements (true - deep).</param>
		/// <returns>IEnumerable object with 'Value' and 'Name' properties.</returns>
		IEnumerable GetElements(dynamic element, bool deep = false);

		/// <summary>
		/// Enumerator for attributes of given configuration element.
		/// </summary>
		/// <param name="element">The configuration element.</param>
		/// <returns>IEnumerable object with 'Value' and 'Name' attributes.</returns>
		IEnumerable GetAttributes(dynamic element);

		/// <summary>
		/// Extension point: type converter for custom types. Optional.
		/// If provided, custom converter will be invoked for all value/type combinations.
		/// It may then attempt conversion of some or all of them.
		/// </summary>
		ConfigurationConverterFunc Converter { get; set; }
	}

	/// <summary>
	/// Custom value converted.
	/// </summary>
	/// <param name="value">String representing value from configuration.</param>
	/// <param name="type">Desired destination type.</param>
	/// <param name="result">Converted value.</param>
	/// <returns>True if type has been converted, false otherwise.</returns>
	public delegate bool ConfigurationConverterFunc(string value, Type type, out object result);

	/// <summary>
	/// This exception is thrown when code tries to read non-nullable value from missing configuration branch.
	/// </summary>
	public class MissingConfigurationValueException : Exception
	{
		public MissingConfigurationValueException(string name)
			: base($"Missing configuration value '{name}'.")
		{ }
	}
}
