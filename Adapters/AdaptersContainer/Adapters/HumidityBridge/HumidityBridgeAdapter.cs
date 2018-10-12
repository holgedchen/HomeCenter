﻿using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.ValueTypes;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.HumidityBridge
{
    [ProxyCodeGenerator]
    public abstract class HumidityBridgeAdapter : Adapter
    {
        private readonly Dictionary<IntValue, DoubleValue> _state = new Dictionary<IntValue, DoubleValue>();

        protected HumidityBridgeAdapter()
        {
            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            var _i2cAddress = this[AdapterProperties.I2cAddress].AsInt();

            foreach (var val in this[AdapterProperties.UsedPins].AsStringList())
            {
                _state.Add(IntValue.FromString(val), 0);
            }

            var registration = new SerialRegistrationCommand(Self, 6, new Format[]
            {
                new Format(1, typeof(byte), "Pin"),
                new Format(2, typeof(float), "Humidity")
            });
            //TODO Send
        }

        protected void Handle(SerialResultCommand serialResultCommand)
        {
            // _state[pin] = await UpdateState(HumidityState.StateName, pin, (DoubleValue)humidity).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new HumidityState(ReadWriteModeValues.Read));
        }
    }
}