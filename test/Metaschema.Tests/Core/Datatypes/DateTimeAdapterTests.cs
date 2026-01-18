// Licensed under the MIT License.

using Metaschema.Core.Datatypes.Adapters;
using Shouldly;
using Xunit;

namespace Metaschema.Core.Datatypes;

public class DateAdapterTests
{
    [Theory]
    [InlineData("2024-01-15", 2024, 1, 15)]
    [InlineData("2000-12-31", 2000, 12, 31)]
    public void DateAdapter_Parse_ValidDate_ShouldSucceed(string input, int year, int month, int day)
    {
        var adapter = new DateAdapter();
        var result = adapter.Parse(input);
        result.ShouldBe(new DateOnly(year, month, day));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-date")]
    [InlineData("2024/01/15")]
    [InlineData("01-15-2024")]
    [InlineData("2024-13-01")]
    [InlineData("2024-01-32")]
    public void DateAdapter_Parse_InvalidDate_ShouldThrow(string value)
    {
        var adapter = new DateAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void DateAdapter_Format_ShouldReturnIsoFormat()
    {
        var adapter = new DateAdapter();
        var date = new DateOnly(2024, 1, 15);
        var result = adapter.Format(date);
        result.ShouldBe("2024-01-15");
    }
}

public class DateWithTimezoneAdapterTests
{
    [Theory]
    [InlineData("2024-01-15Z")]
    [InlineData("2024-01-15+00:00")]
    [InlineData("2024-01-15-05:00")]
    [InlineData("2024-01-15+05:30")]
    public void DateWithTimezoneAdapter_Parse_ValidDate_ShouldSucceed(string input)
    {
        var adapter = new DateWithTimezoneAdapter();
        var result = adapter.Parse(input);
        result.Year.ShouldBe(2024);
        result.Month.ShouldBe(1);
        result.Day.ShouldBe(15);
    }

    [Theory]
    [InlineData("2024-01-15")] // Missing timezone
    [InlineData("not-a-date")]
    public void DateWithTimezoneAdapter_Parse_InvalidDate_ShouldThrow(string value)
    {
        var adapter = new DateWithTimezoneAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }
}

public class DateTimeAdapterTests
{
    [Theory]
    [InlineData("2024-01-15T10:30:00")]
    [InlineData("2024-01-15T10:30:00Z")]
    [InlineData("2024-01-15T10:30:00+05:00")]
    [InlineData("2024-01-15T10:30:00.123")]
    public void DateTimeAdapter_Parse_ValidDateTime_ShouldSucceed(string input)
    {
        var adapter = new DateTimeAdapter();
        var result = adapter.Parse(input);
        result.Year.ShouldBe(2024);
        result.Month.ShouldBe(1);
        result.Day.ShouldBe(15);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-datetime")]
    [InlineData("2024-01-15")]
    public void DateTimeAdapter_Parse_InvalidDateTime_ShouldThrow(string value)
    {
        var adapter = new DateTimeAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void DateTimeAdapter_Format_ShouldReturnIsoFormat()
    {
        var adapter = new DateTimeAdapter();
        var dt = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var result = adapter.Format(dt);
        result.ShouldContain("2024-01-15");
        result.ShouldContain("10:30:45");
    }
}

public class DateTimeWithTimezoneAdapterTests
{
    [Theory]
    [InlineData("2024-01-15T10:30:00Z")]
    [InlineData("2024-01-15T10:30:00+00:00")]
    [InlineData("2024-01-15T10:30:00-05:00")]
    public void DateTimeWithTimezoneAdapter_Parse_ValidDateTime_ShouldSucceed(string input)
    {
        var adapter = new DateTimeWithTimezoneAdapter();
        var result = adapter.Parse(input);
        result.Year.ShouldBe(2024);
        result.Month.ShouldBe(1);
        result.Day.ShouldBe(15);
    }

    [Theory]
    [InlineData("2024-01-15T10:30:00")] // Missing timezone
    [InlineData("not-a-datetime")]
    public void DateTimeWithTimezoneAdapter_Parse_InvalidDateTime_ShouldThrow(string value)
    {
        var adapter = new DateTimeWithTimezoneAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void DateTimeWithTimezoneAdapter_Format_ShouldIncludeTimezone()
    {
        var adapter = new DateTimeWithTimezoneAdapter();
        var dt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var result = adapter.Format(dt);
        result.ShouldContain("2024-01-15");
        result.ShouldContain("+00:00");
    }
}

public class DayTimeDurationAdapterTests
{
    [Theory]
    [InlineData("PT0S", 0, 0, 0, 0)]
    [InlineData("PT1H", 0, 1, 0, 0)]
    [InlineData("PT30M", 0, 0, 30, 0)]
    [InlineData("PT45S", 0, 0, 0, 45)]
    [InlineData("P1D", 1, 0, 0, 0)]
    [InlineData("P1DT12H", 1, 12, 0, 0)]
    [InlineData("P1DT2H30M45S", 1, 2, 30, 45)]
    [InlineData("PT1H30M", 0, 1, 30, 0)]
    public void DayTimeDurationAdapter_Parse_ValidDuration_ShouldSucceed(string input, int days, int hours, int minutes, int seconds)
    {
        var adapter = new DayTimeDurationAdapter();
        var result = adapter.Parse(input);
        result.Days.ShouldBe(days);
        result.Hours.ShouldBe(hours);
        result.Minutes.ShouldBe(minutes);
        result.Seconds.ShouldBe(seconds);
    }

    [Fact]
    public void DayTimeDurationAdapter_Parse_NegativeDuration_ShouldSucceed()
    {
        var adapter = new DayTimeDurationAdapter();
        var result = adapter.Parse("-PT1H");
        result.ShouldBe(TimeSpan.FromHours(-1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-duration")]
    [InlineData("P1Y")] // Year-month duration, not day-time
    [InlineData("P1M")] // Month, not minute
    public void DayTimeDurationAdapter_Parse_InvalidDuration_ShouldThrow(string value)
    {
        var adapter = new DayTimeDurationAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void DayTimeDurationAdapter_Format_ShouldReturnIsoFormat()
    {
        var adapter = new DayTimeDurationAdapter();
        var duration = new TimeSpan(1, 2, 30, 45);
        var result = adapter.Format(duration);
        result.ShouldBe("P1DT2H30M45S");
    }

    [Fact]
    public void DayTimeDurationAdapter_Format_ZeroDuration_ShouldReturnPT0S()
    {
        var adapter = new DayTimeDurationAdapter();
        var result = adapter.Format(TimeSpan.Zero);
        result.ShouldBe("PT0S");
    }

    [Fact]
    public void DayTimeDurationAdapter_Format_NegativeDuration_ShouldHaveMinusPrefix()
    {
        var adapter = new DayTimeDurationAdapter();
        var result = adapter.Format(TimeSpan.FromHours(-1));
        result.ShouldStartWith("-P");
    }

    [Fact]
    public void DayTimeDurationAdapter_RoundTrip_ShouldPreserveValue()
    {
        var adapter = new DayTimeDurationAdapter();
        var original = new TimeSpan(2, 5, 30, 15);
        var formatted = adapter.Format(original);
        var parsed = adapter.Parse(formatted);
        parsed.ShouldBe(original);
    }
}

public class YearMonthDurationAdapterTests
{
    [Theory]
    [InlineData("P1Y", 1, 0, false)]
    [InlineData("P6M", 0, 6, false)]
    [InlineData("P1Y6M", 1, 6, false)]
    [InlineData("P2Y3M", 2, 3, false)]
    [InlineData("-P1Y", 1, 0, true)]
    [InlineData("-P6M", 0, 6, true)]
    public void YearMonthDurationAdapter_Parse_ValidDuration_ShouldSucceed(string input, int years, int months, bool isNegative)
    {
        var adapter = new YearMonthDurationAdapter();
        var result = adapter.Parse(input);
        result.Years.ShouldBe(years);
        result.Months.ShouldBe(months);
        result.IsNegative.ShouldBe(isNegative);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-duration")]
    [InlineData("P1D")] // Day-time duration
    [InlineData("PT1H")] // Day-time duration
    public void YearMonthDurationAdapter_Parse_InvalidDuration_ShouldThrow(string value)
    {
        var adapter = new YearMonthDurationAdapter();
        Should.Throw<DataTypeParseException>(() => adapter.Parse(value));
    }

    [Fact]
    public void YearMonthDurationAdapter_Format_ShouldReturnIsoFormat()
    {
        var adapter = new YearMonthDurationAdapter();
        var duration = new YearMonthDuration(1, 6);
        var result = adapter.Format(duration);
        result.ShouldBe("P1Y6M");
    }

    [Fact]
    public void YearMonthDurationAdapter_Format_YearsOnly_ShouldOmitMonths()
    {
        var adapter = new YearMonthDurationAdapter();
        var duration = new YearMonthDuration(2, 0);
        var result = adapter.Format(duration);
        result.ShouldBe("P2Y");
    }

    [Fact]
    public void YearMonthDurationAdapter_Format_MonthsOnly_ShouldOmitYears()
    {
        var adapter = new YearMonthDurationAdapter();
        var duration = new YearMonthDuration(0, 6);
        var result = adapter.Format(duration);
        result.ShouldBe("P6M");
    }

    [Fact]
    public void YearMonthDuration_TotalMonths_ShouldCalculateCorrectly()
    {
        var positive = new YearMonthDuration(1, 6);
        positive.TotalMonths.ShouldBe(18);

        var negative = new YearMonthDuration(1, 6, true);
        negative.TotalMonths.ShouldBe(-18);
    }

    [Fact]
    public void YearMonthDurationAdapter_RoundTrip_ShouldPreserveValue()
    {
        var adapter = new YearMonthDurationAdapter();
        var original = new YearMonthDuration(2, 6);
        var formatted = adapter.Format(original);
        var parsed = adapter.Parse(formatted);
        parsed.ShouldBe(original);
    }
}
