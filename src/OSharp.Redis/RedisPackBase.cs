﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSharp.Core.Packs;
using OSharp.Exceptions;
using OSharp.Extensions;

namespace OSharp.Redis
{
    /// <summary>
    /// Redis模块基类
    /// </summary>
    public abstract class RedisPackBase : OsharpPack
    {
        /// <summary>
        /// 获取 模块级别，级别越小越先启动
        /// </summary>
        public override PackLevel Level => PackLevel.Framework;

        /// <summary>
        /// 将模块服务添加到依赖注入服务容器中
        /// </summary>
        /// <param name="services">依赖注入服务容器</param>
        /// <returns></returns>
        public override IServiceCollection AddServices(IServiceCollection services)
        {
            var configuration = services.GetConfiguration();
            string config = configuration["OSharp:Redis:Configuration"];
            if (config.IsNullOrEmpty())
            {
                throw new OsharpException("配置文件中Redis节点的Configuration不能为空");
            }
            string name = configuration["OSharp:Redis:InstanceName"].CastTo("RedisName");

            services.RemoveAll(typeof(IDistributedCache));
            services.AddStackExchangeRedisCache(opts =>
            {
                opts.Configuration = config;
                opts.InstanceName = configuration["OSharp:Redis:InstanceName"].CastTo("RedisName");
            });

            return services;
        }
    }
}