using System;

namespace Autoccultist
{

    class RecipeRejectedException : Exception
    {
        public RecipeRejectedException(string message) : base(message) { }
    }
}