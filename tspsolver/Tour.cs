using System;
using System.Collections.Generic;
using System.Linq;

namespace CAB201_Assignment
{
    /// <summary>
    /// A tour instance will have every information point needed to run optimisation, and produce string outputs for console and external files
    /// </summary>
    class Tour
    {
        /// <summary>
        /// The field used to store current station order, used to calculate tour time and length
        /// </summary>
        private Station[] stations;

        /// <summary>
        /// Records fuel remaining etc for the current tour
        /// </summary>
        private Plane currentPlane;

        /// <summary>
        /// Indicate the refuel locations for a tour
        /// </summary>
        private bool[] refuelLocations;

        /// <summary>
        /// Indicate whether a station is reacheable or not
        /// </summary>
        private bool[] feasibleStations;

        private Time tourTime;

        private double tourLength;

        /// <summary>
        /// Strings of current time in the tour in 24 hours format, same order as stations
        /// </summary>
        private List<string> timeStamps;

        /// <summary>
        /// A field only used in the level 3 optimisation to keep track of best length
        /// </summary>
        private double bestLength;

        /// <summary>
        /// A field only used in the level 3 optimisation to keep track of the best tour order
        /// </summary>
        private List<Station> bestList;

        /// <returns>Returns the total number of station</returns>
        public int GetStationCount()
        {
            return stations.Length;
        }

        /// <summary>
        /// Constructing a tour from the imported station and plane objects
        /// </summary>
        /// <param name="stations">A list of stations to initilise the tour</param>
        /// <param name="plane">A plane object for the tour</param>
        public Tour(Station[] stations, Plane plane)
        {
            this.stations = stations;
            currentPlane = plane;
            refuelLocations = new bool[stations.Length];
        }

        /// <summary>
        /// Use the current tour properties to generate the output string used for console output and the file output
        /// </summary>
        /// <returns>A formatted string of the tour summary</returns>
        public string GetTourSummary()
        {
            string length = tourLength.ToString("#.##"); //Specify the rounding
            string output;

            output = ("Tour time: " + tourTime.FullString() + "\n"); 
            output += ("Tour length: " + length + "\n");

            for (int i = 0; i < stations.Length - 1; i++) //Loop through all stations on the list, and generate strings for each line of output in the correct format
            {
                if (refuelLocations[i])
                {
                    Time RefuelTime = new Time(currentPlane.GetRefuelTime());
                    output += string.Format("*** Refuel {0} *** \n", RefuelTime.MinuteString()); //Decide if refuel information line is outputted based on the refuel location indicator array
                }

                output += string.Format("{0, -14}{1, -5}{2,-14}{3,-8}{4,-8} \n", stations[i].Name, "->", stations[i + 1].Name, timeStamps[i], timeStamps[i + 1]); //Output the current and the next station in a spaced out string line
            }
            return output; //Return the string chunck, this can be used for console output or for writing to file
        }

        /// <summary>
        /// A helper method to change the refuel status at a station index to true
        /// </summary>
        /// <param name="index">The index of the station where refuel happens</param>
        private void RefuelAt(int index)
        {
            refuelLocations[index] = true;
        }

        /// <summary>
        /// A less computationally demanding method to calculate the tour distance, used in optimisation algorithms
        /// </summary>
        /// <param name="stations">A list of stations used to find the total length</param>
        /// <returns>A double indicating the length of the tour</returns>
        private static double GetLength(List<Station> stations)
        {
            double length = 0;
            for (int i = 0; i < stations.Count - 1; i++) //Loop through each station
            {
                length += stations[i].Distance(stations[i + 1]); //Add the distance to the next station to the total
            }
            return length;
        }

        /// <summary>
        /// A method that runs the tour using the plane object, to obatin all time stamps and refuel locations
        /// Used only once after optimisations are performed
        /// </summary>
        /// <param name="startTime">A start time of the tour in 24 hours format</param>
        public void RunTour(string startTime)
        {
            //Reset the status of refuel locations and the current plane for a fresh run
            refuelLocations = new bool[stations.Length];
            currentPlane.Reset();

            //Initilialise time stamps
            timeStamps = new List<string>();

            //Use the given time as the current time in 24 hour format
            Time currentTime = new Time(startTime);

            //Record the total time in another time object
            Time totalTime = new Time(0);

            //Double variable for the total length of the tour
            double length = 0;

            //Trip for each leg of the tour
            Trip trip;

            //Looping through the stations array (starting by flying to the first station after post office)
            for (int i = 1; i < stations.Length; i++)
            {
                //Add the current time to the timestamps
                timeStamps.Add(currentTime.HourMinuteString());

                //Call the flyto method to get length, time and refuel information about the current leg
                trip = currentPlane.FlyTo(stations[i]);

                //Use the trip information to add to the length and time variables
                length += trip.Length;
                totalTime.AddHours(trip.Time.timeSpan.TotalHours);
                currentTime.AddHours(trip.Time.timeSpan.TotalHours);

                //Decide if a refuel is needed at the current station
                if (trip.Refuel)
                {
                    RefuelAt(i - 1); //Refuel happens at the station prior to the leg out of range so -1 is applied
                }
                /*if (!trip.Feasible)
                {
                    
                }*/
            }
            //Add one final timestamp when the loop is finished
            timeStamps.Add(currentTime.HourMinuteString());

            //Transfer the temporary variables into the tour object properties
            tourLength = length;
            tourTime = totalTime;
        }

        /// <summary>
        /// This method will take a index of a station suspected to be unreachable, then test it against all stations to see if it's actually unreachable, before deciding whether or not to keep it in the tour
        /// </summary>
        /// <param name="index"></param>
        private void FixImpossibleLeg(int index)
        {
            bool feasible = false;
            foreach (Station staion in stations)
            {
                feasible = currentPlane.FlyTo(stations[index]).Feasible; //If it can be reached by any station then it is a good station
            }
            if (!feasible) //If it can't be reached from any of the stations then it will be removed from the station list
            {
                List<Station> stationList = stations.ToList();
                stationList.RemoveAt(index);
                stations = stationList.ToArray();
            }
        }


        /// <summary>
        /// The level 1 heuristic search algorithm
        /// Uses insertion to approximate a good tour length
        /// </summary>
        public void OptimiseTourLevel1()
        {
            List<Station> searchList = new List<Station>();
            List<Station> resultList = new List<Station>();

            //Add the first the last station (post office) to the result list
            resultList.Add(stations[0]);
            resultList.Add(stations[stations.Length - 1]);

            //Populate a search list to be inserted with the rest of the stations
            for (int i = 1; i < stations.Length - 1; i++)
            {
                searchList.Add(stations[i]);
            }

            int bestIndex;

            double bestLength;

            double tempLength;

            foreach (Station station in searchList)
            {
                bestLength = Double.PositiveInfinity; //Starts off with infinity as we try to find the best length for each station
                bestIndex = -1;
                tempLength = 0;
                for (int i = 1; i < resultList.Count; i++)
                //Loop through all but the first possible index for insertion from the result list
                {
                    resultList.Insert(i, station);
                    tempLength = resultList[i - 1].Distance(resultList[i]) + resultList[i].Distance(resultList[i + 1]) - resultList[i - 1].Distance(resultList[i + 1]); //Subtracting the current edge, adding the addtional edges if the station is inserted

                    if (tempLength < bestLength) //If the insertion yields better length then replace the best length and the best index
                    {
                        bestIndex = i;
                        bestLength = tempLength;
                    }
                    resultList.RemoveAt(i); //Remove the station from the result station list when populating the length list
                }
                resultList.Insert(bestIndex, station);
            }
            resultList.Reverse(); //Just a neat formatter as I find my tours are usually the reverse of the answers
            stations = resultList.ToArray(); //Finally store the list we produced in the tour stations list
        }

        /// <summary>
        /// This method will implement the Improved heuristic search approach as described in the document.
        /// </summary>
        public void OptimiseTourLevel2()
        {
            //Run the level 1 optimisation first
            OptimiseTourLevel1();

            //Indicators for the while and for loop
            bool shorterFound; //Indicate whether or not for the current station a better place for insertion is found
            bool endReached = false; //Indicate whether all the station have been tested by insertion

            List<Station> stationList; //Station list loope through
            Station tempStation; //Temporary list used for insertion/removal

            double edge1; // This is the edges removed from the tour when a station is taken out
            double edge2; // This is the new edge reconnecting the neighbour stations after a station is taken out
            double edge3; // This is the edges generated by inserting a station at a new location
            double edge4; // This is the edge subtracted from the tour when a insertion is made

            double edgeDiff; //This is the impact of removing a station and inserting it at a new location

            //Use a while loop to decide if every station is tried by the insertion method, if so then the best tour using insertion is found
            while (!endReached)
            {
                //Initiate / reset the shorter tour found indicator to false for every time the insertion algorith is used
                shorterFound = false;
                //Loop through all the stations
                for (int i = 1; i < stations.Length - 1 && !shorterFound; i++) //Before running every loop the shorter tour found condition is checked
                {
                    //Get the current station list, try removing the station using the outer for loop's index and inserting it at the inner for loop's index
                    stationList = stations.ToList<Station>();
                    edge1 = (stationList[i - 1].Distance(stationList[i]) + stationList[i].Distance(stationList[i + 1]));
                    edge2 = stationList[i - 1].Distance(stationList[i + 1]);
                    tempStation = stationList[i];

                    stationList.RemoveAt(i);
                    //Loop through all possible insertions
                    for (int j = 1; j < stationList.Count - 1 && !shorterFound; j++) //Before running every loop the shorter tour found condition is checked
                    {
                        edge3 = stationList[j].Distance(tempStation) + stationList[j - 1].Distance(tempStation);
                        edge4 = stationList[j].Distance(stationList[j - 1]);
                        edgeDiff = -edge1 + edge2 + edge3 - edge4;

                        //If the new length is shorter than the current length then set the indicator to true to break out of all the loops
                        if (edgeDiff < -0.01)
                        {
                            shorterFound = true;
                            stationList.Insert(j, tempStation);
                            stations = stationList.ToArray();
                        }
                    }
                    if (!shorterFound) //If no shorter tour is found then insert the removed station back to its original position, if a shorter tour is found then the station would've been inserted at the new location
                    {
                        stationList.Insert(i, tempStation);
                    }
                    //If the last possible station (the station before the last stop) is tried for insertion without a shorter tour found then the algorithm is complete
                    if (i == stations.Length - 2)
                    {
                        endReached = true;
                    }
                }
            }
        }

        /// <summary>
        /// Helper method that swaps two stations in a station list
        /// </summary>
        private static void SwapStations(List<Station> stations, int index1, int index2)
        {
            Station temp = stations[index1];
            stations[index1] = stations[index2];
            stations[index2] = temp;
        }

        /// <summary>
        /// Level three method which uses incursion to initiate the Heap's Algorithm to find the best tour length
        /// </summary>
        public void OptimiseTourLevel3()
        {
            bestLength = Double.PositiveInfinity;
            List<Station> stationList = new List<Station>();

            //We are only interested in finding all permutations of the stations that aren't the post office
            for (int i = 1; i < stations.Length - 1; i++)
            {
                stationList.Add(stations[i]);
            }
            Level3Generate(stationList.Count, stationList); //Initiliase the generation process
            stations = bestList.ToArray();
        }

        /// <summary>
        /// Implementation of Heap's Algorithm
        /// </summary>
        private void Level3Generate(int len, List<Station> list)
        {

            if (len == 1)
            {
                //Add the post office to start and finish before calculating the length
                list.Insert(0, stations[0]);
                list.Add(stations[0]);

                //Get the length of the current list and compare with the best length
                double currentLength = GetLength(list);
                if (currentLength < bestLength)
                {
                    bestLength = currentLength;
                    bestList = new List<Station>(list);
                }
                //Remove the post office stations before permutations
                list.RemoveAt(0);
                list.RemoveAt(list.Count - 1);
            }
            else
            {
                Level3Generate(len - 1, list); //Recursion as per Heap's algorithm
                for (int i = 0; i < len - 1; i++)
                {
                    if (len % 2 == 0) //Check if length is even
                    {
                        SwapStations(list, i, len - 1);
                    }
                    else
                    {
                        SwapStations(list, 0, len - 1);
                    }
                    Level3Generate(len - 1, list); //Recursion as per Heap's algorithm
                }
            }
        }

        /// <summary>
        /// Helper method to perform a two opt swap on a given list and array
        /// This is different from swap, as the stations between index 1 and index 2 are reversed in order
        /// </summary>
        /// <returnsT>The resultant list</returns>
        private List<Station> TwoOptSwap(List<Station> list, int index1, int index2) //As we only use it in the level 4 implementation, index 2 will always be greater than index 1 as inputs.
        {
            List<Station> results = list.ToList(); //ToList so that the whole list is cloned rather than an object reference being passed
            for (int i = 0; i < (index2 - index1) + 1; i++) //Starts from 0 and loop through everything between index 1 and 2
            {
                results[index1 + i] = list[index2 - i];
            }
            return results;
        }
        

        /// <summary>
        /// A implementation of the 2opt swap method.
        /// </summary>
        public void OptimiseTourLevel4()
        {
            OptimiseTourLevel1(); //First use nearest neighbour (level 1) to get a reasonable result

            List<Station> stationList = stations.ToList();

            //The edges used in determining if a 2opt swap yields better result
            double iEdgeBefore;

            double jEdgeBefore;

            double iEdgeAfter;

            double jEdgeAfter;

            //Indicator for the while loop
            bool improved = true;

            while (improved) //Keep initiaiting the for loops as long as improvements are being made
            {
                improved = false;
                for (int i = 1; i < stationList.Count - 2 && !improved; i++) //looping through all swappable stations, and terminating as soon as improvement is found
                {
                    //Loop through all possible insertions
                    for (int j = i + 1; j < stationList.Count - 1 && !improved; j++) //Note the starting value is i + 1, which makes it more efficient than level 2
                    {
                        //Find the possible length difference if a swap is performed
                        iEdgeBefore = stationList[i].Distance(stationList[i-1]);

                        jEdgeBefore = stationList[j].Distance(stationList[j+1]);

                        iEdgeAfter = stationList[i].Distance(stationList[j+1]);

                        jEdgeAfter = stationList[j].Distance(stationList[i-1]);

                        double difference = iEdgeBefore + jEdgeBefore - iEdgeAfter - jEdgeAfter; //Calculating the impact of the swap
                        if (stations.Length == 238)
                        {
                            if (difference > 1e-14) //If the difference is positive then perform the swap. 1e-14 is used for 237 stations file because a special case in the stations causes there to be a exactly the same length 2 opt swap, 
                                //but due to rounding in doubles, it will cause a very slight differnce, therefore sending the program into an infinite loop by switching back and forth.
                            {
                                improved = true;
                                stationList = TwoOptSwap(stationList, i, j);
                            }
                        }
                        else
                        {
                            if (difference > 0) //If the difference is positive then perform the swap.
                            {
                                improved = true;
                                stationList = TwoOptSwap(stationList, i, j);
                            }
                        }
                    }
                }
            }
            stations = stationList.ToArray();
        }

        /// <summary>
        /// A FAILED attempt at nearest/farthest insertion algorithm
        /// </summary>
        public void OptimiseTourLevel5()
        {
            OptimiseTourLevel1();
            //Create a list of result stations starting off empty, and a search list starting off with all stations except for the post office
            List<Station> searchList = new List<Station>();
            List<Station> resultList = new List<Station>();

            //Initiate the result list with the post office station
            resultList.Add(stations[0]);

            //Populate the search list with all the stations between first and final station(both post office)
            for (int i = 1; i < stations.Length - 1; i++)
            {
                searchList.Add(stations[i]);
            }

            double nearest;
            int nearestIndex;
            double tempDist;
            double closestEdge;
            double tempEdge;
            int closestIndex;
            //Use nearest insertion to insert all stations from the search list to the result list
            while (searchList.Count != 0)
            {
                nearest = double.PositiveInfinity;
                nearestIndex = -1;
                //Find the furthest station from any station currently in the result list
                for (int i = 0; i < resultList.Count; i++)
                {
                    for (int j = 0; j < searchList.Count; j++)
                    {
                        tempDist = searchList[j].Distance(resultList[i]);
                        if (nearest > tempDist)
                        {
                            nearest = tempDist;
                            nearestIndex = j;
                        }
                    }
                }
                closestEdge = Double.PositiveInfinity;
                closestIndex = -1;
                //Find the edge closest to the farthest node
                for (int i = 0; i < resultList.Count - 1; i++)
                {
                    tempEdge = resultList[i].Distance(searchList[nearestIndex]) + resultList[i + 1].Distance(searchList[nearestIndex]) - resultList[i].Distance(resultList[i + 1]);
                    if (tempEdge < closestEdge)
                    {
                        closestEdge = tempEdge;
                        closestIndex = i;
                    }
                }
                resultList.Insert(closestIndex + 1, searchList[nearestIndex]);
                searchList.RemoveAt(nearestIndex);
            }
            stations = resultList.ToArray();
        }
    }
}

