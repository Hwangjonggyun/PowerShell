// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSTests.Internal
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PriorityAttribute : Attribute
    {
        public PriorityAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; }
    }
}
