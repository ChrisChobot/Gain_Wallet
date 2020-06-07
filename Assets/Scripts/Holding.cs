using System;
using System.Globalization;

public class Holding
{
    [ParserAttributes("<NAME>")]
    public string Name { get; set; }

    [ParserAttributes("<TICKER>")]
    public string Ticker { get; set; }

    [ParserAttributes("<PER>")]
    public string Per { get; set; }

    private string _date;
    [ParserAttributes("<DATE>")]
    public string Date 
    { 
        get
        {
            return _date;
        }
        set
        {
            _date = value;
            SetDateTimeByDateAndTimeProperty();
        }
    }

    private string _time;
    [ParserAttributes("<TIME>")]
    public string Time
    {
        get
        {
            return _time;
        }
        set
        {
            _time = value;
            SetDateTimeByDateAndTimeProperty();
        }
    }

    public DateTime Timestamp { get; set; }

    [ParserAttributes("<OPEN>")]
    public float Open { get; set; }

    [ParserAttributes("<HIGH>")]
    public float High { get; set; }

    [ParserAttributes("<LOW>")]
    public float Low { get; set; }

    [ParserAttributes("<CLOSE>")]
    public float Close { get; set; }

    [ParserAttributes("<VOL>")]
    public double Vol { get; set; }

    [ParserAttributes("<OPENINT>")]
    public int Openint { get; set; }

    public float OverallGain { get; set; }

    public DateTime LastOverallGainUpdate { get; set; }
    
    public Holding()
    {
    }

    /// <summary>
    /// This method allows set Timestamp property by value of Date and Time properties, which are strings
    /// </summary>
    /// <returns></returns>
    private bool SetDateTimeByDateAndTimeProperty()
    {
        string dateFormat = "yyyyMMdd";
        string timeFormat = "HHmmss";
        string format = $"{dateFormat}{timeFormat}";

        string currentDateAndTimeValue = $"{Date}{Time}";
        bool parseResult = DateTime.TryParseExact(currentDateAndTimeValue, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime);

        if (parseResult)
        {
            Timestamp = parsedDateTime;
        }

        return parseResult;
    }


   
}
