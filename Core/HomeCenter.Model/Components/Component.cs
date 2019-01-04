﻿using CSharpFunctionalExtensions;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Triggers;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using Proto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Model.Components
{
    [ProxyCodeGenerator]
    public class Component : DeviceActor
    {
        private Dictionary<string, State> _capabilities { get; } = new Dictionary<string, State>();
        private Dictionary<string, AdapterReference> _adapterStateMap { get; } = new Dictionary<string, AdapterReference>();

        [Map] private IList<AdapterReference> _adapters { get; set; } = new List<AdapterReference>();
        [Map] private IList<Trigger> _triggers { get; set; } = new List<Trigger>();
        [Map] private Dictionary<string, IValueConverter> _converters { get; set; } = new Dictionary<string, IValueConverter>();

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            if (!IsEnabled) return;

            await InitializeAdapters().ConfigureAwait(false);
            await InitializeTriggers().ConfigureAwait(false);
            SubscribeForRemoteCommands();
        }

        private Task InitializeTriggers()
        {
            InitEventTriggers();
            return InitScheduledTriggers();
        }

        private void InitEventTriggers()
        {
            foreach (var trigger in _triggers.Where(x => x.Schedule == null))
            {
                Subscribe<Event>(GetRoutingFilterFromProperties(trigger.Event));
            }
        }

        private RoutingFilter GetRoutingFilterFromProperties(Event ev)
        {
            var attributes = ev.GetProperties().ToDictionary();
            AddRequiersProperties(ev, attributes);

            return new RoutingFilter(attributes);
        }

        /// <summary>
        /// When trigger is attached to event from internal adapter we add requierd properties to routing filter if they are missing
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="attributes"></param>
        private void AddRequiersProperties(Event ev, Dictionary<string, string> attributes)
        {
            if (ev.ContainsProperty(MessageProperties.MessageSource))
            {
                var adapter = _adapters.FirstOrDefault(a => a.Uid == ev.MessageSource);
                if (adapter != null)
                {
                    foreach (var property in adapter.RequierdProperties)
                    {
                        if (!attributes.ContainsKey(property))
                        {
                            attributes.Add(property, adapter.AsString(property));
                        }
                    }
                }
            }
        }

        private async Task InitScheduledTriggers()
        {
            foreach (var trigger in _triggers.Where(x => x.Schedule != null))
            {
                if (!string.IsNullOrWhiteSpace(trigger.Schedule.CronExpression))
                {
                    await Scheduler.ScheduleCron<TriggerJob, TriggerJobDataDTO>(trigger.ToJobDataWithFinish(Self), trigger.Schedule.CronExpression, Uid, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                }
                else if (trigger.Schedule.ManualSchedules.Count > 0)
                {
                    foreach (var manualTrigger in trigger.Schedule.ManualSchedules)
                    {
                        await Scheduler.ScheduleDailyTimeInterval<TriggerJob, TriggerJobDataDTO>(trigger.ToJobData(Self), manualTrigger.Start, Uid, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                        await Scheduler.ScheduleDailyTimeInterval<TriggerJob, TriggerJobDataDTO>(trigger.ToJobData(Self), manualTrigger.Finish, Uid, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task InitializeAdapters()
        {
            foreach (var adapter in _adapters)
            {
                var capabilities = await MessageBroker.Request<DiscoverQuery, DiscoveryResponse>(DiscoverQuery.CreateQuery(adapter), adapter.Uid).ConfigureAwait(false);
                if (capabilities == null) throw new DiscoveryException($"Failed to initialize adapter {adapter.Uid} in component {Uid}. There is no response from DiscoveryResponse command");

                adapter.AddRequierdProperties(capabilities.RequierdProperties);

                CreateAdapterStateMap(adapter, capabilities.SupportedStates);
                CreateCapabilities(capabilities);

                Subscribe<Event>(adapter.GetRoutingFilter());
            }
        }

        private void CreateAdapterStateMap(AdapterReference adapter, State[] states)
        {
            states.ForEach(state => _adapterStateMap[state.Name] = adapter);
        }

        private void CreateCapabilities(DiscoveryResponse capabilities)
        {
            _capabilities.AddRangeNewOnly(capabilities.SupportedStates.ToDictionary(key => key.Name, val => val));
        }
        
        /// <summary>
        /// Subscribe for commands addressed to this component via event aggregator
        /// </summary>
        private void SubscribeForRemoteCommands() => Subscribe<Command>(new RoutingFilter(Uid));

        /// <summary>
        /// Every message that is not directly should be check for compatibility with connected adapters
        /// </summary>
        protected override async Task UnhandledMessage(object message)
        {
            var actorMessage = message as ActorMessage;

            bool handled = false;

            if (actorMessage is Command command)
            {
                // TODO use value converter before publish
                var supportedCapabilities = _capabilities.Values.Where(capability => capability.IsCommandSupported(command));
                foreach (var state in supportedCapabilities)
                {
                    var adapter = _adapterStateMap[state.Name];
                    var adapterCommand = adapter.GetDeviceCommand(command);
                    MessageBroker.Send(adapterCommand, adapter.Uid);
                    handled = true;
                }
            }

            if (!handled)
            {
                await base.UnhandledMessage(actorMessage).ConfigureAwait(false);
            }
        }

        protected async Task Handle(Event ev)
        {
            var trigger = _triggers.Where(e => e.Event != null).FirstOrDefault(t => t.Event.Equals(ev));
            if (trigger != null)
            {
                await HandleEventInTrigger(trigger).ConfigureAwait(false);
            }

            if (ev is PropertyChangedEvent propertyChanged)
            {
                await HandlePropertyChange(propertyChanged).ConfigureAwait(false);
            }
        }

        private async Task HandlePropertyChange(PropertyChangedEvent propertyChanged)
        {
            var propertyName = propertyChanged.PropertyChangedName;
            if (!_capabilities.ContainsKey(propertyName)) return;

            var state = _capabilities[propertyName];
            var oldValue = state.Value;
            var newValue = propertyChanged.NewValue;

            if (newValue.Equals(oldValue)) return;

            state.Value = newValue;

            await MessageBroker.PublishEvent(PropertyChangedEvent.Create(Uid, propertyName, oldValue, newValue)).ConfigureAwait(false);
        }

        private async Task HandleEventInTrigger(Trigger trigger)
        {
            if (await trigger.ValidateCondition().ConfigureAwait(false))
            {
                foreach (var command in trigger.Commands)
                {
                    if (command.ContainsProperty(MessageProperties.ExecutionDelay))
                    {
                        var executionDelay = command.AsTime(MessageProperties.ExecutionDelay);
                        var cancelPrevious = command.AsBool(MessageProperties.CancelPrevious, false);
                        await Scheduler.DelayExecution<DelayCommandJob>(executionDelay, command, $"{Uid}_{command.Type}", cancelPrevious).ConfigureAwait(false);
                        continue;
                    }

                    MessageBroker.Send(command, Self);
                }
            }
        }

        protected IReadOnlyCollection<string> Handle(CapabilitiesQuery command) => _capabilities.Values
                                                                                                .Select(cap => cap.CapabilityName)
                                                                                                .Distinct()
                                                                                                .ToList()
                                                                                                .AsReadOnly();

        protected IReadOnlyCollection<string> Handle(SupportedStatesQuery command) => _capabilities.Values
                                                                                   .Select(cap => cap.Name)
                                                                                   .Distinct()
                                                                                   .ToList()
                                                                                   .AsReadOnly();

       

        protected string Handle(StateQuery command)
        {
            var stateName = command.AsString(MessageProperties.StateName);

            if (!_capabilities.ContainsKey(stateName)) return null;
            var value = _capabilities[stateName][StateProperties.Value];
            if (_converters.ContainsKey(stateName))
            {
                value = _converters[stateName].Convert(value);
            }

            return value;
        }

        protected IReadOnlyCollection<State> Handle(StatusQuery command) => _capabilities.Values.ToList().AsReadOnly();
    }
}