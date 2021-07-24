
namespace Kebapi.Dto
{
    public class ApiVenueDistance
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte Rating { get; set; }
        public string MainMediaPath { get; set; }
        public double DistanceInMetres { get; set; }
        public double DistanceInKilometres { get; set; }
        public double DistanceInMiles { get; set; }
    }
}
