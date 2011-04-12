using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Seterlund.Wcf.UnitTests
{
    [DebuggerStepThrough]
    public static class ExceptionAssert
    {
        // Methods
        public static void DoesNotThrow(Action action)
        {
            DoesNotThrow<Exception>(action);
        }

        public static void DoesNotThrow<T>(Action action) where T: Exception
        {
            Exception exception = null;
            try
            {
                action();
            }
            catch (T local)
            {
                exception = local;
            }
            if (exception != null)
            {
                throw new AssertFailedException(string.Format("ExceptionAssert.DoesNotThrow failed. Exception <{0}> thrown with message <{1}>", exception.GetType().FullName, exception.Message));
            }
        }

        public static void Throws<T>(Action action) where T: Exception
        {
            Exception actualException = null;
            try
            {
                action();
            }
            catch (Exception exception2)
            {
                actualException = exception2;
            }
            ValidateThrownException<T>(actualException, null);
        }

        public static void Throws<T>(Action action, Action<T> asserts) where T: Exception
        {
            Exception actualException = null;
            try
            {
                action();
            }
            catch (Exception exception2)
            {
                actualException = exception2;
            }
            ValidateThrownException(actualException, asserts);
        }

        public static void Throws<T>(Action action, Action<T> asserts, Action finalAction) where T: Exception
        {
            Exception actualException = null;
            try
            {
                action();
            }
            catch (Exception exception2)
            {
                actualException = exception2;
            }
            finally
            {
                if (finalAction != null)
                {
                    finalAction();
                }
            }
            ValidateThrownException(actualException, asserts);
        }

        private static void ValidateThrownException<T>(Exception actualException, Action<T> asserts) where T: Exception
        {
            if (actualException is T)
            {
                if (asserts != null)
                {
                    asserts(actualException as T);
                }
            }
            else
            {
                if (actualException == null)
                {
                    throw new AssertFailedException(string.Format("ExceptionAssert.Throws failed. No exception was thrown. Expected <{0}>.", typeof(T).FullName));
                }
                throw new AssertFailedException(string.Format("ExceptionAssert.Throws failed. Expected <{0}>. Actual <{1}>", typeof(T).FullName, actualException.GetType().FullName));
            }
        }
    }
}

