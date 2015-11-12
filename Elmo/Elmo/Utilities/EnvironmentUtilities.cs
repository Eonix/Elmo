using System;

namespace Elmo.Utilities
{
    public static class EnvironmentUtilities
    {
        public static string GetMachineNameOrDefault(string defaultValue = "")
        {
            try
            {
                return Environment.MachineName;
            }
            catch (InvalidOperationException)
            {
                return defaultValue;
            }
        }
    }
}
