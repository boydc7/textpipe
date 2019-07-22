using System;
using System.Collections.Generic;
using System.Linq;
using TextWrangler.Configuration;
using TextWrangler.Models;

namespace TextWrangler.Services.Formatters
{
    /// <summary>
    /// Composite wrapper around one or more <see cref="IFieldFormatter" />s
    /// </summary>
    public class CompositeFieldFormatter : IFieldFormatter
    {
        private readonly IEnumerable<IFieldFormatter> _formatters;

        public CompositeFieldFormatter(IEnumerable<IFieldFormatter> formatters)
        {
            _formatters = formatters ?? throw new ArgumentNullException(nameof(formatters));
        }

        /// <summary>
        /// Formats each record in the incoming enumerable through each of this composite's <see cref="IFieldFormatter" />s
        /// </summary>
        /// <param name="records"></param>
        /// <param name="recordConfiguration"></param>
        /// <returns>Lazy enumerable of <see cref="TargetRecord" /></returns>
        public IEnumerable<TargetRecord> Format(IEnumerable<TargetRecord> records, RecordConfiguration recordConfiguration)
            => _formatters.Aggregate(records, (prs, rf) => rf.Format(prs, recordConfiguration));
    }
}
