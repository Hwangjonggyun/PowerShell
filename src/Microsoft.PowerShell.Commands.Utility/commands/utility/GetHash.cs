// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// This class implements Get-FileHash.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "FileHash", DefaultParameterSetName = PathParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=517145")]
    [OutputType(typeof(FileHashInfo))]
    public class GetFileHashCommand : HashCmdletBase
    {
        /// <summary>
        /// Path parameter.
        /// The paths of the files to calculate hash values.
        /// Resolved wildcards.
        /// </summary>
        /// <value></value>
        [Parameter(Mandatory = true, ParameterSetName = PathParameterSet, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string[] Path
        {
            get
            {
                return _paths;
            }

            set
            {
                _paths = value;
            }
        }

        /// <summary>
        /// LiteralPath parameter.
        /// The literal paths of the files to calculate a hashs.
        /// Don't resolved wildcards.
        /// </summary>
        /// <value></value>
        [Parameter(Mandatory = true, ParameterSetName = LiteralPathParameterSet, Position = 0, ValueFromPipelineByPropertyName = true)]
        [Alias("PSPath", "LP")]
        public string[] LiteralPath
        {
            get
            {
                return _paths;
            }

            set
            {
                _paths = value;
            }
        }

        private string[] _paths;

        /// <summary>
        /// InputStream parameter.
        /// The stream of the file to calculate a hash.
        /// </summary>
        /// <value></value>
        [Parameter(Mandatory = true, ParameterSetName = StreamParameterSet, Position = 0)]
        public Stream InputStream { get; set; }

        /// <summary>
        /// BeginProcessing() override.
        /// This is for hash function init.
        /// </summary>
        protected override void BeginProcessing()
        {
            InitHasher(Algorithm);
        }

        /// <summary>
        /// ProcessRecord() override.
        /// This is for paths collecting from pipe.
        /// </summary>
        protected override void ProcessRecord()
        {
            List<string> pathsToProcess = new();
            ProviderInfo provider = null;

            switch (ParameterSetName)
            {
                case PathParameterSet:
                    // Resolve paths and check existence
                    foreach (string path in _paths)
                    {
                        try
                        {
                            Collection<string> newPaths = Context.SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider);
                            if (newPaths != null)
                            {
                                pathsToProcess.AddRange(newPaths);
                            }
                        }
                        catch (ItemNotFoundException e)
                        {
                            if (!WildcardPattern.ContainsWildcardCharacters(path))
                            {
                                ErrorRecord errorRecord = new(e,
                                    "FileNotFound",
                                    ErrorCategory.ObjectNotFound,
                                    path);
                                WriteError(errorRecord);
                            }
                        }
                    }

                    break;
                case LiteralPathParameterSet:
                    foreach (string path in _paths)
                    {
                        string newPath = Context.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
                        pathsToProcess.Add(newPath);
                    }

                    break;
            }

            foreach (string path in pathsToProcess)
            {
                if (ComputeFileHash(path, out string hash))
                {
                    WriteHashResult(Algorithm, hash, path);
                }
            }
        }

        /// <summary>
        /// Perform common error checks.
        /// Populate source code.
        /// </summary>
        protected override void EndProcessing()
        {
            if (ParameterSetName == StreamParameterSet)
            {
                byte[] bytehash = null;
                string hash = null;

                bytehash = hasher.ComputeHash(InputStream);

                hash = BitConverter.ToString(bytehash).Replace("-", string.Empty);
                WriteHashResult(Algorithm, hash, string.Empty);
            }
        }

        /// <summary>
        /// Read the file and calculate the hash.
        /// </summary>
        /// <param name="path">Path to file which will be hashed.</param>
        /// <param name="hash">Will contain the hash of the file content.</param>
        /// <returns>Boolean value indicating whether the hash calculation succeeded or failed.</returns>
        private bool ComputeFileHash(string path, out string hash)
        {
            byte[] bytehash = null;
            Stream openfilestream = null;

            hash = null;

            try
            {
                openfilestream = File.OpenRead(path);

                bytehash = hasher.ComputeHash(openfilestream);
                hash = BitConverter.ToString(bytehash).Replace("-", string.Empty);
            }
            catch (FileNotFoundException ex)
            {
                var errorRecord = new ErrorRecord(
                    ex,
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    path);
                WriteError(errorRecord);
            }
            catch (UnauthorizedAccessException ex)
            {
                var errorRecord = new ErrorRecord(
                    ex,
                    "UnauthorizedAccessError",
                    ErrorCategory.InvalidData,
                    path);
                WriteError(errorRecord);
            }
            catch (IOException ioException)
            {
                var errorRecord = new ErrorRecord(
                    ioException,
                    "FileReadError",
                    ErrorCategory.ReadError,
                    path);
                WriteError(errorRecord);
            }
            finally
            {
                openfilestream?.Dispose();
            }

            return hash != null;
        }

        /// <summary>
        /// Create FileHashInfo object and output it.
        /// </summary>
        private void WriteHashResult(string Algorithm, string hash, string path)
        {
            FileHashInfo result = new();
            result.Algorithm = Algorithm;
            result.Hash = hash;
            result.Path = path;
            WriteObject(result);
        }

        /// <summary>
        /// Parameter set names.
        /// </summary>
        private const string PathParameterSet = "Path";
        private const string LiteralPathParameterSet = "LiteralPath";
        private const string StreamParameterSet = "StreamParameterSet";
    }

    /// <summary>
    /// Base Cmdlet for cmdlets which deal with crypto hashes.
    /// </summary>
    public class HashCmdletBase : PSCmdlet
    {
        /// <summary>
        /// Algorithm parameter.
        /// The hash algorithm name: "SHA1", "SHA256", "SHA384", "SHA512", "MD5".
        /// </summary>
        /// <value></value>
        [Parameter(Position = 1)]
        [ValidateSet(HashAlgorithmNames.SHA1,
                     HashAlgorithmNames.SHA256,
                     HashAlgorithmNames.SHA384,
                     HashAlgorithmNames.SHA512,
                     HashAlgorithmNames.MD5)]
        public string Algorithm
        {
            get
            {
                return _Algorithm;
            }

            set
            {
                // A hash algorithm name is case sensitive
                // and always must be in upper case
                _Algorithm = value.ToUpper();
            }
        }

        private string _Algorithm = HashAlgorithmNames.SHA256;

        /// <summary>
        /// Hash algorithm is used.
        /// </summary>
        protected HashAlgorithm hasher;

        /// <summary>
        /// Hash algorithm names.
        /// </summary>
        internal static class HashAlgorithmNames
        {
            public const string MD5 = "MD5";
            public const string SHA1 = "SHA1";
            public const string SHA256 = "SHA256";
            public const string SHA384 = "SHA384";
            public const string SHA512 = "SHA512";
        }

        /// <summary>
        /// Init a hash algorithm.
        /// </summary>
        protected void InitHasher(string Algorithm)
        {
            try
            {
                switch (Algorithm)
                {
                    case HashAlgorithmNames.SHA1:
                        hasher = SHA1.Create();
                        break;
                    case HashAlgorithmNames.SHA256:
                        hasher = SHA256.Create();
                        break;
                    case HashAlgorithmNames.SHA384:
                        hasher = SHA384.Create();
                        break;
                    case HashAlgorithmNames.SHA512:
                        hasher = SHA512.Create();
                        break;
                    case HashAlgorithmNames.MD5:
                        hasher = MD5.Create();
                        break;
                }
            }
            catch
            {
                // Seems it will never throw! Remove?
                Exception exc = new NotSupportedException(UtilityCommonStrings.AlgorithmTypeNotSupported);
                ThrowTerminatingError(new ErrorRecord(exc, "AlgorithmTypeNotSupported", ErrorCategory.NotImplemented, null));
            }
        }
    }

    /// <summary>
    /// FileHashInfo class contains information about a file hash.
    /// </summary>
    public class FileHashInfo
    {
        /// <summary>
        /// Hash algorithm name.
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Hash value.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// File path.
        /// </summary>
        public string Path { get; set; }
    }
}
