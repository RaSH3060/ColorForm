using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PaintMatcher
{
    public class LabColor
    {
        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }

        public LabColor(double l, double a, double b)
        {
            L = l;
            A = a;
            B = b;
        }

        public static double CalculateDeltaE(LabColor color1, LabColor color2)
        {
            // CIE76 Delta E calculation
            double deltaL = color1.L - color2.L;
            double deltaA = color1.A - color2.A;
            double deltaB = color1.B - color2.B;
            
            return Math.Sqrt(deltaL * deltaL + deltaA * deltaA + deltaB * deltaB);
        }

        public LabColor Add(LabColor other, double weight = 1.0)
        {
            return new LabColor(
                L + (other.L - 50) * weight, // Adjust based on how far from middle gray
                A + (other.A - 0) * weight,
                B + (other.B - 0) * weight
            );
        }
    }

    public class ColorConcentration
    {
        public double InkPercent { get; set; } // Percentage of ink in the mixture
        public double ExtenderPercent { get; set; } // Percentage of extender in the mixture
        public LabColor Lab { get; set; } // L*a*b* values at this concentration

        public ColorConcentration(double inkPercent, double extenderPercent, LabColor lab)
        {
            InkPercent = inkPercent;
            ExtenderPercent = extenderPercent;
            Lab = lab;
        }
    }

    public class PaintColor
    {
        public string Name { get; set; }
        public List<ColorConcentration> Concentrations { get; set; } = new List<ColorConcentration>();
        public Dictionary<string, LabColor> LearningAdjustments { get; set; } = new Dictionary<string, LabColor>();

        public PaintColor(string name)
        {
            Name = name;
        }

        public LabColor GetLabAtConcentration(double inkPercent)
        {
            // Find the two closest concentrations
            var concentrations = Concentrations.FindAll(c => c.InkPercent <= inkPercent).OrderByDescending(c => c.InkPercent).ToList();
            if (concentrations.Count == 0)
            {
                // If requested concentration is lower than the lowest available, use the lowest
                if (Concentrations.Count > 0)
                    return Concentrations.OrderBy(c => c.InkPercent).First().Lab;
                return new LabColor(50, 0, 0); // Default neutral
            }

            var lower = concentrations[0];
            if (Math.Abs(lower.InkPercent - inkPercent) < 0.001)
                return lower.Lab;

            // Find the next higher concentration
            var higher = Concentrations.FindAll(c => c.InkPercent >= inkPercent).OrderBy(c => c.InkPercent).FirstOrDefault();
            if (higher == null)
            {
                // If requested concentration is higher than the highest available, use the highest
                return Concentrations.OrderByDescending(c => c.InkPercent).First().Lab;
            }

            // Linear interpolation between two closest concentrations
            double ratio = (inkPercent - lower.InkPercent) / (higher.InkPercent - lower.InkPercent);
            LabColor interpolated = new LabColor(
                lower.Lab.L + (higher.Lab.L - lower.Lab.L) * ratio,
                lower.Lab.A + (higher.Lab.A - lower.Lab.A) * ratio,
                lower.Lab.B + (higher.Lab.B - lower.Lab.B) * ratio
            );

            // Apply learning adjustment if available
            string key = $"{inkPercent:F1}";
            if (LearningAdjustments.ContainsKey(key))
            {
                var adjustment = LearningAdjustments[key];
                interpolated = new LabColor(
                    interpolated.L + adjustment.L,
                    interpolated.A + adjustment.A,
                    interpolated.B + adjustment.B
                );
            }

            return interpolated;
        }
    }

    public class PaintProfile
    {
        public string Name { get; set; }
        public double BaseExtenderPercent { get; set; } // Always dispensed with colors
        public List<PaintColor> Colors { get; set; } = new List<PaintColor>();
        public Dictionary<string, LabColor> LearningAdjustments { get; set; } = new Dictionary<string, LabColor>();

        public PaintProfile(string name)
        {
            Name = name;
        }
    }

    public class AdjustmentResult
    {
        public Dictionary<string, double> Additives { get; set; } = new Dictionary<string, double>();
        public LabColor PredictedResult { get; set; }
        public double DeltaE { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AdjustmentSession
    {
        public PaintProfile Profile { get; set; }
        public LabColor CurrentLab { get; set; }
        public LabColor TargetLab { get; set; }
        public double CurrentAmount { get; set; } // in kg
        public double MaxAdditivePercent { get; set; } = 50.0; // Max 50% additive
        public Dictionary<string, double> SelectedColors { get; set; } = new Dictionary<string, double>();
        public List<AdjustmentStep> Steps { get; set; } = new List<AdjustmentStep>();
        public bool LearningEnabled { get; set; } = false;
    }

    public class AdjustmentStep
    {
        public Dictionary<string, double> Additives { get; set; } = new Dictionary<string, double>();
        public LabColor ExpectedResult { get; set; }
        public LabColor ActualResult { get; set; }
        public bool ResultRecorded { get; set; } = false;
    }
}