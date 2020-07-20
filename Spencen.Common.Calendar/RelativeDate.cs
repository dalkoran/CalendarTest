namespace Spencen.Common.Calendar
{
    using System;
    using System.Collections.Generic;
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

        private static IDictionary<string, RelativeUnit> Units { get; }

        private static IEnumerable<RelativeUnit> DynamicUnits = new[]
        {           
             new RelativeUnit("y", "Year", 
                (DateTime asOf) => new DateTime(asOf.Year, 1, 1), 
                (DateTime asOf, int number) => asOf.AddYears(number),
                (DateTime asOf, int number) => new DateTime(number, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second, asOf.Millisecond)),
            new RelativeUnit("M", "Month", 
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, 1), 
                (DateTime asOf, int number) => asOf.AddMonths(number), 
                (DateTime asOf, int number) => new DateTime(asOf.Year, number, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second, asOf.Millisecond)),
            new RelativeUnit("w", "Week", 
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day).AddDays(-(int)asOf.DayOfWeek), 
                (DateTime asOf, int number) => asOf.AddDays(number * 7)),
            new RelativeUnit("d", "Day", 
                (DateTime asOf) => asOf.Date, 
                (DateTime asOf, int number) => asOf.AddDays(number), 
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, number, asOf.Hour, asOf.Minute, asOf.Second, asOf.Millisecond)),
            new RelativeUnit("H", "Hour", 
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, 0, 0), 
                (DateTime asOf, int number) => asOf.AddHours(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, asOf.Day, number, asOf.Minute, asOf.Second)),
            new RelativeUnit("m", "Minute", 
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, 0), 
                (DateTime asOf, int number) => asOf.AddMinutes(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, number, asOf.Second)),
            new RelativeUnit("s", "Second", 
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second), 
                (DateTime asOf, int number) => asOf.AddSeconds(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, number)),
            new RelativeUnit("S", "Millisecond", 
                (DateTime asOf) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second, asOf.Millisecond), 
                (DateTime asOf, int number) => asOf.AddMilliseconds(number),
                (DateTime asOf, int number) => new DateTime(asOf.Year, asOf.Month, asOf.Day, asOf.Hour, asOf.Minute, asOf.Second, number)),

            new RelativeUnit("D", "Weekday", 
                (DateTime asOf) => weekdayCalendar.AddBusinessDays(asOf, 0), 
                (asOf, number) => weekdayCalendar.AddBusinessDays(asOf, number)),
            new RelativeUnit("e", "Weekend", 
                (DateTime asOf) => weekendCalendar.GetCurrentOrNextBusinessDay(asOf), 
                (asOf, number) => weekendCalendar.AddBusinessDays(asOf, number)),
        };

        private static Regex Parser;

       static RelativeDate()
        {
            var dayOfWeekUnits = 
                Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Select(dow =>
                    new RelativeUnit(
                        dow.ToString().Substring(0,3).ToLower(),
                        dow.ToString(),
                        (asOf) => asOf.AddDays(-(int)dow).Date,
                        (asOf, number) => asOf.NextDayOfWeek(dow, number),
                        (asOf, number) => new DateTime(asOf.Year, asOf.Month, 1).NextDayOfWeek(dow, number),
                        (asOf) => asOf.DayOfWeek == dow));

            var lastDayOfWeekUnits =
                Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Select(dow =>
                    new RelativeUnit(
                        "!" + dow.ToString().Substring(0, 3).ToLower(),
                        "Last " + dow.ToString(),
                        (asOf) => asOf.AddDays(-(int)dow).Date,
                        (asOf, number) => asOf.NextDayOfWeek(dow, number),
                        (asOf, number) => asOf.AddMonths(1).FirstDayOfMonth().Date.PreviousDayOfWeek(dow, number)));

            var monthUnits = 
                Enumerable.Range(1, 12).Select(mon =>
                    new RelativeUnit(
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mon).Substring(0, 3).ToLower(),
                        CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mon),
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
            string actions = "@|\\+|\\-|\\@|>|<|^|\\?";
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
                var trueExpression = match.Groups["true"].Success ? match.Groups["true"].Value : null;
                var falseExpression = match.Groups["false"].Success ? match.Groups["false"].Value : null;

                ////var unitMatch = Units.FirstOrDefault(u => u.Key == unit);
                if (!Units.TryGetValue(unit, out RelativeUnit unitMatch))
                {
                    throw new InvalidOperationException($"The unit '{unit}' is not supported in the expression {this.Expression}.");
                }
                else
                {
                    switch (action)
                    {
                        case "^":
                            asOf = unitMatch.Start(asOf);
                            break;

                        case "+":
                        case ">":
                            asOf = unitMatch.Add(asOf, number);
                            break;

                        case "-":
                        case "<":
                            asOf = unitMatch.Add(asOf, -number);
                            break;

                        case "@":
                            asOf = unitMatch.Move(asOf, number);
                            break;

                        case "?":
                            var isMatch = unitMatch.IsMatch(asOf);
                            if (isMatch && trueExpression != null)
                            {
                                asOf = new RelativeDate(trueExpression).Apply(asOf);
                            }
                            else if (!isMatch && falseExpression != null)
                            {
                                asOf = new RelativeDate(falseExpression).Apply(asOf);
                            }
                            break;

                        default:
                            throw new InvalidOperationException($"The action '{action}' is not supported in the expression {this.Expression}.");
                    }
                }
            }

            return asOf;
        }
    }

    internal class RelativeUnit
    {
        public RelativeUnit(
            string key, 
            string description, 
            Func<DateTime, DateTime> start, 
            Func<DateTime, int, DateTime> add = null,
            Func<DateTime, int, DateTime> move = null,
            Func<DateTime, bool> match = null)
        {
            this.Key = key;
            this.Description = description;
            this.Start = start;
            this.Add = add;
            this.Move = move;
            this.IsMatch = match;
        }

        public string Key { get; }
        public string Description { get;}
        public Func<DateTime, DateTime> Start { get; }
        public Func<DateTime, int, DateTime> Add { get; }
        public Func<DateTime, int, DateTime> Move { get; }
        public Func<DateTime, bool> IsMatch { get; }
    }
}
