using KiteKey.Core.Models.Calendars;

namespace KiteKey.Core;

/// <summary>Extension methods for <see cref="DateTime" /> and <see cref="DateTime" /></summary>
public static class DateTimeExtensions
{
	private const string _dateFormat = "MMM d \\'yy";
	private const string _dateMonthYearFormat = "MMM \\'yy";
	private const string _timeFormat = " h:mm tt";

	/// <summary>
	///     Gets the next <paramref name="day" /> as a datetime from <paramref name="start" />, including today, if today is <paramref name="day" />.
	/// </summary>
	/// <param name="start">The start <see cref="DateTime" /></param>
	/// <param name="day"><see cref="DayOfWeek" /></param>
	/// <returns>The next</returns>
	/// <remarks>The (... + 7) % 7 ensures we end up with a value in the range [0, 6]</remarks>
	public static DateTime GetNextWeekday(this DateTime start, DayOfWeek day)
	{
		int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
		return start.AddDays(daysToAdd);
	}

	/// <summary>Returns whether the datetime is minimumvalue.</summary>
	/// <param name="dateTime">The datetime to compare.</param>
	/// <returns>True if minimum, false otherwise.</returns>
	public static bool IsMinimumDate(this DateTime dateTime)
		=> dateTime == DateTime.MinValue;

	/// <summary>Set the date of <paramref name="source" /></summary>
	/// <param name="source"><see cref="DateTime" /></param>
	/// <param name="day">Stamps the second of the day on <paramref name="source" /></param>
	/// <returns>A new <see cref="DateTime" /> with the proper second set.</returns>
	public static DateTime SetDay(this DateTime source, int day)
			=> source.SetPart(null, null, day, null, null, null);

	/// <summary>Set the second of <paramref name="source" /></summary>
	/// <param name="source"><see cref="DateTime" /></param>
	/// <param name="hour">Stamps the hour of the day on <paramref name="source" /></param>
	/// <returns>A new <see cref="DateTime" /> with the proper hour set.</returns>
	public static DateTime SetHour(this DateTime source, int hour)
			=> source.SetPart(null, null, null, hour, null, null);

	/// <summary>Set the second of <paramref name="source" /></summary>
	/// <param name="source"><see cref="DateTime" /></param>
	/// <param name="minute">Stamps the minute of the day on <paramref name="source" /></param>
	/// <returns>A new <see cref="DateTime" /> with the proper minute set.</returns>
	public static DateTime SetMinute(this DateTime source, int minute)
			=> source.SetPart(null, null, null, null, minute, null);

	/// <summary>Set the minute of <paramref name="source" /></summary>
	/// <param name="source"><see cref="DateTime" /></param>
	/// <param name="month">Stamps the month on <paramref name="source" /></param>
	/// <returns>A new <see cref="DateTime" /> with the proper month set.</returns>
	public static DateTime SetMonth(this DateTime source, int month)
		=> source.SetPart(null, month, null, null, null, null);

	/// <summary>Add additional time using a <see cref="CalendarUnit"/> argument.</summary>
	/// <param name="source">The source datetime.</param>
	/// <param name="unit"><see cref="CalendarUnit"/></param>
	/// <param name="quantity">The number of <paramref name="unit"/></param>
	/// <returns>Resulting <see cref="DateTime"/></returns>
	public static DateTime Add(this DateTime source, CalendarUnit unit, int quantity)
	{
		DateTime result = unit switch
		{
			CalendarUnit.Days => source.AddDays(quantity),
			CalendarUnit.Weeks => source.AddDays(quantity * 7),
			CalendarUnit.Months => source.AddMonths(quantity),
			_ => throw new ArgumentOutOfRangeException(nameof(unit)),
		};

		return result;
	}

	/// <summary>Creates a new <see cref="DateTime" /> matching <paramref name="source" /> with the possible parts changes (see params).</summary>
	/// <param name="source">The source date time.</param>
	/// <param name="year">Year to set, if any.</param>
	/// <param name="month">Month to set, if any.</param>
	/// <param name="day">Day to set, if any.</param>
	/// <param name="hour">Hour to set, if any.</param>
	/// <param name="minute">Minute to set, if any.</param>
	/// <param name="second">Second to set, if any.</param>
	/// <returns>The new <see cref="DateTime" />.</returns>
	public static DateTime SetPart(this DateTime source, int? year, int? month, int? day, int? hour, int? minute, int? second)
	{
		return new DateTime(
			year ?? source.Year,
			month ?? source.Month,
			day ?? source.Day,
			hour ?? source.Hour,
			minute ?? source.Minute,
			second ?? source.Second,
			source.Millisecond,
			source.Kind).AddTicks(source.Ticks % TimeSpan.TicksPerMillisecond);
	}

	/// <summary>Set the second of <paramref name="source" /></summary>
	/// <param name="source"><see cref="DateTime" /></param>
	/// <param name="second">Stamps the second of the day on <paramref name="source" /></param>
	/// <returns>A new <see cref="DateTime" /> with the proper second set.</returns>
	public static DateTime SetSecond(this DateTime source, int second)
		=> source.SetPart(null, null, null, null, null, second);

	/// <summary>Overwrites <paramref name="source" /> with the time specified.</summary>
	/// <param name="source">The <see cref="DateTime" /> to overwrite.</param>
	/// <param name="time">The <see cref="TimeOnly" /> (time) to set.</param>
	/// <returns>A new <see cref="DateTime" /></returns>
	public static DateTime SetTime(this DateTime source, TimeOnly time)
	{
		return new DateTime(
			source.Year,
			source.Month,
			source.Day,
			time.Hour,
			time.Minute,
			time.Second,
			time.Millisecond,
			source.Kind).AddTicks(time.Ticks % TimeSpan.TicksPerMillisecond);
	}

	/// <summary>Set the year of <paramref name="source" /></summary>
	/// <param name="source"><see cref="DateTime" /></param>
	/// <param name="year">Stamps this on <paramref name="source" /></param>
	/// <returns>A new <see cref="DateTime" /> with the proper year set.</returns>
	public static DateTime SetYear(this DateTime source, int year)
		=> source.SetPart(year, null, null, null, null, null);

	/// <summary>Returns, in local time, the date string formatted as "Sep 22 '20 8:00 AM".</summary>
	/// <param name="dateTime">The date object to render to a user friendly string.</param>
	/// <param name="justMonthYear">Don't show the exact date, just the month and year. If this is set, the time is not shown.</param>
	/// <param name="onlyDate">Set this if you want to hide the time.</param>
	/// <returns>A user friendly string.</returns>
	public static string ToDateDisplayString(this DateTime? dateTime, bool justMonthYear = false, bool onlyDate = false)
	{
		if(dateTime.HasValue)
			return dateTime.Value.ToString(GetFormatString(justMonthYear, onlyDate));
		else
			throw new ArgumentNullException(nameof(dateTime));
	}

	/// <inheritdoc cref="ToDateDisplayString(DateTime, bool, bool)" />
	public static string ToDateDisplayString(this DateTime dateTime, bool justMonthYear = false, bool onlyDate = false)
		 => dateTime.ToString(GetFormatString(justMonthYear, onlyDate));

	/// <summary>Translates the current date to the first day of the month.</summary>
	/// <param name="date">Date to convert to first day of the month.</param>
	/// <returns>Last day of a month at midnight.</returns>
	public static DateTime ToFirstDayOfMonth(this DateTime date)
		=> new(date.Year, date.Month, 1);

	/// <summary>Translates the current date to the last day of the month.</summary>
	/// <param name="date">Date to convert to last day of the month.</param>
	/// <returns>Last day of a month at midnight.</returns>
	public static DateTime ToLastDayOfMonth(this DateTime date)
		=> new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

	/// <summary>Translates the current date to the first day of the month.</summary>
	/// <param name="date">Date to convert to first day of the month.</param>
	/// <returns>Last day of a month at midnight.</returns>
	public static DateOnly ToFirstDayOfMonth(this DateOnly date)
		=> new(date.Year, date.Month, 1);

	/// <summary>Translates the current date to the last day of the month.</summary>
	/// <param name="date">Date to convert to last day of the month.</param>
	/// <returns>Last day of a month at midnight.</returns>
	public static DateOnly ToLastDayOfMonth(this DateOnly date)
		=> new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

	private static string GetFormatString(bool justMonthYear, bool onlyDate)
	{
		string format;
		if(justMonthYear)
		{
			format = _dateMonthYearFormat;
			onlyDate = false; // Ignore whatever was passed in.
		}
		else
		{
			format = _dateFormat;
		}

		if(!onlyDate)
			format += _timeFormat;
		return format;
	}
}
