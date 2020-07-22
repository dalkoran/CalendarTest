namespace Spencen.Common.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Syntax taken from https://doc.flux.ly/display/flux80/Time+Expressions#TimeExpressions-Relativetimeexpressions.
    /// </summary>
    public class RelativeDate
    {
        private static ICalendar weekdayCalendar = CalendarFactory.CreateFromDateRanges("temp", Enumerable.Empty<DateRange>(), CalendarFactory.MondayToFridayWorkWeek);
        private static ICalendar weekendCalendar = CalendarFactory.CreateFromDateRanges("temp", Enumerable.Empty<DateRange>(), (DayOfWeek dow) => dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday);

        private static IDictionary<string, RelativeDateUnit> Units { get; }

        private static IEnumerable<RelativeDateUnit> DynamicUnits = new[]
        {
             new RelativeDateUnit("y", "Year", (n) => $"year {n}",
                (DateTime asOf) => new DateTime(asOf.Year, 1, 1),
                (DateTime asOf, int number) => asOf.AddYears(number),
                (DateTime asOf, int number) => new DateTime(number, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second, asOf.Millisecond)),
            new RelativeDateUnit("M", "Month", (n) => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((n - 1) % 12) + 1),
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, 1),
                (DateTime asOf, int number) => asOf.AddMonths(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, number, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second, asOf.Millisecond)),
            new RelativeDateUnit("w", "Week", (n) => $"Week {n}",
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day).AddDays(-(int)asOf.DayOfWeek),
                (DateTime asOf, int number) => asOf.AddDays(number * 7)),
            new RelativeDateUnit("d", "Day", (n) => $"Day {n}",
                (DateTime asOf) => asOf.Date,
                (DateTime asOf, int number) => asOf.AddDays(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, number, asOf.Hour, asOf.Minute, asOf.Second, asOf.Millisecond)),
            new RelativeDateUnit("H", "Hour", (n) => $"Hour {n}",
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, 0, 0),
                (DateTime asOf, int number) => asOf.AddHours(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, asOf.Day, number, asOf.Minute, asOf.Second)),
            new RelativeDateUnit("m", "Minute", (n) => $"Minute {n}",
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, 0),
                (DateTime asOf, int number) => asOf.AddMinutes(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, number, asOf.Second)),
            new RelativeDateUnit("s", "Second", (n) => $"Second {n}",
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second),
                (DateTime asOf, int number) => asOf.AddSeconds(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, number)),
            new RelativeDateUnit("S", "Millisecond", (n) => $"Millisecond {n}",
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second, asOf.Millisecond),
                (DateTime asOf, int number) => asOf.AddMilliseconds(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second, number)),

            new RelativeDateUnit("D", "Weekday", (n) => $"Weekday {n}",
                (DateTime asOf) => weekdayCalendar.AddBusinessDays(asOf, 0),
                (asOf, number) => weekdayCalendar.AddBusinessDays(asOf, number),
                null,
                (asOf) => weekdayCalendar.IsNormalWorkingDay(asOf)),
            new RelativeDateUnit("e", "Weekend day", (n) => $"Weekend day {n}",
                (DateTime asOf) => weekendCalendar.GetCurrentOrNextBusinessDay(asOf),
                (asOf, number) => weekendCalendar.AddBusinessDays(asOf, number),
                null,
                (asOf) => weekendCalendar.IsNormalWorkingDay(asOf)),
        };

        private static IEnumerable<RelativeDateAction> relativeDateActions = new[]
        {
            new RelativeDateAction("@", "Move to {u}", (unit, asOf, number) => unit.Move(asOf, number)),
            new RelativeDateAction("+", "Add {n} {PU}", (unit, asOf, number) => unit.Add(asOf, number)),
            new RelativeDateAction("-", "Subtract {n} {PU}", (unit, asOf, number) => unit.Add(asOf, -number)),
            new RelativeDateAction(">", "Current or next {pu}", (unit, asOf, number) => CurrentOrNext(unit, asOf, number)),
            new RelativeDateAction("<", "Current or previous {pu}", (unit, asOf, number) => CurrentOrPrevious(unit, asOf, number)),
            new RelativeDateAction("^", "Start of {U}", (RelativeDateUnit unit, DateTime asOf, int number) => unit.Start(asOf)),
            new RelativeDateAction("?", "If {u} then {true} else {false}", (unit, asOf, group) => IfExpression(unit, asOf, group)),
        };

        private static Regex Parser;

       static RelativeDate()
        {
            var dayOfWeekUnits = 
                Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Select(dow =>
                    new RelativeDateUnit(
                        dow.ToString().Substring(0,3).ToLower(),
                        dow.ToString(),
                        n => NumberString(n) + " " + dow.ToString(),
                        (asOf) => asOf.AddDays(-(int)dow).Date,
                        (asOf, number) => asOf.NextDayOfWeek(dow, number),
                        (asOf, number) => new DateTime(asOf.Year, asOf.Month, 1).NextDayOfWeek(dow, number),
                        (asOf) => asOf.DayOfWeek == dow));

            var lastDayOfWeekUnits =
                Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Select(dow =>
                    new RelativeDateUnit(
                        "!" + dow.ToString().Substring(0, 3).ToLower(),
                        "Last " + dow.ToString(),
                        n => NumberString(n) + " last " + dow.ToString(),
                        (asOf) => asOf.AddDays(-(int)dow).Date,
                        (asOf, number) => asOf.NextDayOfWeek(dow, number),
                        (asOf, number) => asOf.AddMonths(1).FirstDayOfMonth().Date.PreviousDayOfWeek(dow, number)));

            var monthUnits = 
                Enumerable.Range(1, 12).Select(mon =>
                    new RelativeDateUnit(
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mon).Substring(0, 3).ToLower(),
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mon),
                        n => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mon),
                        (asOf) => new DateTime(asOf.Year, mon, 1),
                        (asOf, number) => asOf.NextMonth(number, mnth => mnth.Month == mon),
                        null,
                        (asOf) => asOf.Month == mon));

            Units = monthUnits
                .Union(dayOfWeekUnits)
                .Union(lastDayOfWeekUnits)
                .Union(DynamicUnits)
                .ToDictionary(ru => ru.Key);

            // Construct regex
            string actions = string.Join("|", relativeDateActions.Select(rdt => "\\" + rdt.Key)); // "@|\\+|\\-|>|<|^|\\?";
            string units = string.Join("|", Units.Keys);

            Parser = new Regex($"(?<action>[{actions}])(?<number>[0-9]+)?(?<unit>({units}))(\\{{(?<true>.*?)\\}})?(\\{{(?<false>.*?)?\\}})?", RegexOptions.Compiled);
        }

        public RelativeDate(string relativeDateExpression)
        {
            this.Expression = relativeDateExpression;
        }

        public string Expression { get; }

        public DateTime Apply(DateTime asOf)
        {
            var matches = Parser.Matches(this.Expression);

            foreach (Match match in matches)
            {
                var number = match.Groups["number"].Success ? int.Parse(match.Groups["number"].Value) : 1;
                var action = match.Groups["action"].Value;
                var unit = match.Groups["unit"].Value;

                if (!Units.TryGetValue(unit, out RelativeDateUnit unitMatch))
                {
                    throw new InvalidOperationException($"The unit '{unit}' is not supported in the expression {this.Expression}.");
                }
                else
                {
                    var relativeDateAction = relativeDateActions.FirstOrDefault(rdt => rdt.Key == action);
                    if (relativeDateAction == null)
                    {
                        throw new InvalidOperationException($"The action '{action}' is not supported in the expression {this.Expression}.");
                    }
                    else
                    {
                        Debug.Write(BuildDescription(relativeDateAction, unitMatch, match) + " ");

                        if (relativeDateAction.ActionFunc == null)
                        {
                            asOf = relativeDateAction.ActionGroupFunc(unitMatch, asOf, match);
                        }
                        else
                        {
                            asOf = relativeDateAction.ActionFunc(unitMatch, asOf, number);
                        }
                    }
                }
            }

            return asOf;
        }

        private static string BuildDescription(RelativeDateAction action, RelativeDateUnit unit, Match match)
        {
            var number = match.Groups["number"].Success ? int.Parse(match.Groups["number"].Value) : 1;
            var trueExpression = match.Groups["true"].Success ? match.Groups["true"].Value : null;
            var falseExpression = match.Groups["false"].Success ? match.Groups["false"].Value : null;

            return action.Description
                .Replace("{U}", unit.Description)
                .Replace("{PU}", number == 1 ? unit.Description : unit.Description + "s")
                .Replace("{u}", unit.DescriptionBuilder(number))
                .Replace("{pu}", number == 1 ? unit.DescriptionBuilder(number) : unit.DescriptionBuilder(number) + "s")
                .Replace("{n}", number.ToString())
                .Replace("{true}", trueExpression)
                .Replace("{false}", falseExpression);
        }

        private static string NumberString(int number, bool ignoreOne = false)
        {
            if (number == 1 && ignoreOne)
            {
                return string.Empty;
            }

            if (number == 1) return "first";
            if (number == 2) return "second";
            if (number == 3) return "third";
            return number.ToString() + "th";
        }

        private static DateTime CurrentOrNext(RelativeDateUnit unitMatch, DateTime asOf, int number)
        {
            if (unitMatch.IsMatch(asOf))
            {
                number--;
            }

            return unitMatch.Add(asOf, number);
        }

        private static DateTime CurrentOrPrevious(RelativeDateUnit unitMatch, DateTime asOf, int number)
        {
            if (unitMatch.IsMatch(asOf))
            {
                number--;
            }

            return unitMatch.Add(asOf, -number);
        }

        private static DateTime IfExpression(RelativeDateUnit unitMatch, DateTime asOf, Match match)
        {
            var trueExpression = match.Groups["true"].Success ? match.Groups["true"].Value : null;
            var falseExpression = match.Groups["false"].Success ? match.Groups["false"].Value : null;

            var isMatch = unitMatch.IsMatch(asOf);
            if (isMatch && trueExpression != null)
            {
                asOf = new RelativeDate(trueExpression).Apply(asOf);
            }
            else if (!isMatch && falseExpression != null)
            {
                asOf = new RelativeDate(falseExpression).Apply(asOf);
            }

            return asOf;
        }
    }
}
