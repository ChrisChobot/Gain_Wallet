using System;

[AttributeUsage(AttributeTargets.Property)]
public class ParserAttributes : Attribute
{
    /// <summary>
    /// Column name in CSV file
    /// </summary>
    public string CsvName { get; set; }

    public ParserAttributes(string csvName)
    {
        CsvName = csvName;
    }
}
