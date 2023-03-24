using EdiEngine;
using EdiEngine.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdiFileToCsv
{
     class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World");
            //string data = await ReadTextFileAsync("/Users/Anum Mehfooz/Downloads/medbitData/OX810A - Copy.DNL_BP215260063");
            string data = await ReadTextFileAsync(@"C:\Users\Anum Mehfooz\Downloads\medbitData\OX810A - Copy.DNL_BP215260063");

            Parse(data);
            Console.ReadLine();
        }

        static void Parse(string edi)
        {
            EdiDataReader r = new EdiDataReader();
            EdiBatch b = r.FromString(edi);

            //serialize whole batch to json
            JsonDataWriter w1 = new JsonDataWriter();
            string json = w1.WriteToString(b);

            Console.WriteLine(json);

            var items = ExportItems(b.Interchanges);

            File.WriteAllText(@"C:\Users\Anum Mehfooz\Downloads\medbitData\sample-" + items[1] + ".csv", items[0]);
        }

        static async Task<string> ReadTextFileAsync(string filename)
        {
            char[] result;
            StringBuilder builder = new StringBuilder();
            using (StreamReader reader = File.OpenText(filename))
            {
                result = new char[reader.BaseStream.Length];
                await reader.ReadAsync(result, 0,(int)reader.BaseStream.Length);
            }

            foreach(char c in result )
            {
                builder.Append(c.ToString().Replace("\n",""));
            }
            return builder.ToString();
        }

        static string[] ExportItems(List<EdiInterchange> ediInterchange)
        {
            var sb = new StringBuilder();
            string deli = ",";
            string po = string.Empty;
            foreach(var interchange in ediInterchange)
            {
                foreach(var group in interchange.Groups)
                {
                    foreach(var transaction in group.Transactions)
                    {
                        po = string.Empty;
                        foreach(var content in transaction.Content)
                        {
                            if (content.Name.Equals("BEG"))
                            {
                                var poData = content as EdiSegment;
                                if(poData!= null)
                                {
                                    var poSegment = poData.Content as List<DataElementBase> ;
                                    po = poSegment[2].Val;
                                }
                            }
                            if (content.Name.Equals("PO1"))
                            {
                                var items = content as EdiEngine.Runtime.EdiLoop;
                                if(items!= null)
                                {
                                    foreach (var itemCount in items.Content)
                                    {
                                        var segment = itemCount as EdiSegment;
                                        if(segment!= null)
                                        {
                                            if (segment.Name.Equals("PO1"))
                                            {
                                                foreach(var segmentContent in segment.Content)
                                                {
                                                    var data = segmentContent as EdiSimpleDataElement;
                                                    if(data!= null)
                                                    {
                                                        var value = data.Val;
                                                        sb.Append(value + deli);
                                                    }
                                                }
                                                sb.Append("\n");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new string[2] {sb.ToString(),po};
        }
    }
}
