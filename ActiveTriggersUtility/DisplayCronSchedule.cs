using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ActiveTriggersUtility.ObjectRepository;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using UiPath.UIAutomationNext.API.Contracts;
using UiPath.UIAutomationNext.API.Models;
using UiPath.UIAutomationNext.Enums;

namespace CronExpressionHelper
{
    public class DisplayCronSchedule : ActiveTriggersUtility.CodedWorkflow
    {
        private static int occurrence = 0;

        [Workflow]
        public string[] Execute(string cronExpression)
        {


            // string cronExpression =  "0 0 12,13,14,15,16,17,18,19,20,21 ? * MON,TUE,WED,THU,FRI,SAT *";
            // string cronExpression = "0 30 6 ? * MON,TUE,WED,THU,FRI,SAT *";
            // string cronExpression="0 2/30 4-8 ? * MON,TUE,WED,THU,FRI,SAT *";
 
            // To start using services, use IntelliSense (CTRL + Space) to discover the available services:
            // e.g. system.GetAsset(...) 

            List<string> scheduleList = DisplayCronSchedules(cronExpression);

            string[] scheduleArray = scheduleList.ToArray();
            return scheduleArray;
        }

        private static List<string> DisplayCronSchedules(string cronExpression)
        {
            string[] cronParts = cronExpression.Split(' ');

            string minutesField = cronParts[1];
            string[] minuteParts = cronParts[1].Split('/');
            int startingMinute = ParseStartingMinute(minutesField);
            int minuteIncrement = minuteParts.Length > 1 ? ParseMinuteIncrement(minutesField) : 99;
            int[] hoursValues = ParseHoursField(cronParts[2]);
            /*
                string[] hoursParts = cronParts[2].Split('-');
                int hourStart = int.Parse(hoursParts[0]);
                int hourEnd = hoursParts.Length > 1 ? int.Parse(hoursParts[1]) : 99;
                */
            // int hourEnd = int.Parse(hoursParts[1]);            
            string[] daysOfWeek = cronParts[5].Split(',');

            // Display the table format for the first week only
            var headers = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            //  var schedule = GetSchedules(startingMinute, minuteIncrement,hourStart,hourEnd, daysOfWeek);
            var schedule = GetSchedule2(startingMinute, minuteIncrement, hoursValues, daysOfWeek, 0);

            // Combine headers and schedule values
            // var combinedValues = headers.Concat(schedule).ToArray();
            //  Console.WriteLine(CreateFormatString(combinedValues.Length), combinedValues);
            // Print headers
            Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20} {4,-20} {5,-20} {6,-20}", headers.ToArray());

            // Print each row in the schedule
            //  foreach (var row in schedule)
            //   {
            Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20} {4,-20} {5,-20} {6,-20}", schedule.ToArray());
            //  }

            //  Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20} {4,-20} {5,-20} {6,-20}",combinedValues);

            return schedule;
        }

        static int ParseStartingMinute(string minutesField)
        {
            if (minutesField.Contains("/"))
            {
                return int.Parse(minutesField.Split('/')[0]);
            }
            else
            {
                return int.Parse(minutesField);
            }
        }

        static int ParseMinuteIncrement(string minutesField)
        {
            if (minutesField.Contains("/"))
            {
                return int.Parse(minutesField.Split('/')[1]);
            }
            else
            {
                return 1; // Default to 1 if no increment specified
            }
        }

        static int[] ParseHoursField(string hoursField)
        {
            if (hoursField.Contains(","))
            {
                return hoursField.Split(',').Select(int.Parse).ToArray();
            }
            else if (hoursField.Contains("-"))
            {
                string[] range = hoursField.Split('-');
                int start = int.Parse(range[0]);
                int end = int.Parse(range[1]);
                return Enumerable.Range(start, end - start + 1).ToArray();
            }
            else if (hoursField.Contains("/"))
            {
                string[] parts = hoursField.Split('/');
                int start = 0;
                int increment = int.Parse(parts[1]);
                int end = 23;  // Assuming the hours are in the range 0-23

                if (parts[0] != "*")
                {
                    start = int.Parse(parts[0]);
                    end = start;
                }

                return Enumerable.Range(start, (end - start) / increment + 1)
                    .Select(x => x * increment % 24)  // Ensure values are within the valid range
                    .ToArray();
            }
            else
            {
                return new int[] { int.Parse(hoursField) };
            }
        }

    
        static List<string> GetSchedule2(int startingMinute, int minuteIncrement, int[] hoursValues, string[] daysOfWeek, int occurrence)
        {
            DateTime currentDate = DateTime.Now;

            // Find the next Monday
            DateTime nextMonday = currentDate.AddDays((7 - (int)currentDate.DayOfWeek) + (int)DayOfWeek.Monday);

            List<string> schedule = new List<string>();
            String subSchedule;

            for (int i = 0; i < 7; i++)
            {
                DateTime nextDate = nextMonday.AddDays(i);
                string abbreviatedDay = nextDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();

                if (daysOfWeek.Contains(abbreviatedDay) || daysOfWeek.Contains("*"))
                {
                    subSchedule = "";
                    foreach (var hourValue in hoursValues)
                    {
                        DateTime triggerTime = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, hourValue, startingMinute, 0);
                        if (minuteIncrement != 99)
                        {
                            triggerTime = triggerTime.AddMinutes(occurrence * minuteIncrement); // Increment by occurrence intervals

                            // while (triggerTime.Hour == hourValue && triggerTime.Hour >= hoursValues.Min() && triggerTime.Hour <= hoursValues.Max())
                            while (triggerTime.Hour == hourValue && triggerTime.Minute >= startingMinute && triggerTime.Minute <= 59)
                            {
                                subSchedule = String.Concat(subSchedule, triggerTime.ToString("HH:mm"), "-");
                                triggerTime = triggerTime.AddMinutes(minuteIncrement);
                            }
                        }
                        else
                        {
                            subSchedule = String.Concat(subSchedule, triggerTime.ToString("HH:mm"), "-");
                        }
                    }
                    if (subSchedule.Length > 0) // case for cron "0 0 12,13,14,15,16,17,18,19,20,21 ? * MON,TUE,WED,THU,FRI,SAT *";
                    {
                        schedule.Add(subSchedule.Substring(0, subSchedule.Length - 1));
                    }
                }
                else
                {
                    schedule.Add("");
                }
            }

            return schedule;
        }

    }
}