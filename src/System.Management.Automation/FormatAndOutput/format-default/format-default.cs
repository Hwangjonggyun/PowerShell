// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.PowerShell.Commands.Internal.Format;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// Implementation for the format-default command.
    /// </summary>
    [Cmdlet(VerbsCommon.Format, "Default")]
    public class FormatDefaultCommand : FrontEndCommandBase
    {
        /// <summary>
        /// Constructor to set the inner command.
        /// </summary>
        public FormatDefaultCommand()
        {
            this.implementation = new InnerFormatShapeCommand(FormatShape.Undefined);
        }
    }
}
