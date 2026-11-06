using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookStore.Common.Configuration
{
    public static class SharedConfigurationExtensions
    {
        private const string SettingsFolderName = "Settings";
        private const string SharedConfigFileName = "appsettings.json";
        public static string GetSharedSettingsDirectory(string contentRootPath)
        {
            var parentDir = Directory.GetParent(contentRootPath)?.Parent;

            if (parentDir != null)
            {
                var candidatePath = Path.Combine(parentDir.FullName, SettingsFolderName);
                
                if (Directory.Exists(candidatePath))
                    return candidatePath;
            }

            return Path.Combine(contentRootPath, SettingsFolderName);
        }
        public static string GetSharedFilePath(string contentRootPath, string fileName, string? configuredOverridePath = null)
        {
            if (!string.IsNullOrWhiteSpace(configuredOverridePath))
            {
                return Path.IsPathRooted(configuredOverridePath)
                    ? configuredOverridePath
                    : Path.GetFullPath(Path.Combine(contentRootPath, configuredOverridePath));
            }

            var settingsDirectory = GetSharedSettingsDirectory(contentRootPath);
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }

            return Path.Combine(settingsDirectory, fileName);
        }
        public static IHostApplicationBuilder AddSharedConfiguration(this IHostApplicationBuilder builder)
        {
            var settingsDirectory = GetSharedSettingsDirectory(builder.Environment.ContentRootPath);
            var sharedConfigPath = Path.Combine(settingsDirectory, SharedConfigFileName);

            bool reload = builder.Environment.IsDevelopment();

            builder.Configuration.AddJsonFile(sharedConfigPath, optional: true, reloadOnChange: reload);
            builder.Configuration.AddEnvironmentVariables();

            return builder;
        }
    }
}
