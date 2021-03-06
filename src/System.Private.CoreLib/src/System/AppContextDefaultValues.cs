// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace System
{
    internal static partial class AppContextDefaultValues
    {
        public static bool TryGetSwitchOverride(string name, out bool overrideValue)
        {
            bool overrideFound = false;
            bool localValue = false;
            TryGetSwitchOverridePartial(name, ref overrideFound, ref localValue);
            overrideValue = localValue;
            return overrideFound;
        }

        public static void PopulateDefaultValues()
        {
            string platformIdentifier, profile;
            int version;

            // Call into each library to populate their default switches
            PopulateDefaultValuesPartial(platformIdentifier, profile, version);
        }

        // This code was a constructor copied from the FrameworkName class, which is located in System.dll.
        // Parses strings in the following format: "<identifier>, Version=[v|V]<version>, Profile=<profile>"
        //  - The identifier and version is required, profile is optional
        //  - Only three components are allowed.
        //  - The version string must be in the System.Version format; an optional "v" or "V" prefix is allowed
        private static bool TryParseFrameworkName(string frameworkName, out string identifier, out int version, out string profile)
        {
            // For parsing a target Framework moniker, from the FrameworkName class
            const char c_componentSeparator = ',';
            const char c_keyValueSeparator = '=';
            const char c_versionValuePrefix = 'v';
            const string c_versionKey = "Version";
            const string c_profileKey = "Profile";

            identifier = profile = string.Empty;
            version = 0;

            if (frameworkName == null || frameworkName.Length == 0)
            {
                return false;
            }

            string[] components = frameworkName.Split(c_componentSeparator);
            version = 0;

            // Identifer and Version are required, Profile is optional.
            if (components.Length < 2 || components.Length > 3)
            {
                return false;
            }

            //
            // 1) Parse the "Identifier", which must come first. Trim any whitespace
            //
            identifier = components[0].Trim();

            if (identifier.Length == 0)
            {
                return false;
            }

            bool versionFound = false;
            profile = null;

            // 
            // The required "Version" and optional "Profile" component can be in any order
            //
            for (int i = 1; i < components.Length; i++)
            {
                // Get the key/value pair separated by '='
                string[] keyValuePair = components[i].Split(c_keyValueSeparator);

                if (keyValuePair.Length != 2)
                {
                    return false;
                }

                // Get the key and value, trimming any whitespace
                string key = keyValuePair[0].Trim();
                string value = keyValuePair[1].Trim();

                //
                // 2) Parse the required "Version" key value
                //
                if (key.Equals(c_versionKey, StringComparison.OrdinalIgnoreCase))
                {
                    versionFound = true;

                    // Allow the version to include a 'v' or 'V' prefix...
                    if (value.Length > 0 && (value[0] == c_versionValuePrefix || value[0] == 'V'))
                    {
                        value = value.Substring(1);
                    }
                    Version realVersion = Version.Parse(value);
                    // The version class will represent some unset values as -1 internally (instead of 0).
                    version = realVersion.Major * 10000;
                    if (realVersion.Minor > 0)
                        version += realVersion.Minor * 100;
                    if (realVersion.Build > 0)
                        version += realVersion.Build;
                }
                //
                // 3) Parse the optional "Profile" key value
                //
                else if (key.Equals(c_profileKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        profile = value;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (!versionFound)
            {
                return false;
            }

            return true;
        }

        // This is a partial method. Platforms (such as Desktop) can provide an implementation of it that will read override value
        // from whatever mechanism is available on that platform. If no implementation is provided, the compiler is going to remove the calls
        // to it from the code
        static partial void TryGetSwitchOverridePartial(string switchName, ref bool overrideFound, ref bool overrideValue);

        /// This is a partial method. This method is responsible for populating the default values based on a TFM.
        /// It is partial because each library should define this method in their code to contain their defaults.
        static partial void PopulateDefaultValuesPartial(string platformIdentifier, string profile, int version);
    }
}
