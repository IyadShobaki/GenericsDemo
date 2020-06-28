using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleUI.WithGenerics
{
    public class GenericTextFileProcessor
    {
        public static List<T> LoadFromTextFile<T>(string filePath) where T : class, new()
        {
            List<string> lines = System.IO.File.ReadAllLines(filePath).ToList();
            List<T> output = new List<T>();
            T entry = new T();
            // The following called reflection and it bad for the performance
            // but in this situation were we will write and read once to a file,
            // using it is good. So don't use reflection all the time
            System.Reflection.PropertyInfo[] cols = entry.GetType().GetProperties();

            //Checks to be sure we have at least one header row and one data row
            if (lines.Count < 2)
            {
                throw new IndexOutOfRangeException("The file was either empty or missing.");
            }

            // Split the header into one column header per entry
            string[] headers = lines[0].Split(',');

            // Remove the header row from the lines so we don't
            // have to wory about skipping over the first row.
            lines.RemoveAt(0);

            foreach (string row in lines)
            {
                entry = new T();

                // Split the row into individual columns. Now the index
                // of this row matches the index of the header so the 
                // FirstName column header lines up with the FirstName
                // value in this row.
                string[] vals = row.Split(',');

                // Loops through each header entry so we can compare that
                // against the list of columns from reflection. Once we get
                // the matching column, we can do the "SetValue" method to
                // set the column value for our entry variable to the vals
                // item at the same index as this particular header.
                for (int i = 0; i < headers.Length; i++)
                {
                    foreach (System.Reflection.PropertyInfo col in cols)
                    {
                        if (col.Name == headers[i])
                        {
                            col.SetValue(entry, Convert.ChangeType(vals[i], col.PropertyType));
                        }
                    }
                }
                output.Add(entry);
            }
            return output;
        }

        public static void SaveToTextFile<T>(List<T> data, string filePath) where T : class//, new()
        {
            List<string> lines = new List<string>();
            StringBuilder line = new StringBuilder();

            if (data == null || data.Count == 0)
            {
                throw new ArgumentNullException("data", "You must populate the data parameter with at least one line");
            }
            System.Reflection.PropertyInfo[] cols = data[0].GetType().GetProperties();

            // Loop through each column and gets the name so it can comma
            // separate it into the header row.
            foreach (System.Reflection.PropertyInfo col in cols)
            {
                line.Append(col.Name);
                line.Append(",");
            }

            // Adds the column header entries to the first line (removing
            // the last comma from the end first).
            lines.Add(line.ToString().Substring(0, line.Length - 1));

            foreach (T row in data)
            {
                line = new StringBuilder();

                foreach (System.Reflection.PropertyInfo col in cols)
                {
                    line.Append(col.GetValue(row));
                    line.Append(",");
                }

                // Adds the row to the set of lines (removing
                // the last comma from the end first).
                lines.Add(line.ToString().Substring(0, line.Length - 1));

            }

            System.IO.File.WriteAllLines(filePath, lines);
        }
    }
}
