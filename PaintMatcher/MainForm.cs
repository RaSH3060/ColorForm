using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace PaintMatcher
{
    public partial class MainForm : Form
    {
        private ProfileManager profileManager = new ProfileManager();
        private AdjustmentSession currentSession = null;
        private DataGridView colorDataGrid;
        private DataGridView profileDataGrid;
        private DataGridView resultDataGrid;
        private TextBox currentLTextBox, currentATextBox, currentBTextBox;
        private TextBox targetLTextBox, targetATextBox, targetBTextBox;
        private TextBox currentAmountTextBox;
        private TextBox maxAdditivePercentTextBox;
        private ComboBox profileComboBox;
        private ListBox availableColorsListBox;
        private ListBox selectedColorsListBox;
        private Label resultLLabel, resultALabel, resultBLabel;
        private Label deltaELabel;
        private Button calculateButton;
        private Button addColorButton, removeColorButton;
        private Button saveProfileButton, loadProfileButton;
        private Button saveAllProfilesButton, loadAllProfilesButton;
        private Button resetLearningButton;
        private CheckBox learningCheckBox;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadSampleProfiles();
        }

        private void InitializeComponent()
        {
            this.Text = "Paint Matcher";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeCustomComponents()
        {
            // Profile selection
            var profileLabel = new Label { Text = "Profile:", Location = new Point(10, 10), Size = new Size(80, 20) };
            profileComboBox = new ComboBox { Location = new Point(100, 10), Size = new Size(200, 20) };
            profileComboBox.SelectedIndexChanged += ProfileComboBox_SelectedIndexChanged;
            
            saveProfileButton = new Button { Text = "Save Profile", Location = new Point(320, 10), Size = new Size(100, 25) };
            saveProfileButton.Click += SaveProfileButton_Click;
            
            loadProfileButton = new Button { Text = "Load Profile", Location = new Point(430, 10), Size = new Size(100, 25) };
            loadProfileButton.Click += LoadProfileButton_Click;
            
            saveAllProfilesButton = new Button { Text = "Save All", Location = new Point(540, 10), Size = new Size(80, 25) };
            saveAllProfilesButton.Click += SaveAllProfilesButton_Click;
            
            loadAllProfilesButton = new Button { Text = "Load All", Location = new Point(630, 10), Size = new Size(80, 25) };
            loadAllProfilesButton.Click += LoadAllProfilesButton_Click;
            
            resetLearningButton = new Button { Text = "Reset Learning", Location = new Point(720, 10), Size = new Size(100, 25) };
            resetLearningButton.Click += ResetLearningButton_Click;
            
            learningCheckBox = new CheckBox { Text = "Enable Learning", Location = new Point(830, 12), Size = new Size(120, 20) };

            // Current paint properties
            var currentGroup = new GroupBox { Text = "Current Paint", Location = new Point(10, 40), Size = new Size(300, 100) };
            var currentLLabel = new Label { Text = "L*:", Location = new Point(10, 25), Size = new Size(30, 20) };
            currentLTextBox = new TextBox { Location = new Point(45, 25), Size = new Size(60, 20), Text = "73" };
            var currentALabel = new Label { Text = "a*:", Location = new Point(115, 25), Size = new Size(30, 20) };
            currentATextBox = new TextBox { Location = new Point(150, 25), Size = new Size(60, 20), Text = "22" };
            var currentBLabel = new Label { Text = "b*:", Location = new Point(220, 25), Size = new Size(30, 20) };
            currentBTextBox = new TextBox { Location = new Point(255, 25), Size = new Size(60, 20), Text = "59" };
            
            var currentAmountLabel = new Label { Text = "Amount (kg):", Location = new Point(10, 55), Size = new Size(80, 20) };
            currentAmountTextBox = new TextBox { Location = new Point(95, 55), Size = new Size(80, 20), Text = "11.240" };
            
            var maxAdditiveLabel = new Label { Text = "Max Add%:", Location = new Point(185, 55), Size = new Size(70, 20) };
            maxAdditivePercentTextBox = new TextBox { Location = new Point(255, 55), Size = new Size(60, 20), Text = "50" };

            currentGroup.Controls.AddRange(new Control[] { 
                currentLLabel, currentLTextBox, currentALabel, currentATextBox, 
                currentBLabel, currentBTextBox, currentAmountLabel, currentAmountTextBox,
                maxAdditiveLabel, maxAdditivePercentTextBox
            });

            // Target paint properties
            var targetGroup = new GroupBox { Text = "Target Paint", Location = new Point(320, 40), Size = new Size(300, 100) };
            var targetLLabel = new Label { Text = "L*:", Location = new Point(10, 25), Size = new Size(30, 20) };
            targetLTextBox = new TextBox { Location = new Point(45, 25), Size = new Size(60, 20), Text = "67.6" };
            var targetALabel = new Label { Text = "a*:", Location = new Point(115, 25), Size = new Size(30, 20) };
            targetATextBox = new TextBox { Location = new Point(150, 25), Size = new Size(60, 20), Text = "-40.5" };
            var targetBLabel = new Label { Text = "b*:", Location = new Point(220, 25), Size = new Size(30, 20) };
            targetBTextBox = new TextBox { Location = new Point(255, 25), Size = new Size(60, 20), Text = "48.1" };

            targetGroup.Controls.AddRange(new Control[] { 
                targetLLabel, targetLTextBox, targetALabel, targetATextBox, 
                targetBLabel, targetBTextBox
            });

            // Available colors
            var availableColorsLabel = new Label { Text = "Available Colors:", Location = new Point(10, 150), Size = new Size(100, 20) };
            availableColorsListBox = new ListBox { Location = new Point(10, 175), Size = new Size(150, 200) };
            
            addColorButton = new Button { Text = "→", Location = new Point(165, 225), Size = new Size(30, 30) };
            addColorButton.Click += AddColorButton_Click;
            
            removeColorButton = new Button { Text = "←", Location = new Point(165, 265), Size = new Size(30, 30) };
            removeColorButton.Click += RemoveColorButton_Click;
            
            var selectedColorsLabel = new Label { Text = "Selected Colors:", Location = new Point(200, 150), Size = new Size(100, 20) };
            selectedColorsListBox = new ListBox { Location = new Point(200, 175), Size = new Size(150, 200) };

            // Calculate button
            calculateButton = new Button { Text = "Calculate Adjustment", Location = new Point(10, 390), Size = new Size(150, 30) };
            calculateButton.Click += CalculateButton_Click;

            // Results
            var resultsGroup = new GroupBox { Text = "Results", Location = new Point(10, 430), Size = new Size(500, 200) };
            resultLLabel = new Label { Text = "L*: -", Location = new Point(10, 25), Size = new Size(100, 20) };
            resultALabel = new Label { Text = "a*: -", Location = new Point(10, 50), Size = new Size(100, 20) };
            resultBLabel = new Label { Text = "b*: -", Location = new Point(10, 75), Size = new Size(100, 20) };
            deltaELabel = new Label { Text = "ΔE: -", Location = new Point(10, 100), Size = new Size(200, 20) };
            
            resultDataGrid = new DataGridView { 
                Location = new Point(10, 125), 
                Size = new Size(480, 65),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            resultDataGrid.Columns.Add("Color", "Color");
            resultDataGrid.Columns.Add("Amount", "Amount (kg)");
            resultDataGrid.ReadOnly = true;
            resultDataGrid.AllowUserToAddRows = false;

            resultsGroup.Controls.AddRange(new Control[] { 
                resultLLabel, resultALabel, resultBLabel, deltaELabel, resultDataGrid
            });

            // Color data grid for profile editing
            var colorDataLabel = new Label { Text = "Color Concentrations:", Location = new Point(630, 150), Size = new Size(150, 20) };
            colorDataGrid = new DataGridView { 
                Location = new Point(630, 175), 
                Size = new Size(540, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            colorDataGrid.Columns.Add("InkPercent", "Ink %");
            colorDataGrid.Columns.Add("ExtenderPercent", "Extender %");
            colorDataGrid.Columns.Add("L", "L*");
            colorDataGrid.Columns.Add("A", "a*");
            colorDataGrid.Columns.Add("B", "b*");
            colorDataGrid.AllowUserToAddRows = true;

            // Profile data grid
            var profileDataLabel = new Label { Text = "Profile Colors:", Location = new Point(630, 40), Size = new Size(100, 20) };
            profileDataGrid = new DataGridView { 
                Location = new Point(630, 65), 
                Size = new Size(540, 75),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            profileDataGrid.Columns.Add("ColorName", "Color Name");
            profileDataGrid.Columns.Add("BaseExtender", "Base Extender %");
            profileDataGrid.AllowUserToAddRows = true;

            // Add all controls to form
            this.Controls.AddRange(new Control[] {
                profileLabel, profileComboBox, saveProfileButton, loadProfileButton,
                saveAllProfilesButton, loadAllProfilesButton, resetLearningButton, learningCheckBox,
                currentGroup, targetGroup, availableColorsLabel, availableColorsListBox,
                addColorButton, removeColorButton, selectedColorsLabel, selectedColorsListBox,
                calculateButton, resultsGroup, colorDataLabel, colorDataGrid,
                profileDataLabel, profileDataGrid
            });
        }

        private void LoadSampleProfiles()
        {
            // Create a sample PET profile
            var petProfile = new PaintProfile("PET")
            {
                BaseExtenderPercent = 30.0
            };

            // Add a sample CYAN color with multiple concentrations
            var cyanColor = new PaintColor("CYAN");
            cyanColor.Concentrations.Add(new ColorConcentration(0.5, 0.5, new LabColor(99.5, 91.0, -7.0)));
            cyanColor.Concentrations.Add(new ColorConcentration(2.0, 2.0, new LabColor(98.0, 4.4, -17.1)));
            cyanColor.Concentrations.Add(new ColorConcentration(5.0, 5.0, new LabColor(85.2, -25.3, -22.8)));
            cyanColor.Concentrations.Add(new ColorConcentration(10.0, 10.0, new LabColor(65.1, -45.2, -15.6)));
            
            petProfile.Colors.Add(cyanColor);

            // Add another color - MAGENTA
            var magentaColor = new PaintColor("MAGENTA");
            magentaColor.Concentrations.Add(new ColorConcentration(0.5, 0.5, new LabColor(85.3, 25.2, 2.8)));
            magentaColor.Concentrations.Add(new ColorConcentration(2.0, 2.0, new LabColor(72.1, 45.8, -5.2)));
            magentaColor.Concentrations.Add(new ColorConcentration(5.0, 5.0, new LabColor(52.4, 62.5, 18.3)));
            
            petProfile.Colors.Add(magentaColor);

            // Add YELLOW
            var yellowColor = new PaintColor("YELLOW");
            yellowColor.Concentrations.Add(new ColorConcentration(0.5, 0.5, new LabColor(95.6, -5.2, 85.3)));
            yellowColor.Concentrations.Add(new ColorConcentration(2.0, 2.0, new LabColor(89.4, -12.5, 75.8)));
            yellowColor.Concentrations.Add(new ColorConcentration(5.0, 5.0, new LabColor(78.2, -22.1, 65.4)));
            
            petProfile.Colors.Add(yellowColor);

            profileManager.AddProfile(petProfile);
            
            // Refresh UI
            RefreshProfileList();
            profileComboBox.SelectedIndex = 0;
        }

        private void RefreshProfileList()
        {
            profileComboBox.Items.Clear();
            foreach (var profile in profileManager.Profiles)
            {
                profileComboBox.Items.Add(profile.Name);
            }
        }

        private void ProfileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedProfile = profileComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedProfile)) return;

            var profile = profileManager.GetProfile(selectedProfile);
            if (profile == null) return;

            // Update available colors list
            availableColorsListBox.Items.Clear();
            foreach (var color in profile.Colors)
            {
                availableColorsListBox.Items.Add(color.Name);
            }

            // Update profile data grid
            profileDataGrid.Rows.Clear();
            foreach (var color in profile.Colors)
            {
                profileDataGrid.Rows.Add(color.Name, profile.BaseExtenderPercent.ToString("F1"));
            }
        }

        private void AddColorButton_Click(object sender, EventArgs e)
        {
            if (availableColorsListBox.SelectedItem != null)
            {
                string colorName = availableColorsListBox.SelectedItem.ToString();
                if (!selectedColorsListBox.Items.Contains(colorName))
                {
                    selectedColorsListBox.Items.Add(colorName);
                }
            }
        }

        private void RemoveColorButton_Click(object sender, EventArgs e)
        {
            if (selectedColorsListBox.SelectedItem != null)
            {
                selectedColorsListBox.Items.Remove(selectedColorsListBox.SelectedItem);
            }
        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            if (profileComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a profile.");
                return;
            }

            string profileName = profileComboBox.SelectedItem.ToString();
            var profile = profileManager.GetProfile(profileName);
            if (profile == null)
            {
                MessageBox.Show("Selected profile not found.");
                return;
            }

            // Parse inputs
            if (!double.TryParse(currentLTextBox.Text, out double currentL) ||
                !double.TryParse(currentATextBox.Text, out double currentA) ||
                !double.TryParse(currentBTextBox.Text, out double currentB))
            {
                MessageBox.Show("Invalid current L*a*b* values.");
                return;
            }

            if (!double.TryParse(targetLTextBox.Text, out double targetL) ||
                !double.TryParse(targetATextBox.Text, out double targetA) ||
                !double.TryParse(targetBTextBox.Text, out double targetB))
            {
                MessageBox.Show("Invalid target L*a*b* values.");
                return;
            }

            if (!double.TryParse(currentAmountTextBox.Text, out double currentAmount) || currentAmount <= 0)
            {
                MessageBox.Show("Invalid current amount.");
                return;
            }

            if (!double.TryParse(maxAdditivePercentTextBox.Text, out double maxAdditivePercent) || maxAdditivePercent <= 0)
            {
                MessageBox.Show("Invalid max additive percentage.");
                return;
            }

            // Create session
            currentSession = new AdjustmentSession
            {
                Profile = profile,
                CurrentLab = new LabColor(currentL, currentA, currentB),
                TargetLab = new LabColor(targetL, targetA, targetB),
                CurrentAmount = currentAmount,
                MaxAdditivePercent = maxAdditivePercent,
                LearningEnabled = learningCheckBox.Checked
            };

            // Add selected colors
            foreach (string colorName in selectedColorsListBox.Items)
            {
                currentSession.SelectedColors[colorName] = 0; // Amount will be calculated
            }

            // Calculate adjustment
            var result = PaintMatcherLogic.CalculateAdjustment(currentSession);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                MessageBox.Show($"Error: {result.ErrorMessage}");
                return;
            }

            // Display results
            resultLLabel.Text = $"L*: {result.PredictedResult.L:F2}";
            resultALabel.Text = $"a*: {result.PredictedResult.A:F2}";
            resultBLabel.Text = $"b*: {result.PredictedResult.B:F2}";
            deltaELabel.Text = $"ΔE: {result.DeltaE:F2}";

            // Clear and populate result grid
            resultDataGrid.Rows.Clear();
            foreach (var additive in result.Additives)
            {
                resultDataGrid.Rows.Add(additive.Key, additive.Value.ToString("F3"));
            }
        }

        private void SaveProfileButton_Click(object sender, EventArgs e)
        {
            if (profileComboBox.SelectedItem == null) return;

            string profileName = profileComboBox.SelectedItem.ToString();
            var profile = profileManager.GetProfile(profileName);
            if (profile == null) return;

            var saveDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"{profileName}.json"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                profileManager.SaveProfile(profile, saveDialog.FileName);
            }
        }

        private void LoadProfileButton_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var profile = profileManager.LoadProfile(openDialog.FileName);
                    profileManager.AddProfile(profile);
                    RefreshProfileList();
                    
                    // Select the loaded profile
                    profileComboBox.SelectedItem = profile.Name;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading profile: {ex.Message}");
                }
            }
        }

        private void SaveAllProfilesButton_Click(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                profileManager.SaveAllProfiles(folderDialog.SelectedPath);
            }
        }

        private void LoadAllProfilesButton_Click(object sender, EventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                profileManager.LoadAllProfiles(folderDialog.SelectedPath);
                RefreshProfileList();
            }
        }

        private void ResetLearningButton_Click(object sender, EventArgs e)
        {
            if (profileComboBox.SelectedItem == null) return;

            string profileName = profileComboBox.SelectedItem.ToString();
            var profile = profileManager.GetProfile(profileName);
            if (profile == null) return;

            PaintMatcherLogic.ResetLearningAdjustments(profile);
            MessageBox.Show("Learning adjustments reset for the selected profile.");
        }
    }
}