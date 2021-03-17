// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSTests.Parallel
{
    public class SessionStateTests
    {
        [SkippableFact]
        public void TestDrives()
        {
            Skip.IfNot(Platform.IsWindows);
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            PSHost hostInterface = new DefaultHost(currentCulture, currentCulture);
            InitialSessionState iss = InitialSessionState.CreateDefault2();
            AutomationEngine engine = new AutomationEngine(hostInterface, iss);
            ExecutionContext executionContext = new ExecutionContext(engine, hostInterface, iss);
            SessionStateInternal sessionState = new SessionStateInternal(executionContext);
            Collection<PSDriveInfo> drives = sessionState.Drives(null);
            Assert.NotNull(drives);
        }
    }
}
