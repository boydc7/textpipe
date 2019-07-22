using System;

namespace TextWrangler.Models
{
    public abstract class TextWranglerException : Exception
    {
        protected TextWranglerException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
    public class TextWranglerRecordFieldConfigInvalidException : TextWranglerException
    {
        public TextWranglerRecordFieldConfigInvalidException(string recordTypeName, string targetField, string message,
                                                             Exception innerException = null)
            : base(message, innerException)
        {
            RecordTypeName = recordTypeName ?? "N/A";
            TargetField = targetField ?? "N/A";
        }

        public string RecordTypeName { get; }
        public string TargetField { get; }
    }

    public class TextWranglerInvalidTargetStateException : TextWranglerException
    {
        public TextWranglerInvalidTargetStateException(string recordTypeName, string targetField, string message)
            : base(message)
        {
            RecordTypeName = recordTypeName ?? "N/A";
            TargetField = targetField ?? "N/A";
        }

        public string RecordTypeName { get; }
        public string TargetField { get; }
    }

    public class TextWranglerFieldFilterException : TextWranglerException
    {
        public TextWranglerFieldFilterException(string filterName, string message)
            : base(message)
        {
            FilterName = filterName;
        }

        public string FilterName { get; }
    }

    public class TextWranglerValidationException : TextWranglerException
    {
        public TextWranglerValidationException(string message)
            : base(message) { }
    }

    public class TextWranglerReaderException : TextWranglerException
    {
        public TextWranglerReaderException(string message)
            : base(message) { }
    }
}
