namespace DataStore.Interfaces
{
    using System;

    public class MonthInterval : PartitionKeyTimeInterval
    {
        public MonthInterval(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public int Month { get; }

        public int Year { get; }

        public static MonthInterval FromDatTime(DateTime dateTime)
        {
            return new MonthInterval(dateTime.Year, dateTime.Month);
        }

        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D:H:M";
        }
    }

    public class YearInterval : PartitionKeyTimeInterval
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

        public override string ToString()
        {
            return $"Y{Year}:M:D:H:M";
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

        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D{Day}:H:M";
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

        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D{Day}:H{Hour}:M";
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

        public override string ToString()
        {
            return $"Y{Year}:M{Month}:D{Day}:H{Hour}:M{Minute}";
        }
    }

    public interface PartitionKeyTimeInterval
    {
        string ToString();
    }
}