using System;
using System.IO;
namespace CAB201_Assignment
{
    class Program
    {
        /// <summary>
        /// The main method that will be called when the program is run
        /// </summary>
        /// <param name="args">A string array with filepath to maillist, filepath to plane spec, starting time as mandatory and optional -o flag with output filepath</param>
        static void Main(string[] args)
        {
            //This will be the string that stores the formatted console/file output
            string output;

            //Try importing the station from the file
            Station[] myStations;
            try
            {
                //Import the stations using the frist command line argument
                myStations = Station.ImportStations(args[0]);
            }
            //If failed catch the exception, let the user know station file read failed, and exit.
            catch
            {
                Console.WriteLine("Failed to read station file. Did you put the filepath as the first argument?");
                Console.ReadLine();
                return;
            }

            Plane myPlane;
            try
            {
                //Import the plane using the second command line argument
                myPlane = Plane.ImportPlane(args[1], myStations[0]);
            }
            catch
            {
                Console.WriteLine("Failed to read plane file. Did you put the filepath as the second argument?");
                Console.ReadLine();
                return;
            }


            //Create a new tour object using the imported station list and plane          
            Tour myTour = new Tour(myStations, myPlane);

            //If the stations length is smaller than 14 (The finishing station is added in the import process, therefore one station more than specified by assignment requirement)
            //Then the level 3 (exhaustive search) algorith is used
            if (myTour.GetStationCount() < 14)
            {
                //Timerwrapper runs an optimisation method and times the run time, then output a string stating the algorithm run time
                output = Time.TimerWrapper(myTour, 3);
            }

            //If the stations list is of medium length, use algorithm level 2
            else if (myTour.GetStationCount() < 200)
            {
                output = Time.TimerWrapper(myTour, 2);
                
            }
            //If the stations list is long, use algorithm level 4
            else 
            {
                output = Time.TimerWrapper(myTour, 4);
            }

            try
            {
                //Run tour gets the time, length and refuel information about the optimised tour, also uses the third argument as starting time
                myTour.RunTour(args[2]);
            }
            catch
            {
                Console.WriteLine("Failed to read time. Did you put a start time in 24 hour format as the third argument?");
                Console.ReadLine();
                return;
            }

            //Get tour summary gets the stringified output of the tour information
            output += myTour.GetTourSummary();

            //Log the output to the console
            Console.WriteLine(output);

            //If additional arguments for the output file is given then print the result into the file
            if (args.Length > 3)
            {
                try
                {
                    if (File.Exists(args[4]))
                    {
                        FileStream fs = new FileStream(args[4], FileMode.Open);
                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine(output);

                        sw.Close();
                        fs.Close();
                    }
                    else
                    {
                        FileStream fs = File.Create(args[4]);
                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine(output);

                        sw.Close();
                        fs.Close();
                    }
                }
                catch
                {
                    Console.WriteLine("There has been an error writing to the file, please check you have provided the correct file name");
                    Console.ReadLine();
                }

            }
            Console.ReadLine(); //Exit the program on enter
        }
    }
}
