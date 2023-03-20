using UnityEngine;

namespace opengen.maths.shapes
{
    public class RandomShapeGenerator
    {
        public static Vector2[] Generate(Vector2 center, float radius, float irregularity, float spikiness, int pointCount, uint seed = 58)
        {
            RandomGenerator rgen = new(seed);
            irregularity = Numbers.Clamp01(irregularity) * 2 * Mathf.PI / pointCount;
            spikiness = Numbers.Clamp01(spikiness) * radius;

            float[] angleSteps = new float[pointCount];
            float lower = (2 * Mathf.PI / pointCount) - irregularity;
            float upper = (2 * Mathf.PI / pointCount) + irregularity;
            float sum = 0;
            for (int i = 0; i < pointCount; i++)
            {
                float tmp = rgen.Range(lower, upper);
                angleSteps[i] = tmp;
                sum += tmp;
            }

            float k = sum / (2 * Mathf.PI);
            for (int i = 0; i < pointCount; i++)
            {
                angleSteps[i] = angleSteps[i] / k;
            }

            Vector2[] points = new Vector2[pointCount];
            float angle = rgen.Range(0f, 2 * Mathf.PI);
            for (int i = 0; i < pointCount; i++)
            {
                float rI = Mathf.Clamp(rgen.SampleGaussian(radius, spikiness), 0.1f, 2.0f * radius);
                float x = center.x + rI * Mathf.Cos(angle);
                float y = center.y + rI * Mathf.Sin(angle);
                points[i] = new Vector2(x, y);
                angle = angle + angleSteps[i];
            }


            return points;
        }
    }
}

/*
 
 https://stackoverflow.com/questions/8997099/algorithm-to-generate-random-2d-polygon
 
 import math, random

def generatePolygon( ctrX, ctrY, aveRadius, irregularity, spikeyness, numVerts ) :
'''Start with the centre of the polygon at ctrX, ctrY, 
    then creates the polygon by sampling points on a circle around the centre. 
    Randon noise is added by varying the angular spacing between sequential points,
    and by varying the radial distance of each point from the centre.

    Params:
    ctrX, ctrY - coordinates of the "centre" of the polygon
    aveRadius - in px, the average radius of this polygon, this roughly controls how large the polygon is, really only useful for order of magnitude.
    irregularity - [0,1] indicating how much variance there is in the angular spacing of vertices. [0,1] will map to [0, 2pi/numberOfVerts]
    spikeyness - [0,1] indicating how much variance there is in each vertex from the circle of radius aveRadius. [0,1] will map to [0, aveRadius]
    numVerts - self-explanatory

    Returns a list of vertices, in CCW order.
    '''

    irregularity = clip( irregularity, 0,1 ) * 2*math.pi / numVerts
    spikeyness = clip( spikeyness, 0,1 ) * aveRadius

    # generate n angle steps
    angleSteps = []
    lower = (2*math.pi / numVerts) - irregularity
    upper = (2*math.pi / numVerts) + irregularity
    sum = 0
    for i in range(numVerts) :
        tmp = random.uniform(lower, upper)
        angleSteps.append( tmp )
        sum = sum + tmp

    # normalize the steps so that point 0 and point n+1 are the same
    k = sum / (2*math.pi)
    for i in range(numVerts) :
        angleSteps[i] = angleSteps[i] / k

    # now generate the points
    points = []
    angle = random.uniform(0, 2*math.pi)
    for i in range(numVerts) :
        r_i = clip( random.gauss(aveRadius, spikeyness), 0, 2*aveRadius )
        x = ctrX + r_i*math.cos(angle)
        y = ctrY + r_i*math.sin(angle)
        points.append( (int(x),int(y)) )

        angle = angle + angleSteps[i]

    return points

 def clip(x, min, max) :
     if( min > max ) :  return x    
     elif( x < min ) :  return min
     elif( x > max ) :  return max
     else :             return x*/