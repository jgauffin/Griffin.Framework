using System;
using Griffin.Framework.Data;
using Xunit;

namespace Griffin.Framework.Tests.Data
{
    public class SearchQueryTests
    {
        [Fact]
        public void Defaults()
        {
            var query = new MyQuery();

            var sut = query.ToQueryInfo();

            Assert.IsAssignableFrom<IOrderedQueryInfo>(sut);
            Assert.IsAssignableFrom<IPagedQueryInfo>(sut);
            Assert.IsAssignableFrom<IQueryInfo>(sut);
            Assert.Equal(50, ((IPagedQueryInfo)sut).PageSize);
            Assert.Equal(1, ((IPagedQueryInfo)sut).PageNumber);
            Assert.Equal(null, ((IOrderedQueryInfo)sut).OrderByPropertyName);
            Assert.Equal(true, ((IOrderedQueryInfo)sut).OrderAscending);
        }


        [Fact]
        public void Sort__Lambda_OkAsc()
        {
            var query = new MyQuery();

            query.OrderBy(x=> x.FirstName);
            var sut = query.ToQueryInfo();

            Assert.Equal("FirstName", ((IOrderedQueryInfo)sut).OrderByPropertyName);
            Assert.Equal(true, ((IOrderedQueryInfo)sut).OrderAscending);
        }

        [Fact]
        public void Sort_Lambda_OkDesc()
        {
            var query = new MyQuery();

            query.OrderByDescending(x => x.FirstName);
            var sut = query.ToQueryInfo();

            Assert.Equal("FirstName", ((IOrderedQueryInfo)sut).OrderByPropertyName);
            Assert.Equal(false, ((IOrderedQueryInfo)sut).OrderAscending);
        }

        [Fact]
        public void Sort_string_OkDesc()
        {
            var query = new MyQuery();

            query.OrderByDescending("FirstName");
            var sut = query.ToQueryInfo();

            Assert.Equal("FirstName", ((IOrderedQueryInfo)sut).OrderByPropertyName);
            Assert.Equal(false, ((IOrderedQueryInfo)sut).OrderAscending);
        }

        [Fact]
        public void Sort_string_OkAsc()
        {
            var query = new MyQuery();

            query.OrderBy("FirstName");
            var sut = query.ToQueryInfo();

            Assert.Equal("FirstName", ((IOrderedQueryInfo)sut).OrderByPropertyName);
            Assert.Equal(true, ((IOrderedQueryInfo)sut).OrderAscending);
        }

        [Fact]
        public void Sort_string_MissingProperty()
        {
            var query = new MyQuery();

            Assert.Throws<ArgumentOutOfRangeException>(() => query.OrderBy("Mamma"));
        }

        [Fact]
        public void Page_SmallPage()
        {
            var query = new MyQuery();

            query.Page(2, 3);
            var sut = query.ToQueryInfo();

            Assert.Equal(2, ((IPagedQueryInfo)sut).PageNumber);
            Assert.Equal(3, ((IPagedQueryInfo)sut).PageSize);
            
        }

        [Fact]
        public void Page_ZeroAsIndex()
        {
            var query = new MyQuery();

            Assert.Throws<ArgumentOutOfRangeException>(() => query.Page(0, 3));
        }

        
        public class User
        {
            public string FirstName { get; private set; }
        }
        public class MyQuery : SearchQuery<User>
        {
            
        }
    }
}
