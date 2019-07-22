using System.IO;
using Microsoft.Extensions.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Configuration
{
    /// <summary>
    /// Used to build a POCO <see cref="RecordConfiguration" /> object that models a target <see cref="RecordType" /> record in the given
    /// TextWrangler configuration file.
    /// </summary>
    public class RecordConfigurationBuilder
    {
        /// <summary>
        /// Build a <see cref="RecordConfiguration" /> that models the given record configuration in the TextWrangler config file
        /// (or the passed jsonFile argument).
        /// </summary>
        /// <param name="configSectionName"></param>
        /// <param name="jsonFile"></param>
        /// <returns><see cref="RecordConfiguration" /></returns>
        /// <exception cref="TextWranglerValidationException"></exception>
        public static RecordConfiguration Build(string configSectionName, string jsonFile = null)
        {
            if (jsonFile.IsNullOrEmpty())
            {
                jsonFile = TextWranglerConfig.TextWranglerConfigFile;
            }

            if (!File.Exists(jsonFile))
            {
                throw new TextWranglerValidationException($"Could not find TextWranger config file [{jsonFile}].");
            }

            var builder = new ConfigurationBuilder();

            builder.AddJsonFile(jsonFile);

            var configRoot = builder.Build();

            var recordConfig = configRoot.GetSection(configSectionName).Get<RecordConfiguration>();

            if (recordConfig == null)
            {
                throw new TextWranglerValidationException($"No record configSection named [{configSectionName}] could be found in the [{jsonFile}] config file.");
            }

            recordConfig.RecordTypeName = configSectionName;

            return recordConfig;
        }
    }
}
