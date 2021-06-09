using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace GiganticEmu.Shared.Backend
{
    [Serializable]
    public class ValidationException : DataException
    {
        public IEnumerable<EntityValidationResult> ValidationResults { get; }

        public ValidationException()
        {
            ValidationResults = Enumerable.Empty<EntityValidationResult>();
        }

        public ValidationException(IEnumerable<EntityValidationResult> validationResults)
        {
            ValidationResults = validationResults;
        }
    }
}
