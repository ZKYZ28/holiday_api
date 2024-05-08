using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace Holiday.Api.Core.Utilities;

public static class CalendarGenerator
{
        public static string ExportEventToIcs(Repository.Models.Holiday holiday)
        {
            var calendarEvent = new CalendarEvent()
            {
                Summary = holiday.Name,
                Start = new CalDateTime(holiday.StartDate.DateTime),
                End = new CalDateTime(holiday.EndDate.DateTime), 
                Description = holiday.Description,
            };
            
            var calendar = new Calendar();
            calendar.Events.Add(calendarEvent); 
            
            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);

            return serializedCalendar;
        }
}