﻿using System;

namespace HomeCenter.WindowsService.Core.Interop.Enum
{
    /// <summary>
    /// Device State
    /// </summary>
    [Flags]
    public enum AudioDeviceState
    {
        /// <summary>
        /// DEVICE_STATE_ACTIVE
        /// </summary>
        Active = 0x00000001,

        /// <summary>
        /// DEVICE_STATE_DISABLED
        /// </summary>
        Disabled = 0x00000002,

        /// <summary>
        /// DEVICE_STATE_NOTPRESENT
        /// </summary>
        NotPresent = 0x00000004,

        /// <summary>
        /// DEVICE_STATE_UNPLUGGED
        /// </summary>
        Unplugged = 0x00000008,

        /// <summary>
        /// DEVICE_STATEMASK_ALL
        /// </summary>
        All = 0x0000000F
    }
}