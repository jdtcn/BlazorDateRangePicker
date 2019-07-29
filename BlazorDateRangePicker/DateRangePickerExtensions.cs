/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDateRangePicker
{
    public static class DateRangePickerExtensions
    {
        /// <summary>
        /// Adds a singleton <see cref="DateRangePickerConfig"/> instance to the DI
        /// </summary>
        public static IServiceCollection AddDateRangePicker(this IServiceCollection services,
            DateRangePickerConfig configuration,
            string configName = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            configuration.Name = configName;
            services.AddSingleton(configuration);
            return services;
        }

        /// <summary>
        /// Adds a singleton <see cref="DateRangePickerConfig"/> instance to the DI
        /// </summary>
        public static IServiceCollection AddDateRangePicker(this IServiceCollection services,
            Action<DateRangePickerConfig> configure,
            string configName = null)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var options = new DateRangePickerConfig();
            configure(options);

            return AddDateRangePicker(services, options, configName);
        }
    }
}
