namespace AutoccultistNS
{
    using System;
    using AutoccultistNS.Yaml;
    using YamlDotNet.Core;

    public static class ExceptionExtensions
    {
        public static bool WrappedExceptionsContain<T>(this Exception exception)
            where T : Exception
        {
            var trace = exception;
            while ((trace = trace.InnerException) != null)
            {
                if (trace is T)
                {
                    return true;
                }
            }

            return false;
        }

        public static Exception GetInterestingException(this Exception exception)
        {
            var interestingException = exception;
            while ((interestingException = UnwrapException(interestingException)) != null)
            {
                exception = interestingException;
            }

            return exception;
        }

        private static Exception UnwrapException(this Exception exception)
        {
            if (exception is YamlException || exception is YamlFileException)
            {
                return exception.InnerException;
            }

            if (exception is AggregateException)
            {
                return exception.InnerException;
            }

            return null;
        }
    }
}
