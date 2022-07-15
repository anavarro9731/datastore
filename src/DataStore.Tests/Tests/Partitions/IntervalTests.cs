namespace DataStore.Tests.Tests.Partitions
{
    #region

    using System;
    using global::DataStore.Interfaces;
    using Xunit;

    #endregion

    public class WhenCreatingTimePeriodIntervals
    {
        private readonly DayInterval dayInterval;

        private readonly HourInterval hourInterval;

        private readonly MinuteInterval minuteInterval;

        private readonly MonthInterval monthInterval;

        private readonly WeekInterval weekInterval;

        private readonly YearInterval yearInterval;

        
        public WhenCreatingTimePeriodIntervals()
        {
            var testDate = new DateTime(2000, 1, 1, 1, 1, 1);
            
            this.yearInterval = YearInterval.FromDateTime(testDate);
            this.monthInterval = MonthInterval.FromDateTime(testDate);
            this.weekInterval = WeekInterval.FromDateTime(testDate);
            this.dayInterval = DayInterval.FromDateTime(testDate);
            this.hourInterval = HourInterval.FromDateTime(testDate);
            this.minuteInterval = MinuteInterval.FromDateTime(testDate);
            
        }

        [Fact]
        public void TheyShouldResolveToCorrectFormats()
        {
            Assert.Equal("Y2000",this.yearInterval.ToString());
            Assert.Equal("Y2000:M1",this.monthInterval.ToString());
            Assert.Equal("Y2000:W1", this.weekInterval.ToString());
            Assert.Equal("Y2000:M1:D1", this.dayInterval.ToString());
            Assert.Equal("Y2000:M1:D1:H1", this.hourInterval.ToString());
            Assert.Equal("Y2000:M1:D1:H1:I1", this.minuteInterval.ToString());
            
            Assert.True(YearInterval.IsValidString(this.yearInterval.ToString()));
            Assert.True(MonthInterval.IsValidString(this.monthInterval.ToString()));
            Assert.True(WeekInterval.IsValidString(this.weekInterval.ToString()));
            Assert.True(DayInterval.IsValidString(this.dayInterval.ToString()));
            Assert.True(HourInterval.IsValidString(this.hourInterval.ToString()));
            Assert.True(MinuteInterval.IsValidString(this.minuteInterval.ToString()));
        }
    }
}