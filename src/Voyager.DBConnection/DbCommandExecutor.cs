using System;
using System.Data;
using System.Data.Common;
using Voyager.Common.Results;
using Voyager.DBConnection.Events;
using Voyager.DBConnection.Interfaces;

namespace Voyager.DBConnection
{
    public class DbCommandExecutor : IDisposable, IRegisterEvents, IFeatureHost
    {
        private readonly Database db;
        private readonly IMapErrorPolicy errorPolicy;
        private readonly EventHost eventHost = new EventHost();
        private readonly FeatureHost featureHost = new FeatureHost();

        public DbCommandExecutor(Database db, IMapErrorPolicy errorPolicy)
        {
            Guard.DBPolicyGuard(errorPolicy);
            Guard.DbGuard(db);

            this.db = db;
            this.errorPolicy = errorPolicy;
        }

        public DbCommandExecutor(Database db)
        {
            Guard.DbGuard(db);
            this.db = db;
            this.errorPolicy = new DefaultMapError();
        }

        public virtual void Dispose()
        {
            this.db.Dispose();
            featureHost.Dispose();
        }

        public virtual Transaction BeginTransaction()
        {
            return db.BeginTransaction();
        }

        public virtual Result<int> ExecuteNonQuery(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
                var executionResult = ExecuteWithEvents(command, executeNonQuery);
                if (executionResult.IsSuccess)
                    afterCall?.Invoke(command);
                return executionResult;
            }
        }

        public virtual Result<TValue> ExecuteResult<TValue>(IDbCommandFactory commandFactory, Func<DbCommand, Result<TValue>> afterCall)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, int> executeNonQuery = (cmd) => cmd.ExecuteNonQuery();
                var result = ExecuteWithEvents(command, executeNonQuery)
                    .Bind(_ => afterCall.Invoke(command));
                return result;
            }
        }

        public virtual Result<object> ExecuteScalar(IDbCommandFactory commandFactory, Action<DbCommand> afterCall = null)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                Func<DbCommand, object> executeScalar = (cmd) => cmd.ExecuteScalar();
                var result = ExecuteWithEvents(command, executeScalar);
                if (result.IsSuccess)
                    afterCall?.Invoke(command);
                return result;
            }
        }

        public virtual Result<TValue> ExecuteReader<TValue>(IDbCommandFactory commandFactory, IGetConsumer<TValue> consumer, Action<DbCommand> afterCall = null)
        {
            using (DbCommand command = GetCommand(commandFactory))
            {
                var reader = GetDataReader(command);
                if (!reader.IsSuccess)
                    return reader.Error;

                var result = reader
                    .Map(reader => HandleReader(consumer, reader))
                    .Tap(_ => afterCall?.Invoke(command))
                    .Finally(() => reader.Value.Dispose());

                return result;
            }
        }

        private TDomain HandleReader<TDomain>(IGetConsumer<TDomain> consumer, IDataReader dr)
        {
            TDomain result = consumer.GetResults(dr);
            while (dr.NextResult())
            { }
            // dr.Close(); // removed — Dispose is called in Finally()
            return result;
        }
        protected virtual Result<IDataReader> GetDataReader(DbCommand command)
        {
            Func<DbCommand, IDataReader> executeReader = (cmd) => cmd.ExecuteReader();
            return ExecuteWithEvents(command, executeReader);
        }

        protected DbCommand GetCommand(IDbCommandFactory commandFactory)
        {
            return commandFactory.ConstructDbCommand(db);
        }


        private Result<TValue> ExecuteWithEvents<TValue>(DbCommand command, Func<DbCommand, TValue> action)
        {
            var proc = new ProcEvent<TValue>(this.eventHost);

            try
            {
                db.OpenCmd(command);
                return Result<TValue>.Success(proc.Call(() => action.Invoke(command), command));
            }
            catch (Exception ex)
            {
                var handled = HandleSqlException(ex);
                proc.ErrorPublish(handled);
                return handled;
            }
        }

        Common.Results.Error HandleSqlException(Exception ex)
        {
            return errorPolicy.MapError(ex);
        }

        public void AddEvent(Action<SqlCallEvent> logEvent)
        {
            eventHost.AddEvent(logEvent);
        }

        public void RemoveEvent(Action<SqlCallEvent> logEvent)
        {
            eventHost.RemoveEvent(logEvent);
        }

        public void AddFeature(IFeature feature)
        {
            featureHost.AddFeature(feature);
        }
    }
}
