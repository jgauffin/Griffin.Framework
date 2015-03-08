using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data;
using Griffin.Data.Mapper;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class AsyncUnitOfWorkExtensionsTests : IMappingProvider
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        


        //[Fact]
        //public void Try_Get_identity_from_my_local_LocalDb_so_that_this_test_is_screwed_on_the_build_server()
        //{
        //    var entity = new MyEntity { Id = 1, Name = "Arne" };
        //    EntityMappingProvider.Provider = this;
        //    var conStr = @"Data Source=(localdb)\ProjectsV12;Initial Catalog=GriffinFrameworkTests;Integrated Security=True;";
        //    using (var connection = new SqlConnection(conStr))
        //    {
        //        connection.Open();

        //        using (var uow = new AdoNetUnitOfWork(connection))
        //        {
        //            uow.Insert(entity);
        //            uow.SaveChanges();
        //        }
        //    }

        //    entity.Id.Should().NotBe(0);
        //}


        //[Fact]
        //public void Try_Get_identity_from_my_local_LocalDb_so_that_this_test_is_screwed_on_the_build_server()
        //{
        //    var entity = new MyEntity { Id=1, Name = "Arne" };
        //    EntityMappingProvider.Provider = this;
        //    var conStr = @"Data Source=(localdb)\ProjectsV12;Initial Catalog=GriffinFrameworkTests;Integrated Security=True;";
        //    using (var connection = new SqlConnection(conStr))
        //    {
        //        connection.Open();

        //        using (var uow = new AdoNetUnitOfWork(connection))
        //        {
        //            uow.Insert(entity);
        //            uow.SaveChanges();
        //        }
        //    }

        //    entity.Id.Should().NotBe(0);
        //}

        //[Fact]
        //public async Task Try_Get_identity_from_my_local_LocalDb_so_that_this_test_is_screwed_on_the_build_server___async_version()
        //{
        //    var entity = new MyEntity { Name = "Arne" };
        //    EntityMappingProvider.Provider = this;
        //    var conStr = @"Data Source=(localdb)\ProjectsV12;Initial Catalog=GriffinFrameworkTests;Integrated Security=True;";
        //    using (var connection = new SqlConnection(conStr))
        //    {
        //        connection.Open();

        //        using (var uow = new AdoNetUnitOfWork(connection))
        //        {
        //            await uow.InsertAsync(entity);
        //            uow.SaveChanges();
        //        }
        //    }

        //    entity.Id.Should().NotBe(0);
        //}

        public ICrudEntityMapper Get<TEntity>()
        {
            var m= new MyMapper();
            m.Freeze();
            return m;
        }

        public IEntityMapper GetBase<T>()
        {
            var m = new MyMapper();
            m.Freeze();
            return m;
        }


        public class MyTestMapper
        {
        }

        public class MyMapper : CrudEntityMapper<AsyncUnitOfWorkExtensionsTests.MyEntity>
        {
            public MyMapper()
                : base("[Table2]")
            {

                //Property(x => x.Id)
                //    .ColumnName("UserId")
                //    .ToColumnValue(propertyValue => propertyValue.ToString())
                //    .ToPropertyValue(colValue => int.Parse(colValue.ToString()))
                //    .PrimaryKey();

                //// property is considered read only.
                //Property(x => x.Name)
                //    .NotForCrud();
            }


        }
    }

}
