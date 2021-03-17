// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// Implementation for the format-table command.
    /// </summary>
    [Cmdlet(VerbsCommon.Format, "Table", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096703")]
    public class FormatTableCommand : OuterFormatTableBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatTableCommand"/> class
        /// and sets the inner command.
        /// </summary>
        public FormatTableCommand()
        {
            this.implementation = new InnerFormatShapeCommand(FormatShape.Table);
        }
    }
}
