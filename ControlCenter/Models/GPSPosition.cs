namespace ControlCenter.Models
{
    public class GPSPosition
    {
        public double Lat;
        public double Lon;
        public double Alt;

        public int FIXType;

        public int GPSWeek;
        public double GPSSecond;
        public long MicrosTime;

        public double Ve;
        public double Vn;
        public double Vu;
    }
}
