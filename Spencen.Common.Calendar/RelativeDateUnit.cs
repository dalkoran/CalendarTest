namespace Spencen.Common.Calendar
{
    using System;

    public class RelativeDateUnit
    {
        internal RelativeDateUnit(
            string key,
            string description,
            Func<int, string> descriptionBuilder,
            Func<DateTime, DateTime> start,
            Func<DateTime, int, DateTime> add = null,
            Func<DateTime, int, DateTime> move = null,
            Func<DateTime, bool> match = null)
        {
            this.Key = key;
            this.Description = description;
            this.DescriptionBuilder = descriptionBuilder;
            this.Start = start;
            this.Add = add;
            this.Move = move;
            this.IsMatch = match;
        }

        public string Key { get; }
        public string Description { get; }
        internal Func<int, string> DescriptionBuilder { get; }
        internal Func<DateTime, DateTime> Start { get; }
        internal Func<DateTime, int, DateTime> Add { get; }
        internal Func<DateTime, int, DateTime> Move { get; }
        internal Func<DateTime, bool> IsMatch { get; }
    }
}