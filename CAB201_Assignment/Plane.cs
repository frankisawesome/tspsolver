using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CAB201_Assignment
{
    /// <summary>
    /// This class represents the plane used by the postman.
    /// A plane will record its specifications, and additionally its current range and current station.
    /// Several public methods are used in other classes to help generate the tour information
    /// </summary>
    class Plane
    {
        //These are the fields of a plane instance
        private Station startStation;

        private TimeSpan range;

        private double speed;

        private TimeSpan takeOffTime;

        private TimeSpan landingTime;

        private TimeSpan refuelTime;

        //These are the status of the current plane
        private Station currentStation;

        private TimeSpan currentRange;

        /// <summary>
        /// A method used to import plane specifications from a provided file
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="station">Initial station of the plane</param>
        /// <returns>A plane object with the specifications imported</returns>
        public static Plane ImportPlane(string filePath, Station station)
        {
            //Opens the file with the path input
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(file);
            string line;

            //Reads the line
            line = reader.ReadLine();

            string[] items = Regex.Split(line, @"\s+"); //Using regex to split the line into items

            Plane output = new Plane(Double.Parse(items[0]), Double.Parse(items[1]), Double.Parse(items[2]), Double.Parse(items[3]), Double.Parse(items[4]), station);

            return output;
        }

        /// <summary>
        /// A public method that uses the plane to fly to a certain station. Caculations are made to find out the travel time, distance and refuelling information
        /// </summary>
        /// <param name="destination">The target station to fly to</param>
        /// <returns>A trip ojbect which contains basic time, length and refuel indicator</returns>
        public Trip FlyTo(Station destination)
        {
            //Find the time it takes
            double distance = currentStation.Distance(destination);
 
            //Calculate the fly time needed
            double flyTime = distance / speed;

            bool refuel = false;

            //Add take off and landing time to the total trip timße
            Time tripTime = new Time(flyTime);

            tripTime.timeSpan = tripTime.timeSpan.Add(takeOffTime);
            tripTime.timeSpan = tripTime.timeSpan.Add(landingTime);

            //If the flight exceeds current range
            if (tripTime.timeSpan > currentRange)
            {
                if (tripTime.timeSpan > range)
                {
                    return new Trip(false, new Time(0), 0, false); //If a leg exceeds the maximum range of the plane then it's not a feasible leg
                }
                //Refuel
                refuel = true;
                currentRange = range;
                //Subract the current trip time from the range
                currentRange = currentRange.Subtract(tripTime.timeSpan);
                //Add the refuel time to the trip time
                tripTime.timeSpan = tripTime.timeSpan.Add(refuelTime);
            }
            else //Else just subtrat the trip time from the current range
            {
                currentRange = currentRange.Subtract(tripTime.timeSpan);
            }

            //Update the currentstation property
            currentStation = destination;

            //Return the time
            return new Trip(refuel, tripTime, distance, true);
        }

        /// <summary>
        /// A public method that returns the refuel time in double - hours
        /// </summary>
        /// <returns></returns>
        public double GetRefuelTime()
        {
            return refuelTime.TotalHours;
        }
        /// <summary>
        /// Default constructor for a plane object, taking in all plane related specs
        /// </summary>
        public Plane(double ran, double spd, double tot, double ldt, double rft, Station station)
        {
            //Populate the properties using the hour constructor for time objects
            range = TimeSpan.FromHours(ran);
            speed = spd;

            //Populate the properties using minute contructor for time objects
            takeOffTime = TimeSpan.FromMinutes(tot);
            landingTime = TimeSpan.FromMinutes(ldt);
            refuelTime = TimeSpan.FromMinutes(rft);

            //Initialise current station and range
            currentStation = station;
            startStation = station;
            currentRange = range;
        }

        /// <summary>
        /// A public method that resets the current plane to its initial range and station
        /// </summary>
        public void Reset()
        {
            currentRange = range;
            currentStation = startStation;
        }
    }
}
