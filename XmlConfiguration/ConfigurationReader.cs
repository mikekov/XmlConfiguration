using System.IO;

namespace XmlConfiguration
{
	/// <summary>
	/// Create instance of XML configuration reader.
	/// Pass file path or stream to Load methods.
	/// XML documents stored in a string can be used with Parse method.
	/// </summary>
	public static class ConfigurationReader
	{
		public static IXmlConfiguration Load(string path)
		{
			return new Details.ConfigReader(path, true);
		}

		public static IXmlConfiguration Load(Stream stream)
		{
			return new Details.ConfigReader(stream);
		}

		public static IXmlConfiguration Parse(string xml)
		{
			return new Details.ConfigReader(xml, false);
		}
	}
}
