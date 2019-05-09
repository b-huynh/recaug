using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    private static System.Random rand = new System.Random();

    public static double SampleGaussian(double mean, double stddev)
    {
        // The method requires sampling from a uniform random of (0,1]  
        // but Random.NextDouble() returns a sample of [0,1).
        double x1 = 1 - rand.NextDouble();
        double x2 = 1 - rand.NextDouble();

        double y1 = System.Math.Sqrt(-2.0 * System.Math.Log(x1)) * System.Math.Cos(2.0 * System.Math.PI * x2);
        return y1 * stddev + mean;
    }
}
