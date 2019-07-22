using System;
using System.Collections.Generic;
using System.Linq;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Services.Filters;

namespace TextWrangler.Services
{
    public class RecordConfigurationValidator
    {
        private RecordConfigurationValidator() { }

        public static RecordConfigurationValidator Instance { get; } = new RecordConfigurationValidator();

        public void Validate(RecordConfiguration recordConfiguration, IFieldFilterService againstFieldFilterService = null)
        {
            recordConfiguration.Should(s => s != null,
                                       "RecordConfigurationi is null or missing");

            recordConfiguration.Should(s => !s.Fields.IsNullOrEmpty(),
                                       "RecordConfiguration must have at least one field mapping");

            recordConfiguration.Fields
                               .ShouldAll(f => !f.Name.IsNullOrEmpty(),
                                          f => "RecordConfiguration.Fields must all have valid names");

            recordConfiguration.Fields
                               .ShouldAll(f => !f.Format.IsNullOrEmpty(),
                                          f => $"RecordConfiguration.Field [{f.Name}] does not have a valid format value");

            recordConfiguration.Fields
                               .Where(f => !f.Sources.IsNullOrEmpty())
                               .Each(f => f.Sources.ShouldAll(s => !s.Name.IsNullOrEmpty(),
                                                              m => $"RecordConfiguration.Field [{m.Name}] has source(s) without any name"));

            recordConfiguration.Fields
                               .Where(f => !f.Sources.IsNullOrEmpty())
                               .Each(f => f.Sources.ShouldAll(s => !s.Name.IsNullOrEmpty(),
                                                              m => $"RecordConfiguration.Field [{m.Name}] has source(s) without a name"));

            // Get all the filters used in the config, they must all be mapped in the given filter service
            var filters = new HashSet<string>(recordConfiguration.Fields
                                                                 .Where(f => !f.Filters.IsNullOrEmpty())
                                                                 .SelectMany(f => f.Filters)
                                                                 .Union(recordConfiguration.Fields
                                                                                           .Where(f => !f.Sources.IsNullOrEmpty())
                                                                                           .SelectMany(s => s.Sources
                                                                                                             .SelectMany(fs => fs.Filters ?? Enumerable.Empty<string>()))),
                                              StringComparer.OrdinalIgnoreCase);

            var filterService = againstFieldFilterService ?? TextWranglerConfig.DefaultFieldFilterService;

            filters.ShouldAll(f => filterService.FilterExists(f), fn => $"Filter named [{fn}] does not exist or is not mapped correctly");

            // Get all the types used in the config, they must all be mappable to a system type
            var types = new HashSet<string>(recordConfiguration.Fields
                                                               .Where(f => !f.Type.IsNullOrEmpty())
                                                               .Select(f => f.Type)
                                                               .Union(recordConfiguration.Fields
                                                                                         .Where(f => !f.Sources.IsNullOrEmpty())
                                                                                         .SelectMany(s => s.Sources
                                                                                                           .Where(fs => !fs.Type.IsNullOrEmpty())
                                                                                                           .Select(fs => fs.Type))),
                                            StringComparer.OrdinalIgnoreCase);

            types.ShouldAll(t => t.TryGetSystemType(out _), tn => $"Type named [{tn}] is not a valid system type");
        }
    }
}
