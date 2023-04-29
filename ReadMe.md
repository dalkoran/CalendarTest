# Calendar
This repository consists of the following three .NET projects.

## Spencen.Common.Calendar
.NET Standard library for performing calendar day calculations, e.g. second business day of each week in July 2020.

### Main Classes
#### RelativeDate
Allows the description of a relative date based on the [flux syntax](https://doc.flux.ly/display/flux80/Time+Expressions#TimeExpressions-Relativetimeexpressions), examples include:
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