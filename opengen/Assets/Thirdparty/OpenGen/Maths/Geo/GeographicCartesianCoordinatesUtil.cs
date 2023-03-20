using System;
using System.Collections;
using System.Collections.Generic;
using opengen.types;
using UnityEngine;


namespace opengen.maths.geo
{
    public class GeographicCartesianCoordinatesUtil
    {
        public static Vector2 CalculateEastNorthings(Vector2 input)
        {
            //This will not work unless you have your lats and longs in decimal degrees.
            input.x = Numbers.Deg2Rad * input.x;
            input.y = Numbers.Deg2Rad * input.y;
            float a = 6377563.396f, b = 6356256.910f;// Airy 1830 major &amp; minor semi-axes
            //float a = 6378137.0, b = 6356752.314245; WGS84 major &amp; minor semi-axes
            float F0 = 0.9996012717f;// NatGrid scale factor on central meridian
            float lat0 = Numbers.Deg2Rad * (49);
            float lon0 = Numbers.Deg2Rad * (-2);// NatGrid true origin
            float N0 = -100000, E0 = 400000;// northing &amp; easting of true origin, metres
            float e2 = 1 - (b * b) / (a * a);// eccentricity squared
            float n = (a - b) / (a + b), n2 = n * n, n3 = n * n * n;
            float cosLat = Mathf.Cos(input.y), sinLat = Mathf.Sin(input.y);
            float nu = a * F0 / Mathf.Sqrt(1 - e2 * sinLat * sinLat);// transverse radius of curvature
            float rho = a * F0 * (1 - e2) / Mathf.Pow(1 - e2 * sinLat * sinLat, 1.5f);// meridional radius of curvature
            float eta2 = nu / rho - 1;
            float Ma = (1 + n + (5 / 4) * n2 + (5 / 4) * n3) * (input.y - lat0);
            float Mb = (3 * n + 3 * n * n + (21 / 8) * n3) * Mathf.Sin(input.y - lat0) * Mathf.Cos(input.y + lat0);
            float Mc = ((15 / 8) * n2 + (15 / 8) * n3) * Mathf.Sin(2 * (input.y - lat0)) * Mathf.Cos(2 * (input.y + lat0));
            float Md = (35 / 24) * n3 * Mathf.Sin(3 * (input.y - lat0)) * Mathf.Cos(3 * (input.y + lat0));
            float M = b * F0 * (Ma - Mb + Mc - Md);// meridional arc
            float cos3lat = cosLat * cosLat * cosLat;
            float cos5lat = cos3lat * cosLat * cosLat;
            float tan2lat = Mathf.Tan(input.y) * Mathf.Tan(input.y);
            float tan4lat = tan2lat * tan2lat;
            float I = M + N0;
            float II = (nu / 2) * sinLat * cosLat;
            float III = (nu / 24) * sinLat * cos3lat * (5 - tan2lat + 9 * eta2);
            float IIIA = (nu / 720) * sinLat * cos5lat * (61 - 58 * tan2lat + tan4lat);
            float IV = nu * cosLat;
            float V = (nu / 6) * cos3lat * (nu / rho - tan2lat);
            float VI = (nu / 120) * cos5lat * (5 - 18 * tan2lat + tan4lat + 14 * eta2 - 58 * tan2lat * eta2);
            float dLon = input.x - lon0;
            float dLon2 = dLon * dLon, dLon3 = dLon2 * dLon, dLon4 = dLon3 * dLon, dLon5 = dLon4 * dLon, dLon6 = dLon5 * dLon;
            Vector2 output = new();
            output.y = I + II * dLon2 + III * dLon4 + IIIA * dLon6;//This is the northing
            output.x = E0 + IV * dLon + V * dLon3 + VI * dLon5;//This is the easting 
            return output;
        }

        //http://bugorfeature.blogspot.com/2006/10/converting-from-uk-eastingnorthing-to.html
        public static Vector2 CalculateLatLong(Vector2 input)
        {
            float E = input.x;
            float N = input.y;

            // Airy 1830 major & minor semi-axes
            float a = 6377563.396f;
            float b = 6356256.910f;

            // NatGrid scale factor on central meridian
            float F0 = 0.9996012717f;

            // NatGrid true origin
            float lat0 = 49 * Mathf.PI / 180f;
            float lon0 = -2 * Mathf.PI / 180f;

            // northing & easting of true origin, metres
            int N0 = -100000;
            int E0 = 400000;

            // eccentricity squared
            float e2 = 1 - (b * b) / (a * a);
            float n = (a - b) / (a + b), n2 = n * n, n3 = n * n * n;
            float lat = lat0;
            float M = 0;
            int maxIt = 100;
            while(N - N0 - M >= 1f)// until < 1 metre
            {
                lat = (N - N0 - M) / (a * F0) + lat;
                float Ma = (1 + n + (5 / 4) * n2 + (5 / 4) * n3) * (lat - lat0);
                float Mb = (3 * n + 3 * n * n + (21 / 8) * n3) * Mathf.Sin(lat - lat0) * Mathf.Cos(lat + lat0);
                float Mc = ((15 / 8) * n2 + (15 / 8) * n3) * Mathf.Sin(2 * (lat - lat0)) * Mathf.Cos(2 * (lat + lat0));
                float Md = (35 / 24) * n3 * Mathf.Sin(3 * (lat - lat0)) * Mathf.Cos(3 * (lat + lat0));
                M = b * F0 * (Ma - Mb + Mc - Md);// meridional arc
                maxIt--;
                if(maxIt == 0)
                {
                    throw new Exception("CalculateLatLong : MAX ITERATIONS "+(N - N0 - M));
                    break;
                }
            }

            float cosLat = Mathf.Cos(lat), sinLat = Mathf.Sin(lat);
            float nu = a * F0 / Mathf.Sqrt(1 - e2 * sinLat * sinLat);// transverse radius of curvature
            float rho = a * F0 * (1 - e2) / Mathf.Pow(1 - e2 * sinLat * sinLat, 1.5f);// meridional radius of curvature
            float eta2 = nu / rho - 1;
            float tanLat = Mathf.Tan(lat);
            float tan2lat = tanLat * tanLat, tan4lat = tan2lat * tan2lat, tan6lat = tan4lat * tan2lat;
            float secLat = 1 / cosLat;
            float nu3 = nu * nu * nu, nu5 = nu3 * nu * nu, nu7 = nu5 * nu * nu;
            float VII = tanLat / (2 * rho * nu);
            float VIII = tanLat / (24 * rho * nu3) * (5 + 3 * tan2lat + eta2 - 9 * tan2lat * eta2);
            float IX = tanLat / (720 * rho * nu5) * (61 + 90 * tan2lat + 45 * tan4lat);
            float X = secLat / nu;
            float XI = secLat / (6 * nu3) * (nu / rho + 2 * tan2lat);
            float XII = secLat / (120 * nu5) * (5 + 28 * tan2lat + 24 * tan4lat);
            float XIIA = secLat / (5040 * nu7) * (61 + 662 * tan2lat + 1320 * tan4lat + 720 * tan6lat);
            float dE = (E - E0), dE2 = dE * dE, dE3 = dE2 * dE, dE4 = dE2 * dE2, dE5 = dE3 * dE2, dE6 = dE4 * dE2, dE7 = dE5 * dE2;
            lat = lat - VII * dE2 + VIII * dE4 - IX * dE6;
            float lon = lon0 + X * dE - XI * dE3 + XII * dE5 - XIIA * dE7;
            Vector2 output = new();
            output.x = lon * 180 / Mathf.PI;
            output.y = lat * 180 / Mathf.PI;
            return output;
        }
    }
}