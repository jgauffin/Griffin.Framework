using System;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Uses to generate all all bus types.
    /// </summary>
    public class IocBusBuilder
    {
        private readonly IContainer _container;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IocBusBuilder" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public IocBusBuilder(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
            CommandBus = new IocCommandBus(_container);
            RequestBus = new IocRequestReplyBus(container);
            EventBus = new IocEventBus(container);
            QueryBus = new IocQueryBus(container);
        }

        /// <summary>
        ///     Same instance is shared between everything that this class builds.
        /// </summary>
        public IocCommandBus CommandBus { get; set; }

        /// <summary>
        ///     Same instance is shared between everything that this class builds.
        /// </summary>
        public IocEventBus EventBus { get; set; }

        /// <summary>
        ///     Same instance is shared between everything that this class builds.
        /// </summary>
        public IocQueryBus QueryBus { get; set; }

        /// <summary>
        ///     Same instance is shared between everything that this class builds.
        /// </summary>
        public IocRequestReplyBus RequestBus { get; set; }

        /// <summary>
        ///     Builds the message processor.
        /// </summary>
        /// <returns></returns>
        public CqsMessageProcessor BuildMessageProcessor()
        {
            return new CqsMessageProcessor
            {
                CommandBus = CommandBus,
                EventBus = EventBus,
                QueryBus = QueryBus,
                RequestReplyBus = RequestBus
            };
        }
    }
}