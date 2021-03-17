// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if !UNIX

namespace System.Management.Automation.Tracing
{
    internal interface IMethodInvoker
    {
        Delegate Invoker { get; }

        object[] CreateInvokerArgs(Delegate methodToInvoke, object[] methodToInvokeArgs);
    }
}

#endif
