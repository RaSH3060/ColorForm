using System;
using System.Collections.Generic;
using System.Linq;

namespace PaintMatcher
{
    public class PaintMatcherLogic
    {
        public static AdjustmentResult CalculateAdjustment(AdjustmentSession session)
        {
            var result = new AdjustmentResult();
            
            // Validate inputs
            if (session.CurrentAmount <= 0)
            {
                result.ErrorMessage = "Current amount must be greater than 0";
                return result;
            }

            if (session.SelectedColors.Count == 0)
            {
                result.ErrorMessage = "At least one color must be selected";
                return result;
            }

            // Limit to maximum 5 selected colors
            var selectedColors = session.SelectedColors.Keys.Take(5).ToList();
            
            // Calculate the adjustment needed
            var additives = FindOptimalAdditives(
                session.CurrentLab, 
                session.TargetLab, 
                session.CurrentAmount,
                session.Profile,
                selectedColors,
                session.MaxAdditivePercent
            );

            if (additives == null)
            {
                result.ErrorMessage = "Could not find a suitable combination of additives";
                return result;
            }

            // Calculate predicted result
            var predictedResult = CalculateResultingColor(
                session.CurrentLab, 
                session.CurrentAmount, 
                additives, 
                session.Profile
            );

            result.Additives = additives;
            result.PredictedResult = predictedResult;
            result.DeltaE = LabColor.CalculateDeltaE(session.TargetLab, predictedResult);

            return result;
        }

        private static Dictionary<string, double> FindOptimalAdditives(
            LabColor currentLab, 
            LabColor targetLab, 
            double currentAmount, 
            PaintProfile profile, 
            List<string> selectedColors, 
            double maxAdditivePercent)
        {
            // Maximum additive amount allowed (in kg)
            double maxAdditiveAmount = (maxAdditivePercent / 100.0) * currentAmount;

            // Try different combinations of colors, starting with fewer colors
            for (int colorCount = 1; colorCount <= selectedColors.Count; colorCount++)
            {
                var combinations = GetCombinations(selectedColors, colorCount);
                
                foreach (var combination in combinations)
                {
                    var result = FindAdditivesForColors(currentLab, targetLab, currentAmount, profile, combination, maxAdditiveAmount);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null; // No suitable combination found
        }

        private static Dictionary<string, double> FindAdditivesForColors(
            LabColor currentLab, 
            LabColor targetLab, 
            double currentAmount, 
            PaintProfile profile, 
            List<string> colorNames, 
            double maxAdditiveAmount)
        {
            // This is a simplified approach - in a real implementation, 
            // this would involve more complex optimization algorithms
            var additives = new Dictionary<string, double>();
            
            // Calculate color differences needed
            double deltaL = targetLab.L - currentLab.L;
            double deltaA = targetLab.A - currentLab.A;
            double deltaB = targetLab.B - currentLab.B;

            // Distribute the adjustment among selected colors
            double totalAdjustment = Math.Abs(deltaL) + Math.Abs(deltaA) + Math.Abs(deltaB);
            if (totalAdjustment < 0.1) // Already very close
            {
                return new Dictionary<string, double>(); // No adjustment needed
            }

            double remainingAdditiveAmount = maxAdditiveAmount;
            double remainingL = deltaL;
            double remainingA = deltaA;
            double remainingB = deltaB;

            foreach (var colorName in colorNames)
            {
                var color = profile.Colors.FirstOrDefault(c => c.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase));
                if (color == null) continue;

                // Estimate how much of this color is needed to move toward target
                // This is a simplified estimation - real implementation would be more complex
                double estimatedAmount = EstimateColorAmount(remainingL, remainingA, remainingB, color, profile.BaseExtenderPercent);
                
                // Limit by remaining additive amount
                if (estimatedAmount > remainingAdditiveAmount)
                {
                    estimatedAmount = remainingAdditiveAmount;
                }

                if (estimatedAmount > 0)
                {
                    additives[colorName] = estimatedAmount;
                    remainingAdditiveAmount -= estimatedAmount;

                    // Update remaining differences based on this addition
                    var addedColor = CalculateResultingColor(currentLab, 1.0, new Dictionary<string, double> { { colorName, estimatedAmount } }, profile);
                    remainingL = targetLab.L - addedColor.L;
                    remainingA = targetLab.A - addedColor.A;
                    remainingB = targetLab.B - addedColor.B;
                }

                if (remainingAdditiveAmount <= 0.001) break; // No more additive allowed
            }

            // Verify the solution is valid
            if (additives.Values.Sum() <= maxAdditiveAmount)
            {
                return additives;
            }

            return null; // Solution exceeds limits
        }

        private static double EstimateColorAmount(double deltaL, double deltaA, double deltaB, PaintColor color, double baseExtenderPercent)
        {
            // Find a suitable concentration that moves toward the target
            // This is a simplified estimation
            double targetMagnitude = Math.Sqrt(deltaL * deltaL + deltaA * deltaA + deltaB * deltaB);
            if (targetMagnitude < 0.1) return 0;

            // Try a small concentration first
            var testColor = color.GetLabAtConcentration(1.0); // 1% concentration
            
            // Calculate how much this color would move us toward the target
            double lEffect = testColor.L - 50; // Base color is around L=50
            double aEffect = testColor.A - 0;  // Base color is around a=0
            double bEffect = testColor.B - 0;  // Base color is around b=0

            // Calculate how much of this color we'd need to move by the required amount
            double neededL = Math.Abs(deltaL) > 0.1 ? deltaL / lEffect : 0;
            double neededA = Math.Abs(deltaA) > 0.1 ? deltaA / aEffect : 0;
            double neededB = Math.Abs(deltaB) > 0.1 ? deltaB / bEffect : 0;

            // Take the average as a rough estimate
            double estimate = Math.Abs(neededL + neededA + neededB) / 3.0;
            
            // Limit to reasonable amounts
            if (estimate > 0.5) estimate = 0.5; // Max 500g per kg of base
            if (estimate < 0) estimate = 0;

            return Math.Abs(estimate);
        }

        public static LabColor CalculateResultingColor(
            LabColor baseColor, 
            double baseAmount, 
            Dictionary<string, double> additives, 
            PaintProfile profile)
        {
            // Calculate total weight
            double totalWeight = baseAmount;
            foreach (var additive in additives)
            {
                totalWeight += additive.Value;
            }

            if (totalWeight <= 0) return baseColor;

            // Calculate weighted average of colors
            double totalL = baseColor.L * baseAmount;
            double totalA = baseColor.A * baseAmount;
            double totalB = baseColor.B * baseAmount;

            foreach (var additive in additives)
            {
                var color = profile.Colors.FirstOrDefault(c => c.Name.Equals(additive.Key, StringComparison.OrdinalIgnoreCase));
                if (color != null)
                {
                    // Get the effective color of this additive (including base extender)
                    var additiveColor = color.GetLabAtConcentration(100.0); // Pure color
                    
                    totalL += additiveColor.L * additive.Value;
                    totalA += additiveColor.A * additive.Value;
                    totalB += additiveColor.B * additive.Value;
                }
            }

            // Weighted average
            double resultL = totalL / totalWeight;
            double resultA = totalA / totalWeight;
            double resultB = totalB / totalWeight;

            return new LabColor(resultL, resultA, resultB);
        }

        public static List<List<T>> GetCombinations<T>(List<T> list, int length)
        {
            var result = new List<List<T>>();
            if (length == 1)
            {
                foreach (var item in list)
                {
                    result.Add(new List<T> { item });
                }
                return result;
            }

            for (int i = 0; i <= list.Count - length; i++)
            {
                var remaining = list.Skip(i + 1).ToList();
                var subCombinations = GetCombinations(remaining, length - 1);
                
                foreach (var subCombination in subCombinations)
                {
                    var newCombination = new List<T> { list[i] };
                    newCombination.AddRange(subCombination);
                    result.Add(newCombination);
                }
            }

            return result;
        }

        public static void ApplyLearningAdjustment(AdjustmentSession session, LabColor actualResult)
        {
            if (!session.LearningEnabled) return;

            // Calculate the difference between expected and actual results
            var lastStep = session.Steps.LastOrDefault();
            if (lastStep == null) return;

            double deltaL = actualResult.L - lastStep.ExpectedResult.L;
            double deltaA = actualResult.A - lastStep.ExpectedResult.A;
            double deltaB = actualResult.B - lastStep.ExpectedResult.B;

            // Update learning adjustments for each color used
            foreach (var additive in lastStep.Additives)
            {
                var color = session.Profile.Colors.FirstOrDefault(c => c.Name.Equals(additive.Key, StringComparison.OrdinalIgnoreCase));
                if (color != null)
                {
                    // Calculate the concentration that was actually used
                    double concentration = (additive.Value / session.CurrentAmount) * 100;
                    string key = $"{concentration:F1}";

                    // Average the adjustment with existing values
                    if (color.LearningAdjustments.ContainsKey(key))
                    {
                        var existing = color.LearningAdjustments[key];
                        color.LearningAdjustments[key] = new LabColor(
                            (existing.L + deltaL) / 2,
                            (existing.A + deltaA) / 2,
                            (existing.B + deltaB) / 2
                        );
                    }
                    else
                    {
                        color.LearningAdjustments[key] = new LabColor(deltaL, deltaA, deltaB);
                    }
                }
            }
        }

        public static void ResetLearningAdjustments(PaintProfile profile)
        {
            profile.LearningAdjustments.Clear();
            foreach (var color in profile.Colors)
            {
                color.LearningAdjustments.Clear();
            }
        }
    }
}