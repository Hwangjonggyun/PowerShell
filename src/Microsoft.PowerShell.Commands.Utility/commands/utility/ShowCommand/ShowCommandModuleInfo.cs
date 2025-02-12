// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.Commands.ShowCommandExtension
{
    /// <summary>
    /// Implements a facade around PSModuleInfo and its deserialized counterpart.
    /// </summary>
    public class ShowCommandModuleInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowCommandModuleInfo"/> class
        /// with the specified <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="other">
        /// The object to wrap.
        /// </param>
        public ShowCommandModuleInfo(PSModuleInfo other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Name = other.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowCommandModuleInfo"/> class
        /// with the specified <see cref="PSObject"/>.
        /// </summary>
        /// <param name="other">
        /// The object to wrap.
        /// </param>
        public ShowCommandModuleInfo(PSObject other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Name = other.Members["Name"].Value as string;
        }

        /// <summary>
        /// Gets the name of this module.
        /// </summary>
        public string Name { get; }
    }
}
