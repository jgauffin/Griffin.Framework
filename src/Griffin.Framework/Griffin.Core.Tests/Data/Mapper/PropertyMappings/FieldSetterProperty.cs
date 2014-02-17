namespace Griffin.Core.Tests.Data.Mapper.PropertyMappings
{
    class FieldSetterProperty
    {
        private string _id;

        public string Id { get { return _id; } set { _id = value; } }
    }

    class FieldGetterProperty
    {
        private string _id = "10";

        public string Id { get { return _id; } }
    }
}