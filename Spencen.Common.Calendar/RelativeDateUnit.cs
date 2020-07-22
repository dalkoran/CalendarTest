namespace Spencen.Common.Calendar
{
    using System;

    internal class RelativeDateUnit
    {
        public RelativeDateUnit(
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
        public Func<int, string> DescriptionBuilder { get; }
        public Func<DateTime, DateTime> Start { get; }
        public Func<DateTime, int, DateTime> Add { get; }
        public Func<DateTime, int, DateTime> Move { get; }
        public Func<DateTime, bool> IsMatch { get; }
    }
}