namespace Spencen.Common.Calendar
{
    using System;

    public static class RelativeDateTimeExtension
    {
        public static DateTime RelativeDate(this DateTime asOf, string relativeDateExpression)
        {
            var relativeDate = new RelativeDate(relativeDateExpression);
            return relativeDate.Apply(asOf);
        }
    }
}
