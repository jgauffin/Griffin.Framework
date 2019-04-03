using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;
using FluentAssertions;
using Griffin.Cqs.InversionOfControl;
using Griffin.Cqs.Tests.InversionOfControl.Subjects;
using Xunit;

namespace Griffin.Cqs.Tests.InversionOfControl
{
    public class EventHandlerRegistryTests
    {
        public class SupriseHandler : IApplicationEventSubscriber<SupriseSucceeded>
        {
            public async Task HandleAsync(SupriseSucceeded e)
            {
                
            }
        }

        [Fact]
        public void should_Be_able_to_resolve_a_handler()
        {
            var sut = new EventHandlerRegistry();
            sut.Map<SupriseSucceeded>(typeof(SupriseHandler));

            var handlers = sut.Lookup(typeof (SupriseSucceeded)).ToList();

            handlers.Count.Should().Be(1);
            handlers[0].Should().Be(typeof (SupriseHandler));
        }

        [Fact]
        public void should_not_Be_able_To_map_other_classes_than_Event_handlers()
        {
            var sut = new EventHandlerRegistry();

            Action actual = () => sut.Map<SupriseSucceeded>(GetType());

            actual.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void should_not_Be_able_To_map_lookup_other_types_of_objects_than_events()
        {
            var sut = new EventHandlerRegistry();
            sut.Map<SupriseSucceeded>(typeof(SupriseHandler));

            Action actual = () => sut.Lookup(GetType());

            actual.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void should_not_Be_able_To_map_using_assembly_scanning()
        {
            var sut = new EventHandlerRegistry();
            sut.ScanAssembly(Assembly.GetExecutingAssembly());

            var handlers = sut.Lookup(typeof(SupriseSucceeded)).ToList();

            handlers.Count.Should().Be(1);
            handlers[0].Should().Be(typeof(SupriseHandler));
        }
    }
}
