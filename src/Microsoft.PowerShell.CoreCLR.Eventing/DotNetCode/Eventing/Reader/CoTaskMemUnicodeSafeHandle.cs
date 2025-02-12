// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/*============================================================
**
**
** Purpose:
** This internal class is a SafeHandle implementation over a
** native CoTaskMem allocated via SecureStringToCoTaskMemUnicode.
**
============================================================*/

namespace System.Diagnostics.Eventing.Reader
{
    //
    // Marked as SecurityCritical due to link demands from inherited
    // SafeHandle members.
    //
    [System.Security.SecurityCritical]
    internal sealed class CoTaskMemUnicodeSafeHandle : SafeHandle
    {
        internal CoTaskMemUnicodeSafeHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal CoTaskMemUnicodeSafeHandle(IntPtr handle, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
            SetHandle(handle);
        }

        internal void SetMemory(IntPtr handle)
        {
            SetHandle(handle);
        }

        internal IntPtr GetMemory()
        {
            return handle;
        }

        public override bool IsInvalid
        {
            get
            {
                return IsClosed || handle == IntPtr.Zero;
            }
        }

        protected override bool ReleaseHandle()
        {
            Marshal.ZeroFreeCoTaskMemUnicode(handle);
            handle = IntPtr.Zero;
            return true;
        }

        // DONT compare CoTaskMemUnicodeSafeHandle with CoTaskMemUnicodeSafeHandle.Zero
        // use IsInvalid instead. Zero is provided where a NULL handle needed
        public static CoTaskMemUnicodeSafeHandle Zero
        {
            get
            {
                return new CoTaskMemUnicodeSafeHandle();
            }
        }
    }
}
