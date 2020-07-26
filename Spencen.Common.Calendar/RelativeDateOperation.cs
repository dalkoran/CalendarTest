using System;

namespace Spencen.Common.Calendar
{
    internal class RelativeDateOperation
    {
        public RelativeDateOperation(string description, Func<RelativeDateContext, DateTime> action, RelativeDateUnit unit, int number)
        {
            this.Description = description;
            this.Action = action;
            this.Unit = unit;
            this.Number = number;
        }

        public string Description { get; }
        public Func<RelativeDateContext, DateTime> Action { get; protected set; }
        public RelativeDateUnit Unit { get; }
        public int Number { get; }
    }

    internal class RelativeDateConditionalOperation : RelativeDateOperation
    {
        public RelativeDateConditionalOperation(
            string description, 
            RelativeDateUnit unit, 
            Func<DateTime, bool> match, 
            RelativeDate trueOperation = null, 
            RelativeDate falseOperation = null)
            : base(description, null, unit, 1)
        {
            this.Action = (context) =>
            {
                if (match(context.AsOf))
                {
                    if (trueOperation != null)
                    {
                        return trueOperation.Apply(context.AsOf);
                    }
                }
                else if (falseOperation != null)
                {
                    return falseOperation.Apply(context.AsOf);
                }

                return context.AsOf; // unchanged
            };
        }
    }
}

