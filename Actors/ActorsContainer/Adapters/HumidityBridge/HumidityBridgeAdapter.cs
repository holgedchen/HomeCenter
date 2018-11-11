﻿using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Service;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.HumidityBridge
{
    [ProxyCodeGenerator]
    public abstract class HumidityBridgeAdapter : Adapter
    {
        private readonly Dictionary<int, double> _state = new Dictionary<int, double>();

        protected HumidityBridgeAdapter()
        {
            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            var _i2cAddress = AsInt(AdapterProperties.I2cAddress);

            foreach (var val in AsList(AdapterProperties.UsedPins))
            {
                _state.Add(int.Parse(val), 0);
            }

            var registration = new SerialRegistrationCommand(Self, 6, new Format[]
            {
                new Format(1, typeof(byte), "Pin"),
                new Format(2, typeof(float), "Humidity")
            });
            await MessageBroker.SendToService(registration).ConfigureAwait(false);
        }

        protected async Task Handle(SerialResultEvent serialResult)
        {
            var pin = serialResult.AsByte("Pin");
            var humidity = serialResult.AsDouble("Humidity");

            var oldValue = _state[pin];

            _state[pin] = await UpdateState(HumidityState.StateName, oldValue, humidity).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new HumidityState(ReadWriteMode.Read));
        }
    }
}