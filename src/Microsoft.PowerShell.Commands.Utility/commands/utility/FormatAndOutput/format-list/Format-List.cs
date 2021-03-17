// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// Implementation for the format-table command.
    /// </summary>
    [Cmdlet(VerbsCommon.Format, "List", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096928")]
    public class FormatListCommand : OuterFormatTableAndListBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatListCommand"/> class
        /// and sets the inner command.
        /// </summary>
        public FormatListCommand()
        {
            this.implementation = new InnerFormatShapeCommand(FormatShape.List);
        }
    }
}
