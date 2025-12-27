# Paint Matcher Application - Functional Requirements

## Core Features

### 1. Profile Management
- Create/Manage paint profiles (e.g., PET profile)
- Each profile contains:
  - Base extender percentage (always dispensed with colors)
  - Color definitions with L*a*b* values at different concentrations
- Import/Export profiles to file

### 2. Color Definition System
- Define colors within profiles with concentration % and corresponding L*a*b* values
- Example:
  - % CYAN INK | Extender% | L* | a* | b*
  - 0.5% | 0.5% | 99.5 | 91.0 | -7.0
  - 2% | 2% | 98% | 4.4 | -17.1

### 3. Matching Algorithm
- Input: Current paint L*a*b*, Target L*a*b*, Current paint amount
- Output: Required additives to reach target
- Use minimum number of colors (max 4-5)
- Respect maximum additive percentage (max 50% of base paint)
- Allow user to limit maximum additive amount

### 4. Adjustment Mode
- After initial mixing, user can input actual amounts added
- Compare expected vs actual results
- Learn from discrepancies to improve future predictions
- Option to reset learning data without affecting profiles

### 5. Real-time Calculations
- Support kg and grams (e.g., 10.264 kg)
- Show predicted result during adjustment
- Allow user to experiment with different additives

### 6. User Interface
- Profile selection
- Current/target L*a*b* input
- Available colors selection
- Adjustment controls
- Results display with amounts in kg/g
- Deviation display (dL*, da*, db*)

## Technical Implementation Plan

### Data Models
- Profile: Contains name, base extender%, list of colors
- Color: Name, concentration-L*a*b* mappings
- AdjustmentSession: Current state, learning data

### Algorithms
- Color matching: Optimization to minimize color difference using fewest additives
- Learning: Adjust expected values based on actual results
- Prediction: Calculate resulting L*a*b* from given additives

### UI Components
- Profile management
- Color library viewer
- Matching calculator
- Adjustment interface
- Learning controls