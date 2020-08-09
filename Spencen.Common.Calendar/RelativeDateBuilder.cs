namespace Spencen.Common.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;

    public class RelativeDateBuilder
    {
        internal List<RelativeDateOperation> Operations { get; } = new List<RelativeDateOperation>();

        public RelativeDate Create()
        {
            return new RelativeDate(this.Operations);
        }

        public DateTime Apply(DateTime asOf, ICalendar calendar = null)
        {
            return this.Create().Apply(asOf);
        }

        /// <summary>
        /// Move to the beginning of the specified date interval. Going to the beginning of one time unit casuses 
        /// any sub-units (such as minute, second, and millisecond) to go to the beginning also.
        /// </summary>
        /// <param name="interval">The period of time, e.g. year, month, week, day, hour, minute, second, millisecond.</param>
        public RelativeDateBuilder MoveToBeginningOf(DateTimeInterval interval)
        {
            var unit = ConvertIntervalToUnit(interval);
            var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.Start);
            this.Operations.Add(new RelativeDateOperation(action.Description, context => unit.Start(context.AsOf), unit, 1));
            return this;
        }

        /// <summary>
        /// Move to the nth year, month, day-of-month, hour, minute, second or millisecond.
        /// </summary>
        /// <param name="interval">The period of time in which the move is described, e.g. year, month, day, hour, minute, second, millisecond.</param>
        /// <param name="intervalIndex">The index that identifies the interval to which we are moving, e.g. 2015 for year 2015, 11 for November, 16 for 4pm.</param>
        public RelativeDateBuilder MoveTo(DateTimeInterval interval, int intervalIndex)
        {
            var unit = ConvertIntervalToUnit(interval);
            var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.MoveTo);
            this.Operations.Add(new RelativeDateOperation(action.Description, context => unit.Move(context.AsOf, context.Number), unit, intervalIndex));
            return this;
        }

        /// <summary>
        /// Move to the nth day of the week for the current month, e.g. 3rd Friday in this month.
        /// </summary>
        /// <param name="dayOfWeek">The day of the week to which we are moving.</param>
        /// <param name="number">The nth day of the week for the current month, e.g. 3rd Friday from start of current month.</param>
        public RelativeDateBuilder MoveTo(DayOfWeek dayOfWeek, int number = 1)
        {
            var unit = RelativeDate.Units[dayOfWeek.ToString().ToLower().Substring(0, 3)];
            var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.MoveTo);
            this.Operations.Add(new RelativeDateOperation(action.Description, context => unit.Move(context.AsOf, context.Number), unit, number));
            return this;
        }


        /// <summary>
        /// Move to the nth last day of the week for the current month, e.g. 3rd Friday in this month.
        /// </summary>
        /// <param name="dayOfWeek">The day of the week to which we are moving.</param>
        /// <param name="number">The nth last day of the week for the current month, e.g. 2rd last Friday from end of current month.</param>
        public RelativeDateBuilder MoveToLast(DayOfWeek dayOfWeek, int number = 1)
        {
            var unit = RelativeDate.Units["!" + dayOfWeek.ToString().ToLower().Substring(0, 3)];
            var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.MoveTo);
            this.Operations.Add(new RelativeDateOperation(action.Description, context => unit.Move(context.AsOf, context.Number), unit, number));
            return this;
        }

        /// <summary>
        /// Adds the specified <paramref name="number"/> of date <paramref name="interval"/>s.
        /// </summary>
        /// <param name="interval">The interval of time to add, e.g. years, months, weeks, days, hours, minutes, seconds or milliseconds.</param>
        /// <param name="number">The number of time intervals.</param>
        /// <returns></returns>
        public RelativeDateBuilder Add(DateTimeInterval interval, int number = 1)
        {
            var unit = ConvertIntervalToUnit(interval);
            var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.Add);
            this.Operations.Add(new RelativeDateOperation(action.Description, context => unit.Add(context.AsOf, context.Number), unit, number));
            return this;
        }

        /// <summary>
        /// Subtracts the specified <paramref name="number"/> of date <paramref name="interval"/>s.
        /// </summary>
        /// <param name="interval">The interval of time to subtract, e.g. years, months, weeks, days, hours, minutes, seconds or milliseconds.</param>
        /// <param name="number">The number of time intervals.</param>
        /// <returns></returns>
        public RelativeDateBuilder Subtract(DateTimeInterval interval, int number = 1)
        {
            var unit = ConvertIntervalToUnit(interval);
            var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.Subtract);
            this.Operations.Add(new RelativeDateOperation(action.Description, context => unit.Add(context.AsOf, -context.Number), unit, number));
            return this;
        }

        /// <summary>
        /// Moves a <paramref name="number"/> of <paramref name="interval"/>s forward in time. If the current date matches the specified 
        /// movement interval then the <paramref name="number"/> is drecremented by one (we treat current as a match).
        /// </summary>
        /// <param name="interval">The interval unit in which to move, e.g. days, months, weekdays, weekends, business days.</param>
        /// <param name="number">The number of intervals to move.</param>
        public RelativeDateBuilder MoveNext(DateTimeInterval interval, int number)
        {
            var unit = ConvertIntervalToUnit(interval);
            var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.MoveToNext);
            this.Operations.Add(new RelativeDateOperation(action.Description, context => unit.Add(context.AsOf, context.Number), unit, number));
            return this;
        }

        /// <summary>
        /// Moves a <paramref name="number"/> of <paramref name="interval"/>s backward in time. If the current date matches the specified 
        /// movement interval then the <paramref name="number"/> is drecremented by one (we treat current as a match).
        /// </summary>
        /// <param name="interval">The interval unit in which to move, e.g. days, months, weekdays, weekends, business days.</param>
        /// <param name="number">The number of intervals to move.</param>
        public RelativeDateBuilder MovePrevious(DateTimeInterval interval, int number)
        {
            var unit = ConvertIntervalToUnit(interval);
            var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.MoveToPrevious);
            this.Operations.Add(new RelativeDateOperation(action.Description, context => unit.Add(context.AsOf, -context.Number), unit, number));
            return this;
        }

        public RelativeDateConditionalBuilder If(DateTimeInterval interval, int intervalIndex)
        {
            var unit = ConvertIntervalToUnit(interval);
            var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.If);
            ////this.Operations.Add(new RelativeDateConditionalOperation(action.Description, unit, unit.IsMatch, trueOperation, falseOperation));
            return new RelativeDateConditionalBuilder(this, unit.IsMatch);
        }

        public RelativeDateConditionalBuilder If(DateTimeInterval interval, Func<DateTime, bool> matchFunc)
        {
            ////var unit = ConvertIntervalToUnit(interval);
            ////var action = RelativeDate.RelativeDateActions.FirstOrDefault(rdt => rdt.Key == RelativeDate.ActionKeys.If);
            ////this.Operations.Add(new RelativeDateConditionalOperation(action.Description, unit, matchFunc, trueOperation, falseOperation));
            return new RelativeDateConditionalBuilder(this, matchFunc);
        }

        private static RelativeDateUnit ConvertIntervalToUnit(DateTimeInterval interval)
        {
            var key = MapIntervalToUnitKey(interval);

            if (string.IsNullOrWhiteSpace(key) || !RelativeDate.Units.TryGetValue(key, out RelativeDateUnit unit))
            {
                throw new ArgumentException($"Cannot convert DateTimeInterval.{interval} to RelativeDateUnit.", nameof(interval));
            }

            return unit;
        }

        private static string MapIntervalToUnitKey(DateTimeInterval interval)
        {
            switch (interval)
            {
                case DateTimeInterval.Millsecond:
                    return "S";
                case DateTimeInterval.Second:
                    return "s";
                case DateTimeInterval.Minute:
                    return "m";
                case DateTimeInterval.Hour:
                    return "H";
                case DateTimeInterval.Day:
                    return "d";
                case DateTimeInterval.WeekDay:
                    return "D";
                case DateTimeInterval.WeekEnd:
                    return "e";
                case DateTimeInterval.BusinessDay:
                    return "b";
                case DateTimeInterval.Week:
                    return "w";
                case DateTimeInterval.Month:
                    return "M";
                case DateTimeInterval.Year:
                    return "y";

                case DateTimeInterval.Quarter:
                case DateTimeInterval.Decade:
                case DateTimeInterval.Century:
                default:
                    return null;
            }
        }
    }

    public class RelativeDateConditionalBuilder
    {
        internal RelativeDateConditionalBuilder(RelativeDateBuilder parent, Func<DateTime, bool> matchFunc)
        {
            this.Parent = parent;
            this.MatchFunc = matchFunc;
        }

        public RelativeDateBuilder Parent { get; }
        private Func<DateTime, bool> MatchFunc { get; }
        private RelativeDate TrueRelativeDate { get; set; }
        private RelativeDate FalseRelativeDate { get; set; }

        public RelativeDateConditionalBuilder Then(Action<RelativeDateBuilder> trueBuilderAction)
        {
            var trueBuilder = new RelativeDateBuilder();
            trueBuilderAction(trueBuilder);
            this.TrueRelativeDate = trueBuilder.Create();

            return this;
        }

        public RelativeDateConditionalBuilder Else(Action<RelativeDateBuilder> falseBuilderAction)
        {
            var falseBuilder = new RelativeDateBuilder();
            falseBuilderAction(falseBuilder);
            this.FalseRelativeDate = falseBuilder.Create();

            return this;
        }

        public RelativeDateBuilder End()
        {
            var conditional = new RelativeDateConditionalOperation("if", null, this.MatchFunc, this.TrueRelativeDate, this.FalseRelativeDate);
            this.Parent.Operations.Add(conditional);
            return this.Parent;
        }
    }
}
