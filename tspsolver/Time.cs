using System;
using System.Diagnostics;

namespace CAB201_Assignment
{
    /// <summary>
    /// The tour and plane methods will be using this customised Time method to store and use time objects
    /// This objects is an extension to the built in TimeSpan class
    /// And provides a variety of constructor options and string output methods
    /// </summary>
    class Time
    {
        public TimeSpan timeSpan; //The core data of time is stored using this timeSpan property

        //Many of the calculation results in this project uses double type to represent hours so a constructor for hours
        public Time (double hours, double minutes = 0)
        {
            timeSpan = new TimeSpan(0, 0, 0, 0, 0); //This constructor is invoked to make sure we have the day component in the object
            timeSpan = timeSpan.Add(TimeSpan.FromHours(hours));

            timeSpan = timeSpan.Add(TimeSpan.FromMinutes(minutes));
        }

        //A time in 00:00 format string can also be used to construct a time object
        public Time (string stringTime)
        {
            timeSpan = new TimeSpan(0, 0, 0, 0, 0);

            //Split the string, parse to double, use the From methods, add to existing timespan object.
            string[] hourMinute = stringTime.Split(':');
            timeSpan = timeSpan.Add(TimeSpan.FromHours(Double.Parse(hourMinute[0])));
            timeSpan = timeSpan.Add(TimeSpan.FromMinutes(Double.Parse(hourMinute[1])));

        }

        //Adding a double type hour to the timspan is the most common operation in the program
        public void AddHours(double hour)
        {
            timeSpan = timeSpan.Add(TimeSpan.FromHours(hour));
        }

        //A method that returns day hour minute or hour minute string for the tour summary
        public string FullString()
        {
            if (timeSpan.Days != 0)
            {
                return String.Format("{0} days {1} hours {2} minutes", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
            }

            else
            {
                return string.Format("{0} hours {1} minutes", timeSpan.Hours, timeSpan.Minutes);
            }
        }

        public string MinuteString()
        {
            return string.Format("{0} minutes", timeSpan.TotalMinutes);
        }

        public string HourMinuteString()
        {
            return string.Format("{0:00}:{1:00}", timeSpan.Hours, timeSpan.Minutes);
        }

        //This static method is invoked on a tour object with a selected level of optimisation
        //This method will record the time for a particular optimisation method to run
        //And return a string that can be added to the output.
        public static string TimerWrapper(Tour tour, int level)
        {
            string output;

            //Start a timer
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            if (level == 1)
            {
                tour.OptimiseTourLevel1();
                output = "Optimising tour length: Level 1... \n";
            }

            else if (level == 2)
            {
                tour.OptimiseTourLevel2();
                output = "Optimising tour length: Level 2... \n";
            }
            
            else if (level == 3)
            {
                tour.OptimiseTourLevel3();
                output = "Optimising tour length: Level 3... \n";
            }

            else
            {
                tour.OptimiseTourLevel4();
                output = "Optimising tour length: Level 4... \n";
            }
            //Stop the timer now
            stopWatch.Stop();

            //Write the elapsed time
            TimeSpan ts = stopWatch.Elapsed;

            output += string.Format("Elapsed time: {0:0.000} seconds \n", ts.TotalSeconds);

            return output;
        }
    }
}
