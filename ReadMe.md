# Calendar
This repository consists of the following three .NET projects.

## Spencen.Common.Calendar
.NET Standard library for performing calendar day calculations, e.g. second business day of each week in July 2020.

### Main Classes
#### RelativeDate
Allows the description of a relative date based on the [flux syntax](https://docs.flux.ly/flux-8-1-docs/time-expressions.html#relative-time-expressions), examples include:
* Seven AM today `now.RelativeDate("^d+7H")`
* Three weekdays from now `now.RelativeDate("+3D")` (skips Sat/Sun)
* 25 days from now `now.RelativeDate("+25d")`
* This Saturday `now.RelativeDate(">sat")`
* Fourth Monday in current month `now.RelativeDate("^M>4mon")`
* Second last Friday of current month `now.RelativeDate("@2!fri")`
* Move weekend to nearest weekday `now.RelativeDate("?sat{-d}?sun{+d}")`
* Fourth of July observed holiday this year `now.RelativeDate("@7M@4d?sat{<D}?sun{>D}")`

#### RelativeDateBuilder
While the flux syntax is very powerful and concise it can be somewhat daunting to understand at first. 
To this end the `RelativeDateBuilder` class provides a fluent syntax for creating `RelativeDate` instances.

_Thanksgiving_
```csharp
var thanksgiving = new RelativeDateBuilder()
    .MoveTo(DateTimeInterval.Month, 11)
    .MoveTo(DayOfWeek.Thursday, 4)
    .Create();
```

_Observed Holidiay for July 4th (Sat moves to Friday, Sun moves to Monday)_
```csharp
var julyFourthObservedHoliday = new RelativeDateBuilder()
    .MoveTo(DateTimeInterval.Month, 7)
    .MoveTo(DateTimeInterval.Day, 4)
    .If(DateTimeInterval.Day, asOf => asOf.DayOfWeek == DayOfWeek.Saturday)
        .Then(builder => builder.Subtract(DateTimeInterval.Day))
        .End()
    .If(DateTimeInterval.Day, asOf => asOf.DayOfWeek == DayOfWeek.Sunday)
        .Then(builder => builder.Add(DateTimeInterval.Day))
        .End()
    .Create();
```
#### Calendar
In order to support business days (D) as opposed to calendar days (d) we can resolve
RelativeDates with a `Calendar` instance. Calendars implement a simple `ICalendar` interface
thus allowing them to be sourced from multiple places (database, service such as Calenderific etc.).

```csharp
public interface ICalendar
{
    /// <summary>
    /// Gets a key to unqiuely identify a calendar within a collection.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets the holidays specific to this calendar. Holidays do not need to include days of the week
    /// that are non-working days, e.g. IsWorkingDay == false.
    /// </summary>
    IEnumerable<IHoliday> Holidays { get; }

    /// <summary>
    /// Determines whether the specified <paramref name="dayOfWeek"/> is considered to be a working
    /// day for this calendar.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week being checked.</param>
    /// <returns>True if the day of the week is normally considered a working day (excluding holidays), false otherwise.</returns>
    bool IsWorkingDay(DayOfWeek dayOfWeek);
}

public interface IHoliday
{
    /// <summary>
    /// Gets the range of dates (must be specified as full days) that are considered a holiday (non-working day).
    /// </summary>
    DateRange Dates { get; }
        
    /// <summary>
    /// Gets an optional description or the holiday, e.g. "Memorial Day".
    /// </summary>
    string Description { get; }
}
```

Here we create a calendar directly using `SimpleCalendar` and `SimpleHoliday`, implementations provided by the library.

```csharp
var testCalendar = new SimpleCalendar(
    "US2020",
    new[]
    {
        new SimpleHoliday(new DateTime(2020, 01, 01), "New Year's Day"),
        new SimpleHoliday(new DateTime(2020, 01, 20), "Martin Luther King Jr Day"),
        new SimpleHoliday(new DateTime(2020, 02, 17), "Presidents Day"),
        new SimpleHoliday(new DateTime(2020, 04, 10), "Good Friday"),
        new SimpleHoliday(new DateTime(2020, 05, 25), "Memorial Day"),
        new SimpleHoliday(new DateTime(2020, 07, 03), "Independence Day"),
        new SimpleHoliday(new DateTime(2020, 09, 07), "Labor Day"),
        new SimpleHoliday(new DateTime(2020, 11, 26), "Thanksgiving"),
        new SimpleHoliday(new DateTime(2020, 12, 25), "Christmas Day"),
    },
    CalendarFactory.MondayToFridayWorkWeek);
```

#### CalendarFactory

The `CalendarFactory` static class provides a helper method, together with two very simple functions for
creating a default 5 or 7 day work week calendar.

```csharp
/// <summary>
/// Creates a simple ICalendar implementation from an enumeration of date ranges, where each item
/// in the enumeration is considered to be a holiday.
/// </summary>
/// <param name="key">A key to uniquely identify the calendar.</param>
/// <param name="dateRanges">An enumeration of date ranges that are considered to be holidays.</param>
/// <param name="isWorkingDayFunc">A function that determines whether or not a day of the week is a working day (excluding holidays). 
/// This will default to a standard five day working week from Monday through Friday inclusive.</param>
/// <returns>An instance that implements the ICalendar interface.</returns>
public static ICalendar CreateFromDateRanges(string key, IEnumerable<DateRange> dateRanges, Func<DayOfWeek, bool> isWorkingDayFunc = null)
```

#### CalendarContext
This class provides a scope in which a `Calendar` is to be used to resolve `RelativeDate` instances.
It allows optimized use of the `Calendar` instance which often will have some sort of backing store
that will need to be initialized, often dependent on the date range in question (e.g. do we need
calendar information for ALL time, or just this year?).

The `CalendarContext` provides a host of helper methods for performing business day arithmetic,
for example:
* GetCurrentOrNextBusinessDay
* AddBusinessDays
* GetFirstBusinessDAysOfWeek

A `CalendarContext` is initialized with any number of `ICalendar`s and will merge the results. 
This allows separate calendar implementations for public holidays vs. company/office specific.

## Spencen.Common.Calendar.Test
Unit tests for Spencen.Common.Calendar.

```csharp
[TestMethod]
public void Get_Australian_Holidays_From_Service()
{
    var dateRange = new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
    var asOfDate = new DateTime(2020, 4, 10); // Good Friday
    var service = new CalendarificService();
    using (var context = new CalendarContext(service, dateRange))
    {
        var result = context.GetCurrentOrNextBusinessDay("au", asOfDate);

        Assert.AreEqual(new DateTime(2020, 4, 14), result, "Australia observes Good Monday, so skip to 14th.");
    }
}
```

#### RelativeDateTests
Basic `RelativeDate` tests using flux syntax.
#### CalendarTest
Business day logic tests that include calendars.
Also includes performance test for calendar caching and using Calenderifc service to populate an `ICalendar`.
#### RelativeDateBuilderTests
Tests to cover `RelativeDateBuilder` generates the correct `RelativeDate` output.
#### RelativeHolidayTests
Tests to cover a variety of public holidays across multiple years.

## CalendarTest
Console app to showcase library usage.
