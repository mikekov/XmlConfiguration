# XmlConfiguration

Reader for XML configuration files and XML documents.

To use it, load existing XML file or use parse method to pass XML in a string:
```csharp
var config =  ConfigurationReader.Load(@"C:\Temp\config.xml");
var another = ConfigurationReader.Parse(verbatim_xml);
```
All configuration nodes are located under the Root element. Reference them as if they were properties. For instance to read string value of LogDirectory element one can access it like this:
```csharp
string dir = config.Root.LogDirectory;
```
Reading numbers:
```csharp
double f = config.Root.SomeFactor;
```
Placing type name (*string*, *double*) instructs configuration reader to attempt to convert element's value. If conversion fails, runtime exception will be thrown.

Attribute access uses the same syntax as property access. Given `<Item id='5'>` we can access it as follows:
```csharp
int id = config.Root.Item.id;
```

##### Using defaults for optional values
If some values are optional (may be missing) we can use defaults:
```csharp
var f = (double?)config.Root.SomeFactor ?? 1.0;
```
Note the cast to nullable double. If SomeFactor element is missing, 1.0 will be assigned to `f`. If SomeFactor is present and well-formed
 its value will be assigned to `f`. We can still encounter an exception if SomeFactor exists, but is malformed.


##### Access to repeated elements
Indexing can be used to read N-th element.
```csharp
var item = config.Root.Items[2];  // get third element under the node <Items>
```

##### Iterating over elements
Shallow iteration over direct descendants of selected node:
```csharp
foreach (var item in config.GetElements(config.Root.Items))
    Console.WriteLine((string)item.Value);
```
or shorter:
```csharp
foreach (var item in config.Root.Items)
    Console.WriteLine((string)item.Value);
```

To iterate over all descendant elements (deep, recursive):
```csharp
foreach (var item in config.GetElements(config.Root.Items, deep: true))
    Console.WriteLine((string)item.Value);
```

##### Iterating over element attributes
Apart from directly accessing attributes with dot notation, we can also iterate over all of them:
```csharp
foreach (var a in config.GetAttribute(config.Root.Item))
    Console.WriteLine($"attribute: {a.Name} value: {(string)a.Value}");
```

##### Testing nodes for existance
Building paths to missing elements is not going to throw; attempts to dereference them may. We can always test for their existance:
```csharp
var element = config.Root.SameElement.AndAnother.OneMore;
if (config.Exists(element))
    ...
```

##### Missing elements
Given path to a missing element one can attempt to read it, like so:
```csharp
string maybe_value = config.Root.SameMissingElement;
double? maybe_number = config.Root.AnotherMissingElement;
```
In this case `maybe_value` will be null, while `maybe_number.HasValue` will be false.

However, this will fail with runtime exception if AnotherMissingElement is missing:
```csharp
double need_number = config.Root.AnotherMissingElement;
```
There are no *null* doubles.

##### Parsing user-defined types, extending XML configuration reader

Default XML reader can parse basic C# types (numbers, bool, TimeSpan, string).
If you want to handle anything else converter function will be needed.

Let's try to add support for *System.Windows.Media.Color* type. Define converter function first:

```csharp
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
```

Then attach it to the configuration reader to parse *Color* values:
```csharp
config.Converter = ColorConverter;
System.Windows.Media.Color c = config.Root.SomeColor;
```

