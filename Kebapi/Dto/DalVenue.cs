
namespace Kebapi.Dto
{
    public class DalVenue 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal GeoLat { get; set; }
        public decimal GeoLng { get; set; }
        public string Address { get; set; }
        public byte Rating { get; set; }
        public string MainMediaPath { get; set; }
    }
}
