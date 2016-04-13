using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class UnitTests
	{
		[TestMethod]
		public void TestParseEmpty()
		{
			var config = XmlConfiguration.ConfigurationReader.Parse("<root></root>");
			Assert.IsNotNull(config);
		}

		[TestMethod]
		public void TestParse()
		{
			var config = CreateTestConfig();
			Assert.IsNotNull(config);
		}

		[TestMethod]
		public void TestMissing()
		{
			var config = CreateTestConfig();
			var el = config.Root.some_element_not_there;
			Assert.IsFalse(config.Exists(el));
			var el2 = config.Root.some_element_not_there.count;
			Assert.IsFalse(config.Exists(el2));
		}

		[TestMethod]
		public void TestPresent()
		{
			var config = CreateTestConfig();
			var el = config.Root.path;
			Assert.IsTrue(config.Exists(el));
			var el2 = config.Root.list.item;
			Assert.IsTrue(config.Exists(el2));
		}

		[TestMethod]
		public void TestString()
		{
			var config = CreateTestConfig();
			string el = config.Root.path;
			Assert.AreEqual(@"c:\log", el);
		}

		[TestMethod]
		public void TestInt()
		{
			var config = CreateTestConfig();
			int el = config.Root.max;
			Assert.AreEqual(123, el);
		}

		[TestMethod]
		[ExpectedException(typeof(FormatException), "bad: fraction as int accepted")]
		public void TestBadInt()
		{
			var config = CreateTestConfig();
			// parsing string with double as int should fail
			int el = config.Root.factor;
		}

		[TestMethod]
		[ExpectedException(typeof(XmlConfiguration.MissingConfigurationValueException))]
		public void TestReadMissingInt()
		{
			var config = CreateTestConfig();
			// implicit cast to int on missing element should fail
			int el = config.Root.does_not_exist;
		}

		[TestMethod]
		public void TestReadMissingString()
		{
			var config = CreateTestConfig();
			// implicit cast to string on missing element succeeds with null
			string el = config.Root.does_not_exist;
			Assert.IsNull(el);
		}

		[TestMethod]
		public void TestReadMissingNullable()
		{
			var config = CreateTestConfig();
			// implicit cast to 'int?'
			int? el = config.Root.does_not_exist;
			Assert.IsFalse(el.HasValue);
		}

		[TestMethod]
		public void TestDouble()
		{
			var config = CreateTestConfig();
			double el = config.Root.factor;
			Assert.AreEqual(0.333, el);
		}

		[TestMethod]
		public void TestBool()
		{
			var config = CreateTestConfig();
			bool el = config.Root.enable;
			Assert.AreEqual(true, el);
			bool el2 = config.Root.disable;
			Assert.AreEqual(false, el2);
		}

		[TestMethod]
		public void TestTimeSpan()
		{
			var config = CreateTestConfig();
			TimeSpan el = config.Root.span;
			Assert.AreEqual(TimeSpan.FromSeconds(59.9), el);
		}

		[TestMethod]
		public void TestCustomType()
		{
			var config = CreateTestConfig();
			config.Converter = ColorConverter;
			System.Windows.Media.Color el = config.Root.color;
			Assert.AreEqual(System.Windows.Media.Colors.Red, el);
		}

		[TestMethod]
		public void TestCustomBool()
		{
			var config = CreateTestConfig();
			config.Converter = BoolConverter;
			bool el = config.Root.flag;
			Assert.AreEqual(true, el);
			bool el2 = config.Root.enable;
			Assert.AreEqual(true, el2);
		}

		[TestMethod]
		public void TestListDepth()
		{
			var config = CreateTestConfig();

			var deep = Count(config.GetElements(config.Root, deep: true));
			var shallow = Count(config.GetElements(config.Root, deep: false));

			Assert.IsTrue(deep > shallow);
		}

		[TestMethod]
		public void TestListDepth2()
		{
			var config = CreateTestConfig();

			var deep = Count(config.GetElements(config.Root, true));
			IEnumerable e = config.Root;
			var shallow = Count(e);

			Assert.IsTrue(deep > shallow);
		}

		[TestMethod]
		public void TestList3()
		{
			var config = CreateTestConfig();

			IEnumerable list = config.GetElements(config.Root);
			var it = list.GetEnumerator();
			foreach (var el in config.Root.list)
			{
				it.MoveNext();
				Assert.AreEqual(el.Value as string, it.Current as string);
			}
		}

		static int Count(IEnumerable e)
		{
			int count = 0;
			foreach (var el in e)
				count++;
			return count;
		}

		static bool BoolConverter(string value, Type type, out object result)
		{
			if (type.FullName == "System.Boolean")
			{
				switch (value.ToLower())
				{
					case "on":
						result = true;
						return true;
					case "off":
						result = false;
						return true;
					default:
						result = null;
						return false;		// use default converter
				}
			}
			else
			{
				result = null;
				return false;
			}
		}

		static bool ColorConverter(string value, Type type, out object result)
		{
			if (type.FullName == "System.Windows.Media.Color")
			{
				result = System.Windows.Media.ColorConverter.ConvertFromString(value);
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		[TestMethod]
		public void TestDefaults()
		{
			var config = CreateTestConfig();
			var el = (int?)config.Root.missing_element ?? 100;
			Assert.AreEqual(100, el);
			var el2 = (int?)config.Root.max ?? 0;
			Assert.AreEqual(123, el2);
		}

		[TestMethod]
		public void TestList()
		{
			var config = CreateTestConfig();
			int count = 0;
			foreach (var el in config.GetElements(config.Root.list))
			{
				Assert.AreEqual("item", el.Name);
				string val = el.Value;
				if (count == 0)
					Assert.AreEqual("a", val);
				else
					Assert.AreEqual("", val);
				count++;
			}
			Assert.AreEqual(2, count);
		}

		[TestMethod]
		public void TestList2()
		{
			var config = CreateTestConfig();
			int count = 0;
			foreach (var el in config.Root.list)
			{
				Assert.AreEqual("item", el.Name);
				string val = el.Value;
				if (count == 0)
					Assert.AreEqual("a", val);
				else
					Assert.AreEqual("", val);
				count++;
			}
			Assert.AreEqual(2, count);
		}

		[TestMethod]
		public void TestAttributes()
		{
			var config = CreateTestConfig();
			int count = 0;
			foreach (var el in config.GetElements(config.Root.list))
			{
				foreach (var at in config.GetAttributes(el))
				{
					if (at.Name == "id")
					{
						int id = at.Value;
						if (count == 0)
							Assert.AreEqual(1, id);
						else
							Assert.AreEqual(10, id);
					}
				}
				count++;
			}
			Assert.AreEqual(2, count);
		}

		[TestMethod]
		public void TestIndex0()
		{
			var config = CreateTestConfig();
			string el = config.Root[0];
			Assert.AreEqual(@"c:\log", el);
		}

		[TestMethod]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void TestIndexNeg()
		{
			var config = CreateTestConfig();
			string el = config.Root[-1];
		}

		[TestMethod]
		public void TestIndex1()
		{
			var config = CreateTestConfig();
			int id = config.Root.list[1].id;
			Assert.AreEqual(10, id);
		}


		XmlConfiguration.IXmlConfiguration CreateTestConfig()
		{
			return XmlConfiguration.ConfigurationReader.Parse(
@"<root>
	<path>c:\log</path>
	<max>123</max>
	<factor>0.333</factor>
	<enable>true</enable>
	<disable>false</disable>
	<span>00:00:59.9</span>
	<color>#ff0000</color>
	<flag>on</flag>
	<list>
		<item id='1' name='alpha'>a</item>
		<item id='10' name='beta'/>
	</list>
</root>"
);
		}
	}
}
