#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

using AopAlliance.Intercept;
using NUnit.Framework;
using Rhino.Mocks;

#endregion

namespace Spring.Aop.Framework
{
	/// <summary>
	/// Unit tests for the AbstractMethodInvocation class.
	/// </summary>
	/// <author>Rick Evans</author>
    /// <author>Bruno Baia</author>
	[TestFixture]
	public abstract class AbstractMethodInvocationTests
	{
	    private MockRepository mocks;

        [SetUp]
        public virtual void SetUp()
        {
            mocks = new MockRepository();
        }

	    protected abstract AbstractMethodInvocation CreateMethodInvocation(
            object proxy, object target, MethodInfo method, MethodInfo onProxyMethod, 
            object[] arguments, Type targetType, IList interceptors);

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithNullMethod()
		{
            CreateMethodInvocation(null, this, null, null, null, GetType(), null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithNullTarget()
		{
            CreateMethodInvocation(null, null, null, null, null, GetType(), null);
		}

		[Test]
		public void ProceedWithNullInterceptorChain()
		{
			Target target = new Target();
            AbstractMethodInvocation join = CreateMethodInvocation(
                null, target, target.GetTargetMethodNoArgs(), null, null, target.GetType(), null);
			string score = (string) join.Proceed();
			Assert.AreEqual(Target.DefaultScore + Target.Suffix, score);
		}

		[Test]
		public void ProceedWithEmptyInterceptorChain()
		{
			Target target = new Target();
            AbstractMethodInvocation join = CreateMethodInvocation(
                null, target, target.GetTargetMethodNoArgs(), null, null, target.GetType(), new ArrayList());
			string score = (string) join.Proceed();
			Assert.AreEqual(Target.DefaultScore + Target.Suffix, score);
		}

		[Test]
		public void ToStringWithoutArguments()
		{
			Target target = new Target();
            AbstractMethodInvocation join = CreateMethodInvocation(
                null, target, target.GetTargetMethodNoArgs(), null, null, target.GetType(), new ArrayList());
			CheckToStringDoesntThrowAnException(join);
		}

		[Test]
		public void ToStringWithArguments()
		{
			Target target = new Target();
            AbstractMethodInvocation join = CreateMethodInvocation(
                null, target, target.GetTargetMethod(), null, new string[] { "Five" }, target.GetType(), new ArrayList());
			CheckToStringDoesntThrowAnException(join);
		}

		[Test]
		public void ToStringMustNotInvokeToStringOnTarget()
		{
			Target target = new TargetWithBadToString();
            AbstractMethodInvocation join = CreateMethodInvocation(
                null, target, target.GetTargetMethodNoArgs(), null, null, target.GetType(), new ArrayList());
			// if it hits the target the test will fail with NotSupportedException...
			CheckToStringDoesntThrowAnException(join);
		}

		private static string CheckToStringDoesntThrowAnException(AbstractMethodInvocation join)
		{
			return join.ToString();
		}

		public class Target
		{
			public const string Suffix = "!!!";
			public const string DefaultScore = "ONE HUNDRED AND EIGHTY";

			public MethodInfo GetTargetMethodNoArgs()
			{
				return GetType().GetMethod("BullseyeMethod", Type.EmptyTypes);
			}

			public MethodInfo GetTargetMethod()
			{
				return GetType().GetMethod("BullseyeMethod", new Type[] {typeof (string)});
			}

			public string BullseyeMethod()
			{
				return BullseyeMethod(DefaultScore);
			}

			public string BullseyeMethod(string score)
			{
				return score + Suffix;
			}
		}

		public interface ICommand
		{
			void Execute();
		}

		public sealed class BadCommand : ICommand
		{
			public void Execute()
			{
				throw new NotImplementedException();
			}

			public MethodInfo GetTargetMethod()
			{
				return GetType().GetMethod("Execute", Type.EmptyTypes);
			}
		}

		private sealed class TargetWithBadToString : Target
		{
			public override string ToString()
			{
				throw new NotSupportedException("ToString");
			}
		}

		[Test]
		public void ValidInvocation()
		{/*
			Target target = new Target();
            MockRepository repository = new MockRepository();
		    IMethodInterceptor interceptor = (IMethodInterceptor) repository.CreateMock(typeof (IMethodInterceptor));
            AbstractMethodInvocation join = CreateMethodInvocation(
               null, target, target.GetTargetMethodNoArgs(), null, null, target.GetType(), new object[] { interceptor });
		    Expect.Call(interceptor.Invoke(join)).Return(target.BullseyeMethod().ToLower(CultureInfo.InvariantCulture));
            repository.ReplayAll();
		    string score = (string) join.Proceed();
            Assert.AreEqual(target.BullseyeMethod().ToLower(CultureInfo.InvariantCulture) + Target.Suffix, score);
            repository.VerifyAll();
            */

            Target target = new Target();
            IMethodInterceptor mock = (IMethodInterceptor) mocks.CreateMock(typeof(IMethodInterceptor));
            AbstractMethodInvocation join = CreateMethodInvocation(
                null, target, target.GetTargetMethodNoArgs(), null, null, target.GetType(), new object[] { mock });
			
            Expect.Call(mock.Invoke(null)).IgnoreArguments().Return(target.BullseyeMethod().ToLower(CultureInfo.InvariantCulture));
            mocks.ReplayAll();

			string score = (string) join.Proceed();
			Assert.AreEqual(Target.DefaultScore.ToLower(CultureInfo.InvariantCulture) + Target.Suffix, score);
			
            mocks.VerifyAll();
            
		}

		[Test]
		public void UnwrapsTargetInvocationException_NoInterceptors()
		{
			BadCommand target = new BadCommand();
            AbstractMethodInvocation join = CreateMethodInvocation(
                null, target, target.GetTargetMethod(), null, null, target.GetType(), new object[] { });
			try
			{
				join.Proceed();
			}
			catch (NotImplementedException)
			{
				// this is good, we want this exception to bubble up...
			}
			catch (TargetInvocationException)
			{
				Assert.Fail("Must have unwrapped this.");
			}
		}

		[Test]
		public void UnwrapsTargetInvocationException_WithInterceptor()
		{
			BadCommand target = new BadCommand();
            IMethodInterceptor mock = (IMethodInterceptor) mocks.CreateMock(typeof(IMethodInterceptor));
            AbstractMethodInvocation join = CreateMethodInvocation(
                null, target, target.GetTargetMethod(), null, null, target.GetType(), new object[] { mock });
		    
            Expect.Call(mock.Invoke(null)).IgnoreArguments().Return(null);
            mocks.ReplayAll();

			try
			{
				join.Proceed();
			}
			catch (NotImplementedException)
			{
				// this is good, we want this exception to bubble up...
			}
			catch (TargetInvocationException)
			{
				Assert.Fail("Must have unwrapped this.");
			}
			mocks.VerifyAll();
		}

		[Test]
		public void UnwrapsTargetInvocationException_WithInterceptorThatThrowsAnException()
		{
			BadCommand target = new BadCommand();
            IMethodInterceptor mock = (IMethodInterceptor) mocks.CreateMock(typeof(IMethodInterceptor));
            AbstractMethodInvocation join = CreateMethodInvocation(
                null, target, target.GetTargetMethod(), null, null, target.GetType(), new object[] { mock });
		    Expect.Call(mock.Invoke(null)).IgnoreArguments().Throw(new NotImplementedException());
            mocks.ReplayAll();

			try
			{
				join.Proceed();
			}
			catch (NotImplementedException)
			{
				// this is good, we want this exception to bubble up...
			}
			catch (TargetInvocationException)
			{
				Assert.Fail("Must have unwrapped this.");
			}
			mocks.VerifyAll();
		}
	}
}