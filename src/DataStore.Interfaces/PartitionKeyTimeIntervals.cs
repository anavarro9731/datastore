namespace DataStore.Interfaces
{
    #region

    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    #endregion

    public class MonthInterval : PartitionKeyTimeInterval
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

        public static bool IsValidString(string s) => new Regex("^Y\\d{4}:M\\d{1,2}$").IsMatch(s);
        
        public static MonthInterval FromUtcNow() => FromDateTime(DateTime.UtcNow);

        public override string ToString()
        {
            return $"Y{Year}:M{Month}";
        }
    }

    public class YearInterval : PartitionKeyTimeInterval
    {
        public YearInterval(int year)
        {
            Year = year;
        }

        public int Year { get; }

        public static bool IsValidString(string s) => new Regex("^Y\\d{4}$").IsMatch(s);
        
        public static YearInterval FromDateTime(DateTime dateTime)
        {
            return new YearInterval(dateTime.Year);
        }
        
        public static YearInterval FromUtcNow() => FromDateTime(DateTime.UtcNow);

        public override string ToString()
        {
            return $"Y{Year}";
        }
    }

    public class WeekInterval : PartitionKeyTimeInterval
    {
        public WeekInterval(int year, int month, int week)
        {
            Year = year;
            Month = month;
            Week = week;
        }

        public int Month { get; }

        public int Week { get; }

        public int Year { get; }

        public static WeekInterval FromDateTime(DateTime dateTime)
        {
            var weekOfYear = new GregorianCalendar(GregorianCalendarTypes.Localized).GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
            return new WeekInterval(dateTime.Year, dateTime.Month, weekOfYear);
        }
        
        public static WeekInterval FromUtcNow() => FromDateTime(DateTime.UtcNow);

        public static bool IsValidString(string s) => new Regex("^Y\\d{4}:W\\d{1,2}$").IsMatch(s);
        
        public override string ToString()
        {
            return $"Y{Year}:W{Week}";
        }
    }

    public class DayInterval : PartitionKeyTimeInterval
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

        public static DayInterval FromUtcNow() => FromDateTime(DateTime.UtcNow);
        
        public static bool IsValidString(string s) => new Regex("^Y\\d{4}:M\\d{1,2}:D\\d{1,2}$").IsMatch(s);

        
        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D{Day}";
        }
    }

    public class HourInterval : PartitionKeyTimeInterval
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
        
        public static bool IsValidString(string s) => new Regex("^Y\\d{4}:M\\d{1,2}:D\\d{1,2}:H\\d{1,2}$").IsMatch(s);
        
        public static HourInterval FromUtcNow() => FromDateTime(DateTime.UtcNow);


        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D{Day}:H{Hour}";
        }
    }

    public class MinuteInterval : PartitionKeyTimeInterval
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
        public static bool IsValidString(string s) => new Regex("^Y\\d{4}:M\\d{1,2}:D\\d{1,2}:H\\d{1,2}:I\\d{1,2}$").IsMatch(s);
        
        public static MinuteInterval FromUtcNow() => FromDateTime(DateTime.UtcNow);


        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D{Day}:H{Hour}:I{Minute}";
        }
    }

    public interface PartitionKeyTimeInterval
    {
        string ToString();
    }
}