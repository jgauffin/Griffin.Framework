using System;
using System.Reflection;
using System.Threading;
using NSubstitute;
using Xunit;

namespace Griffin.Framework.Tests.Text
{
    public class LocalizeTests
    {
        [Fact]
        public void NoDataSource()
        {
            Localize.A.DataSource = null;

            Assert.Throws<InvalidOperationException>(() => Localize.A.String("MyId", "Hello"));
        }


        [Fact]
        public void NoTraining_Default_Found()
        {
            Localize.A.TrainingMode = false;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextDataSource>();
            Localize.A.DataSource.Get("MyId").Returns("Mamma");

            var text = Localize.A.String("MyId", "Hello");

            Assert.Equal("Mamma", text);
        }

        [Fact]
        public void NoTraining_Default_NotFound()
        {
            Localize.A.TrainingMode = false;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextDataSource>();

            var text = Localize.A.String("MyId", "Hello");

            Assert.Equal("Hello", text);
        }

        [Fact]
        public void NoTraining_Default_NotFound_Repository()
        {
            Localize.A.TrainingMode = false;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextRepository>();


            var text = Localize.A.String("MyId", "Hello");

            Assert.Equal("Hello", text);
            ((ITextRepository) Localize.A.DataSource).DidNotReceive().Create("MyId", "");
        }


        [Fact]
        public void Training_Default_Found()
        {
            Localize.A.TrainingMode = true;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextDataSource>();
            Localize.A.DataSource.Get("MyId").Returns("Mamma");

            var text = Localize.A.String("MyId", "Hello");

            Assert.Equal("Mamma", text);
        }

        [Fact]
        public void Training_Method_Found()
        {
            Localize.A.TrainingMode = true;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextDataSource>();
            Localize.A.DataSource.Get("Griffin.Tests.Text.LocalizeTests.Training_Method_Found").Returns("Mamma");

            var text = Localize.A.Method(MethodBase.GetCurrentMethod(), "Hello");

            Assert.Equal("Mamma", text);
        }

        [Fact]
        public void Training_Method_Found_MetaData()
        {
            Localize.A.TrainingMode = true;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextDataSource>();
            Localize.A.DataSource.Get("Griffin.Tests.Text.LocalizeTests.Training_Method_Found_MetaData", "Failed").Returns("Mamma");

            var text = Localize.A.Method(MethodBase.GetCurrentMethod(), "Failed", "Hello");

            Assert.Equal("Mamma", text);
        }

        [Fact]
        public void Training_Default_NotFound()
        {
            Localize.A.TrainingMode = true;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextDataSource>();

            var text = Localize.A.String("MyId", "Hello");

            Assert.Equal("-(MyId)-", text);
        }


        [Fact]
        public void Training_Default_NotFound_Repository()
        {
            Localize.A.TrainingMode = true;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextRepository>();


            var text = Localize.A.String("MyId", "Hello");

            Assert.Equal("-(MyId)-", text);
            ((ITextRepository) Localize.A.DataSource).Received().Create("MyId", "");
        }
    }
}