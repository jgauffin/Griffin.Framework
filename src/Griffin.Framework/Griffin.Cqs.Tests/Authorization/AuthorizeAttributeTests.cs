using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Cqs.Authorization;
using Xunit;

namespace Griffin.Cqs.Tests.Authorization
{
    public class AuthorizeAttributeTests
    {
        [Fact]
        public void Roles_may_be_empty_if_user_should_just_have_been_authenticated()
        {

            var sut = new AuthorizeAttribute();

            sut.Roles.Should().BeEmpty();
        }

        [Fact]
        public void roles_are_assigned_properly()
        {
            
            var sut = new AuthorizeAttribute("User", "Admin");

            sut.Roles.First().Should().Be("User");
            sut.Roles.Last().Should().Be("Admin");
        }

        [Fact]
        public void must_not_specify_null()
        {

            Action actual = () => new AuthorizeAttribute(null);

            actual.Should().Throw<ArgumentNullException>();
        }
    }
}
