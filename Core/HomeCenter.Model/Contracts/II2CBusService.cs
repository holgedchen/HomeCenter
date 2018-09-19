﻿using HomeCenter.Core.Interface.Native;
using HomeCenter.Model.Core;

namespace HomeCenter.Core.Services.I2C
{
    public interface II2CBusService : IService
    {
        I2cTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true);

        I2cTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true);

        I2cTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true);
    }
}