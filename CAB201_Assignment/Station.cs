using System;
using System.IO;
using System.Collections.Generic;

namespace CAB201_Assignment
{
    /// <summary>
    /// Each station will have their name and coordinates
    /// The distance method calculates distance between two stations using the coordinates
    /// </summary>
    class Station
    {
        private int[] coordinate;

        public string Name { get; }

        //Construcor for a station instance
        public Station(string name, int x, int y)
        {
            coordinate = new int[] { x, y };
            Name = name;
        }


        /// <summary>
        /// Imports station from a given file path
        /// </summary>
        /// <param name="filePath">A string that specifies the file name</param>
        /// <returns>A constructed station array</returns>
        public static Station[] ImportStations(string filePath)
        {
            //Let the user know the correct input file is used
            Console.WriteLine("Reading input from " + filePath);
            
            //Create a list of stations
            List<Station> outputList = new List<Station>();
            
            //Opens the file with the path input
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(file);
            string line;
            
            //Reads the lines 
            line = reader.ReadLine();

            //Extract the coordinate information from each line to create a new station object
            string[] itemsInLine;
            string name;
            int xCoord;
            int yCoord;
            while (line != null) //Loop until there are no new lines
            {
                itemsInLine = line.Split(' '); //Use any number of spaces to split the string
                name = itemsInLine[0];
                xCoord = Int32.Parse(itemsInLine[1]);
                yCoord = Int32.Parse(itemsInLine[2]);
                outputList.Add(new Station(name, xCoord, yCoord));
                line = reader.ReadLine();
            }
            
            //Close the streams
            reader.Close();
            file.Close();

            //Add the post office to the list as a last stop
            outputList.Add(outputList[0]);

            //Turn the list into an array as the final output
            return outputList.ToArray();
        }

        /// <summary>
        /// Calculates the euclidean distance between two stations
        /// </summary>
        /// <param name="target">The target station</param>
        /// <returns>Distance in double type</returns>
        public double Distance (Station target)
        {
            double dif = Convert.ToDouble(Math.Pow((this.coordinate[0] - target.coordinate[0]),2) + Math.Pow((this.coordinate[1] - target.coordinate[1]),2)); //The actual formula
            double result = Math.Sqrt(dif);
            return result;
        }
    }
}
