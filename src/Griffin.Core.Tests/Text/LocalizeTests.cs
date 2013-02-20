using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Griffin.Framework.Text;
using NSubstitute;
using Xunit;

namespace Griffin.Framework.Tests.Text
{
    [Localize("sv-se", "Tjenna världen!")]
    [Localize("sv-se", "Strict", "Hej världen!")]
    [Localize("en-us", "Hello world!")]
    public class LocalizeTests
    {
        [Fact]
        public void NoDataSource()
        {
            Localize.A.DataSource = null;

            Assert.Throws<InvalidOperationException>(() => Localize.A.String("/", "MyId", "Hello"));
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
        [Localize("en-us", "Hello world")]
        public void Training_Method_Found()
        {
            Localize.A.TrainingMode = true;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextDataSource>();
            Localize.A.DataSource.Get(GetType().FullName +  ".Training_Method_Found").Returns("Mamma");

            var text = Localize.A.Method(MethodBase.GetCurrentMethod());

            Assert.Equal("Mamma", text);
        }

        [Fact]
        [Localize("en-us", "Hello world")]
        public void Training_Method_Found_Default()
        {
            Localize.A.TrainingMode = true;
            Localize.A.SourceCulture = new CultureInfo("en-us");

            var text = Localize.A.Method(MethodBase.GetCurrentMethod());

            Assert.Equal("Hello world", text);
        }

        [Fact]
        [Localize("en-us", "Key", "Hello world")]
        public void Training_Method_FoundMeta()
        {
            Localize.A.TrainingMode = true;
            Localize.A.SourceCulture = Thread.CurrentThread.CurrentCulture;
            Localize.A.DataSource = Substitute.For<ITextDataSource>();
            Localize.A.DataSource.Get(GetType().FullName + ".Training_Method_FoundMeta/Key").Returns("Mamma");

            var text = Localize.A.Method(MethodBase.GetCurrentMethod(), "Key");

            Assert.Equal("Mamma", text);
        }

        [Fact]
        [Localize("en-us", "Key", "Hello world2")]
        public void Training_Method_FoundMeta_Default()
        {
            Localize.A.TrainingMode = true;
            Localize.A.SourceCulture = new CultureInfo("en-us");

            var text = Localize.A.Method(MethodBase.GetCurrentMethod(), "Key");

            Assert.Equal("Hello world2", text);
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

        [Fact]
        public void Type()
        {
            Localize.A.TrainingMode = false;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("sv-se");

            var text = Localize.A.Type<LocalizeTests>();

            Assert.Equal("Tjenna världen!", text);
        }


        [Fact]
        public void Type_Meta()
        {
            Localize.A.TrainingMode = false;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("sv-se");

            var text = Localize.A.Type<LocalizeTests>("Strict");

            Assert.Equal("Hej världen!", text);
        }
    }
}