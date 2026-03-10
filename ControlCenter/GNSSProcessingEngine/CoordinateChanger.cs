using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static System.Math;

namespace ControlCenter.GNSSProcessingEngine
{
    public class CoordinateChanger
    {
        //private const double PI 3.14159265;
        private const double a = 6378137.0;  // Semieje mayor de la tierra (en metros) 
        private const double b = 6356752.3142;
        private const double f = 1 / 298.257223563;  // Factor de achatamiento 
        private const double deg2rad = PI / 180;
        private const double rad2deg = 180 / PI;
        private const double e2 = 2 * f - f * f;  // first eccentricity squared 

        private static double[] body2ned(double x, double y, double z, double _roll, double _pitch, double _yaw)
        {
            double[] ned = new double[3];
            ned[0] = Cos(_pitch) * Cos(_yaw) * x + (Cos(_yaw) * Sin(_pitch) * Sin(_roll) - Sin(_yaw) * Cos(_roll)) * y + (Cos(_yaw) * Sin(_pitch) * Cos(_roll) + Sin(_yaw) * Sin(_roll)) * z;
            ned[1] = Cos(_pitch) * Sin(_yaw) * x + (Cos(_yaw) * Cos(_roll) + Sin(_yaw) * Sin(_pitch) * Sin(_roll)) * y + (Sin(_yaw) * Sin(_pitch) * Cos(_roll) - Cos(_yaw) * Sin(_roll)) * z;
            ned[2] = -Sin(_pitch) * x + (Cos(_pitch) * Sin(_roll)) * y + (Cos(_pitch) * Cos(_roll)) * z;
            return ned;
        }

        private static double[] ned2ecef(double[] refLLA, double[] ned)
        {
            double[] ecef = new double[3];
			double[] refLLA_radians = new double[3];
			
			refLLA_radians[0] = refLLA[0] * deg2rad;
			refLLA_radians[1] = refLLA[1] * deg2rad;
			refLLA_radians[2] = refLLA[2];
			
            double[] ecefRef = lla2ecef(refLLA_radians[0], refLLA_radians[1], refLLA_radians[2]);
            // refLat y refLon están dadas en radianes  
            // Localización del punto de referencia expresado en coordenadas ECEF 

            // refLat y refLon están dadas en radianes  
            ecef[0] = -ned[0] * Sin(refLLA_radians[0]) * Cos(refLLA_radians[1]) - ned[1] * Sin(refLLA_radians[1]) - ned[2] * Cos(refLLA_radians[0]) * Cos(refLLA_radians[1]) + ecefRef[0];
            ecef[1] = -ned[0] * Sin(refLLA_radians[0]) * Sin(refLLA_radians[1]) + ned[1] * Cos(refLLA_radians[1]) - ned[2] * Cos(refLLA_radians[0]) * Sin(refLLA_radians[1]) + ecefRef[1];
            ecef[2] = ned[0] * Cos(refLLA_radians[0]) - ned[2] * Sin(refLLA_radians[0]) + ecefRef[2];

            return ecef;
        }

        private static double[] lla2ecef(double lat, double lon, double h, bool radians = true)
        {
            if (!radians)
            {
                lat *= deg2rad;
                lon *= deg2rad;
            }

            double chi = Sqrt(1 - e2 * Sin(lat) * Sin(lat));

            double[] ecef = new double[3];

            ecef[0] = (a / chi + h) * Cos(lat) * Cos(lon);
            ecef[1] = (a / chi + h) * Cos(lat) * Sin(lon);
            ecef[2] = (a * (1 - e2) / chi + h) * Sin(lat);

            //double N = Pow(a, 2) / (0.5 * (Pow(a*Cos(lat), 2) + Pow(b*Cos(lon),2)));

            //ecef[0] = (N + h) * Cos(lat) * Cos(lon);
            //ecef[1] = (N + h) * Cos(lat) * Sin(lon);
            //ecef[2] = (Pow(b/a,2)*N + h) * Sin(lat);

            return ecef;



            //double a = 6378137; // radius
            //double e = 8.1819190842622e-2;  // eccentricity

            //double asq = Pow(a, 2);
            //double esq = Pow(e, 2);

            //double N = a / Sqrt(1 - esq * Pow(Sin(lat), 2));

            //double x = (N + h) * Cos(lat) * Cos(lon);
            //double y = (N + h) * Cos(lat) * Sin(lon);
            //double z = ((1 - esq) * N + h) * Sin(lat);

            //double[] ret = { x, y, z };
            //return ret;
        }

        private static double[] ecef2lla_Ruben(double[] ecef)
        {
            //flattening
            double f = 1d / 298.257223563d;

            //equatorial radius
            double R = 6378137.0d;

            //Determine longitude
            double lambda = Atan2(ecef[1], ecef[0]) * 180 / PI;

            //Determine radial distance from polar axis
            double rho = Sqrt(ecef[0] * ecef[0] + ecef[1] * ecef[1]);

            //Determine geodetic latitude and ellipsoidal height
            double[] phi_h = cylindrical2geodetic(rho, ecef[2], R, f, true);

            double[] lla = new double[3] { phi_h[0] , lambda , phi_h[1] };
            return lla;
        }

        private static double[]  cylindrical2geodetic(double rho, double z, double a, double f, bool inDegrees)
        {
            //Spheroid properties
            double b = (double)(1 - f) * a;      //Semiminor axis
            double e2 = f * (double)(2 - f);     //Square of (first) eccentricity
            double ep2 = (double)e2 / (double)(1 - e2);  //Square of second eccentricity

            //Bowring's formula for initial parametric (beta) and geodetic
            //(phi) latitudes
            double beta = Atan2(z, (1 - f) * rho) * 180 / PI;
            double phi = Atan2(z + b * ep2 * Pow(Sin(beta * PI / 180), 3), rho - a * e2 * Pow(Cos(beta * PI / 180), 3)) * 180 / PI;

            //Fixed-point iteration with Bowring's formula
            //(typically converges within two or three iterations)
            double betaNew = Atan2((1 - f) * Sin(phi * PI / 180), Cos(phi * PI / 180)) * 180 / PI;
            int count = 0;
            while (beta != betaNew && count < 25)//this is very very weird------------------------
            {
                beta = betaNew;
                phi = Atan2(z + b * ep2 * Pow(Sin(beta * PI / 180), 3), rho - a * e2 * Pow(Cos(beta * PI / 180), 3)) * 180 / PI;
                betaNew = Atan2((1 - f) * Sin(phi * PI / 180), Cos(phi * PI / 180)) * 180 / PI;
                count = count + 1;
            }


            //Ellipsoidal height from final value for latitude
            double sinphi = Sin(phi * PI / 180);
            double N = a / Sqrt(1 - e2 * sinphi * sinphi);
            double h = rho * Cos(phi * PI / 180) + (z + e2 * N * sinphi) * sinphi - N;

            double[] result = new double[2] {phi, h};

            return result;
        }

        private static double[] ecef2lla(double[] ecef)
        {
            double[] lla = new double[3];

            const double b = a * (1 - f);  // Semieje menor de la tierra (en metros) 
            const double e2=2*f-f*f;  // first eccentricity squared  
            const double ep2=f*(2-f)/((1-f)*(1-f));  // second eccentricity squared 

            double r2, r, E2, F, G, c, s, P, Q, ro, tmp, U, V, zo;

            r2 = ecef[0] * ecef[0] + ecef[1] * ecef[1];
            r = Sqrt(r2); E2 = a * a - b * b;
            F = 54 * b * b * ecef[2] * ecef[2];
            G = r2 + (1 - e2) * ecef[2] * ecef[2] - e2 * E2;
            c = (e2 * e2 * F * r2) / (G * G * G);
            s = Pow(1 + c + Sqrt(c * c + 2 * c), 1 / 3);
            P = F / (3 * (s + 1 / s + 1) * (s + 1 / s + 1) * G * G);
            Q = Sqrt(1 + 2 * e2 * e2 * P); ro = -(e2 * P * r) / (1 + Q) + Sqrt((a * a / 2) * (1 + 1 / Q) - ((1e2) * P * ecef[2] * ecef[2]) / (Q * (1 + Q)) - P * r2 / 2);
            tmp = (r - e2 * ro) * (r - e2 * ro); U = Sqrt(tmp + ecef[2] * ecef[2]);
            V = Sqrt(tmp + (1 - e2) * ecef[2] * ecef[2]);
            zo = (b * b * ecef[2]) / (a * V);

            lla[0] = Atan2(ecef[1], ecef[0]) * rad2deg;  // lambda = longitud (en grados)  
            lla[1]=Atan((ecef[2]+ep2*zo)/r)*rad2deg;  // phi = latitud (en grados)  
            lla[2]=U*(1-b*b/(a*V));  // h 

            return lla;
        }

        public static double DistanceStraight3D(double lat1, double lon1, double alt1, double lat2, double lon2, double alt2)
        {
            double[] ecef1 = lla2ecef(lat1, lon1, alt1, false);
            double[] ecef2 = lla2ecef(lat2, lon2, alt2, false);

            double[] dist = new double[3];
            for (int i=0; i<3;i++)
                dist[i] = ecef1[i] - ecef2[i];

            double dist3D = Math.Sqrt(Math.Pow(dist[0],2) + Math.Pow(dist[1], 2) + Math.Pow(dist[2], 2));

            return dist3D;
        }

        // RPY in degrees
        // LLA in degrees
        public static double[] TrasladarPunto(double[] llaOrig, double[] offset, double[] RPY)
        {
            double[] llaResult;
            double[] RPY_radians = new double[3];
            double[] ned;
            double[] ecef;
            double[] lla;


            for (int i = 0; i < 3; i++)
                RPY_radians[i] = RPY[i] * deg2rad;

            ned = body2ned(offset[0], offset[1], offset[2], RPY_radians[0], RPY_radians[1], RPY_radians[2]);
            ecef = ned2ecef(llaOrig, ned);
            lla = ecef2lla_Ruben(ecef);

            return lla;
        }

        private static double wrapTo180(double angle)
        {
            double newAngle = angle;
            while (newAngle <= -180) newAngle += 360;
            while (newAngle > 180) newAngle -= 360;
            return newAngle;
        }
    }

    public class RollPitchRotation
    {
        const double deg2rad = PI / 180d;
        const double rad2deg = 180d / PI;

        const double PI_2 = PI / 2;

        public static void Rotate(double roll, double pitch, double yaw, out double roll_out, out double pitch_out)
        {
            double roll_r = roll * deg2rad;
            double pitch_r = pitch * deg2rad;
            double yaw_r = yaw * deg2rad;

            double[] P_pitch = new double[3] { 1, 0, Tan(pitch_r) };
            double[] P_roll = new double[3] { 0, 1, -Tan(roll_r) };
            double[] Po = new double[3] { 0, 0, 0 };

            double[] normalVector = CalculateNormalVector(P_pitch, P_roll);

            double[] P_pitch_CAL = new double[3] { Cos(yaw_r), Sin(yaw_r), CalculateZ(normalVector, Po, Cos(yaw_r), Sin(yaw_r)) };
            double[] P_roll_CAL = new double[3] { Cos(yaw_r + PI_2), Sin(yaw_r + PI_2), CalculateZ(normalVector, Po, Cos(yaw_r + PI_2), Sin(yaw_r + PI_2)) };

            // Angles for that points
            double pitch_out_r = -Atan2(P_pitch_CAL[2], Sqrt(Pow(P_pitch_CAL[0], 2) + Pow(P_pitch_CAL[1], 2)));
            double roll_out_r = Atan2(P_roll_CAL[2], Sqrt(Pow(P_roll_CAL[0], 2) + Pow(P_roll_CAL[1], 2)));

            roll_out = roll_out_r * rad2deg;
            pitch_out = pitch_out_r * rad2deg;
        }

        private static double CalculateZ(double[] normalVector, double[] Po, double x, double y)
        {
            double A = normalVector[0] * (x - Po[0]);
            double B = normalVector[1] * (y - Po[1]);

            double z = ((-A - B) / normalVector[2]) + Po[2];

            return -z;
        }

        private static double[] CalculateNormalVector(double[] P_pitch, double[] P_roll)
        {
            Vector3D vP = new Vector3D(P_pitch[0], P_pitch[1], P_pitch[2]);
            Vector3D vR = new Vector3D(P_roll[0], P_roll[1], P_roll[2]);

            Vector3D res = Vector3D.CrossProduct(vP, vR);

            return new double[3] { res.X, res.Y, res.Z };
        }
    }

    public class HeadingCalculator
    {
        public static double HeadingFromVelocities(double vn, double ve)
        {
            double heading = Math.Atan2(ve, vn); // This function returns the angle, in radians, between - pi and pi.
            heading *= 180.0 / Math.PI;            // Convert from radians to degrees.Now heading is in the range[-180, 180]
            if (heading < 0.0)
                heading += 360.0;               // Get rid of negative headings.

            return heading;
        }
    }

    public class PositionTools
    {
        //public static void MoveGPSPointByOffset(double lat, double lon, double alt, double xMeter, double yMeter, double zMeter, out double latO, out double lonO, out double altO)
        //{

        //}
    }
}
