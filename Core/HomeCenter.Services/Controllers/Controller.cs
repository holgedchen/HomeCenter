﻿using AutoMapper;
using CSharpFunctionalExtensions;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Services.Configuration;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.Roslyn;
using HomeCenter.Utils;
using HTTPnet.Core.Pipeline;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace HomeCenter.Services.Controllers
{
    [ProxyCodeGenerator]
    public abstract class Controller : DeviceActor
    {
        private readonly IScheduler _scheduler;
        private readonly IRoslynCompilerService _roslynCompilerService;
        private readonly IControllerOptions _controllerOptions;
        private readonly IConfigurationService _configurationService;
        private readonly IResourceLocatorService _resourceLocatorService;
        private readonly IMapper _mapper;
        private readonly IHttpServerService _httpServerService;

        private HomeCenterConfiguration _homeConfiguration;

        protected Controller(IEventAggregator eventAggregator, IMapper mapper, IHttpServerService httpServerService, IScheduler scheduler, IRoslynCompilerService roslynCompilerService,
            IResourceLocatorService resourceLocatorService, IConfigurationService configurationService, IControllerOptions controllerOptions) : base(eventAggregator)
        {
            _scheduler = scheduler;
            _roslynCompilerService = roslynCompilerService;
            _controllerOptions = controllerOptions;
            _configurationService = configurationService;
            _resourceLocatorService = resourceLocatorService;
            _mapper = mapper;
            _httpServerService = httpServerService;
        }

        protected override async Task OnStarted(Proto.IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            RegisterRestCommandHanler();
            LoadDynamicAdapters(_controllerOptions.AdapterMode);

            await LoadCalendars().ConfigureAwait(false);
            InitializeConfiguration();
            await RunScheduler().ConfigureAwait(false);
        }

        private void RegisterRestCommandHanler()
        {
            _httpServerService.AddRequestHandler(new RestCommandHandler(_eventAggregator, _mapper));

            if (_controllerOptions.HttpServerPort.HasValue)
            {
                _httpServerService.UpdateServerPort(_controllerOptions.HttpServerPort.Value);
            }
        }

        private async Task LoadCalendars()
        {
            foreach (var calendarType in AssemblyHelper.GetAllTypes<ICalendar>())
            {
                var cal = calendarType.CreateInstance<ICalendar>();
                await _scheduler.AddCalendar(calendarType.Name, cal, false, false).ConfigureAwait(false);
            }
        }

        private Task RunScheduler() => _scheduler.Start(_disposables.Token);

        private void LoadDynamicAdapters(AdapterMode adapterMode)
        {
            _logger.LogInformation($"Loading adapters in mode: {adapterMode}");

            if (adapterMode == AdapterMode.Compiled)
            {
                var result = _roslynCompilerService.CompileAssemblies(_resourceLocatorService.GetRepositoyLocation());
                var veryfy = Result.Combine(result.ToArray());
                if (veryfy.IsFailure) throw new CompilationException($"Error while compiling adapters: {veryfy.Error}");

                foreach (var adapter in result)
                {
                    Assembly.LoadFrom(adapter.Value);
                }
            }
            else
            {
                _logger.LogInformation($"Using only build in adapters");
            }
        }

        private void InitializeConfiguration()
        {
            _homeConfiguration = _configurationService.ReadConfiguration(_controllerOptions.AdapterMode);
        }
    }

    public class RestCommandHandler : IHttpContextPipelineHandler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMapper _mapper;

        public RestCommandHandler(IEventAggregator eventAggregator, IMapper mapper)
        {
            _eventAggregator = eventAggregator;
            _mapper = mapper;
        }

        public async Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
        {
            if (context.HttpContext.Request.Method.Equals(HttpMethod.Post.Method) && context.HttpContext.Request.Uri.Equals("/api"))
            {
                using (var reader = new StreamReader(context.HttpContext.Request.Body))
                {
                    var rawCommandString = await reader.ReadToEndAsync().ConfigureAwait(false);
                    var result = JsonConvert.DeserializeObject<CommandDTO>(rawCommandString);

                    var command = _mapper.Map<Command>(result);

                    //TODO DNF
                    //await _eventAggregator.PublishDeviceCommnd(command).ConfigureAwait(false);
                }

                //context.HttpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(s.ToUpperInvariant()));
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK; // OK is also default

                context.BreakPipeline = true;
                return;
            }

            return;
        }

        public Task ProcessResponseAsync(HttpContextPipelineHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }
}