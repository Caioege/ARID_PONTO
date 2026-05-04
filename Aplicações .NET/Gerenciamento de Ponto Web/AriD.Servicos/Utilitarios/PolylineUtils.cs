using System;
using System.Collections.Generic;

namespace AriD.Servicos.Utilitarios
{
    public static class PolylineUtils
    {
        public struct Coordinate { public double Latitude; public double Longitude; }

        public static List<Coordinate> Decode(string encodedPolyline, double precision = 1e5)
        {
            var polylineChars = encodedPolyline.ToCharArray();
            var index = 0;
            var currentLat = 0;
            var currentLng = 0;
            var path = new List<Coordinate>();

            while (index < polylineChars.Length)
            {
                var sum = 0;
                var shifter = 0;
                int next5Bits;
                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                } while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5Bits >= 32)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                sum = 0;
                shifter = 0;
                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                } while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5Bits >= 32)
                    break;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                path.Add(new Coordinate
                {
                    Latitude = currentLat / precision,
                    Longitude = currentLng / precision
                });
            }
            return path;
        }

        public static double DistanceToSegment(Coordinate p, Coordinate v, Coordinate w)
        {
            double l2 = DistSq(v, w);
            if (l2 == 0) return HaversineDistance(p, v);
            
            double t = ((p.Longitude - v.Longitude) * (w.Longitude - v.Longitude) + (p.Latitude - v.Latitude) * (w.Latitude - v.Latitude)) / l2;
            t = Math.Max(0, Math.Min(1, t));
            
            Coordinate projection = new Coordinate {
                Latitude = v.Latitude + t * (w.Latitude - v.Latitude),
                Longitude = v.Longitude + t * (w.Longitude - v.Longitude)
            };
            
            return HaversineDistance(p, projection);
        }

        private static double DistSq(Coordinate a, Coordinate b)
        {
            return (a.Longitude - b.Longitude) * (a.Longitude - b.Longitude) + (a.Latitude - b.Latitude) * (a.Latitude - b.Latitude);
        }

        public static double HaversineDistance(Coordinate p1, Coordinate p2)
        {
            var R = 6371e3; // meters
            var p1lat = p1.Latitude * Math.PI / 180.0;
            var p2lat = p2.Latitude * Math.PI / 180.0;
            var deltaLat = (p2.Latitude - p1.Latitude) * Math.PI / 180.0;
            var deltaLon = (p2.Longitude - p1.Longitude) * Math.PI / 180.0;

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(p1lat) * Math.Cos(p2lat) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}
