using System;

public class EvalFunctions
{
    public double sin(double v)
    {
        return Math.Sin(v);
    }

    public double cos(double v)
    {
        return Math.Cos(v);
    }

    public double sqrt(double v)
    {
        return Math.Sqrt(v);
    }

    public DateTime now()
    {
        return DateTime.Now;
    }

    public DateTime today()
    {
        return DateTime.Today;
    }

    public int rnd()
    {
        var rnd = new Random();
        return System.Convert.ToInt32(rnd.Next(0, 100));
    }

    public double ifn(bool cond, double TrueValue, double FalseValue)
    {
        if (cond)
            return TrueValue;
        else
            return FalseValue;
    }

    public DateTime ifd(bool cond, DateTime TrueValue, DateTime FalseValue)
    {
        if (cond)
            return TrueValue;
        else
            return FalseValue;
    }

    public string ifs(bool cond, string TrueValue, string FalseValue)
    {
        if (cond)
            return TrueValue;
        else
            return FalseValue;
    }

    public string UCase(string value)
    {
        return value.ToUpper();
    }

    public string LCase(string value)
    {
        return value.ToLower();
    }

    public DateTime Date(double year, double month, double day)
    {
        return new DateTime(System.Convert.ToInt32(year), System.Convert.ToInt32(month), System.Convert.ToInt32(day));
    }

    public double Int(double value)
    {
        return Convert.ToInt32(value);
    }

    public double Round(double value)
    {
        return Math.Round(value);
    }
}
