using AspNetCore.Specification.OrderByMapping;
using AspNetCore.Specification.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace AspNetCore.Specification
{
    public static class ConfigurationExtensions
    {
        #region Model Binder and Input Formatters
        /// <summary>
        /// Adds the User Specification model binders for UserIncludeSpecification<>, UserFilterSpecification<>, UserOrderBySpecification<>, UserFieldsSpecification<> to the application.
        /// </summary>
        public static IMvcBuilder AddMvcUserSpecificationModelBinders(this IMvcBuilder builder)
        {
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, UserSpecificationsMvcOptionsSetup>();
            return builder;
        }
        #endregion

        #region Order By Mapper
        public static IServiceCollection ConfigureOrderByMapper(this IServiceCollection services, Action<OrderByMapperOptions> configure)
        {
            return services.Configure(configure);
        }

        public static IServiceCollection AddOrderByMapper(this IServiceCollection services)
        {
            return services.AddSingleton<IOrderByMapper, OrderByMapper>();
        }

        public static IServiceCollection AddOrderByMapper(this IServiceCollection services, Action<OrderByMapperOptions> configure)
        {
            services.ConfigureOrderByMapper(configure);
            return services.AddOrderByMapper();
        }
        #endregion
    }
}
