﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Apizr.Configuring;
using Apizr.Configuring.Manager;
using Apizr.Logging;
using Apizr.Mediation.Extending;
using Apizr.Mediation.Requesting.Sending;
using Apizr.Optional.Extending;
using Apizr.Optional.Requesting.Sending;
using Apizr.Progressing;
using Apizr.Requesting;
using Apizr.Tests.Apis;
using Apizr.Tests.Helpers;
using Apizr.Tests.Models;
using Apizr.Tests.Settings;
using Apizr.Transferring.Managing;
using Apizr.Transferring.Requesting;
using AutoMapper.Internal;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonkeyCache.FileStore;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Refit;
using Xunit;
using Xunit.Abstractions;
using IHttpBinService = Apizr.Tests.Apis.IHttpBinService;

namespace Apizr.Tests
{
    public class ApizrExtendedTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ResiliencePipelineBuilder<HttpResponseMessage> _resiliencePipelineBuilder;
        private readonly RefitSettings _refitSettings;
        private readonly Assembly _assembly;

        public ApizrExtendedTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _resiliencePipelineBuilder = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(
                    new RetryStrategyOptions<HttpResponseMessage>
                    {
                        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                            .Handle<HttpRequestException>()
                            .HandleResult(response =>
                                response.StatusCode is >= HttpStatusCode.InternalServerError
                                    or HttpStatusCode.RequestTimeout),
                        Delay = TimeSpan.FromSeconds(1),
                        MaxRetryAttempts = 3,
                        UseJitter = true,
                        BackoffType = DelayBackoffType.Exponential
                    });

            var opts = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            _refitSettings = new RefitSettings(new SystemTextJsonContentSerializer(opts));

            _assembly = Assembly.GetExecutingAssembly();

            Barrel.ApplicationId = nameof(ApizrExtendedRegistryTests);
        }

        [Fact]
        public void ServiceCollection_Should_Contain_Managers()
        {
            var services = new ServiceCollection();
            services.AddApizrManagerFor<IReqResUserService>()
                .AddApizrManagerFor<IHttpBinService>()

                .AddApizrCrudManagerFor<User, int, PagedResult<User>, IDictionary<string, object>>()

                .AddApizrUploadManager(options => options.WithBaseAddress("https://test.com"))
                .AddApizrUploadManagerFor<ITransferSampleApi>()
                .AddApizrUploadManagerWith<string>(options => options.WithBaseAddress("https://test.com"))

                .AddApizrDownloadManager(options => options.WithBaseAddress("https://test.com"))
                .AddApizrDownloadManagerFor<ITransferSampleApi>()
                .AddApizrDownloadManagerWith<User>(options => options.WithBaseAddress("https://test.com"))

                .AddApizrTransferManager(options => options.WithBaseAddress("https://test.com"))
                .AddApizrTransferManagerFor<ITransferSampleApi>()
                .AddApizrTransferManagerWith<User, string>(options => options.WithBaseAddress("https://test.com"));
            
            services.Should().Contain(x => x.ServiceType == typeof(IApizrManager<IReqResUserService>))
                .And.Contain(x => x.ServiceType == typeof(IApizrManager<IHttpBinService>))

                .And.Contain(x => x.ServiceType == typeof(IApizrManager<ICrudApi<User, int, PagedResult<User>, IDictionary<string, object>>>))

                .And.Contain(x => x.ServiceType == typeof(IApizrUploadManager<IUploadApi>))
                .And.Contain(x => x.ServiceType == typeof(IApizrUploadManager<ITransferSampleApi>))
                .And.Contain(x => x.ServiceType == typeof(IApizrUploadManager<IUploadApi<string>, string>))
                .And.Contain(x => x.ServiceType == typeof(IApizrUploadManagerWith<string>))

                .And.Contain(x => x.ServiceType == typeof(IApizrDownloadManager<IDownloadApi>))
                .And.Contain(x => x.ServiceType == typeof(IApizrDownloadManager<ITransferSampleApi>))
                .And.Contain(x => x.ServiceType == typeof(IApizrDownloadManager<IDownloadApi<User>, User>))
                .And.Contain(x => x.ServiceType == typeof(IApizrDownloadManagerWith<User>))

                .And.Contain(x => x.ServiceType == typeof(IApizrTransferManager<ITransferApi>))
                .And.Contain(x => x.ServiceType == typeof(IApizrTransferManager<ITransferSampleApi>))
                .And.Contain(x => x.ServiceType == typeof(IApizrTransferManager<ITransferApi<User, string>, User, string>))
                .And.Contain(x => x.ServiceType == typeof(IApizrTransferManagerWith<User, string>));
        }

        [Fact]
        public void ServiceProvider_Should_Resolve_Managers()
        {
            var services = new ServiceCollection();
            services.AddApizrManagerFor<IReqResUserService>()
                .AddApizrManagerFor<IHttpBinService>()

                .AddApizrCrudManagerFor<User, int, PagedResult<User>, IDictionary<string, object>>()

                .AddApizrUploadManager(options => options.WithBaseAddress("https://test.com"))
                .AddApizrUploadManagerFor<ITransferSampleApi>()
                .AddApizrUploadManagerWith<string>(options => options.WithBaseAddress("https://test.com"))

                .AddApizrDownloadManager(options => options.WithBaseAddress("https://test.com"))
                .AddApizrDownloadManagerFor<ITransferSampleApi>()
                .AddApizrDownloadManagerWith<User>(options => options.WithBaseAddress("https://test.com"))

                .AddApizrTransferManager(options => options.WithBaseAddress("https://test.com"))
                .AddApizrTransferManagerFor<ITransferSampleApi>()
                .AddApizrTransferManagerWith<User, string>(options => options.WithBaseAddress("https://test.com"));

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<IApizrManager<IReqResUserService>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrManager<IHttpBinService>>().Should().NotBeNull();

            serviceProvider.GetService<IApizrManager<ICrudApi<User, int, PagedResult<User>, IDictionary<string, object>>>>().Should().NotBeNull();

            serviceProvider.GetService<IApizrUploadManager<IUploadApi>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrUploadManager<ITransferSampleApi>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrUploadManager<IUploadApi<string>, string>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrUploadManagerWith<string>>().Should().NotBeNull();

            serviceProvider.GetService<IApizrDownloadManager<IDownloadApi>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrDownloadManager<ITransferSampleApi>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrDownloadManager<IDownloadApi<User>, User>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrDownloadManagerWith<User>>().Should().NotBeNull();

            serviceProvider.GetService<IApizrTransferManager<ITransferApi>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrTransferManager<ITransferSampleApi>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrTransferManager<ITransferApi<User, string>, User, string>>().Should().NotBeNull();
            serviceProvider.GetService<IApizrTransferManagerWith<User, string>>().Should().NotBeNull();
        }

        [Fact]
        public void ServiceProvider_Should_Resolve_Scanned_Managers()
        {
            var services = new ServiceCollection();
            services.AddApizrCrudManagerFor(_assembly);

            var serviceProvider = services.BuildServiceProvider();
            var userManager = serviceProvider.GetService<IApizrManager<ICrudApi<User, int, PagedResult<User>, IDictionary<string, object>>>>();
            
            userManager.Should().NotBeNull();
        }

        [Fact]
        public void Calling_WithBaseAddress_Should_Set_BaseAddress()
        {
            var attributeUri = "https://reqres.in/api";
            var uri1 = new Uri("http://uri1.com");

            // By attribute
            var services = new ServiceCollection();
            services.AddApizrManagerFor<IReqResUserService>();

            var serviceProvider = services.BuildServiceProvider();
            var fixture = serviceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            fixture.Options.BaseUri.Should().Be(attributeUri);

            // By proper option overriding attribute
            services = new ServiceCollection();
            services.AddApizrManagerFor<IReqResUserService>(options => options.WithBaseAddress(uri1));

            serviceProvider = services.BuildServiceProvider();
            fixture = serviceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            fixture.Options.BaseUri.Should().Be(uri1);
        }

        [Fact]
        public void Calling_WithBaseAddress_And_WithBasePath_Should_Set_BaseUri()
        {
            var baseAddress = "https://reqres.in/api";
            var basePath = "users";
            var baseUri = $"{baseAddress}/{basePath}";

            var services = new ServiceCollection();
            services.AddApizrManagerFor<IReqResUserService>(options => options.WithBaseAddress(baseAddress).WithBasePath(basePath));

            var serviceProvider = services.BuildServiceProvider();
            var fixture = serviceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            fixture.Options.BaseUri.Should().Be(baseUri);
        }

        [Fact]
        public async Task Calling_WithAuthenticationHandler_ProperOption_Should_Authenticate_Request()
        {
            string token = null;

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IHttpBinService>(options => options
                        .WithLogging()
                        .WithAuthenticationHandler(_ => Task.FromResult(token = "token")));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var httpBinManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IHttpBinService>>();

            var result = await httpBinManager.ExecuteAsync(api => api.AuthBearerAsync());

            result.IsSuccessStatusCode.Should().BeTrue();
            token.Should().Be("token");
        }

        [Fact]
        public async Task Calling_WithAuthenticationHandler_With_Default_Token_Should_Authenticate_Request()
        {
            var testSettings = new TestSettings("token");

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(testSettings);

                    services.AddApizrManagerFor<IHttpBinService>(options => options
                        .WithLogging()
                        .WithAuthenticationHandler<TestSettings>(settings => settings.TestJsonString));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var httpBinManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IHttpBinService>>();

            var result = await httpBinManager.ExecuteAsync(api => api.AuthBearerAsync());

            result.IsSuccessStatusCode.Should().BeTrue();
        }

        [Fact]
        public void Calling_WithLogging_Should_Set_LoggingSettings()
        {
            var services = new ServiceCollection();
            services.AddApizrManagerFor<IReqResUserService>(options => options.WithLogging((HttpTracerMode) HttpTracerMode.ExceptionsOnly, (HttpMessageParts) HttpMessageParts.RequestCookies, LogLevel.Warning));

            var serviceProvider = services.BuildServiceProvider();
            var fixture = serviceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            fixture.Options.HttpTracerMode.Should().Be(HttpTracerMode.ExceptionsOnly);
            fixture.Options.TrafficVerbosity.Should().Be(HttpMessageParts.RequestCookies);
            fixture.Options.LogLevels.Should().AllBeEquivalentTo(LogLevel.Warning);
        }

        [Fact]
        public async Task Calling_WithAuthenticationHandler_Should_Authenticate_Request()
        {
            string token = null;

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IHttpBinService>(options => options
                        .WithLogging()
                        .WithAuthenticationHandler(_ => Task.FromResult(token = "tokenA")));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var httpBinManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IHttpBinService>>();

            var result = await httpBinManager.ExecuteAsync(api => api.AuthBearerAsync());

            result.IsSuccessStatusCode.Should().BeTrue();
            token.Should().Be("tokenA");
        }

        [Fact]
        public async Task Calling_WithAkavacheCacheHandler_Should_Cache_Result()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(config => config
                        .WithLogging()
                        .WithAkavacheCacheHandler()
                        .AddDelegatingHandler(new TestRequestHandler()));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            // Clear cache
            await reqResManager.ClearCacheAsync();

            // Defining a throwing request
            Func<Task> act = () => reqResManager.ExecuteAsync(api => api.GetUsersAsync(HttpStatusCode.BadRequest));

            // Calling it at first execution should throw as expected without any cached result
            var ex = await act.Should().ThrowAsync<ApizrException<ApiResult<User>>>();
            ex.And.CachedResult.Should().BeNull();

            // This one should succeed
            var result = await reqResManager.ExecuteAsync(api => api.GetUsersAsync());

            // and cache result in-memory
            result.Should().NotBeNull();
            result.Data.Should().NotBeNullOrEmpty();

            // This one should fail but with cached result
            var ex2 = await act.Should().ThrowAsync<ApizrException<ApiResult<User>>>();
            ex2.And.CachedResult.Should().NotBeNull();
        }

        [Fact]
        public async Task RequestTimeout_Should_Be_Handled_By_Polly()
        {
            var maxRetryAttempts = 3;
            var retryCount = 0;

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddMemoryCache();

                    services.AddApizrManagerFor<IReqResUserService>(config => config
                        .WithLogging()
                        .AddDelegatingHandler(new TestRequestHandler()));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddRetry(
                            new RetryStrategyOptions<HttpResponseMessage>
                            {
                                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                    .Handle<HttpRequestException>()
                                    .HandleResult(response =>
                                        response.StatusCode is >= HttpStatusCode.InternalServerError
                                            or HttpStatusCode.RequestTimeout),
                                Delay = TimeSpan.FromSeconds(1),
                                MaxRetryAttempts = maxRetryAttempts,
                                UseJitter = true,
                                BackoffType = DelayBackoffType.Exponential,
                                OnRetry = args =>
                                {
                                    retryCount = args.AttemptNumber + 1;
                                    return default;
                                }
                            }));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            // Defining a transient throwing request
            Func<Task> act = () => reqResManager.ExecuteAsync(api => api.GetUsersAsync(HttpStatusCode.RequestTimeout));

            // Calling it should throw but handled by Polly
            await act.Should().ThrowAsync<ApizrException>();

            // attempts should be equal to total retry count
            retryCount.Should().Be(maxRetryAttempts);
        }

        [Fact]
        public async Task Calling_WithConnectivityHandler_Should_Check_Connectivity()
        {
            var isConnected = false;

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(config => config
                        .WithLogging()
                        .WithConnectivityHandler(() => isConnected));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            // Defining a request
            Func<Task> act = () => reqResManager.ExecuteAsync(api => api.GetUsersAsync());

            // Calling it should throw as isConnected is at false
            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<IOException>();

            // Setting isConnected to true
            isConnected = true;

            // Then request should succeed
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public void Calling_WithRefitSettings_Should_Set_Settings()
        {
            var services = new ServiceCollection();
            services.AddAutoMapper(_assembly);
            services.AddApizrManagerFor<IReqResUserService>(config => config
                    .WithRefitSettings(_refitSettings));

            var serviceProvider = services.BuildServiceProvider();
            var apizrOptions = serviceProvider.GetRequiredService<IApizrManagerOptions<IReqResUserService>>();

            apizrOptions.RefitSettings.Should().Be(_refitSettings);
        }

        [Fact]
        public async Task Calling_WithAutoMapperMappingHandler_Should_Map_Data()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddAutoMapper(_assembly);
                    services.AddApizrManagerFor<IReqResUserService>(config => config
                        .WithLogging()
                        .WithRefitSettings(_refitSettings)
                        .WithAutoMapperMappingHandler());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            var minUser = new MinUser { Name = "John" };

            // This one should succeed
            var result = await reqResManager.ExecuteAsync<MinUser, User>((api, user) => api.CreateUser(user, CancellationToken.None), minUser);

            result.Should().NotBeNull();
            result.Name.Should().Be(minUser.Name);
            result.Id.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task Calling_WithMappingHandler_With_AutoMapper_Should_Map_Data()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddAutoMapper(_assembly);

                    services.AddApizrManagerFor<IReqResUserService>(config => config
                        .WithLogging()
                        .WithRefitSettings(_refitSettings)
                        .WithMappingHandler<AutoMapperMappingHandler>());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            var minUser = new MinUser { Name = "John" };

            // This one should succeed
            var result = await reqResManager.ExecuteAsync<MinUser, User>((api, user) => api.CreateUser(user, CancellationToken.None), minUser);

            result.Should().NotBeNull();
            result.Name.Should().Be(minUser.Name);
            result.Id.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task Calling_WithMapsterMappingHandler_Should_Map_Data()
        {
            var mapsterConfig = new TypeAdapterConfig();
            mapsterConfig.NewConfig<User, MinUser>()
                .TwoWays()
                .Map(minUser => minUser.Name, user => user.FirstName);

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(mapsterConfig);
                    services.AddSingleton<IMapper, ServiceMapper>();

                    services.AddApizrManagerFor<IReqResUserService>(config => config
                        .WithLogging()
                        .WithRefitSettings(_refitSettings)
                        .WithMapsterMappingHandler());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            var minUser = new MinUser { Name = "John" };

            // This one should succeed
            var result = await reqResManager.ExecuteAsync<MinUser, User>((api, user) => api.CreateUser(user, CancellationToken.None), minUser);

            result.Should().NotBeNull();
            result.Name.Should().Be(minUser.Name);
            result.Id.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task Calling_WithMappingHandler_With_Mapster_Should_Map_Data()
        {
            var mapsterConfig = new TypeAdapterConfig();
            mapsterConfig.NewConfig<User, MinUser>()
                .TwoWays()
                .Map(minUser => minUser.Name, user => user.FirstName);

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(mapsterConfig);
                    services.AddSingleton<IMapper, ServiceMapper>();

                    services.AddApizrManagerFor<IReqResUserService>(config => config
                        .WithLogging()
                        .WithRefitSettings(_refitSettings)
                        .WithMappingHandler<MapsterMappingHandler>());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            var minUser = new MinUser { Name = "John" };

            // This one should succeed
            var result = await reqResManager.ExecuteAsync<MinUser, User>((api, user) => api.CreateUser(user, CancellationToken.None), minUser);

            result.Should().NotBeNull();
            result.Name.Should().Be(minUser.Name);
            result.Id.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task Requesting_With_Context_into_Options_Should_Set_Context()
        {
            var watcher = new WatchingRequestHandler();

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResSimpleService>(options => options
                        .WithLogging()
                        .WithResilienceContextOptions(opt =>
                            opt.ReturnToPoolOnComplete(false))
                        .AddDelegatingHandler(watcher));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();
            
            // Get instances from the container
            var apizrManager = scope.ServiceProvider.GetService<IApizrManager<IReqResSimpleService>>();
            apizrManager.Should().NotBeNull();

            ResiliencePropertyKey<int> testKey = new("TestKey1");
            var testValue = 1;

            await apizrManager.ExecuteAsync((opt, api) => api.GetUsersAsync(opt), options => options.WithResilienceProperty(testKey, testValue));
            watcher.Context.Should().NotBeNull();
            watcher.Context.Properties.TryGetValue(testKey, out var value).Should().BeTrue();
            value.Should().Be(testValue);
        }

        [Fact]
        public async Task Requesting_With_ResilienceProperties_At_Multiple_Levels_Should_Merge_It_All_At_The_End()
        {
            var watcher = new WatchingRequestHandler();

            ResiliencePropertyKey<string> testKey1 = new(nameof(testKey1));
            ResiliencePropertyKey<string> testKey2 = new(nameof(testKey2));
            ResiliencePropertyKey<string> testKey3 = new(nameof(testKey3));

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResSimpleService>(options =>
                        options
                            .WithLogging()
                            .WithResilienceContextOptions(opt =>
                                opt.ReturnToPoolOnComplete(false))
                            .WithResilienceProperty(testKey1, _ => "testValue1")
                            .WithResilienceProperty(testKey2, _ => "testValue2.1")
                            .AddDelegatingHandler(watcher));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            // Get instances from the container
            var apizrManager = scope.ServiceProvider.GetService<IApizrManager<IReqResSimpleService>>();

            await apizrManager.ExecuteAsync((opt, api) => api.GetUsersAsync(opt),
                options => options.WithResilienceProperty(testKey2, "testValue2.2")
                    .WithResilienceProperty(testKey3, "testValue3"));

            watcher.Context.Should().NotBeNull();
            watcher.Context.Properties.TryGetValue(testKey1, out var valueKey1).Should().BeTrue(); // Set by manager option
            valueKey1.Should().Be("testValue1");
            watcher.Context.Properties.TryGetValue(testKey2, out var valueKey2).Should().BeTrue(); // Set by manager option then updated by the request one
            valueKey2.Should().Be("testValue2.2");
            watcher.Context.Properties.TryGetValue(testKey3, out var valueKey3).Should().BeTrue(); // Set by request option
            valueKey3.Should().Be("testValue3");
        }

        [Fact]
        public async Task Calling_WithMediation_Should_Handle_Requests()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

                    services.AddApizrManagerFor<IReqResUserService>(config => config
                        .WithLogging()
                        .WithMediation());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResMediator = scope.ServiceProvider.GetRequiredService<IApizrMediator<IReqResUserService>>();

            reqResMediator.Should().NotBeNull();
            var result = await reqResMediator.SendFor(api => api.GetUsersAsync());
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
            result.Data.Count.Should().BeGreaterOrEqualTo(1);

            var response = await reqResMediator.SendFor(api => api.SafeGetUsersAsync());
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.Exception.Should().BeNull();
            response.Result.Should().NotBeNull();
            response.Result.Data.Should().NotBeNull();
            response.Result.Data.Count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public async Task Calling_WithOptionalMediation_Should_Handle_Requests_With_Optional_Result()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

                    services.AddApizrManagerFor<IReqResUserService>(config => config
                        .WithLogging()
                        .WithOptionalMediation());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResMediator = scope.ServiceProvider.GetRequiredService<IApizrOptionalMediator<IReqResUserService>>();

            reqResMediator.Should().NotBeNull();
            var result = await reqResMediator.SendFor(api => api.GetUsersAsync());
            result.Should().NotBeNull();
            result.Match(userList =>
            {
                userList.Should().NotBeNull();
                userList.Data.Should().NotBeNull();
                userList.Data.Count.Should().BeGreaterOrEqualTo(1);
            },
                e => {
                    // ignore error
                });
        }

        [Fact]
        public async Task Downloading_File_Should_Succeed()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrTransferManager(options => options.WithBaseAddress("http://speedtest.ftp.otenet.gr/files"))
                        .AddApizrTransferManagerFor<ITransferUndefinedApi>(options => options.WithBaseAddress("http://speedtest.ftp.otenet.gr/files"))
                        .AddApizrDownloadManager(options => options.WithBaseAddress("http://speedtest.ftp.otenet.gr/files"))
                        .AddApizrDownloadManagerFor<ITransferUndefinedApi>(options => options.WithBaseAddress("http://speedtest.ftp.otenet.gr/files"));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            // Get instances from the container
            var apizrTransferManager = scope.ServiceProvider.GetService<IApizrTransferManager>(); // Built-in
            var apizrTransferTypedManager = scope.ServiceProvider.GetService<IApizrTransferManager<ITransferApi>>(); // Built-in
            var apizrCustomTransferManager = scope.ServiceProvider.GetService<IApizrTransferManager<ITransferUndefinedApi>>(); // Custom
            var apizrDownloadManager = scope.ServiceProvider.GetService<IApizrDownloadManager>(); // Built-in
            var apizrDownloadTypedManager = scope.ServiceProvider.GetService<IApizrDownloadManager<IDownloadApi>>(); // Built-in
            var apizrCustomDownloadManager = scope.ServiceProvider.GetService<IApizrDownloadManager<ITransferUndefinedApi>>(); // Custom

            apizrTransferManager.Should().NotBeNull(); // Built-in
            apizrTransferTypedManager.Should().NotBeNull(); // Built-in
            apizrCustomTransferManager.Should().NotBeNull(); // Custom
            apizrDownloadManager.Should().NotBeNull(); // Built-in
            apizrDownloadTypedManager.Should().NotBeNull(); // Built-in
            apizrCustomDownloadManager.Should().NotBeNull(); // Custom

            // Transfer
            // Built-in
            var apizrTransferManagerResult = await apizrTransferManager.DownloadAsync(new FileInfo("test100k.db"));
            apizrTransferManagerResult.Should().NotBeNull();
            apizrTransferManagerResult.Length.Should().BePositive();

            // Built-in
            var apizrTransferTypedManagerResult = await apizrTransferTypedManager.DownloadAsync(new FileInfo("test100k.db"));
            apizrTransferTypedManagerResult.Should().NotBeNull();
            apizrTransferTypedManagerResult.Length.Should().BePositive();

            // Custom
            var apizrCustomTransferManagerResult = await apizrCustomTransferManager.DownloadAsync(new FileInfo("test100k.db"));
            apizrCustomTransferManagerResult.Should().NotBeNull();
            apizrCustomTransferManagerResult.Length.Should().BePositive();

            // Download
            // Built-in
            var apizrDownloadManagerResult = await apizrDownloadManager.DownloadAsync(new FileInfo("test100k.db"));
            apizrDownloadManagerResult.Should().NotBeNull();
            apizrDownloadManagerResult.Length.Should().BePositive();

            // Built-in
            var apizrDownloadTypedManagerResult = await apizrDownloadTypedManager.DownloadAsync(new FileInfo("test100k.db"));
            apizrDownloadTypedManagerResult.Should().NotBeNull();
            apizrDownloadTypedManagerResult.Length.Should().BePositive();

            // Custom
            var apizrCustomDownloadManagerResult = await apizrCustomDownloadManager.DownloadAsync(new FileInfo("test100k.db"));
            apizrCustomDownloadManagerResult.Should().NotBeNull();
            apizrCustomDownloadManagerResult.Length.Should().BePositive();
        }

        [Fact]
        public async Task Downloading_File_With_Local_Progress_Should_Report_Progress()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrTransferManager(options => options
                        .WithLogging()
                        .WithBaseAddress("http://speedtest.ftp.otenet.gr/files")
                        .WithProgress());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var apizrTransferManager = scope.ServiceProvider.GetService<IApizrTransferManager>(); // Built-in
            apizrTransferManager.Should().NotBeNull(); // Built-in

            var percentage = 0;
            var progress = new ApizrProgress();
            progress.ProgressChanged += (sender, args) =>
            {
                percentage = args.ProgressPercentage;
            };

            var fileInfo = await apizrTransferManager.DownloadAsync(new FileInfo("test10Mb.db"), options => options.WithProgress(progress)).ConfigureAwait(false);

            percentage.Should().Be(100);
            fileInfo.Length.Should().BePositive();
        }

        [Fact]
        public async Task Downloading_File_With_Global_Progress_Should_Report_Progress()
        {
            var percentage = 0;
            var progress = new ApizrProgress();
            progress.ProgressChanged += (sender, args) =>
            {
                percentage = args.ProgressPercentage;
            };

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrTransferManager(options => options
                        .WithLogging()
                        .WithBaseAddress("http://speedtest.ftp.otenet.gr/files")
                        .WithProgress(progress));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var apizrTransferManager = scope.ServiceProvider.GetService<IApizrTransferManager>(); // Built-in
            apizrTransferManager.Should().NotBeNull(); // Built-in

            var fileInfo = await apizrTransferManager.DownloadAsync(new FileInfo("test10Mb.db")).ConfigureAwait(false);

            percentage.Should().Be(100);
            fileInfo.Length.Should().BePositive();
        }

        [Fact]
        public async Task Uploading_File_Should_Succeed()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrTransferManager(options => options.WithBaseAddress("https://httpbin.org/post"))
                        .AddApizrTransferManagerFor<ITransferUndefinedApi>(options => options.WithBaseAddress("https://httpbin.org/post"))
                        .AddApizrUploadManager(options => options.WithBaseAddress("https://httpbin.org/post"))
                        .AddApizrUploadManagerFor<ITransferUndefinedApi>(options => options.WithBaseAddress("https://httpbin.org/post"));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            // Get instances from the container
            var apizrTransferManager = scope.ServiceProvider.GetService<IApizrTransferManager>(); // Built-in
            var apizrTransferTypedManager = scope.ServiceProvider.GetService<IApizrTransferManager<ITransferApi>>(); // Built-in
            var apizrCustomTransferManager = scope.ServiceProvider.GetService<IApizrTransferManager<ITransferUndefinedApi>>(); // Custom
            var apizrUploadManager = scope.ServiceProvider.GetService<IApizrUploadManager>(); // Built-in
            var apizrUploadTypedManager = scope.ServiceProvider.GetService<IApizrUploadManager<IUploadApi>>(); // Built-in
            var apizrCustomUploadManager = scope.ServiceProvider.GetService<IApizrUploadManager<ITransferUndefinedApi>>(); // Custom

            apizrTransferManager.Should().NotBeNull(); // Built-in
            apizrTransferTypedManager.Should().NotBeNull(); // Built-in
            apizrCustomTransferManager.Should().NotBeNull(); // Custom
            apizrUploadManager.Should().NotBeNull(); // Built-in
            apizrUploadTypedManager.Should().NotBeNull(); // Built-in
            apizrCustomUploadManager.Should().NotBeNull(); // Custom

            // Transfer
            // Built-in
            var apizrTransferManagerResult = await apizrTransferManager.UploadAsync(FileHelper.GetTestFileStreamPart("small"));
            apizrTransferManagerResult.Should().NotBeNull();
            apizrTransferManagerResult.StatusCode.Should().Be(HttpStatusCode.OK);

            // Built-in
            var apizrTransferTypedManagerResult = await apizrTransferTypedManager.UploadAsync(FileHelper.GetTestFileStreamPart("small"));
            apizrTransferTypedManagerResult.Should().NotBeNull();
            apizrTransferTypedManagerResult.StatusCode.Should().Be(HttpStatusCode.OK);

            // Custom
            var apizrCustomTransferManagerResult = await apizrCustomTransferManager.UploadAsync(FileHelper.GetTestFileStreamPart("small"));
            apizrCustomTransferManagerResult.Should().NotBeNull();
            apizrCustomTransferManagerResult.StatusCode.Should().Be(HttpStatusCode.OK);

            // Download
            // Built-in
            var apizrUploadManagerResult = await apizrUploadManager.UploadAsync(FileHelper.GetTestFileStreamPart("small"));
            apizrUploadManagerResult.Should().NotBeNull();
            apizrUploadManagerResult.StatusCode.Should().Be(HttpStatusCode.OK);

            // Built-in
            var apizrUploadTypedManagerResult = await apizrUploadTypedManager.UploadAsync(FileHelper.GetTestFileStreamPart("small"));
            apizrUploadTypedManagerResult.Should().NotBeNull();
            apizrUploadTypedManagerResult.StatusCode.Should().Be(HttpStatusCode.OK);

            // Custom
            var apizrCustomUploadManagerResult = await apizrCustomUploadManager.UploadAsync(FileHelper.GetTestFileStreamPart("small"));
            apizrCustomUploadManagerResult.Should().NotBeNull();
            apizrCustomUploadManagerResult.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Uploading_File_With_Local_Progress_Should_Report_Progress()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrTransferManager(options => options
                        .WithLogging()
                        .WithBaseAddress("https://httpbin.org/post")
                        .WithProgress());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var apizrTransferManager = scope.ServiceProvider.GetService<IApizrTransferManager>(); // Built-in
            apizrTransferManager.Should().NotBeNull(); // Built-in

            var percentage = 0;
            var progress = new ApizrProgress();
            progress.ProgressChanged += (sender, args) =>
            {
                percentage = args.ProgressPercentage;
            };

            var apizrTransferManagerResult = await apizrTransferManager.UploadAsync(FileHelper.GetTestFileStreamPart("small"), options => options.WithProgress(progress));

            apizrTransferManagerResult.Should().NotBeNull();
            apizrTransferManagerResult.StatusCode.Should().Be(HttpStatusCode.OK);
            percentage.Should().Be(100);
        }

        [Fact]
        public async Task Uploading_File_With_Global_Progress_Should_Report_Progress()
        {
            var percentage = 0;
            var progress = new ApizrProgress();
            progress.ProgressChanged += (sender, args) =>
            {
                percentage = args.ProgressPercentage;
            };

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrTransferManager(options => options
                        .WithLogging()
                        .WithBaseAddress("https://httpbin.org/post")
                        .WithProgress(progress));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var apizrTransferManager = scope.ServiceProvider.GetService<IApizrTransferManager>(); // Built-in
            apizrTransferManager.Should().NotBeNull(); // Built-in

            var apizrTransferManagerResult = await apizrTransferManager.UploadAsync(FileHelper.GetTestFileStreamPart("small"));

            apizrTransferManagerResult.Should().NotBeNull();
            apizrTransferManagerResult.StatusCode.Should().Be(HttpStatusCode.OK);
            percentage.Should().Be(100);
        }

        [Fact]
        public async Task Calling_WithFileTransferMediation_Should_Handle_Requests()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

                    services.AddApizrTransferManager(config => config
                        .WithLogging()
                        .WithBaseAddress("http://speedtest.ftp.otenet.gr/files")
                        .WithFileTransferMediation());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var apizrMediator = scope.ServiceProvider.GetRequiredService<IApizrMediator>();

            apizrMediator.Should().NotBeNull();
            var result = await apizrMediator.SendDownloadQuery(new FileInfo("test100k.db"));
            result.Should().NotBeNull();
            result.Length.Should().BePositive();
        }

        [Fact]
        public async Task Calling_WithFileTransferOptionalMediation_Should_Handle_Requests_With_Optional_Result()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

                    services.AddApizrTransferManager(config => config
                        .WithLogging()
                        .WithBaseAddress("http://speedtest.ftp.otenet.gr/files")
                        .WithFileTransferOptionalMediation());

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var apizrMediator = scope.ServiceProvider.GetRequiredService<IApizrOptionalMediator>();

            apizrMediator.Should().NotBeNull();
            var result = await apizrMediator.SendDownloadOptionalQuery(new FileInfo("test100k.db"));
            result.Should().NotBeNull();
            result.Match(fileInfo =>
            {
                fileInfo.Length.Should().BePositive();
            },
                e => {
                    // ignore error
                });
        }

        [Fact]
        public async Task Requesting_With_Headers_Should_Set_Headers()
        {
            var watcher = new WatchingRequestHandler();

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {

                    services.AddApizrManagerFor<IReqResSimpleService>(options => options
                        .WithLogging()
                        .WithBaseAddress("https://reqres.in/api")
                        .WithHeaders(["testKey2: testValue2.2", "testKey3: testValue3.1"])
                        .WithLoggedHeadersRedactionNames(new[] { "testKey2" })
                        .AddDelegatingHandler(watcher));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            // Get instances from the container
            var apizrManager = scope.ServiceProvider.GetService<IApizrManager<IReqResSimpleService>>();

            // Shortcut
            await apizrManager.ExecuteAsync((opt, api) => api.GetUsersAsync(opt), options => 
                options.WithHeaders(["testKey3: testValue3.2", "testKey4: testValue4"])
                    .WithLoggedHeadersRedactionRule(header => header == "testKey3"));
            watcher.Headers.Should().NotBeNull();
            watcher.Headers.Should().ContainKeys("testKey1", "testKey2", "testKey3", "testKey4");
            watcher.Headers.GetValues("testKey1").Should().HaveCount(1).And.Contain("testValue1"); // Set by attribute
            watcher.Headers.GetValues("testKey2").Should().HaveCount(1).And.Contain("testValue2.2"); // Set by attribute then updated by common option
            watcher.Headers.GetValues("testKey3").Should().HaveCount(1).And.Contain("testValue3.2"); // Set by common option then updated by request option
            watcher.Headers.GetValues("testKey4").Should().HaveCount(1).And.Contain("testValue4"); // Set by request option
        }

        [Fact]
        public async Task Requesting_With_Headers_Factory_Should_Set_And_Keep_Updated_Headers()
        {
            var watcher = new WatchingRequestHandler();
            var apiHeaders = new List<string> { "testKey2: testValue2.2" };
            var requestHeaders = new List<string> { "testKey3: testValue3.1", "testKey4: testValue4.1" };
            var testStore = new TestSettings("testStoreKey2: testStoreValue2.1");

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddSettings();
                    services.AddSingleton(_ => testStore);

                    services.AddApizrManagerFor<IReqResSimpleService>(options => options
                        .WithLogging()
                        .WithBaseAddress("https://reqres.in/api")
                        .WithHeaders(_ => apiHeaders, scope: ApizrLifetimeScope.Api)
                        .WithHeaders(_ => requestHeaders, scope: ApizrLifetimeScope.Request)
                        .WithHeaders<IOptions<TestSettings>>([settings => settings.Value.TestJsonString], scope: ApizrLifetimeScope.Request)
                        .WithHeaders(["testKey5: testValue5.1", "testKey6: testValue6.1"])
                        .WithHeaders(["testStoreKey1: testStoreValue1.1", "testStoreKey3: testStoreValue3.1"], mode: ApizrRegistrationMode.Store)
                        .WithHeaders<TestSettings>([settings => settings.TestJsonString], scope: ApizrLifetimeScope.Request, mode: ApizrRegistrationMode.Store)
                        .AddDelegatingHandler(watcher));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            // Get instances from the container
            var apizrManager = scope.ServiceProvider.GetService<IApizrManager<IReqResSimpleService>>();

            // Merge all
            await apizrManager.ExecuteAsync((opt, api) => api.GetUsersAsync(opt),
                options => options.WithHeaders(["testKey4: testValue4.2",
                    "testKey5: testValue5.2"]));
            watcher.Headers.Should().NotBeNull();
            watcher.Headers.Should().ContainKeys("testKey1", "testKey2", "testKey3", "testKey4", "testKey5", "testKey6", "testSettingsKey1", "testStoreKey1", "testStoreKey2")
                .And.NotContainKey("testStoreKey3:");
            watcher.Headers.GetValues("testKey1").Should().HaveCount(1).And.Contain("testValue1"); // Set by attribute
            watcher.Headers.GetValues("testKey2").Should().HaveCount(1).And.Contain("testValue2.2"); // Set by attribute then updated by common option within api scope factory
            watcher.Headers.GetValues("testKey3").Should().HaveCount(1).And.Contain("testValue3.1"); // Set by common option within request scope factory
            watcher.Headers.GetValues("testKey4").Should().HaveCount(1).And.Contain("testValue4.2"); // Set by common option within request scope factory then updated by request option
            watcher.Headers.GetValues("testKey5").Should().HaveCount(1).And.Contain("testValue5.2"); // Set by common option then updated by request option
            watcher.Headers.GetValues("testKey6").Should().HaveCount(1).And.Contain("testValue6.1"); // Set by common option
            watcher.Headers.GetValues("testSettingsKey1").Should().HaveCount(1).And.Contain("testSettingsValue1.1"); // Set by common option expression
            watcher.Headers.GetValues("testStoreKey1").Should().HaveCount(1).And.Contain("testStoreValue1.1"); // Set by common option from Store
            watcher.Headers.GetValues("testStoreKey2").Should().HaveCount(1).And.Contain("testStoreValue2.1"); // Set by common option from Store

            // Keep updated
            apiHeaders[0] = "testKey2: testValue2.3"; // will not be updated (scope: Api)
            requestHeaders[0] = "testKey3: testValue3.2"; // will be updated (scope: Request)
            requestHeaders[1] = "testKey4: testValue4.3"; // should be updated (scope: Request) but updated then by request option
            testStore.TestJsonString = "testStoreKey2: testStoreValue2.2";
            var settings = scope.ServiceProvider.GetService<IOptions<TestSettings>>();
            settings.Value.TestJsonString = "testSettingsKey1: testSettingsValue1.2"; // will be updated (scope: Request)

            await apizrManager.ExecuteAsync((opt, api) => api.GetUsersAsync(opt),
                options => options.WithHeaders(["testKey4: testValue4.4",
                    "testKey5: testValue5.3",
                    "testStoreKey1: testStoreValue1.2",
                    "testStoreKey3:"]));
            watcher.Headers.Should().NotBeNull();
            watcher.Headers.Should().ContainKeys("testKey1", "testKey2", "testKey3", "testKey4", "testKey5", "testKey6", "testSettingsKey1", "testStoreKey1", "testStoreKey2", "testStoreKey3");
            watcher.Headers.GetValues("testKey1").Should().HaveCount(1).And.Contain("testValue1"); // Same as previous value
            watcher.Headers.GetValues("testKey2").Should().HaveCount(1).And.Contain("testValue2.2"); // Same as previous value
            watcher.Headers.GetValues("testKey3").Should().HaveCount(1).And.Contain("testValue3.2"); // Updated at request time (scope: Request)
            watcher.Headers.GetValues("testKey4").Should().HaveCount(1).And.Contain("testValue4.4"); // Updated at request time (scope: Request) then by request option
            watcher.Headers.GetValues("testKey5").Should().HaveCount(1).And.Contain("testValue5.3"); // Updated by request option
            watcher.Headers.GetValues("testKey6").Should().HaveCount(1).And.Contain("testValue6.1"); // Same as previous value
            watcher.Headers.GetValues("testSettingsKey1").Should().HaveCount(1).And.Contain("testSettingsValue1.2"); // Updated at request time (scope: Request)
            watcher.Headers.GetValues("testStoreKey1").Should().HaveCount(1).And.Contain("testStoreValue1.2"); // Updated by request option
            watcher.Headers.GetValues("testStoreKey2").Should().HaveCount(1).And.Contain("testStoreValue2.2"); // Updated at request time (scope: Request)
            watcher.Headers.GetValues("testStoreKey3").Should().HaveCount(1).And.Contain("testStoreValue3.1"); // Set by request option from Store
        }

        [Fact]
        public async Task Requesting_With_Both_Attribute_And_Fluent_Headers_Should_Set_Merged_Headers()
        {
            var watcher = new WatchingRequestHandler();

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResSimpleService>(options => options
                        .WithLogging().WithHeaders(["testKey2: testValue2"])
                        .AddDelegatingHandler(watcher));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddPipeline(_resiliencePipelineBuilder.Build()));
                })
                .Build();

            var scope = host.Services.CreateScope();

            // Get instances from the container
            var reqResManager = scope.ServiceProvider.GetService<IApizrManager<IReqResSimpleService>>(); // Custom

            await reqResManager.ExecuteAsync((opt, api) => api.GetUsersAsync(opt),
                options => options.WithHeaders(["testKey3: testValue3", "testKey4: testValue4"]));
            watcher.Headers.Should().NotBeNull();
            watcher.Headers.Should().ContainKeys("testKey1", "testKey2", "testKey3", "testKey4");
        }

        [Fact]
        public async Task Concurrent_Requests_Should_Not_Throw()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResSimpleService>(options => options
                        .WithLogging());
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResSimpleService>>();

            var tasks = new List<Task>();
            for (var i = 0; i < 10; ++i)
            {
                tasks.Add(Task.Run(async () =>
                {
                    Func<Task> act = () => reqResManager.ExecuteAsync(api => api.GetUsersAsync());
                    await act.Should().NotThrowAsync();
                }));
            }
            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task Cancelling_A_Get_Request_Should_Throw_An_OperationCanceledException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(options => options
                        .WithLogging());
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            var ct = new CancellationTokenSource();
            ct.CancelAfter(TimeSpan.FromSeconds(2));

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(5, opt),
                    options => options.WithCancellation(ct.Token));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<OperationCanceledException>();
        }

        [Fact]
        public async Task Cancelling_A_Post_Request_Should_Throw_An_OperationCanceledException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IHttpBinService>(options => options
                        .WithLogging());
                })
                .Build();

            var scope = host.Services.CreateScope();

            var manager = scope.ServiceProvider.GetRequiredService<IApizrManager<IHttpBinService>>();

            var streamPart = FileHelper.GetTestFileStreamPart("medium");
            var ct = new CancellationTokenSource();
            ct.CancelAfter(TimeSpan.FromSeconds(2));

            Func<Task> act = () => manager.ExecuteAsync((opt, api) => api.UploadAsync(streamPart, opt),
                options => options.WithCancellation(ct.Token));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<OperationCanceledException>();
        }

        [Fact]
        public async Task Calling_ConfigureClient_Should_Configure_HttpClient()
        {
            var watcher = new WatchingRequestHandler();

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(options =>
                        options
                            .WithLogging()
                            .ConfigureHttpClientBuilder(builder => builder.ConfigureHttpClient(client =>
                                client.DefaultRequestHeaders.Add("HttpClientHeaderKey", "HttpClientHeaderValue")))
                            .AddDelegatingHandler(watcher));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            await reqResManager.ExecuteAsync((opt, api) => api.GetUsersAsync(opt));
            watcher.Headers.Should().NotBeNull();
            watcher.Headers.Should().ContainKey("HttpClientHeaderKey");
        }

        [Fact]
        public async Task When_Calling_BA_WithOperationTimeout_Then_Request_Should_Throw_A_TimeoutException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(
                        options => options
                            .WithLogging()
                            .WithOperationTimeout(TimeSpan.FromSeconds(4)));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(6, opt),
                    options => options.WithOperationTimeout(TimeSpan.FromSeconds(2)));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();
        }

        [Fact]
        public async Task When_Calling_AB_WithOperationTimeout_Then_Client_Should_Throw_A_TimeoutException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(options =>
                        options
                            .WithLogging()
                            .WithOperationTimeout(TimeSpan.FromSeconds(2)));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(6, opt),
                    options => options.WithOperationTimeout(TimeSpan.FromSeconds(4)));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();
        }

        [Fact]
        public async Task When_Calling_BA_WithRequestTimeout_Then_Request_Should_Throw_A_TimeoutException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(
                        options => options
                            .WithLogging()
                            .WithRequestTimeout(TimeSpan.FromSeconds(4)));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(6, opt),
                    options => options.WithRequestTimeout(TimeSpan.FromSeconds(2)));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();
        }

        [Fact]
        public async Task When_Calling_AB_WithRequestTimeout_Then_Client_Should_Throw_A_TimeoutException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(options =>
                        options
                            .WithLogging()
                            .WithRequestTimeout(TimeSpan.FromSeconds(2)));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(6, opt),
                    options => options.WithRequestTimeout(TimeSpan.FromSeconds(4)));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();
        }

        [Fact]
        public async Task When_Calling_DCBA_WithOperationTimeout_And_WithRequestTimeout_Then_Request_Should_Throw_A_TimeoutException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(
                        options => options
                            .WithLogging()
                            .WithOperationTimeout(TimeSpan.FromSeconds(8))
                            .WithRequestTimeout(TimeSpan.FromSeconds(6)));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(6, opt),
                    options => options.WithOperationTimeout(TimeSpan.FromSeconds(4))
                        .WithRequestTimeout(TimeSpan.FromSeconds(2)));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();
        }

        [Fact]
        public async Task When_Calling_ABCD_WithOperationTimeout_And_WithRequestTimeout_Then_Client_Should_Throw_A_TimeoutException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(options =>
                        options
                            .WithLogging()
                            .WithOperationTimeout(TimeSpan.FromSeconds(2))
                            .WithRequestTimeout(TimeSpan.FromSeconds(4)));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(6, opt),
                    options => options.WithOperationTimeout(TimeSpan.FromSeconds(6))
                        .WithRequestTimeout(TimeSpan.FromSeconds(8)));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();
        }

        [Fact]
        public async Task Calling_BCA_Both_WithTimeout_And_WithCancellation_Should_Throw_A_Request_TimeoutRejectedException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(options =>
                        options
                            .WithLogging()
                            .WithOperationTimeout(TimeSpan.FromSeconds(4)));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(6));

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(8, opt),
                    options => options.WithOperationTimeout(TimeSpan.FromSeconds(2))
                        .WithCancellation(cts.Token));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();
        }

        [Fact]
        public async Task Calling_ACB_Both_WithTimeout_And_WithCancellation_Should_Throw_A_Client_TimeoutRejectedException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(options =>
                        options
                            .WithLogging()
                            .WithOperationTimeout(TimeSpan.FromSeconds(2)));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(6));

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(8, opt),
                    options => options.WithOperationTimeout(TimeSpan.FromSeconds(4))
                        .WithCancellation(cts.Token));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();
        }

        [Fact]
        public async Task Calling_BAC_Both_WithTimeout_And_WithCancellation_Should_Throw_An_OperationCanceledException()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(options =>
                        options
                            .WithLogging()
                            .WithOperationTimeout(TimeSpan.FromSeconds(4)));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(8, opt),
                    options => options.WithOperationTimeout(TimeSpan.FromSeconds(6))
                        .WithCancellation(cts.Token));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<OperationCanceledException>();
        }

        [Fact]
        public async Task When_Calling_WithRequestTimeout_With_TimeoutRejected_Strategy_Then_It_Should_Retry_3_On_3_Times()
        {
            var watcher = new WatchingRequestHandler();

            var maxRetryCount = 3;
            var retryCount = 0;

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(
                        options => options
                            .WithLogging()
                            .WithResilienceContextOptions(opt =>
                                opt.ReturnToPoolOnComplete(false))
                            .WithRequestTimeout(TimeSpan.FromSeconds(3))
                            .AddDelegatingHandler(watcher));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddRetry(
                            new RetryStrategyOptions<HttpResponseMessage>
                            {
                                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                    .Handle<HttpRequestException>()
                                    .Handle<TimeoutRejectedException>()
                                    .HandleResult(response =>
                                        response.StatusCode is >= HttpStatusCode.InternalServerError
                                            or HttpStatusCode.RequestTimeout),
                                MaxRetryAttempts = maxRetryCount,
                                DelayGenerator = static args =>
                                {
                                    var delay = args.AttemptNumber switch
                                    {
                                        0 => TimeSpan.FromSeconds(1),
                                        1 => TimeSpan.FromSeconds(2),
                                        _ => TimeSpan.FromSeconds(3)
                                    };

                                    // This example uses a synchronous delay generator,
                                    // but the API also supports asynchronous implementations.
                                    return new ValueTask<TimeSpan?>(delay);
                                },
                                OnRetry = args =>
                                {
                                    retryCount = args.AttemptNumber + 1;
                                    return default;
                                }
                            }));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(6, opt));//,
            //options => options.WithTimeout(TimeSpan.FromSeconds(3)));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();

            // attempts should be equal to 2 as request timed out before the 3rd retry
            retryCount.Should().Be(3);
            watcher.Attempts.Should().Be(4);
        }

        [Fact]
        public async Task When_Calling_WithOperationTimeout_With_TimeoutRejected_Strategy_Then_It_Should_Retry_2_On_3_Times()
        {
            var watcher = new WatchingRequestHandler();

            var maxRetryCount = 3;
            var retryCount = 0;

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(
                        options => options
                            .WithLogging()
                            .WithResilienceContextOptions(opt =>
                                opt.ReturnToPoolOnComplete(false))
                            .WithOperationTimeout(TimeSpan.FromSeconds(10))
                            .AddDelegatingHandler(watcher));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddRetry(
                            new RetryStrategyOptions<HttpResponseMessage>
                            {
                                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                    .Handle<HttpRequestException>()
                                    .Handle<TimeoutRejectedException>()
                                    .HandleResult(response =>
                                        response.StatusCode is >= HttpStatusCode.InternalServerError
                                            or HttpStatusCode.RequestTimeout),
                                MaxRetryAttempts = maxRetryCount,
                                DelayGenerator = static args =>
                                {
                                    var delay = args.AttemptNumber switch
                                    {
                                        0 => TimeSpan.FromSeconds(1),
                                        1 => TimeSpan.FromSeconds(2),
                                        _ => TimeSpan.FromSeconds(3)
                                    };

                                    // This example uses a synchronous delay generator,
                                    // but the API also supports asynchronous implementations.
                                    return new ValueTask<TimeSpan?>(delay);
                                },
                                OnRetry = args =>
                                {
                                    retryCount = args.AttemptNumber + 1;
                                    return default;
                                }
                            }));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(6, opt),
                    options => options.WithRequestTimeout(TimeSpan.FromSeconds(3)));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();

            // attempts should be equal to 2 as request timed out before the 3rd retry
            retryCount.Should().Be(2);
            watcher.Attempts.Should().Be(3);
        }

        [Fact]
        public async Task When_Calling_WithRequestTimeout_WithOperationTimeout_WithCancellation_And_With_TimeoutRejected_Strategy_Then_It_Should_Retry_1_On_3_Times()
        {
            var watcher = new WatchingRequestHandler();

            var maxRetryCount = 3;
            var retryCount = 0;

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(
                        options => options
                            .WithLogging()
                            .WithResilienceContextOptions(opt =>
                                opt.ReturnToPoolOnComplete(false))
                            .WithOperationTimeout(TimeSpan.FromSeconds(10))
                            .AddDelegatingHandler(watcher));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddRetry(
                            new RetryStrategyOptions<HttpResponseMessage>
                            {
                                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                    .Handle<HttpRequestException>()
                                    .Handle<TimeoutRejectedException>()
                                    .HandleResult(response =>
                                        response.StatusCode is >= HttpStatusCode.InternalServerError
                                            or HttpStatusCode.RequestTimeout),
                                MaxRetryAttempts = maxRetryCount,
                                DelayGenerator = static args =>
                                {
                                    var delay = args.AttemptNumber switch
                                    {
                                        0 => TimeSpan.FromSeconds(1),
                                        1 => TimeSpan.FromSeconds(2),
                                        _ => TimeSpan.FromSeconds(3)
                                    };

                                    // This example uses a synchronous delay generator,
                                    // but the API also supports asynchronous implementations.
                                    return new ValueTask<TimeSpan?>(delay);
                                },
                                OnRetry = args =>
                                {
                                    retryCount = args.AttemptNumber + 1;
                                    return default;
                                }
                            }));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetDelayedUsersAsync(6, opt),
                    options => options.WithRequestTimeout(TimeSpan.FromSeconds(3))
                        .WithCancellation(cts.Token));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TaskCanceledException>();

            // attempts should be equal to 1 as request timed out before other retries
            retryCount.Should().Be(1);
            watcher.Attempts.Should().Be(2);
        }

        [Fact]
        public async Task Request_Returning_Timeout_Should_Time_Out_Before_Polly_Could_Complete_All_Retries()
        {
            var testHandler = new TestRequestHandler();

            var maxRetryCount = 3;
            var retryCount = 0;

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, builder) =>
                    builder.AddXUnit(_outputHelper)
                        .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices((_, services) =>
                {
                    services.AddApizrManagerFor<IReqResUserService>(
                        options => options
                            .WithLogging()
                            .WithResilienceContextOptions(opt =>
                                opt.ReturnToPoolOnComplete(false))
                            .AddDelegatingHandler(testHandler)
                            .WithOperationTimeout(TimeSpan.FromSeconds(3)));

                    services.AddResiliencePipeline<string, HttpResponseMessage>("TransientHttpError",
                        builder => builder.AddRetry(
                            new RetryStrategyOptions<HttpResponseMessage>
                            {
                                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                    .Handle<HttpRequestException>()
                                    .HandleResult(response =>
                                        response.StatusCode is >= HttpStatusCode.InternalServerError
                                            or HttpStatusCode.RequestTimeout),
                                MaxRetryAttempts = maxRetryCount,
                                DelayGenerator = static args =>
                                {
                                    var delay = args.AttemptNumber switch
                                    {
                                        0 => TimeSpan.FromSeconds(1),
                                        1 => TimeSpan.FromSeconds(2),
                                        _ => TimeSpan.FromSeconds(3)
                                    };

                                    // This example uses a synchronous delay generator,
                                    // but the API also supports asynchronous implementations.
                                    return new ValueTask<TimeSpan?>(delay);
                                },
                                OnRetry = args =>
                                {
                                    retryCount = args.AttemptNumber + 1;
                                    return default;
                                }
                            }));
                })
                .Build();

            var scope = host.Services.CreateScope();

            var reqResManager = scope.ServiceProvider.GetRequiredService<IApizrManager<IReqResUserService>>();

            Func<Task> act = () =>
                reqResManager.ExecuteAsync((opt, api) => api.GetUsersAsync(HttpStatusCode.RequestTimeout, opt));//,
                                                                                                                //options => options.WithTimeout(TimeSpan.FromSeconds(3)));

            var ex = await act.Should().ThrowAsync<ApizrException>();
            ex.WithInnerException<TimeoutRejectedException>();

            // attempts should be equal to 2 as request timed out before the 3rd retry
            retryCount.Should().Be(2);
            testHandler.Attempts.Should().Be(2);
        }
    }
}
