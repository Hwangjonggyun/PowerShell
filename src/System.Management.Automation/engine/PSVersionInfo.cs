// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace System.Management.Automation
{
    /// <summary>
    /// <para>
    /// Encapsulates $PSVersionTable.
    /// </para>
    /// <para>
    /// Provides a simple interface to retrieve details from the PowerShell version table:
    /// <code>
    ///    PSVersionInfo.PSVersion;
    /// </code>
    /// The above statement retrieves the PowerShell version.
    /// <code>
    ///    PSVersionInfo.PSEdition;
    /// </code>
    /// The above statement retrieves the PowerShell edition.
    /// </para>
    /// </summary>
    public class PSVersionInfo
    {
        internal const string PSVersionTableName = "PSVersionTable";
        internal const string PSRemotingProtocolVersionName = "PSRemotingProtocolVersion";
        internal const string PSVersionName = "PSVersion";
        internal const string PSEditionName = "PSEdition";
        internal const string PSGitCommitIdName = "GitCommitId";
        internal const string PSCompatibleVersionsName = "PSCompatibleVersions";
        internal const string PSPlatformName = "Platform";
        internal const string PSOSName = "OS";
        internal const string SerializationVersionName = "SerializationVersion";
        internal const string WSManStackVersionName = "WSManStackVersion";

        private static readonly PSVersionHashTable s_psVersionTable;

        /// <summary>
        /// A constant to track current PowerShell Version.
        /// </summary>
        /// <remarks>
        /// We can't depend on assembly version for PowerShell version.
        ///
        /// This is why we hard code the PowerShell version here.
        ///
        /// For each later release of PowerShell, this constant needs to
        /// be updated to reflect the right version.
        /// </remarks>
        private static readonly Version s_psV1Version = new Version(1, 0);
        private static readonly Version s_psV2Version = new Version(2, 0);
        private static readonly Version s_psV3Version = new Version(3, 0);
        private static readonly Version s_psV4Version = new Version(4, 0);
        private static readonly Version s_psV5Version = new Version(5, 0);
        private static readonly Version s_psV51Version = new Version(5, 1, NTVerpVars.PRODUCTBUILD, NTVerpVars.PRODUCTBUILD_QFE);
        private static readonly SemanticVersion s_psV6Version = new SemanticVersion(6, 0, 0, preReleaseLabel: null, buildLabel: null);
        private static readonly SemanticVersion s_psV61Version = new SemanticVersion(6, 1, 0, preReleaseLabel: null, buildLabel: null);
        private static readonly SemanticVersion s_psV62Version = new SemanticVersion(6, 2, 0, preReleaseLabel: null, buildLabel: null);
        private static readonly SemanticVersion s_psV7Version = new SemanticVersion(7, 0, 0, preReleaseLabel: null, buildLabel: null);
        private static readonly SemanticVersion s_psV71Version = new SemanticVersion(7, 1, 0, preReleaseLabel: null, buildLabel: null);
        private static readonly SemanticVersion s_psSemVersion;
        private static readonly Version s_psVersion;

        /// <summary>
        /// A constant to track current PowerShell Edition.
        /// </summary>
        internal const string PSEditionValue = "Core";

        // Static Constructor.
        static PSVersionInfo()
        {
            s_psVersionTable = new PSVersionHashTable(StringComparer.OrdinalIgnoreCase);

            Assembly currentAssembly = typeof(PSVersionInfo).Assembly;
            string productVersion = currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

            // Get 'GitCommitId' and 'PSVersion' from the 'productVersion' assembly attribute.
            //
            // The strings can be one of the following format examples:
            //    when powershell is built from a commit:
            //      productVersion = '6.0.0-beta.7 Commits: 29 SHA: 52c6b...' convert to GitCommitId = 'v6.0.0-beta.7-29-g52c6b...'
            //                                                                           PSVersion   = '6.0.0-beta.7'
            //    when powershell is built from a release tag:
            //      productVersion = '6.0.0-beta.7 SHA: f1ec9...'             convert to GitCommitId = 'v6.0.0-beta.7'
            //                                                                           PSVersion   = '6.0.0-beta.7'
            //    when powershell is built from a release tag for RTM:
            //      productVersion = '6.0.0 SHA: f1ec9...'                    convert to GitCommitId = 'v6.0.0'
            //                                                                           PSVersion   = '6.0.0'
            string rawGitCommitId;
            string mainVersion = productVersion.Substring(0, productVersion.IndexOf(' '));

            if (productVersion.Contains(" Commits: "))
            {
                rawGitCommitId = productVersion.Replace(" Commits: ", "-").Replace(" SHA: ", "-g");
            }
            else
            {
                rawGitCommitId = mainVersion;
            }

            s_psSemVersion = new SemanticVersion(mainVersion);
            s_psVersion = (Version)s_psSemVersion;

            s_psVersionTable[PSVersionInfo.PSVersionName] = s_psSemVersion;
            s_psVersionTable[PSVersionInfo.PSEditionName] = PSEditionValue;
            s_psVersionTable[PSGitCommitIdName] = rawGitCommitId;
            s_psVersionTable[PSCompatibleVersionsName] = new Version[] { s_psV1Version, s_psV2Version, s_psV3Version, s_psV4Version, s_psV5Version, s_psV51Version, s_psV6Version, s_psV61Version, s_psV62Version, s_psV7Version, s_psV71Version, s_psVersion };
            s_psVersionTable[PSVersionInfo.SerializationVersionName] = new Version(InternalSerializer.DefaultVersion);
            s_psVersionTable[PSVersionInfo.PSRemotingProtocolVersionName] = RemotingConstants.ProtocolVersion;
            s_psVersionTable[PSVersionInfo.WSManStackVersionName] = GetWSManStackVersion();
            s_psVersionTable[PSPlatformName] = Environment.OSVersion.Platform.ToString();
            s_psVersionTable[PSOSName] = Runtime.InteropServices.RuntimeInformation.OSDescription;
        }

        internal static PSVersionHashTable GetPSVersionTable()
        {
            return s_psVersionTable;
        }

        internal static Hashtable GetPSVersionTableForDownLevel()
        {
            var result = (Hashtable)s_psVersionTable.Clone();
            // Downlevel systems don't support SemanticVersion, but Version is most likely good enough anyway.
            result[PSVersionInfo.PSVersionName] = s_psVersion;
            return result;
        }

        #region Private helper methods

        // Gets the current WSMan stack version from the registry.
        private static Version GetWSManStackVersion()
        {
            Version version = null;

#if !UNIX
            try
            {
                using (RegistryKey wsManStackVersionKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\WSMAN"))
                {
                    if (wsManStackVersionKey != null)
                    {
                        object wsManStackVersionObj = wsManStackVersionKey.GetValue("ServiceStackVersion");
                        string wsManStackVersion = (wsManStackVersionObj != null) ? (string)wsManStackVersionObj : null;
                        if (!string.IsNullOrEmpty(wsManStackVersion))
                        {
                            version = new Version(wsManStackVersion.Trim());
                        }
                    }
                }
            }
            catch (ObjectDisposedException) { }
            catch (System.Security.SecurityException) { }
            catch (ArgumentException) { }
            catch (System.IO.IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (FormatException) { }
            catch (OverflowException) { }
            catch (InvalidCastException) { }
#endif

            return version ?? System.Management.Automation.Remoting.Client.WSManNativeApi.WSMAN_STACK_VERSION;
        }

        #endregion

        #region Programmer APIs

        /// <summary>
        /// Gets the version of PowerShell.
        /// </summary>
        public static Version PSVersion
        {
            get
            {
                return s_psVersion;
            }
        }

        internal static string GitCommitId
        {
            get
            {
                return (string)s_psVersionTable[PSGitCommitIdName];
            }
        }

        internal static Version[] PSCompatibleVersions
        {
            get
            {
                return (Version[])s_psVersionTable[PSCompatibleVersionsName];
            }
        }

        /// <summary>
        /// Gets the edition of PowerShell.
        /// </summary>
        public static string PSEdition
        {
            get
            {
                return (string)s_psVersionTable[PSVersionInfo.PSEditionName];
            }
        }

        internal static Version SerializationVersion
        {
            get
            {
                return (Version)s_psVersionTable[SerializationVersionName];
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// For 2.0 PowerShell, we still use "1" as the registry version key.
        /// For >=3.0 PowerShell, we still use "1" as the registry version key for
        /// Snapin and Custom shell lookup/discovery.
        /// </remarks>
        internal static string RegistryVersion1Key
        {
            get
            {
                return "1";
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// For 3.0 PowerShell, we use "3" as the registry version key only for Engine
        /// related data like ApplicationBase.
        /// For 3.0 PowerShell, we still use "1" as the registry version key for
        /// Snapin and Custom shell lookup/discovery.
        /// </remarks>
        internal static string RegistryVersionKey
        {
            get
            {
                // PowerShell >=4 is compatible with PowerShell 3 and hence reg key is 3.
                return "3";
            }
        }

        internal static string GetRegistryVersionKeyForSnapinDiscovery(string majorVersion)
        {
            int tempMajorVersion = 0;
            LanguagePrimitives.TryConvertTo<int>(majorVersion, out tempMajorVersion);

            if ((tempMajorVersion >= 1) && (tempMajorVersion <= PSVersionInfo.PSVersion.Major))
            {
                // PowerShell version 3 took a dependency on CLR4 and went with:
                // SxS approach in GAC/Registry and in-place upgrade approach for
                // FileSystem.
                // For >=3.0 PowerShell, we still use "1" as the registry version key for
                // Snapin and Custom shell lookup/discovery.
                return "1";
            }

            return null;
        }

        internal static string FeatureVersionString
        {
            get
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}", PSVersionInfo.PSVersion.Major, PSVersionInfo.PSVersion.Minor);
            }
        }

        internal static bool IsValidPSVersion(Version version)
        {
            if (version.Major == s_psSemVersion.Major)
            {
                return version.Minor == s_psSemVersion.Minor;
            }

            if (version.Major == s_psV6Version.Major)
            {
                return version.Minor == s_psV6Version.Minor;
            }

            if (version.Major == s_psV5Version.Major)
            {
                return (version.Minor == s_psV5Version.Minor || version.Minor == s_psV51Version.Minor);
            }

            if (version.Major == s_psV4Version.Major)
            {
                return (version.Minor == s_psV4Version.Minor);
            }
            else if (version.Major == s_psV3Version.Major)
            {
                return version.Minor == s_psV3Version.Minor;
            }
            else if (version.Major == s_psV2Version.Major)
            {
                return version.Minor == s_psV2Version.Minor;
            }
            else if (version.Major == s_psV1Version.Major)
            {
                return version.Minor == s_psV1Version.Minor;
            }

            return false;
        }

        internal static Version PSV4Version
        {
            get { return s_psV4Version; }
        }

        internal static Version PSV5Version
        {
            get { return s_psV5Version; }
        }

        internal static Version PSV51Version
        {
            get { return s_psV51Version; }
        }

        internal static SemanticVersion PSV6Version
        {
            get { return s_psV6Version; }
        }

        internal static SemanticVersion PSV7Version
        {
            get { return s_psV7Version; }
        }

        internal static SemanticVersion PSCurrentVersion
        {
            get { return s_psSemVersion; }
        }

        #endregion
    }

    /// <summary>
    /// Represents an implementation of '$PSVersionTable' variable.
    /// The implementation contains ordered 'Keys' and 'GetEnumerator' to get user-friendly output.
    /// </summary>
    public sealed class PSVersionHashTable : Hashtable, IEnumerable
    {
        private static readonly PSVersionTableComparer s_keysComparer = new PSVersionTableComparer();

        internal PSVersionHashTable(IEqualityComparer equalityComparer) : base(equalityComparer)
        {
        }

        /// <summary>
        /// Returns ordered collection with Keys of 'PSVersionHashTable'
        /// We want see special order:
        ///     1. PSVersionName
        ///     2. PSEditionName
        ///     3. Remaining properties in alphabetical order.
        /// </summary>
        public override ICollection Keys
        {
            get
            {
                ArrayList keyList = new ArrayList(base.Keys);
                keyList.Sort(s_keysComparer);
                return keyList;
            }
        }

        private class PSVersionTableComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                string xString = (string)LanguagePrimitives.ConvertTo(x, typeof(string), CultureInfo.CurrentCulture);
                string yString = (string)LanguagePrimitives.ConvertTo(y, typeof(string), CultureInfo.CurrentCulture);
                if (PSVersionInfo.PSVersionName.Equals(xString, StringComparison.OrdinalIgnoreCase))
                {
                    return -1;
                }
                else if (PSVersionInfo.PSVersionName.Equals(yString, StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }
                else if (PSVersionInfo.PSEditionName.Equals(xString, StringComparison.OrdinalIgnoreCase))
                {
                    return -1;
                }
                else if (PSVersionInfo.PSEditionName.Equals(yString, StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }
                else
                {
                    return string.Compare(xString, yString, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        /// <summary>
        /// Returns an enumerator for 'PSVersionHashTable'.
        /// The enumeration is ordered (based on ordered version of 'Keys').
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (object key in Keys)
            {
                yield return new DictionaryEntry(key, this[key]);
            }
        }
    }

    /// <summary>
    /// An implementation of semantic versioning (https://semver.org)
    /// that can be converted to/from <see cref="System.Version"/>.
    ///
    /// When converting to <see cref="Version"/>, a PSNoteProperty is
    /// added to the instance to store the semantic version label so
    /// that it can be recovered when creating a new SemanticVersion.
    /// </summary>
    public sealed class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private const string VersionSansRegEx = @"^(?<major>\d+)(\.(?<minor>\d+))?(\.(?<patch>\d+))?$";
        private const string LabelRegEx = @"^((?<preLabel>[0-9A-Za-z][0-9A-Za-z\-\.]*))?(\+(?<buildLabel>[0-9A-Za-z][0-9A-Za-z\-\.]*))?$";
        private const string LabelUnitRegEx = @"^[0-9A-Za-z][0-9A-Za-z\-\.]*$";
        private const string PreLabelPropertyName = "PSSemVerPreReleaseLabel";
        private const string BuildLabelPropertyName = "PSSemVerBuildLabel";
        private const string TypeNameForVersionWithLabel = "System.Version#IncludeLabel";

        private string versionString;

        /// <summary>
        /// Construct a SemanticVersion from a string.
        /// </summary>
        /// <param name="version">The version to parse.</param>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        public SemanticVersion(string version)
        {
            var v = SemanticVersion.Parse(version);

            Major = v.Major;
            Minor = v.Minor;
            Patch = v.Patch < 0 ? 0 : v.Patch;
            PreReleaseLabel = v.PreReleaseLabel;
            BuildLabel = v.BuildLabel;
        }

        /// <summary>
        /// Construct a SemanticVersion.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch version.</param>
        /// <param name="preReleaseLabel">The pre-release label for the version.</param>
        /// <param name="buildLabel">The build metadata for the version.</param>
        /// <exception cref="FormatException">
        /// If <paramref name="preReleaseLabel"/> don't match 'LabelUnitRegEx'.
        /// If <paramref name="buildLabel"/> don't match 'LabelUnitRegEx'.
        /// </exception>
        public SemanticVersion(int major, int minor, int patch, string preReleaseLabel, string buildLabel)
            : this(major, minor, patch)
        {
            if (!string.IsNullOrEmpty(preReleaseLabel))
            {
                if (!Regex.IsMatch(preReleaseLabel, LabelUnitRegEx)) throw new FormatException(nameof(preReleaseLabel));

                PreReleaseLabel = preReleaseLabel;
            }

            if (!string.IsNullOrEmpty(buildLabel))
            {
                if (!Regex.IsMatch(buildLabel, LabelUnitRegEx)) throw new FormatException(nameof(buildLabel));

                BuildLabel = buildLabel;
            }
        }

        /// <summary>
        /// Construct a SemanticVersion.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The minor version.</param>
        /// <param name="label">The label for the version.</param>
        /// <exception cref="PSArgumentException">
        /// <exception cref="FormatException">
        /// If <paramref name="label"/> don't match 'LabelRegEx'.
        /// </exception>
        public SemanticVersion(int major, int minor, int patch, string label)
            : this(major, minor, patch)
        {
            // We presume the SymVer :
            // 1) major.minor.patch-label
            // 2) 'label' starts with letter or digit.
            if (!string.IsNullOrEmpty(label))
            {
                var match = Regex.Match(label, LabelRegEx);
                if (!match.Success) throw new FormatException(nameof(label));

                PreReleaseLabel = match.Groups["preLabel"].Value;
                BuildLabel = match.Groups["buildLabel"].Value;
            }
        }

        /// <summary>
        /// Construct a SemanticVersion.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The minor version.</param>
        /// <exception cref="PSArgumentException">
        /// If <paramref name="major"/>, <paramref name="minor"/>, or <paramref name="patch"/> is less than 0.
        /// </exception>
        public SemanticVersion(int major, int minor, int patch)
        {
            if (major < 0) throw PSTraceSource.NewArgumentException(nameof(major));
            if (minor < 0) throw PSTraceSource.NewArgumentException(nameof(minor));
            if (patch < 0) throw PSTraceSource.NewArgumentException(nameof(patch));

            Major = major;
            Minor = minor;
            Patch = patch;
            // We presume:
            // PreReleaseLabel = null;
            // BuildLabel = null;
        }

        /// <summary>
        /// Construct a SemanticVersion.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <exception cref="PSArgumentException">
        /// If <paramref name="major"/> or <paramref name="minor"/> is less than 0.
        /// </exception>
        public SemanticVersion(int major, int minor) : this(major, minor, 0) { }

        /// <summary>
        /// Construct a SemanticVersion.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <exception cref="PSArgumentException">
        /// If <paramref name="major"/> is less than 0.
        /// </exception>
        public SemanticVersion(int major) : this(major, 0, 0) { }

        /// <summary>
        /// Construct a <see cref="SemanticVersion"/> from a <see cref="Version"/>,
        /// copying the NoteProperty storing the label if the expected property exists.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="version"/> is null.
        /// </exception>
        /// <exception cref="PSArgumentException">
        /// If <paramref name="version.Revision"/> is more than 0.
        /// </exception>
        public SemanticVersion(Version version)
        {
            if (version == null) throw PSTraceSource.NewArgumentNullException(nameof(version));
            if (version.Revision > 0) throw PSTraceSource.NewArgumentException(nameof(version));

            Major = version.Major;
            Minor = version.Minor;
            Patch = version.Build == -1 ? 0 : version.Build;
            var psobj = new PSObject(version);
            var preLabelNote = psobj.Properties[PreLabelPropertyName];
            if (preLabelNote != null)
            {
                PreReleaseLabel = preLabelNote.Value as string;
            }

            var buildLabelNote = psobj.Properties[BuildLabelPropertyName];
            if (buildLabelNote != null)
            {
                BuildLabel = buildLabelNote.Value as string;
            }
        }

        /// <summary>
        /// Convert a <see cref="SemanticVersion"/> to a <see cref="Version"/>.
        /// If there is a <see cref="PreReleaseLabel"/> or/and a <see cref="BuildLabel"/>,
        /// it is added as a NoteProperty to the result so that you can round trip
        /// back to a <see cref="SemanticVersion"/> without losing the label.
        /// </summary>
        /// <param name="semver"></param>
        public static implicit operator Version(SemanticVersion semver)
        {
            PSObject psobj;

            var result = new Version(semver.Major, semver.Minor, semver.Patch);

            if (!string.IsNullOrEmpty(semver.PreReleaseLabel) || !string.IsNullOrEmpty(semver.BuildLabel))
            {
                psobj = new PSObject(result);

                if (!string.IsNullOrEmpty(semver.PreReleaseLabel))
                {
                    psobj.Properties.Add(new PSNoteProperty(PreLabelPropertyName, semver.PreReleaseLabel));
                }

                if (!string.IsNullOrEmpty(semver.BuildLabel))
                {
                    psobj.Properties.Add(new PSNoteProperty(BuildLabelPropertyName, semver.BuildLabel));
                }

                psobj.TypeNames.Insert(0, TypeNameForVersionWithLabel);
            }

            return result;
        }

        /// <summary>
        /// The major version number, never negative.
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// The minor version number, never negative.
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// The patch version, -1 if not specified.
        /// </summary>
        public int Patch { get; }

        /// <summary>
        /// PreReleaseLabel position in the SymVer string 'major.minor.patch-PreReleaseLabel+BuildLabel'.
        /// </summary>
        public string PreReleaseLabel { get; }

        /// <summary>
        /// BuildLabel position in the SymVer string 'major.minor.patch-PreReleaseLabel+BuildLabel'.
        /// </summary>
        public string BuildLabel { get; }

        /// <summary>
        /// Parse <paramref name="version"/> and return the result if it is a valid <see cref="SemanticVersion"/>, otherwise throws an exception.
        /// </summary>
        /// <param name="version">The string to parse.</param>
        /// <returns></returns>
        /// <exception cref="PSArgumentException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        public static SemanticVersion Parse(string version)
        {
            if (version == null) throw PSTraceSource.NewArgumentNullException(nameof(version));
            if (version == string.Empty) throw new FormatException(nameof(version));

            var r = new VersionResult();
            r.Init(true);
            TryParseVersion(version, ref r);

            return r._parsedVersion;
        }

        /// <summary>
        /// Parse <paramref name="version"/> and return true if it is a valid <see cref="SemanticVersion"/>, otherwise return false.
        /// No exceptions are raised.
        /// </summary>
        /// <param name="version">The string to parse.</param>
        /// <param name="result">The return value when the string is a valid <see cref="SemanticVersion"/></param>
        public static bool TryParse(string version, out SemanticVersion result)
        {
            if (version != null)
            {
                var r = new VersionResult();
                r.Init(false);

                if (TryParseVersion(version, ref r))
                {
                    result = r._parsedVersion;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static bool TryParseVersion(string version, ref VersionResult result)
        {
            if (version.EndsWith('-') || version.EndsWith('+') || version.EndsWith('.'))
            {
                result.SetFailure(ParseFailureKind.FormatException);
                return false;
            }

            string versionSansLabel = null;
            var major = 0;
            var minor = 0;
            var patch = 0;
            string preLabel = null;
            string buildLabel = null;

            // We parse the SymVer 'version' string 'major.minor.patch-PreReleaseLabel+BuildLabel'.
            var dashIndex = version.IndexOf('-');
            var plusIndex = version.IndexOf('+');

            if (dashIndex > plusIndex)
            {
                // 'PreReleaseLabel' can contains dashes.
                if (plusIndex == -1)
                {
                    // No buildLabel: buildLabel == null
                    // Format is 'major.minor.patch-PreReleaseLabel'
                    preLabel = version.Substring(dashIndex + 1);
                    versionSansLabel = version.Substring(0, dashIndex);
                }
                else
                {
                    // No PreReleaseLabel: preLabel == null
                    // Format is 'major.minor.patch+BuildLabel'
                    buildLabel = version.Substring(plusIndex + 1);
                    versionSansLabel = version.Substring(0, plusIndex);
                    dashIndex = -1;
                }
            }
            else
            {
                if (dashIndex == -1)
                {
                    // Here dashIndex == plusIndex == -1
                    // No preLabel - preLabel == null;
                    // No buildLabel - buildLabel == null;
                    // Format is 'major.minor.patch'
                    versionSansLabel = version;
                }
                else
                {
                    // Format is 'major.minor.patch-PreReleaseLabel+BuildLabel'
                    preLabel = version.Substring(dashIndex + 1, plusIndex - dashIndex - 1);
                    buildLabel = version.Substring(plusIndex + 1);
                    versionSansLabel = version.Substring(0, dashIndex);
                }
            }

            if ((dashIndex != -1 && string.IsNullOrEmpty(preLabel)) ||
                (plusIndex != -1 && string.IsNullOrEmpty(buildLabel)) ||
                string.IsNullOrEmpty(versionSansLabel))
            {
                // We have dash and no preReleaseLabel  or
                // we have plus and no buildLabel or
                // we have no main version part (versionSansLabel==null)
                result.SetFailure(ParseFailureKind.FormatException);
                return false;
            }

            var match = Regex.Match(versionSansLabel, VersionSansRegEx);
            if (!match.Success)
            {
                result.SetFailure(ParseFailureKind.FormatException);
                return false;
            }

            if (!int.TryParse(match.Groups["major"].Value, out major))
            {
                result.SetFailure(ParseFailureKind.FormatException);
                return false;
            }

            if (match.Groups["minor"].Success && !int.TryParse(match.Groups["minor"].Value, out minor))
            {
                result.SetFailure(ParseFailureKind.FormatException);
                return false;
            }

            if (match.Groups["patch"].Success && !int.TryParse(match.Groups["patch"].Value, out patch))
            {
                result.SetFailure(ParseFailureKind.FormatException);
                return false;
            }

            if (preLabel != null && !Regex.IsMatch(preLabel, LabelUnitRegEx) ||
               (buildLabel != null && !Regex.IsMatch(buildLabel, LabelUnitRegEx)))
            {
                result.SetFailure(ParseFailureKind.FormatException);
                return false;
            }

            result._parsedVersion = new SemanticVersion(major, minor, patch, preLabel, buildLabel);
            return true;
        }

        /// <summary>
        /// Implement ToString()
        /// </summary>
        public override string ToString()
        {
            if (versionString == null)
            {
                StringBuilder result = new StringBuilder();

                result.Append(Major).Append(Utils.Separators.Dot).Append(Minor).Append(Utils.Separators.Dot).Append(Patch);

                if (!string.IsNullOrEmpty(PreReleaseLabel))
                {
                    result.Append('-').Append(PreReleaseLabel);
                }

                if (!string.IsNullOrEmpty(BuildLabel))
                {
                    result.Append('+').Append(BuildLabel);
                }

                versionString = result.ToString();
            }

            return versionString;
        }

        /// <summary>
        /// Implement Compare.
        /// </summary>
        public static int Compare(SemanticVersion versionA, SemanticVersion versionB)
        {
            if (versionA != null)
            {
                return versionA.CompareTo(versionB);
            }

            if (versionB != null)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Implement <see cref="IComparable.CompareTo"/>
        /// </summary>
        public int CompareTo(object version)
        {
            if (version == null)
            {
                return 1;
            }

            if (!(version is SemanticVersion v))
            {
                throw PSTraceSource.NewArgumentException(nameof(version));
            }

            return CompareTo(v);
        }

        /// <summary>
        /// Implement <see cref="IComparable{T}.CompareTo"/>.
        /// Meets SymVer 2.0 p.11 https://semver.org/
        /// </summary>
        public int CompareTo(SemanticVersion value)
        {
            if (value is null)
                return 1;

            if (Major != value.Major)
                return Major > value.Major ? 1 : -1;

            if (Minor != value.Minor)
                return Minor > value.Minor ? 1 : -1;

            if (Patch != value.Patch)
                return Patch > value.Patch ? 1 : -1;

            // SymVer 2.0 standard requires to ignore 'BuildLabel' (Build metadata).
            return ComparePreLabel(this.PreReleaseLabel, value.PreReleaseLabel);
        }

        /// <summary>
        /// Override <see cref="object.Equals(object)"/>
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as SemanticVersion);
        }

        /// <summary>
        /// Implement <see cref="IEquatable{T}.Equals(T)"/>
        /// </summary>
        public bool Equals(SemanticVersion other)
        {
            // SymVer 2.0 standard requires to ignore 'BuildLabel' (Build metadata).
            return other != null &&
                   (Major == other.Major) && (Minor == other.Minor) && (Patch == other.Patch) &&
                   string.Equals(PreReleaseLabel, other.PreReleaseLabel, StringComparison.Ordinal);
        }

        /// <summary>
        /// Override <see cref="object.GetHashCode()"/>
        /// </summary>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Overloaded == operator.
        /// </summary>
        public static bool operator ==(SemanticVersion v1, SemanticVersion v2)
        {
            if (v1 is null)
            {
                return v2 is null;
            }

            return v1.Equals(v2);
        }

        /// <summary>
        /// Overloaded != operator.
        /// </summary>
        public static bool operator !=(SemanticVersion v1, SemanticVersion v2)
        {
            return !(v1 == v2);
        }

        /// <summary>
        /// Overloaded &lt; operator.
        /// </summary>
        public static bool operator <(SemanticVersion v1, SemanticVersion v2)
        {
            return (Compare(v1, v2) < 0);
        }

        /// <summary>
        /// Overloaded &lt;= operator.
        /// </summary>
        public static bool operator <=(SemanticVersion v1, SemanticVersion v2)
        {
            return (Compare(v1, v2) <= 0);
        }

        /// <summary>
        /// Overloaded &gt; operator.
        /// </summary>
        public static bool operator >(SemanticVersion v1, SemanticVersion v2)
        {
            return (Compare(v1, v2) > 0);
        }

        /// <summary>
        /// Overloaded &gt;= operator.
        /// </summary>
        public static bool operator >=(SemanticVersion v1, SemanticVersion v2)
        {
            return (Compare(v1, v2) >= 0);
        }

        private static int ComparePreLabel(string preLabel1, string preLabel2)
        {
            // Symver 2.0 standard p.9
            // Pre-release versions have a lower precedence than the associated normal version.
            // Comparing each dot separated identifier from left to right
            // until a difference is found as follows:
            //     identifiers consisting of only digits are compared numerically
            //     and identifiers with letters or hyphens are compared lexically in ASCII sort order.
            // Numeric identifiers always have lower precedence than non-numeric identifiers.
            // A larger set of pre-release fields has a higher precedence than a smaller set,
            // if all of the preceding identifiers are equal.
            if (string.IsNullOrEmpty(preLabel1)) { return string.IsNullOrEmpty(preLabel2) ? 0 : 1; }

            if (string.IsNullOrEmpty(preLabel2)) { return -1; }

            var units1 = preLabel1.Split('.');
            var units2 = preLabel2.Split('.');

            var minLength = units1.Length < units2.Length ? units1.Length : units2.Length;

            for (int i = 0; i < minLength; i++)
            {
                var ac = units1[i];
                var bc = units2[i];
                int number1, number2;
                var isNumber1 = Int32.TryParse(ac, out number1);
                var isNumber2 = Int32.TryParse(bc, out number2);

                if (isNumber1 && isNumber2)
                {
                    if (number1 != number2) { return number1 < number2 ? -1 : 1; }
                }
                else
                {
                    if (isNumber1) { return -1; }

                    if (isNumber2) { return 1; }

                    int result = string.CompareOrdinal(ac, bc);
                    if (result != 0) { return result; }
                }
            }

            return units1.Length.CompareTo(units2.Length);
        }

        internal enum ParseFailureKind
        {
            ArgumentException,
            ArgumentOutOfRangeException,
            FormatException
        }

        internal struct VersionResult
        {
            internal SemanticVersion _parsedVersion;
            internal ParseFailureKind _failure;
            internal string _exceptionArgument;
            internal bool _canThrow;

            internal void Init(bool canThrow)
            {
                _canThrow = canThrow;
            }

            internal void SetFailure(ParseFailureKind failure)
            {
                SetFailure(failure, string.Empty);
            }

            internal void SetFailure(ParseFailureKind failure, string argument)
            {
                _failure = failure;
                _exceptionArgument = argument;
                if (_canThrow)
                {
                    throw GetVersionParseException();
                }
            }

            internal Exception GetVersionParseException()
            {
                switch (_failure)
                {
                    case ParseFailureKind.ArgumentException:
                        return PSTraceSource.NewArgumentException("version");
                    case ParseFailureKind.ArgumentOutOfRangeException:
                        throw new ValidationMetadataException("ValidateRangeTooSmall",
                            null, Metadata.ValidateRangeSmallerThanMinRangeFailure,
                            _exceptionArgument, "0");
                    case ParseFailureKind.FormatException:
                        // Regenerate the FormatException as would be thrown by Int32.Parse()
                        try
                        {
                            Int32.Parse(_exceptionArgument, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException e)
                        {
                            return e;
                        }
                        catch (OverflowException e)
                        {
                            return e;
                        }

                        break;
                }

                return PSTraceSource.NewArgumentException("version");
            }
        }
    }
}
