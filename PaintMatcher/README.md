# Paint Matcher Application

A comprehensive paint matching application that helps users match paint colors by calculating the precise amounts of additives needed to reach a target L*a*b* color specification.

## Features

### Profile Management
- Create and manage paint profiles (e.g., PET profile)
- Each profile contains:
  - Base extender percentage (always dispensed with colors)
  - Color definitions with L*a*b* values at different concentrations
- Import/Export profiles to JSON files

### Color Definition System
- Define colors within profiles with concentration % and corresponding L*a*b* values
- Example:
  - % CYAN INK | Extender% | L* | a* | b*
  - 0.5% | 0.5% | 99.5 | 91.0 | -7.0
  - 2% | 2% | 98% | 4.4 | -17.1

### Matching Algorithm
- Input: Current paint L*a*b*, Target L*a*b*, Current paint amount
- Output: Required additives to reach target
- Use minimum number of colors (max 4-5)
- Respect maximum additive percentage (max 50% of base paint)
- Allow user to limit maximum additive amount

### Adjustment Mode
- After initial mixing, user can input actual amounts added
- Compare expected vs actual results
- Learn from discrepancies to improve future predictions
- Option to reset learning data without affecting profiles

### Real-time Calculations
- Support kg and grams (e.g., 10.264 kg)
- Show predicted result during adjustment
- Allow user to experiment with different additives

### User Interface
- Profile selection
- Current/target L*a*b* input
- Available colors selection
- Adjustment controls
- Results display with amounts in kg/g
- Deviation display (dL*, da*, db*)

## How to Use

### Building the Application
```bash
cd /workspace/PaintMatcher
dotnet build
```

### Running the Application
```bash
dotnet run
```

### Using the Application
1. Select a paint profile from the dropdown
2. Enter current paint L*a*b* values and amount in kg
3. Enter target paint L*a*b* values
4. Select colors to use for adjustment from the available colors list
5. Click "Calculate Adjustment" to get the recommended additive amounts
6. The results will show the predicted L*a*b* values and ΔE (color difference)
7. The additive amounts needed will be displayed in kg

### Profile Management
- Save individual profiles using the "Save Profile" button
- Load individual profiles using the "Load Profile" button
- Save all profiles using the "Save All" button
- Load all profiles using the "Load All" button
- Reset learning adjustments using the "Reset Learning" button

### Learning Feature
- Enable learning by checking the "Enable Learning" checkbox
- The application will learn from discrepancies between expected and actual results
- This improves future predictions without changing the original profile data

## Example Use Case

Current paint: L*73 a*22 b*59, Amount: 11.240 kg
Target paint: L*67.6 a*-40.5 b*48.1

1. Select the PET profile
2. Enter current L*a*b* values (73, 22, 59) and amount (11.240 kg)
3. Enter target L*a*b* values (67.6, -40.5, 48.1)
4. Select colors like CYAN, MAGENTA, YELLOW from available colors
5. Click "Calculate Adjustment"
6. The application will recommend additive amounts to reach the target color
7. Additives will be limited to 50% of the base paint amount by default

## Technical Details

The application uses:
- C# with .NET 6.0
- Windows Forms for the GUI
- Newtonsoft.Json for profile serialization
- CIE76 Delta E calculation for color difference measurement
- Linear interpolation for color concentration estimation
- Combination algorithms to find optimal color mixes

The matching algorithm prioritizes using the minimum number of colors (1-5) to achieve the target color while respecting maximum additive limits.