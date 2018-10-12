﻿using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    [TestClass]
    public class AdaptersTests : ReactiveTest
    {
        [TestMethod]
        public async Task AdapterCommandExecuteShouldGetResult()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("ComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);

            await Task.Delay(1000000);

            //var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            //var adapter = new TestAdapter("adapter1", adapterServiceFactory);
            //   await adapter.Initialize().ConfigureAwait(false);

            //TODO Test
            //var result = await adapter.ExecuteCommand<DiscoveryResponse>(CommandFatory.DiscoverCapabilitiesCommand).ConfigureAwait(false);

            //Assert.AreEqual(1, result.SupportedStates.Length);
            //Assert.IsInstanceOfType(result.SupportedStates[0], typeof(PowerState));
        }

        //[TestMethod]
        //public async Task MultiThreadAdapterCommandsExecuteShouldBeQueued()
        //{
        //    var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
        //                                                               .BuildAndRun()
        //                                                               .ConfigureAwait(false);
        //    var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
        //    var adapter = new TestAdapter("adapter1", adapterServiceFactory);
        //    await adapter.Initialize().ConfigureAwait(false);

        //    var taskList = new List<Task>();

        //    for (int i = 0; i < 10; i++)
        //    {
        //        taskList.Add(Task.Run(() => adapter.ExecuteCommand(RefreshCommand.Default)));
        //    }

        //    await Task.WhenAll(taskList).ConfigureAwait(false);

        //    Assert.AreEqual(0, adapter.Counter);
        //}

        //[TestMethod]
        //public async Task AdapterCommandViaEventAggregatorExecuteShouldGetResult()
        //{
        //    var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
        //                                                               .BuildAndRun()
        //                                                               .ConfigureAwait(false);
        //    var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
        //    var eventAggregator = container.GetInstance<IEventAggregator>();
        //    var adapter = new TestAdapter("adapter1", adapterServiceFactory);
        //    await adapter.Initialize().ConfigureAwait(false);

        //    var result = await eventAggregator.QueryDeviceAsync<DiscoveryResponse>(DiscoverQuery.Query(adapter.Uid)).ConfigureAwait(false);

        //    Assert.AreEqual(1, result.SupportedStates.Length);
        //    Assert.IsInstanceOfType(result.SupportedStates[0], typeof(PowerState));
        //}

        //[TestMethod]
        //[ExpectedException(typeof(TimeoutException))]
        //public async Task AdapterCommandViaEventAggregatorExecuteShouldTimeoutWhenExecuteToLong()
        //{
        //    var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
        //                                                               .BuildAndRun()
        //                                                               .ConfigureAwait(false);
        //    var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
        //    var eventAggregator = container.GetInstance<IEventAggregator>();
        //    var adapter = new TestAdapter("adapter1", adapterServiceFactory);
        //    await adapter.Initialize().ConfigureAwait(false);

        //    var result = await eventAggregator.QueryDeviceAsync<DiscoveryResponse>(DiscoverQuery.Query(adapter.Uid), TimeSpan.FromMilliseconds(20)).ConfigureAwait(false);
        //}

        //TODO
        //[TestMethod]
        //public async Task GenerateAdapter()
        //{
        //    var adaptersRepo = Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..")), "Test");

        //    var (controller, container) = await new ControllerBuilder()//.WithAdapterRepositoryPath(adaptersRepo)
        //                                                               .WithConfiguration("oneComponentConfiguration")
        //                                                               .BuildAndRun()
        //                                                               .ConfigureAwait(false);

        //    var component = await controller.ExecuteCommand<Component>(CommandFatory.GetComponentCommand("RemoteLamp")).ConfigureAwait(false);
        //    await component.ExecuteCommand(CommandFatory.TurnOnCommand).ConfigureAwait(false);
        //}
    }
}