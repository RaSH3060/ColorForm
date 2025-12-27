using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace PaintMatcher
{
    public class ProfileManager
    {
        private List<PaintProfile> profiles = new List<PaintProfile>();
        
        public List<PaintProfile> Profiles => profiles;

        public void AddProfile(PaintProfile profile)
        {
            profiles.Add(profile);
        }

        public void RemoveProfile(string profileName)
        {
            profiles.RemoveAll(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        }

        public PaintProfile GetProfile(string profileName)
        {
            return profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        }

        public void SaveProfile(PaintProfile profile, string filePath)
        {
            string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public PaintProfile LoadProfile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<PaintProfile>(json);
        }

        public void SaveAllProfiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            foreach (var profile in profiles)
            {
                string filePath = Path.Combine(directoryPath, $"{profile.Name}.json");
                SaveProfile(profile, filePath);
            }
        }

        public void LoadAllProfiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            string[] files = Directory.GetFiles(directoryPath, "*.json");
            profiles.Clear();

            foreach (string file in files)
            {
                try
                {
                    var profile = LoadProfile(file);
                    profiles.Add(profile);
                }
                catch (Exception ex)
                {
                    // Log error but continue loading other profiles
                    Console.WriteLine($"Error loading profile from {file}: {ex.Message}");
                }
            }
        }

        public List<string> GetAllColorNames(string profileName)
        {
            var profile = GetProfile(profileName);
            if (profile == null) return new List<string>();

            return profile.Colors.Select(c => c.Name).ToList();
        }

        public PaintColor GetColor(string profileName, string colorName)
        {
            var profile = GetProfile(profileName);
            if (profile == null) return null;

            return profile.Colors.FirstOrDefault(c => c.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase));
        }

        public void AddColorToProfile(string profileName, PaintColor color)
        {
            var profile = GetProfile(profileName);
            if (profile != null)
            {
                // Check if color already exists and replace it
                int index = profile.Colors.FindIndex(c => c.Name.Equals(color.Name, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    profile.Colors[index] = color;
                }
                else
                {
                    profile.Colors.Add(color);
                }
            }
        }

        public void RemoveColorFromProfile(string profileName, string colorName)
        {
            var profile = GetProfile(profileName);
            if (profile != null)
            {
                profile.Colors.RemoveAll(c => c.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}