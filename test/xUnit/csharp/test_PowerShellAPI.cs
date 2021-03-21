// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSTests.Sequential
{
    public static class PowerShellHostingScenario
    {
        [Fact]
        public static void TestStartJobThrowTerminatingException()
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Start-Job").AddParameter("ScriptBlock", ScriptBlock.Create("1+1"));
                var ex = Assert.Throws<CmdletInvocationException>(() => ps.Invoke());
                Assert.IsType<PSNotSupportedException>(ex.InnerException);
                Assert.Equal("IPCPwshExecutableNotFound,Microsoft.PowerShell.Commands.StartJobCommand", ex.ErrorRecord.FullyQualifiedErrorId);
            }
        }
    }
}
