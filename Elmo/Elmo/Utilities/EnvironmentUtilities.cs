using System;

namespace Elmo.Utilities
{
    internal static class EnvironmentUtilities
    {
        public static string GetMachineNameOrDefault(string defaultValue = "")
        {
            try
            {
                return Environment.MachineName;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
