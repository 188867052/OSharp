﻿// -----------------------------------------------------------------------
//  <copyright file="ServiceExtensions.cs" company="OSharp开源团队">
//      Copyright (c) 2014-2020 OSharp. All rights reserved.
//  </copyright>
//  <site>http://www.osharp.org</site>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2020-03-25 1:53</last-date>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OSharp.Core.Builders;
using OSharp.Core.Options;
using OSharp.Core.Packs;
using OSharp.Data;
using OSharp.Dependency;
using OSharp.Entity;
using OSharp.EventBuses;
using OSharp.Reflection;


namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceExtensions
    {
        #region IServiceCollection

        /// <summary>
        /// 创建OSharp构建器，开始构建OSharp服务
        /// </summary>
        public static IOsharpBuilder AddOSharp(this IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            IConfiguration configuration = services.GetConfiguration();
            Singleton<IConfiguration>.Instance = configuration;

            //初始化所有程序集查找器
            services.TryAddSingleton<IAllAssemblyFinder>(new AppDomainAllAssemblyFinder());

            IOsharpBuilder builder = services.GetSingletonInstanceOrNull<IOsharpBuilder>() ?? new OsharpBuilder(services);
            services.TryAddSingleton<IOsharpBuilder>(builder);

            builder.AddCorePack();

            return builder;
        }

        /// <summary>
        /// 获取<see cref="IConfiguration"/>配置信息
        /// </summary>
        public static IConfiguration GetConfiguration(this IServiceCollection services)
        {
            return services.GetSingletonInstanceOrNull<IConfiguration>();
        }
        /// <summary>
        /// 替换服务
        /// </summary>
        public static IServiceCollection Replace<TService, TImplement>(this IServiceCollection services, ServiceLifetime lifetime)
        {
            ServiceDescriptor descriptor = new ServiceDescriptor(typeof(TService), typeof(TImplement), lifetime);
            services.Replace(descriptor);
            return services;
        }

        /// <summary>
        /// 如果指定服务不存在，添加指定服务
        /// </summary>
        public static ServiceDescriptor GetOrAdd(this IServiceCollection services, ServiceDescriptor toAdDescriptor)
        {
            ServiceDescriptor descriptor = services.FirstOrDefault(m => m.ServiceType == toAdDescriptor.ServiceType);
            if (descriptor != null)
            {
                return descriptor;
            }

            services.Add(toAdDescriptor);
            return toAdDescriptor;
        }

        /// <summary>
        /// 获取或添加指定类型查找器
        /// </summary>
        public static TTypeFinder GetOrAddTypeFinder<TTypeFinder>(this IServiceCollection services, Func<IAllAssemblyFinder, TTypeFinder> factory)
            where TTypeFinder : class
        {
            return services.GetOrAddSingletonInstance<TTypeFinder>(() =>
            {
                IAllAssemblyFinder allAssemblyFinder =
                    services.GetOrAddSingletonInstance<IAllAssemblyFinder>(() => new AppDomainAllAssemblyFinder(true));
                return factory(allAssemblyFinder);
            });
        }

        /// <summary>
        /// 如果指定服务不存在，创建实例并添加
        /// </summary>
        public static TServiceType GetOrAddSingletonInstance<TServiceType>(this IServiceCollection services, Func<TServiceType> factory) where TServiceType : class
        {
            TServiceType item = GetSingletonInstanceOrNull<TServiceType>(services);
            if (item == null)
            {
                item = factory();
                services.AddSingleton<TServiceType>(item);
            }
            return item;
        }

        /// <summary>
        /// 获取单例注册服务对象
        /// </summary>
        public static T GetSingletonInstanceOrNull<T>(this IServiceCollection services)
        {
            ServiceDescriptor descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(T) && d.Lifetime == ServiceLifetime.Singleton);

            if (descriptor?.ImplementationInstance != null)
            {
                return (T)descriptor.ImplementationInstance;
            }

            if (descriptor?.ImplementationFactory != null)
            {
                return (T)descriptor.ImplementationFactory.Invoke(null);
            }

            return default;
        }

        /// <summary>
        /// 获取单例注册服务对象
        /// </summary>
        public static T GetSingletonInstance<T>(this IServiceCollection services)
        {
            var instance = services.GetSingletonInstanceOrNull<T>();
            if (instance == null)
            {
                throw new InvalidOperationException($"无法找到已注册的单例服务：{typeof(T).AssemblyQualifiedName}");
            }

            return instance;
        }

        /// <summary>
        /// 加载事件处理器
        /// </summary>
        public static IServiceCollection AddEventHandler<T>(this IServiceCollection services) where T : class, IEventHandler
        {
            return services.AddTransient<T>();
        }

        /// <summary>
        /// 从Scoped字典中获取指定类型的值
        /// </summary>
        public static T GetValue<T>(this ScopedDictionary dict, string key) where T : class
        {
            if (dict.TryGetValue(key, out object obj))
            {
                return obj as T;
            }

            return default;
        }

        #endregion

        #region IServiceProvider

        /// <summary>
        /// 从服务提供者中获取OSharpOptions
        /// </summary>
        public static OsharpOptions GetOSharpOptions(this IServiceProvider provider)
        {
            return provider.GetService<IOptions<OsharpOptions>>()?.Value;
        }

        /// <summary>
        /// 获取指定类型的日志对象
        /// </summary>
        /// <typeparam name="T">非静态强类型</typeparam>
        /// <returns>日志对象</returns>
        public static ILogger<T> GetLogger<T>(this IServiceProvider provider)
        {
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            return factory.CreateLogger<T>();
        }

        /// <summary>
        /// 获取指定类型的日志对象
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="type">指定类型</param>
        /// <returns>日志对象</returns>
        public static ILogger GetLogger(this IServiceProvider provider, Type type)
        {
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            return factory.CreateLogger(type);
        }

        /// <summary>
        /// 获取指定名称的日志对象
        /// </summary>
        public static ILogger GetLogger(this IServiceProvider provider, string name)
        {
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            return factory.CreateLogger(name);
        }

        /// <summary>
        /// 获取指定实体类的上下文所在工作单元
        /// </summary>
        public static IUnitOfWork GetUnitOfWork<TEntity, TKey>(this IServiceProvider provider) where TEntity : IEntity<TKey>
        {
            IUnitOfWorkManager unitOfWorkManager = provider.GetService<IUnitOfWorkManager>();
            return unitOfWorkManager.GetUnitOfWork<TEntity, TKey>();
        }

        /// <summary>
        /// 获取指定实体类型的上下文对象
        /// </summary>
        public static IDbContext GetDbContext<TEntity, TKey>(this IServiceProvider provider) where TEntity : IEntity<TKey>
        {
            IUnitOfWorkManager unitOfWorkManager = provider.GetService<IUnitOfWorkManager>();
            return unitOfWorkManager.GetDbContext<TEntity, TKey>();
        }

        /// <summary>
        /// 获取所有模块信息
        /// </summary>
        public static OsharpPack[] GetAllPacks(this IServiceProvider provider)
        {
            return provider.GetServices<OsharpPack>().OrderBy(m => m.Level).ThenBy(m => m.Order).ThenBy(m => m.GetType().FullName).ToArray();
        }

        /// <summary>
        /// OSharp框架初始化，适用于非AspNetCore环境
        /// </summary>
        public static IServiceProvider UseOsharp(this IServiceProvider provider)
        {
            ILogger logger = provider.GetLogger(typeof(ServiceExtensions));
            logger.LogInformation("OSharp框架初始化开始");
            Stopwatch watch = Stopwatch.StartNew();

            OsharpPack[] packs = provider.GetServices<OsharpPack>().ToArray();
            foreach (OsharpPack pack in packs)
            {
                pack.UsePack(provider);
                logger.LogInformation($"模块{pack.GetType()}加载成功");
            }

            watch.Stop();
            logger.LogInformation($"OSharp框架初始化完毕，耗时：{watch.Elapsed}");

            return provider;
        }
        /// <summary>
        /// 执行<see cref="ServiceLifetime.Scoped"/>生命周期的业务逻辑
        /// 1.当前处理<see cref="ServiceLifetime.Scoped"/>生命周期外，使用CreateScope创建<see cref="ServiceLifetime.Scoped"/>
        /// 生命周期的ServiceProvider来执行，并释放资源
        /// 2.当前处于<see cref="ServiceLifetime.Scoped"/>生命周期内，直接使用<see cref="ServiceLifetime.Scoped"/>的ServiceProvider来执行
        /// </summary>
        public static void ExecuteScopedWork(this IServiceProvider provider, Action<IServiceProvider> action, bool useHttpScope = true)
        {
            using var scope = useHttpScope
                ? provider.GetService<IHybridServiceScopeFactory>().CreateScope()
                : provider.CreateScope();
            action(scope.ServiceProvider);
        }

        /// <summary>
        /// 异步执行<see cref="ServiceLifetime.Scoped"/>生命周期的业务逻辑
        /// 1.当前处理<see cref="ServiceLifetime.Scoped"/>生命周期外，使用CreateScope创建<see cref="ServiceLifetime.Scoped"/>
        /// 生命周期的ServiceProvider来执行，并释放资源
        /// 2.当前处于<see cref="ServiceLifetime.Scoped"/>生命周期内，直接使用<see cref="ServiceLifetime.Scoped"/>的ServiceProvider来执行
        /// </summary>
        public static async Task ExecuteScopedWorkAsync(this IServiceProvider provider, Func<IServiceProvider, Task> action, bool useHttpScope = true)
        {
            using var scope = useHttpScope
                ? provider.GetService<IHybridServiceScopeFactory>().CreateScope()
                : provider.CreateScope();
            await action(scope.ServiceProvider);
        }

        /// <summary>
        /// 执行<see cref="ServiceLifetime.Scoped"/>生命周期的业务逻辑，并获取返回值
        /// 1.当前处理<see cref="ServiceLifetime.Scoped"/>生命周期外，使用CreateScope创建<see cref="ServiceLifetime.Scoped"/>
        /// 生命周期的ServiceProvider来执行，并释放资源
        /// 2.当前处于<see cref="ServiceLifetime.Scoped"/>生命周期内，直接使用<see cref="ServiceLifetime.Scoped"/>的ServiceProvider来执行
        /// </summary>
        public static TResult ExecuteScopedWork<TResult>(this IServiceProvider provider, Func<IServiceProvider, TResult> func, bool useHttpScope = true)
        {
            using var scope = useHttpScope
                ? provider.GetService<IHybridServiceScopeFactory>().CreateScope()
                : provider.CreateScope();
            return func(scope.ServiceProvider);
        }

        /// <summary>
        /// 执行<see cref="ServiceLifetime.Scoped"/>生命周期的业务逻辑，并获取返回值
        /// 1.当前处理<see cref="ServiceLifetime.Scoped"/>生命周期外，使用CreateScope创建<see cref="ServiceLifetime.Scoped"/>
        /// 生命周期的ServiceProvider来执行，并释放资源
        /// 2.当前处于<see cref="ServiceLifetime.Scoped"/>生命周期内，直接使用<see cref="ServiceLifetime.Scoped"/>的ServiceProvider来执行
        /// </summary>
        public static async Task<TResult> ExecuteScopedWorkAsync<TResult>(this IServiceProvider provider, Func<IServiceProvider, Task<TResult>> func, bool useHttpScope = true)
        {
            using var scope = useHttpScope
                ? provider.GetService<IHybridServiceScopeFactory>().CreateScope()
                : provider.CreateScope();
            return await func(scope.ServiceProvider);
        }

        /// <summary>
        /// 获取当前用户
        /// </summary>
        public static ClaimsPrincipal GetCurrentUser(this IServiceProvider provider)
        {
            try
            {
                IPrincipal user = provider.GetService<IPrincipal>();
                return user as ClaimsPrincipal;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 开启一个事务处理
        /// </summary>
        /// <param name="provider">信赖注入服务提供程序</param>
        /// <param name="action">要执行的业务委托</param>
        /// <param name="createScope">是否创建一个新的<see cref="IServiceScope"/>，如false，则使用传入的 provider</param>
        public static void BeginUnitOfWorkTransaction(this IServiceProvider provider, Action<IServiceProvider> action, bool createScope = false)
        {
            Check.NotNull(provider, nameof(provider));
            Check.NotNull(action, nameof(action));
            if (!createScope)
            {
                IServiceProvider scopeProvider = provider;
                IUnitOfWorkManager unitOfWorkManager = scopeProvider.GetService<IUnitOfWorkManager>();
                action(scopeProvider);
                unitOfWorkManager.Commit();
            }
            else
            {
                using IServiceScope scope = provider.CreateScope();
                IServiceProvider scopeProvider = scope.ServiceProvider;
                IUnitOfWorkManager unitOfWorkManager = scopeProvider.GetService<IUnitOfWorkManager>();
                action(scopeProvider);
                unitOfWorkManager.Commit();
            }
        }

        /// <summary>
        /// 开启一个事务处理
        /// </summary>
        /// <param name="provider">信赖注入服务提供程序</param>
        /// <param name="actionAsync">要执行的业务委托</param>
        /// <param name="createScope">是否创建一个新的<see cref="IServiceScope"/>，如false，则使用传入的 provider</param>
        public static async Task BeginUnitOfWorkTransactionAsync(this IServiceProvider provider,
            Func<IServiceProvider, Task> actionAsync,
            bool createScope = false)
        {
            Check.NotNull(provider, nameof(provider));
            Check.NotNull(actionAsync, nameof(actionAsync));
            IServiceProvider scopeProvider = provider;
            if (createScope)
            {
                using IServiceScope scope = provider.CreateScope();
                scopeProvider = scope.ServiceProvider;
            }

            IUnitOfWorkManager unitOfWorkManager = scopeProvider.GetService<IUnitOfWorkManager>();
            await actionAsync(scopeProvider);
            unitOfWorkManager.Commit();
        }
        #endregion

    }
}