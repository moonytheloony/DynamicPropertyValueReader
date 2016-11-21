using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPropertyValueReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running Test For: Dynamically Reading Values from Object");
            TestDynamicPropertyValueReaderFunction();
            Console.ReadKey();
        }
        private static List<T> CreateAnonymousList<T>(T value)
        {
            return new List<T> { value };
        }
        private static void TestDynamicPropertyValueReaderFunction()
        {
            Console.WriteLine("Creating a complex anonymous object");

            //// Add some elelemnts to a complex list of unknown type. This illustrates, utmost complexity.
            var complexList = CreateAnonymousList(new { Name = "SomeElementName", Value = "SomeElementValue" });
            complexList.Add(new { Name = "AnotherElementName", Value = "AnotherElementValue" });
            var testObject =
                new
                {
                    SimpleProperty = "Some Value",
                    ListProperty = complexList,
                    SimpleListProperty = new List<int> { 1, 2, 3 },
                    NesTedObject =
                            new
                            {
                                AnotherNestedObject =
                                        new
                                        {
                                            YetAnotherNestedObject = new { NestedProperty = "SuperNestedPropertyValue" }
                                        }
                            }
                };

            Console.WriteLine(
                "Getting value of SimpleProperty: "
                + DynamicPropertyValueReader.GetPropertyValue(testObject, "SimpleProperty"));
            Console.WriteLine(
                "Getting value of SimpleListProperty: "
                + (DynamicPropertyValueReader.GetPropertyValue(testObject, "SimpleListProperty") as IEnumerable<int>)
                      .First());

            //// Simple filter on list
            Console.WriteLine(
                "Getting value of ListProperty through a simple filter: "
                + DynamicPropertyValueReader.GetPropertyValue(testObject, "ListProperty[Name==SomeElementName].Value"));

            Console.WriteLine(
                "Getting value of ListProperty through a simple filter: "
                + DynamicPropertyValueReader.GetPropertyValue(
                    testObject, "ListProperty[Value==AnotherElementValue].Name"));

            var returnedValue = DynamicPropertyValueReader.GetPropertyValue(
                testObject, "ListProperty[Value==asda].Name");
            if (returnedValue == null)
            {
                Console.WriteLine("No values matched this filter");
            }

            try
            {
                DynamicPropertyValueReader.GetPropertyValue(testObject, "NotAvailableProperty");
            }
            catch (KeyNotFoundException exception)
            {
                Console.WriteLine("The property was not found");
            }

            ////NestedObject
            Console.WriteLine(
                "Getting value of ListProperty through a simple filter: "
                + DynamicPropertyValueReader.GetPropertyValue(
                    testObject, "NesTedObject.AnotherNestedObject.YetAnotherNestedObject").NestedProperty);
        }
    }
}
