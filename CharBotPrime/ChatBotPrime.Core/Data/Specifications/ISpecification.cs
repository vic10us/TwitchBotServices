﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ChatBotPrime.Core.Data.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        IList<Expression<Func<T, object>>> Includes { get; }
        IList<string> IncludeStrings { get; }
        void AddInclude(Expression<Func<T, object>> expression);
    }
}
