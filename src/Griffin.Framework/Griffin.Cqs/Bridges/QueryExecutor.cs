//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using DotNetCqs;

//namespace Griffin.Cqs.Bridges
//{
//    class QueryExecutor : IQueryExecutor
//    {
//        private readonly IQueryBus _queryBus;

//        public QueryExecutor(IQueryBus queryBus)
//        {
//            if (queryBus == null) throw new ArgumentNullException("queryBus");
//            _queryBus = queryBus;
//        }

//        public Task<object> ExecuteAsync(IQuery query)
//        {
            
//        }
//    }

//    public class CommandExecutor : ICommandExecutor
//    {
//        private ICommandBus _commandBus;

//        public CommandExecutor(ICommandBus commandBus)
//        {
//            _commandBus = commandBus;
//        }

//        public async Task ExecuteAsync(Command command)
//        {
//            var type = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
//            try
//            {
//                await _commandBus.ExecuteAsync(command);
//                //var method = type.GetMethod("ExecuteAsync");
//                //var task = (Task)method.Invoke(handlers[0], new object[] { command });
//                //await task;
//            }
//            catch (Exception exception)
//            {
//                if (exception is TargetInvocationException)
//                    exception = exception.InnerException;
//                HandlerFailed(this, new CommandHandlerFailedEventArgs(command, handlers[0], exception));
//            }
//        }
//    }
//}
