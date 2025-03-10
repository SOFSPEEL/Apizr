6.0.0
---

### All

- [New][Target] Now **targeting multiple frameworks based on Refit targets**: `netstandard2.0;netstandard2.1;net462;net6.0;net7.0;net8.0`
- [New][Exceptions] Now we can **handle `IApizrResponse` safe response (based on Refit's `IApiResponse`) instead of catching exceptions**
- [New][Attributes] Now we can **provide types to attributes thanks to generic arguments** instead of using typeof() as constructor parameters
- [New/Breaking][Polly] Now **supporting only the brand new Polly v8+ Resilience Strategies/Pipelines/Registry** instead of former Polly v7- Policies
- [New][HttpTracer] Now we can **filter out unwanted http message parts** with the brand new IgnoreMessageParts fluent option
- [New][HttpTracer] Now we can **pick RequestAllButBody, HeadersOnly or AllButBody http message parts** when configuring logging options fluently or using attribute
- [New][HttpTracer/Headers] Now we can **redact any header sensitive values from logs** thanks to `WithLoggedHeadersRedactionNames` or `WithLoggedHeadersRedactionRule` fluent options
- [New][Headers] Now we can **choose to refresh a header value at request time or not**, depending on the brand new WithHeaders's ApizrLifetimeScope optional parameter
- [New][Headers] Now we can **choose to set headers values right the way to the request or store it for further key-only header attribute match use**, depending on the brand new WithHeaders's ApizrRegistrationMode optional parameter
- [New][Headers] Now we can **set headers values thanks to an expression tree**
- [Breaking][Headers] Now **WithHeaders options take an enumerable parameter instead of a parameter array** so that we could provide some more optional parameters
- [Breaking][Removed] **Previsoulsy deprecated extensions methods for backward compatibility have been removed** as they were not suitable anymore

### Apizr.Integrations.MediatR

- [New][Exceptions] Now we can **send safe request returning an `IApizrResponse` response to handle (based on Refit's `IApiResponse`) instead of catching exceptions**

### Apizr.Tools.NSwag

- [Update][Polly] Templates have been updated to **use the new Polly v8+ Resilience Strategies/Pipelines/Registry** instead of former Polly v7- Policies

5.4.0
---

### All

- [Unified][Cancellation] Now **cancelling a request on Android trows an OperationCanceledException** instead of a WebException/SocketClosedException
- [Unified][Cancellation] Now **cancelling a request on iOS trows an OperationCanceledException** instead of a TimeoutException/TaskCanceledException
- [New][Timeout] Now we can **set a request timeout (each request try) and/or an operation timeout (overall request tries)** thanks to both dedicated attributes or fluent options
- [Unified][Timeout] Now a **request that times out on client side throws a TimeoutRejectedException** provided by Polly instead of a TimeoutException
- [Fix][Headers] Now the **headers feature fully support composite configuration**
- [Fix][Context] Now the **context feature fully support composite configuration**

5.3.0
---

### All

- [New][Timeout] Now we can **set a request timeout thanks to both dedicated attributes or fluent option**
- [New][Headers] Now we can **set global headers without passing any options request parameter** (HttpClient's DefaultRequestHeaders)
- [New][Auth] Now we can access to a **dedicated logger instance while inheriting from AuthenticationHadlerBase**
- [Fix][Cancellation] Now we really can **cancel a request by providing a token trough the options**
- [Fix][Log] **CRUD's log attributes now target Class only**

### Apizr

- [Breaking][HttpClient] Now we can **configure the HttpClient instead of providing one** (same as extended experience) with the brand new ConfigureHttpClient fluent option
 
### Apizr.Extensions.Microsoft.DependencyInjection

- [Fix][HttpClient] Now **HttpClientBuilder really apply composite configurations**

### Apizr.Integrations.FileTransfer.MediatR

- [Fix] Now **default upload command returns HttpResponseMessage**

### Apizr.Integrations.FileTransfer.Optional

- [Fix] Now **default upload optional command returns Option<HttpResponseMessage, ApizrException>**
 
5.2.0
---

### Apizr.Integrations.FileTransfer

- [New] Now **Upload api uses the "file" alias**
- [New] Now we can **create transfer managers with shortcuts**

### Apizr.Extensions.Microsoft.FileTransfer

- [New] Now we can **get transfer managers with shortcuts**

### Apizr.Integrations.FileTransfer.MediatR

- [New] Now we can **send requests with shortcuts**

### Apizr.Integrations.FileTransfer.Optional

- [New] Now we can **send optional requests with shortcuts**

5.1.0
---

### All

- [Bump] **NuGet reference packages updated**

### Apizr

- [New][Polly] Now providing a **PolicyRegistry is not mandatory anymore** so that Polly could be turned off by default

5.0.1
---

### Apizr

- [Fix] **No more NullRefEx** when getting cookies from CookieContainer
- [Fix] Typo fix

5.0
---

### All

- [New] Now we can **group registry common configurations** at any level to share configurations without any limit
- [New] Now we can **set a base path** with the brand new WithBasePath fluent option or using attribute
- [New] Now we can **set options at request time** with the brand new fluent request options builder
- [New] Now we can **execute api requests directly from the registry** containing the managed api
- [New] Now we can **set headers** with the brand new WithHeaders fluent option
- [New] Now we can **set a default readonly authentication token** with a dedicated WithAuthenticationHandler fluent option

### Apizr

- [New] [Core] Now we can **set a custom HttpClient** with the brand new WithHttpClient fluent option
- [Improvement] [Core] **No more dependency to Microsoft.Extensions.Http.Polly** from the core package
- [Breaking] [Naming] Now **ApizrBuilder static class offers only a Current property returning its own instance to get acces to its methods**, so that it could be extended then by other packages
- [Fix] Some bugs and performance issues fixed

### Apizr.Integrations.Mapster

- [New] Brand new integration with Mapster

### Apizr.Integrations.FileTransfer

- [New] Brand new integration package to **manage file transfers** like a breeze with static registration/instanciation
- [New] We can **track file transfer progress** with the brand new WithProgress fluent option

### Apizr.Extensions.Microsoft.FileTransfer

- [New] Brand new integration package to **manage file transfers** like a breeze with extended registration

### Apizr.Integrations.FileTransfer.MediatR

- [New] Brand new integration package to **manage file transfers** like a breeze with mediator pattern

### Apizr.Integrations.FileTransfer.Optional

- [New] Brand new integration package to **manage file transfers** like a breeze with mediator pattern and optional result

### Apizr.Tools.NSwag

- [New] Brand new CLI tool so that **Apizr files can now be generated by command line** (Models, Apis and Registrations) from an OpenApi/Swagger definition using NSwag

4.1
---

### Apizr

- [Breaking] [Naming] **Apizr static class renamed to ApizrBuilder to match its purpose** and doesn't conflict with its namespace anymore
- [Breaking] [Naming] **ApizrBuilder's methods renamed to match their return type** so that we know what we're about to build (e.g. CreateRegistry, AddManagerFor, CreateManagerFor)
- [Breaking] [Naming] **ApizrRegistry's methods renamed to match their return type** so that we know what we're about to get (e.g. GetManagerFor, GetCrudManagerFor, ContainsManagerFor)
- [Fix] [Connectivity] **No more exception** while using Apizr the extended way but **without providing any IConnectivityHandler implementation** with Fusillade priority management enabled
- [Improvement] [Address] Now we can **set base address at both common and proper levels** so we can define a base address shared by all apis, but also a specific one if needed

### Apizr.Extensions.Microsoft.DependencyInjection

- [Breaking] [Naming] **Extension methods renamed to match their return type** so that we know what we're about to build (e.g. AddManagerFor, AddCrudManagerFor)
- [Breaking] [Naming] **ApizrExtendedRegistry's methods renamed to match their return type** so that we know what we're about to get (e.g. GetManagerFor, GetCrudManagerFor, ContainsManagerFor)

### Apizr.Integrations.MediatR

- [Breaking] [Naming] **ApizrMediationRegistry's methods renamed to match their return type** so that we know what we're about to get (e.g. GetMediatorFor, GetCrudMediatorFor, ContainsMediatorFor)

### Apizr.Integrations.Optional

- [Breaking] [Naming] **ApizrOptionalMediationRegistry's methods renamed to match their return type** so that we know what we're about to get (e.g. GetOptionalMediatorFor, GetCrudOptionalMediatorFor, ContainsOptionalMediatorFor)


4.0
---

### Apizr

- [New] [Logging] Now fully relies on **MS Logging extensions**
- [New] [Logging] Static fluent configuration now offers a **WithLoggerFactory option** to provide a custom logger factory
- [Breaking] [Logging] TraceAttribute has been **renamed back to LogAttribute**
- [Breaking] [Logging] Now we can set a **LogLevel value for each Low, Medium and High severity** by attribute or fluent configuration
- [New] [Logging] Now we can set logging settings within **LogAttribute at method level**
- [New] [Logging] Now we can set **http tracing mode within LogAttribute** to ajust log writting conditions (ExceptionsOnly, ErrorsAndExceptionsOnly or Everything)
- [New] [Logging] **No more HttpTracer NuGet package dependency** as source code has been integrated and largely adjusted to Apizr needs
- [New] [Configuring] Both static and extended fluent configuration now offers a **Registry to set common options once for all** registered apis, while keeping proper options applied to selected apis
- [New] [Configuring] Static fluent configuration could return the **registry that expose a Populate method** to register each generated Apizr manager in a container
- [New] [Configuring] Generated **registry exposes GetFor, TryGetFor, GetCrudFor and TryGetCrudFor methods** so that it could be used everywhere to get managers, instead of direct access
- [New] [Policing] Now we can **provide a custom PollyContext** if defined into the called api interface method, it will carry all logging settings for DelegatingHandler use.
- [New] [Mapping] Now we can enjoy **data mapping with both static and extended configurations**
- [Improvement] [Mapping] Now we can let Apizr **auto map data** right before sending request and/or after recieving response **by providing types on ExecuteAsync call**
- [New] [Caching] Now we can ask for **clearing request cache before executing**
- [New] [Exceptions] Brand new **onException Action parameter to handle it globally** (e.g. user dialog/toast) and let potential cached data return to caller as expected (e.g. refreshing UI)

### Apizr.Extensions.Microsoft.DependencyInjection

- [New] We can now **auto register crud managers for all scanned classes** decorated by crud attributes


### Apizr.Extensions.Microsoft.Caching

- [New] Brand new integration project to set **MS Caching extensions** as caching handler

### Apizr.Integrations.Akavache

- [Improvement] Now we can **ajust Akavache settings** while configuring
- [New] Now we can register Akavache directly with the brand new **WithAkavacheCacheHandler option**

### Apizr.Integrations.MediatR

- [Breaking] Now **Apizr.Integrations.MediatR targets .Net Standard 2.1** as MediatR v10+ does
- [New] Now we can let Apizr **auto map data** right before sending request and/or after recieving response **by providing types on Apizr mediators Send call**
- [New] Brand **new IApizrMediator & IApizrCrudMediator interfaces** to get things shorter than IMediator
- [New] Now we can ask for **clearing request cache before executing**
- [New] Brand new **onException Action parameter to handle it globally** (e.g. user dialog/toast) and let potential cached data return to caller as expected (e.g. refreshing UI)

### Apizr.Integrations.Optional

- [Breaking] Now **Apizr.Integrations.Optional targets .Net Standard 2.1** as Apizr.Integrations.MediatR v4+ does
- [New] Now we can let Apizr **auto map data** right before sending request and/or after recieving response **by providing types on Apizr optional mediators Send call**
- [New] Brand **new IApizrOptionalMediator & IApizrCrudOptionalMediator interfaces** to get things shorter than IMediator 
- [New] Now we can ask for **clearing request cache before executing**

### Apizr.Integrations.AutoMapper

- [Breaking] Now **Apizr.Integrations.AutoMapper targets .Net Standard 2.1** as AutoMapper v11+ does
- [New] Now we can register AutoMapper directly with the brand new **WithAutoMapperMappingHandler option**
- [Improvement] **No more extended package dependency** to enjoy data mapping with both static and extended configurations

>[!WARNING]
>
>**Apizr.Integrations.Shiny has been discontinued**
>
>This integration project has been dropped out as Shiny no longer provide built-in caching and logging feature anymore. Apizr now either relies on MS Caching extensions, Akavache or MonkeyCache for caching feature and MS Logging extensions for logging feature. You'll have to provide a connectivity handler if you want Apizr to check it.

3.0
---

### Apizr

- [New] Now **based on Refit v6+ which introduce properties parameter** provided to DelegateHandlers by the Http message. This is used by the new Fusillade priority management package.
- [New] Now **based on Refit v6+ which introduce dynamic headers dictionary parameter** allowing adding multiple dynamic headers in a single parameter thanks to [HeaderCollection] IDictionary<string, string> headers
- [New] Now **based on Refit v6+ which now relies on System.Text.Json** instead of Newtonsoft.Json. If you'd like to continue to use Newtonsoft.Json, add the Refit.Newtonsoft.Json NuGet package and follow the new Readme instructions
- [New] Now **AuthenticationHandler is log level sensitive** so we could tell it to keep quiet while using it heavily
- [New] Now **caching could be disabled with CacheIt attribute** thanks to CacheMode.None parameter to compose with assembly, interface and method level cache rules all together (e.g. you could now enable Cache globaly at assembly level but turn it off for a specific method like the login one)
- [New] Now you can **initialize parameters directly from the managed method**. No more pitfalls
- [Improvement] **Cache key generator has been deeply reshaped** to support all scenarios, including path parameters
- [Improvement] **Nuget references have been updated** to latest versions
- [BreakingChange] **Fusillade has been moved to an integration package**. If you used to play with it, just install it from its brand new dedicated integration package and follow the new Readme instructions
- [Fix] **No more exception while using Apizr with Prism.Magician** with Fusillade priority management enabled

### Apizr.Integrations.Fusillade

- [New] Brand new integration package to use Apizr with Fusillade, only if you need it (no more core references and NuGet dependencies)

2.0
---

### Apizr

- [New] Now **initialization options are typed to be dedicated** to each api interface manager. It means you can now get a specific configuration for each Apizr manager instance, like for caching, logging, and so on...
- [New] Now **caching could be defined at method level** for CRUD api to. It means you can define specific cache settings for each Read and ReadAll request for each your CRUD model class
- [New] Now **caching could be defined at class level** for CRUD api like you does for classic interface one. It means you can define specific cache settings for both Read and ReadAll requests for each your CRUD model class
- [New] Now **caching could be defined at assembly level** for global cache settings. It means you can define global cache settings for all your apis in one place, and then define specific settings at sub-levels to override this behavior when needed
- [New] Now **policy keys could be defined at method level** for CRUD api to. It means you can define specific policy keys for each request of each CRUD model class
- [New] Now **policy keys could be defined at class level** for CRUD api like you does for classic interface one. It means you can define specific policy keys for all requests of each CRUD model class
- [New] Now **logging could be defined at class level** for CRUD api like you does for classic interface one. It means you can define specific logging settings for all requests of each CRUD model class
- [New] Now **logging could be defined at assembly level** for global logging settings. It means you can define global logging settings for all your apis in one place, and then define specific settings at sub-levels to override this behavior when needed
- [BreakingChange] **TraceAttribute renamed to LogItAttribute** to suits its tracing and logging both features activation
- [BreakingChange] **CacheAttribute renamed to CacheItAttribute** to keep things consistent
- [BreakingChange] **No more cache and policy attribute decorating CRUD api** by default. You can activate it fluently with the options builder.

1.9
---

### Apizr

- [New] Handling complex type as CacheKey
- [New] Now we can set Apizr log level within TraceAttribute to manage execution tracing verbosity

### Apizr.Integrations.MediatR

- [Change] Mediation's ICommand interface renamed to IMediationCommand, avoiding conflict with System.Windows.Input.ICommand

1.8.1
---

### Apizr

- [Fix] Parsing life span representation as TimeSpan from CacheAttribute

1.8
---

### Apizr.Integrations.Optional

- [New] Introducing CatchAsync optional extension method to return result from fetch or cache, no matter of execption handled on the other side by an action callback to inform the user

1.7
---

### Apizr

- [New] Now we can toggle Fusillade priority management activation
- [New] Now we can provide a base uri factory (e.g. depending on config)

### Apizr.Extensions.Microsoft.DependencyInjection

- [New] Now we can toggle Fusillade priority management activation
- [New] Now we can provide a base uri factory (e.g. depending on DI resovled settings)

1.6
---

- [Fix] Preserve attribute added

### Apizr

- [New] Now we can provide a custom HttpClientHandler instance

### Apizr.Extensions.Microsoft.DependencyInjection

- [New] Now we can provide a custom HttpClientHandler instance

1.5
---

### Apizr

- [Fix] Now the manager waits for task with no result to handle exceptions properly

### Apizr.Integrations.MediatR

- [New] Introducing typed mediator and typed crud mediator for shorter request
- [Fix] Now MediatR handlers are registered correctly when asked from a manual registration context
- [Fix] Mapping null object now works correctly
- [Fix] Now MediatR handlers wait for its handling task to handle exceptions properly

### Apizr.Integrations.Optional

- [New] Introducing typed optional mediator and typed crud optional mediator for shorter request
- [New] Introducing OnResultAsync optional extension method to make all the thing shorter than ever
- [Fix] Now Optional handlers are registered correctly when asked from a manual registration context
- [Fix] Optional request handlers now handle exceptions as expected
- [Fix] Now Optional handlers wait for its handling task to handle exceptions properly

1.4.2
---

### Apizr.Integrations.MediatR

- [Fix] Now nuget package as library both reference MediatR.Extensions.Microsoft.DependencyInjection nuget package for assembly version compatibility

1.4.1
---

### Apizr.Extensions.Microsoft.DependencyInjection

- [Fix] Now Apizr works with DryIoc and Unity containers, returning a single UserInitiated instance, while waiting for external issues beeing fixed

### Apizr.Integrations.MediatR

- [Workaround] Doc updated to work with MediatR alongside DryIoc or Unity container, while waiting for external issues beeing fixed
- [Fix] No more ```WithCrudMediation``` method available but only ```WithMediation```

1.4.0
---

### Apizr.Extensions.Microsoft.DependencyInjection

- [New] We can now auto register both crud and classic api interfaces

### Apizr.Integrations.MediatR

- [New] We can now use mediation with both crud and classic api interfaces
- [New] We can now use execution priority with both crud and classic api mediation
- [BreakingChange] ```WithCrudMediation``` renamed to ```WithMediation```

### Apizr.Integrations.Optional

- [New] We can now use optional mediation with both crud and classic api interfaces
- [New] We can now use execution priority with both crud and classic api optional mediation
- [BreakingChange] ```WithCrudOptionalMediation``` renamed to ```WithOptionalMediation```

### Apizr.Integrations.Shiny

- [New] Shiny integration now offers all the same registration extensions methods

### Apizr.Integrations.AutoMapper

- [New] We can now use auto mapping with both crud and classic api mediation and optional mediation 

1.3.0
---

### Apizr

- [New] We can now define mapped model entity type from the ```CrudEntityAttribute``` above api entities for automatic crud registration

### Apizr.Extensions.Microsoft.DependencyInjection

- [New] We can now provide an IMappingHandler implementation to the options builder for auto mapping
- [New] We can now decorate model entities with ```MappedCrudEntityAttribute``` to define mapped crud settings for automatic crud registration
- [New] We can now associate api and model entities with ```MappedEntity<TModelEntity, TApiEntity>``` during manual crud registration

### Apizr.Integrations.MediatR

- [Fix] Cacheable ReadQuery now use the key value when defining cache key
- [Fix] Auto handling now works as expected with manual crud registration

### Apizr.Integrations.Optional

- [Fix] Cacheable ReadOptionalQuery now use the key value when defining cache key

### Apizr.Integrations.AutoMapper

- [New] Brand new integration with AutoMapper, to let Apizr handle crud entity mapping during mediation handling

1.2.0
---

### Apizr

- [BreakingChange] Apizr instantiation/registration methods names standardized to Apizr.For and Apizr.CrudFor
- [New] Introducing ICrudApi service to manage standard CRUD api calls built-in

### Apizr.Extensions.Microsoft.DependencyInjection

- [BreakingChange] Apizr instantiation/registration methods names standardized to services.AddApizrFor and services.AddApizrCrudFor
- [New] Enabling ICrudApi auto registration feature with CrudEntityAttribute and assembly scanning

### Apizr.Integrations.Shiny

- [BreakingChange] Apizr instantiation/registration methods names standardized to services.UseApizrFor and services.UseApizrCrudFor
- [New] Enabling ICrudApi auto registration feature with CrudEntityAttribute and assembly scanning

### Apizr.Integrations.MediatR

- [New] Brand new integration with MediatR, to let Apizr handle crud requests execution with mediation

### Apizr.Integrations.Optional

- [New] Brand new integration with Optional, to let Apizr handle crud requests execution with mediation and optional result

1.1.0
---

### Apizr

- [New] Aibility to manage generic web apis by setting base address with the options builder

### Apizr.Extensions.Microsoft.DependencyInjection

- [New] Same as Apizr

### Apizr.Integrations.Shiny

- [New] Same as Apizr

1.0.0
---
Initial Release for
- Apizr
- Apizr.Extensions.Microsoft.DependencyInjection
- Apizr.Integrations.Akavache
- Apizr.Integrations.MonkeyCache
- Apizr.Integrations.Shiny