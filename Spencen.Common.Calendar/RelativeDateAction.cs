namespace Spencen.Common.Calendar
{
    using System;
    using System.Text.RegularExpressions;

    internal class RelativeDateAction
    {
        public RelativeDateAction(string key, string description, Func<RelativeDateUnit, DateTime, int, DateTime> actionFunc)
        {
            this.Key = key;
            this.Description = description;
            this.ActionFunc = actionFunc;
        }

        public RelativeDateAction(string key, string description, Func<RelativeDateUnit, DateTime, Match, DateTime> actionFunc)
        {
            this.Key = key;
            this.Description = description;
            this.ActionGroupFunc = actionFunc;
        }

        public string Key { get; }
        public string Description { get; }
        public Func<RelativeDateUnit, DateTime, int, DateTime> ActionFunc;
        public Func<RelativeDateUnit, DateTime, Match, DateTime> ActionGroupFunc;
    }
}
