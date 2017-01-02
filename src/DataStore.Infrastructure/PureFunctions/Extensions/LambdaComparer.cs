﻿namespace DataStore.Infrastructure.PureFunctions.Extensions
{
    using System;
    using System.Collections.Generic;

    // see http://stackoverflow.com/questions/6277760/can-i-use-linqs-except-with-a-lambda-expression-comparer
    // Usage: //var availableItems = allItems.Except(filterItems, (p, p1) => p.Id== p1.Id);

    public class LambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _expression;

        public LambdaComparer(Func<T, T, bool> lambda)
        {
            _expression = lambda;
        }

        public bool Equals(T x, T y)
        {
            return _expression(x, y);
        }

        public int GetHashCode(T obj)
        {
            /*
             If you just return 0 for the hash the Equals comparer will kick in. 
             The underlying evaluation checks the hash and then short circuits the evaluation if it is false.
             Otherwise, it checks the Equals. If you force the hash to be true (by assuming 0 for both objects), 
             you will always fall through to the Equals check which is what we are always going for.
            */
            return 0;
        }
    }
}