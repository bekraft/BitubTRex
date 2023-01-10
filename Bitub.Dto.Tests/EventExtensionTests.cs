using System;
using Bitub.Dto;
using NUnit.Framework;

namespace Bitub.Dto.Tests
{
    public class EventExtensionTests : TestBase<EventExtensionTests>
    {
        protected Action<string> WriteStdOutT1 = (s1) => Console.WriteLine(s1);
        protected Action<string, string> WriteStdOutT2 = (s1, s2) => Console.WriteLine($"{s1}: {s2}");
        protected Action<string, string, string> WriteStdOutT3 = (s1, s2, s3) => Console.WriteLine($"{s1}: {s2}: {s3}");

        [SetUp]
        public void Setup()
        {
            InternallySetup();
        }

        [Test]
        public void RaiseAsyncTestT1() 
        {
            WriteStdOutT1.RaiseAsync("RaiseAsyncTestT1 called");
        }

        [Test]
        public void RaiseAsyncTestT2() 
        {
            WriteStdOutT2.RaiseAsync("RaiseAsyncTestT2 called with", "Arg2");
        }

        [Test]
        public void RaiseAsyncTestT3() 
        {
            WriteStdOutT3.RaiseAsync("RaiseAsyncTestT2 called with", "Arg2", "Arg3");
        }

    }
}