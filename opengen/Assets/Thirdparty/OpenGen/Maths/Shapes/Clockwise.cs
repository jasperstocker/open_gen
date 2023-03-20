using System.Collections.Generic;
using opengen.types;
using UnityEngine;


namespace opengen.maths.shapes
{
    public class Clockwise
    {
        public static bool Check(IList<Vector2Fixed> points)
        {
            int numberOfPoints = points.Count;
            int i, j, k;
            int count = 0;
            float z;

            if (numberOfPoints < 3)
            {
	            return (false);
            }

            for (i = 0; i < numberOfPoints; i++)
            {
                j = (i + 1) % numberOfPoints;
                k = (i + 2) % numberOfPoints;

                Vector2 pointA = points[i].vector2;
                Vector2 pointB = points[j].vector2;
                Vector2 pointC = points[k].vector2;

                z = (pointB.x - pointA.x) * (pointC.y - pointA.y);
                z -= (pointB.y - pointA.y) * (pointC.x - pointA.x);

                if (z < 0)
                {
	                count--;
                }
                else if (z > 0)
                {
	                count++;
                }
            }

            if (count > 0)
            {
	            return (true);
            }
            else if (count < 0)
            {
	            return (false);
            }
            else
            {
	            return (false);
            }
        }

        public static bool Check(IList<Vector2> points) {
            int numberOfPoints = points.Count;
            int i, j, k;
            int count = 0;
            float z;

            if (numberOfPoints < 3)
            {
	            return (false);
            }

            for (i = 0; i < numberOfPoints; i++) {
                j = (i + 1) % numberOfPoints;
                k = (i + 2) % numberOfPoints;

                Vector2 pointA = points[i];
                Vector2 pointB = points[j];
                Vector2 pointC = points[k];

                z = (pointB.x - pointA.x) * (pointC.y - pointA.y);
                z -= (pointB.y - pointA.y) * (pointC.x - pointA.x);

                if (z < 0)
                {
	                count--;
                }
                else if (z > 0)
                {
	                count++;
                }
            }

            if (count > 0)
            {
	            return (true);
            }
            else if (count < 0)
            {
	            return (false);
            }
            else
            {
	            return (false);
            }
        }
        
	    public static bool Check(IList<Vector3> points)
	    {
		    int numberOfPoints = points.Count;
		    int i, j, k;
		    int count = 0;
		    float z;

		    if (numberOfPoints < 3)
		    {
			    return (false);
		    }

		    for (i = 0; i < numberOfPoints; i++)
		    {
			    j = (i + 1) % numberOfPoints;
			    k = (i + 2) % numberOfPoints;

			    Vector3 pointA = points[i];
			    Vector3 pointB = points[j];
			    Vector3 pointC = points[k];

			    z = (pointB.x - pointA.x) * (pointC.z - pointA.z);
			    z -= (pointB.z - pointA.z) * (pointC.x - pointA.x);

			    if (z < 0)
			    {
				    count--;
			    }
			    else if (z > 0)
			    {
				    count++;
			    }
		    }

		    if (count > 0)
		    {
			    return (true);
		    }
			return (false);
	    }
	}
}