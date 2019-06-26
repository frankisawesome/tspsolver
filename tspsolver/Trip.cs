
namespace CAB201_Assignment
{
    /// <summary>
    /// A mini class that tackles the problem of returning different types of data from the FlyTo() method
    /// Of the Plane class. 
    /// Contains properties that are easily accessible
    /// </summary>
    class Trip
    {
        public bool Refuel { get; }

        public Time Time { get; }

        public double Length { get; }

        public bool Feasible { get; }

        //Constructor used for each leg between stations
        public Trip(bool refuel, Time time, double length, bool fes)
        {
            Refuel = refuel;

            Time = time;

            Length = length;

            Feasible = fes;
        }
    }
}
