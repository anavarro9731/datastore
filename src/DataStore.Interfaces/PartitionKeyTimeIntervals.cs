namespace DataStore.Interfaces
{
    #region

    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using CircuitBoard;
    using DataStore.Interfaces.LowLevel;

    #endregion

    public class MonthInterval : IPartitionKeyTimeInterval
    {
        public MonthInterval(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public int Month { get; }

        public int Year { get; }

        public static MonthInterval FromDateTime(DateTime dateTime)
        {
            return new MonthInterval(dateTime.Year, dateTime.Month);
        }

        public static MonthInterval FromIntervalParts(IntervalParts intervalParts)
        {
            return new MonthInterval(intervalParts.Year, intervalParts.Month);
        }

        public static MonthInterval FromUtcNow()
        {
            return FromDateTime(DateTime.UtcNow);
        }

        public static bool IsValidString(string s)
        {
            return new Regex("^Y\\d{4}:M\\d{1,2}$").IsMatch(s);
        }

        public override string ToString()
        {
            return $"Y{Year}:M{Month}";
        }
    }

    public class YearInterval : IPartitionKeyTimeInterval
    {
        public YearInterval(int year)
        {
            Year = year;
        }

        public int Year { get; }

        public static YearInterval FromDateTime(DateTime dateTime)
        {
            return new YearInterval(dateTime.Year);
        }

        public static YearInterval FromIntervalParts(IntervalParts intervalParts)
        {
            return new YearInterval(intervalParts.Year);
        }

        public static YearInterval FromUtcNow()
        {
            return FromDateTime(DateTime.UtcNow);
        }

        public static bool IsValidString(string s)
        {
            return new Regex("^Y\\d{4}$").IsMatch(s);
        }

        public override string ToString()
        {
            return $"Y{Year}";
        }
    }

    public class WeekInterval : IPartitionKeyTimeInterval
    {
        public WeekInterval(int year, int week)
        {
            Year = year;
            Week = week;
        }

        public int Week { get; }

        public int Year { get; }

        public static WeekInterval FromDateTime(DateTime dateTime)
        {
            var weekOfYear = new GregorianCalendar(GregorianCalendarTypes.Localized).GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
            return new WeekInterval(dateTime.Year, weekOfYear);
        }

        public static WeekInterval FromIntervalParts(IntervalParts intervalParts)
        {
            return new WeekInterval(intervalParts.Year, intervalParts.Week);
        }

        public static WeekInterval FromUtcNow()
        {
            return FromDateTime(DateTime.UtcNow);
        }

        public static bool IsValidString(string s)
        {
            return new Regex("^Y\\d{4}:W\\d{1,2}$").IsMatch(s);
        }

        public override string ToString()
        {
            return $"Y{Year}:W{Week}";
        }
    }

    public class DayInterval : IPartitionKeyTimeInterval
    {
        public DayInterval(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public int Day { get; }

        public int Month { get; }

        public int Year { get; }

        public static DayInterval FromDateTime(DateTime dateTime)
        {
            return new DayInterval(dateTime.Year, dateTime.Month, dateTime.Day);
        }

        public static DayInterval FromIntervalParts(IntervalParts intervalParts)
        {
            return new DayInterval(intervalParts.Year, intervalParts.Month, intervalParts.Day);
        }

        public static DayInterval FromUtcNow()
        {
            return FromDateTime(DateTime.UtcNow);
        }

        public static bool IsValidString(string s)
        {
            return new Regex("^Y\\d{4}:M\\d{1,2}:D\\d{1,2}$").IsMatch(s);
        }

        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D{Day}";
        }
    }

    public class HourInterval : IPartitionKeyTimeInterval
    {
        public HourInterval(int year, int month, int day, int hour)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
        }

        public int Day { get; }

        public int Hour { get; }

        public int Month { get; }

        public int Year { get; }

        public static HourInterval FromDateTime(DateTime dateTime)
        {
            return new HourInterval(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour);
        }

        public static HourInterval FromIntervalParts(IntervalParts intervalParts)
        {
            return new HourInterval(intervalParts.Year, intervalParts.Month, intervalParts.Day, intervalParts.Hour);
        }

        public static HourInterval FromUtcNow()
        {
            return FromDateTime(DateTime.UtcNow);
        }

        public static bool IsValidString(string s)
        {
            return new Regex("^Y\\d{4}:M\\d{1,2}:D\\d{1,2}:H\\d{1,2}$").IsMatch(s);
        }

        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D{Day}:H{Hour}";
        }
    }

    public class IntervalParts
    {
        public short Day;

        public short Hour;

        public short Minute;

        public short Month;

        public short Week;

        public short Year;

        public static IntervalParts FromString(string s)
        {
            var parts = new IntervalParts();

            var regex = new Regex(
                "^(Y(?'year'\\d{4}):?)?((M(?'month'\\d{1,2}):?)|(W(?'week'\\d{1,2}):?))?(D(?'day'\\d{1,2}):?)?(H(?'hour'\\d{1,2}):?)?(I(?'minute'\\d{1,2}))?$");

            var result = regex.Match(s);

            if (result.Success == false) throw new CircuitException("Could not parse time interval string " + s);
            
            if (result.Groups["year"].Success) parts.Year = short.Parse(result.Groups["year"].Value);
            if (result.Groups["month"].Success) parts.Month = short.Parse(result.Groups["month"].Value);
            if (result.Groups["week"].Success) parts.Week = short.Parse(result.Groups["week"].Value);
            if (result.Groups["day"].Success) parts.Day = short.Parse(result.Groups["day"].Value);
            if (result.Groups["hour"].Success) parts.Hour = short.Parse(result.Groups["hour"].Value);
            if (result.Groups["minute"].Success) parts.Minute = short.Parse(result.Groups["minute"].Value);

            return parts;
        }
    }

    public class MinuteInterval : IPartitionKeyTimeInterval
    {
        public MinuteInterval(int year, int month, int day, int hour, int minute)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
        }

        public int Day { get; }

        public int Hour { get; }

        public int Minute { get; }

        public int Month { get; }

        public int Year { get; }

        public static MinuteInterval FromDateTime(DateTime dateTime)
        {
            return new MinuteInterval(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute);
        }

        public static MinuteInterval FromIntervalParts(IntervalParts intervalParts)
        {
            return new MinuteInterval(intervalParts.Year, intervalParts.Month, intervalParts.Day, intervalParts.Hour, intervalParts.Minute);
        }

        public static MinuteInterval FromUtcNow()
        {
            return FromDateTime(DateTime.UtcNow);
        }

        public static bool IsValidString(string s)
        {
            return new Regex("^Y\\d{4}:M\\d{1,2}:D\\d{1,2}:H\\d{1,2}:I\\d{1,2}$").IsMatch(s);
        }

        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D{Day}:H{Hour}:I{Minute}";
        }
    }
}