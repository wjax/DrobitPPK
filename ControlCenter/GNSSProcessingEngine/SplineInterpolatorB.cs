using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlCenter.GNSSProcessingEngine
{
    class SplineInterpolatorB
    {
        int CAPACITY = 3;

        List<KeyValuePair<double, double>> values = new List<KeyValuePair<double, double>>();

        /*public bool push(double value)
        {
            return push_value(value, DateTime.Now.Ticks);
        }*/

        public SplineInterpolatorB(int capacity)
        {
            CAPACITY = capacity;
        }
        public SplineInterpolatorB()
        {

        }


        public bool push(double value, double ticks)
        {
            return push_value(value, ticks);
        }

        private bool push_value(double value, double ticks)
        {
            bool bResult = false;
            if (values.Count < CAPACITY)
            {
                values.Add(new KeyValuePair<double, double>(ticks, value));
                bResult = true;

            }
            else if (values.Count >= CAPACITY)
            {
                // remove first one to keep the list as CAPACITY
                values.RemoveAt(0);
                values.Add(new KeyValuePair<double, double>(ticks, value));
                bResult = true;
            }
            return bResult;
        }

        public bool ready()
        { 
            bool bResult = false;
            if (values.Count >= CAPACITY)
                bResult = true;
            return bResult;
        }

        public double value_estimate_spline()
        {
            long now = DateTime.Now.Ticks;
            return SpLine(values, now);
        }

        public double value_estimate_spline(double ticks)
        {
            if (values.Count > 0)
            {
                if (ticks > values[values.Count - 1].Key)
                    return values[values.Count - 1].Value;
                else
                    return SpLine(values, ticks);
            }
            else
                return 0;

        }

        public void clear()
        {
            values.Clear();
        }


        public static double SpLine(List<KeyValuePair<double, double>> knownSamples, double z)
        {
            try
            {

                int np = knownSamples.Count;
                if (np > 1)
                {
                    double[] a = new double[np];
                    double x1;
                    double x2;
                    double y;

                    double[] h = new double[np];

                    for (int i = 1; i <= np - 1; i++)
                    {
                        h[i] = knownSamples[i].Key - knownSamples[i - 1].Key;
                    }

                    if (np > 2)
                    {
                        double[] sub = new double[np - 1];
                        double[] diag = new double[np - 1];
                        double[] sup = new double[np - 1];
                        for (int i = 1; i <= np - 2; i++)
                        {
                            diag[i] = (h[i] + h[i + 1]) / 3;
                            sup[i] = h[i + 1] / 6;
                            sub[i] = h[i] / 6;
                            a[i] = (knownSamples[i + 1].Value - knownSamples[i].Value) / h[i + 1] -
                                   (knownSamples[i].Value - knownSamples[i - 1].Value) / h[i];

                        }
                        // SolveTridiag is a support function, see Marco Roello's original code
                        // for more information at
                        // http://www.codeproject.com/useritems/SplineInterpolation.asp
                        solveTridiag(sub, diag, sup, ref a, np - 2);
                    }

                    int gap = 0;
                    double previous = 0.0;

                    // At the end of this iteration, "gap" will contain the index of the interval
                    // between two known values, which contains the unknown z, and "previous" will
                    // contain the biggest z value among the known samples, left of the unknown z

                    for (int i = 0; i < knownSamples.Count; i++)
                    {
                        if (knownSamples[i].Key < z && knownSamples[i].Key > previous)
                        {
                            previous = knownSamples[i].Key;
                            gap = i + 1;
                        }
                    }
                    x1 = z - previous;
                    x2 = h[gap] - x1;
                    y = ((-a[gap - 1] / 6 * (x2 + h[gap]) * x1 + knownSamples[gap - 1].Value) * x2 +
                        (-a[gap] / 6 * (x1 + h[gap]) * x2 + knownSamples[gap].Value) * x1) / h[gap];

                    return y;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
            return 0;
        }

        private static void solveTridiag(double[] sub, double[] diag, double[] sup, ref double[] b, int n)
        {
            /*                  solve linear system with tridiagonal n by n matrix a
                                using Gaussian elimination *without* pivoting
                                where   a(i,i-1) = sub[i]  for 2<=i<=n
                                        a(i,i)   = diag[i] for 1<=i<=n
                                        a(i,i+1) = sup[i]  for 1<=i<=n-1
                                (the values sub[1], sup[n] are ignored)
                                right hand side vector b[1:n] is overwritten with solution 
                                NOTE: 1...n is used in all arrays, 0 is unused */
            int i;
            /*                  factorization and forward substitution */
            for (i = 2; i <= n; i++)
            {
                sub[i] = sub[i] / diag[i - 1];
                diag[i] = diag[i] - sub[i] * sup[i - 1];
                b[i] = b[i] - sub[i] * b[i - 1];
            }
            b[n] = b[n] / diag[n];
            for (i = n - 1; i >= 1; i--)
            {
                b[i] = (b[i] - sup[i] * b[i + 1]) / diag[i];
            }
        }

    }
}
