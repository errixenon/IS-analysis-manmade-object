using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovaya30
{
    public partial class Form1
    {
        public class PhaseAnalysis
        {
            public string Label { get; set; }
            public double MPlus { get; set; }
            public double M { get; set; }
            public double MMinus { get; set; }
            public double APlus { get; set; }
            public double A { get; set; }
            public double AMinus { get; set; }
            public double R { get; set; }
            public double L { get; set; }
            public string StatusText { get; set; }
        }

        private List<PhaseAnalysis> CalculatePhaseForColumns(List<DataColumn> pointColumns, string labelPrefix)
        {
            DataTable source = GetCalculationTable();
            if (source == null || source.Rows.Count == 0 || pointColumns == null)
                return new List<PhaseAnalysis>();
            List<string> pointNames = pointColumns.Select(c => c.ColumnName).Where(n => source.Columns.Contains(n)).ToList();
            if (pointNames.Count == 0)
                return new List<PhaseAnalysis>();

            double eps = ReadEpsilon();
            int sensorCount = pointNames.Count;

            double[] baseValues = new double[sensorCount];
            double[] basePlus = new double[sensorCount];
            double[] baseMinus = new double[sensorCount];

            for (int j = 0; j < sensorCount; j++)
            {
                DataColumn col = source.Columns[pointNames[j]];
                baseValues[j] = ReadCellDouble(source.Rows[0], col);
                basePlus[j] = baseValues[j] + eps;
                baseMinus[j] = baseValues[j] - eps;
            }

            double baseM = Norm(baseValues);
            double baseMP = Norm(basePlus);
            double baseMM = Norm(baseMinus);
            List<PhaseAnalysis> report = new List<PhaseAnalysis>();

            for (int i = 0; i < source.Rows.Count; i++)
            {
                double[] current = new double[sensorCount];
                double[] currentPlus = new double[sensorCount];
                double[] currentMinus = new double[sensorCount];

                for (int j = 0; j < sensorCount; j++)
                {
                    DataColumn col = source.Columns[pointNames[j]];
                    current[j] = ReadCellDouble(source.Rows[i], col);
                    currentPlus[j] = current[j] + eps;
                    currentMinus[j] = current[j] - eps;
                }

                double m = Norm(current);
                double mp = Norm(currentPlus);
                double mm = Norm(currentMinus);
                double a = Angle(baseValues, current, baseM, m);
                double ap = Angle(basePlus, currentPlus, baseMP, mp);
                double am = Angle(baseMinus, currentMinus, baseMM, mm);

                double r = Math.Abs(mp - mm) / 2.0;
                double l = Math.Abs(m - baseM);

                report.Add(new PhaseAnalysis
                {
                    Label = Convert.ToString(source.Rows[i][0]),
                    M = Round(m),
                    MPlus = Round(mp),
                    MMinus = Round(mm),
                    A = Round(a),
                    APlus = Round(ap),
                    AMinus = Round(am),
                    R = Round(r),
                    L = Round(l),
                    StatusText = GetStateByLimit(l, r)
                });
            }

            return report;
        }

        private string GetStateByLimit(double l, double r)
        {
            double tolerance = Math.Max(1e-9, r * 0.01);
            if (l < r - tolerance) return "Не изменяемое";
            if (Math.Abs(l - r) <= tolerance) return "Предаварийное";
            return "Аварийное";
        }

        private double Norm(double[] values)
        {
            double sum = 0;
            foreach (double v in values)
                sum += v * v;
            return Math.Sqrt(sum);
        }

        private double Angle(double[] a, double[] b, double normA, double normB)
        {
            if (a == null || b == null || a.Length == 0 || b.Length == 0 || normA == 0 || normB == 0)
                return 0;

            int count = Math.Min(a.Length, b.Length);
            double dot = 0;
            for (int i = 0; i < count; i++)
                dot += a[i] * b[i];

            double projectionCoefficient = dot / (normA * normA);
            double orthogonalNormSquared = 0;
            for (int i = 0; i < count; i++)
            {
                double orthogonalComponent = b[i] - projectionCoefficient * a[i];
                orthogonalNormSquared += orthogonalComponent * orthogonalComponent;
            }

            double orthogonalNorm = Math.Sqrt(Math.Max(0, orthogonalNormSquared));
            return Math.Atan2(normA * orthogonalNorm, dot);
        }

        private double ResponseAngleByDisplacement(double[] baseVector, double[] currentVector)
        {
            if (baseVector == null || currentVector == null || baseVector.Length == 0 || currentVector.Length == 0)
                return 0;

            int count = Math.Min(baseVector.Length, currentVector.Length);
            double displacementSquared = 0;
            double baseSquared = 0;
            for (int i = 0; i < count; i++)
            {
                double d = currentVector[i] - baseVector[i];
                displacementSquared += d * d;
                baseSquared += baseVector[i] * baseVector[i];
            }

            double displacementNorm = Math.Sqrt(displacementSquared);
            double baseNorm = Math.Sqrt(baseSquared);
            if (baseNorm < 1e-12)
                baseNorm = 1e-12;

            return Math.Atan2(displacementNorm, baseNorm);
        }

        private double RadToArcSeconds(double radians)
        {
            return radians * 206265;
        }

        private double Round(double value)
        {
            return Math.Round(value, 6);

        }

        private List<double> ExponentialSmooth(List<double> values, double alpha)
        {
            List<double> result = new List<double>();
            if (values == null || values.Count == 0)
                return result;
            double smoothed = values[0];
            result.Add(smoothed);
            for (int i = 1; i < values.Count; i++)
            {
                smoothed = alpha * values[i] + (1.0 - alpha) * smoothed;
                result.Add(smoothed);
            }
            return result;
        }

        private double ForecastBySmoothing(List<double> values, double alpha)
        {
            if (values == null || values.Count == 0)
                return 0.0;
            double smoothed = values[0];
            for (int i = 1; i < values.Count; i++)
                smoothed = alpha * values[i] + (1.0 - alpha) * smoothed;
            return smoothed;
        }

    }
}
