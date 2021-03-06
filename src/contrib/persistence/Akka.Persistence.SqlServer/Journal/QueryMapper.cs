﻿using System;
using System.Data.SqlClient;

namespace Akka.Persistence.SqlServer.Journal
{
    /// <summary>
    /// Mapper used for generating persistent representations based on SQL query results.
    /// </summary>
    public interface IJournalQueryMapper
    {
        /// <summary>
        /// Takes a current row from the SQL data reader and produces a persistent representation object in result.
        /// It's not supposed to move reader's cursor in any way.
        /// </summary>
        IPersistentRepresentation Map(SqlDataReader reader);
    }

    internal class DefaultJournalQueryMapper : IJournalQueryMapper
    {
        private readonly Akka.Serialization.Serialization _serialization;

        public DefaultJournalQueryMapper(Akka.Serialization.Serialization serialization)
        {
            _serialization = serialization;
        }

        public IPersistentRepresentation Map(SqlDataReader reader)
        {
            var persistenceId = reader.GetString(0);
            var sequenceNr = reader.GetInt64(1);
            var isDeleted = reader.GetBoolean(2);
            var payload = GetPayload(reader);

            return new Persistent(payload, sequenceNr, persistenceId, isDeleted);
        }

        private object GetPayload(SqlDataReader reader)
        {
            var payloadType = reader.GetString(3);
            var type = Type.GetType(payloadType, true);
            var binary = (byte[]) reader[4];

            var serializer = _serialization.FindSerializerForType(type);
            return serializer.FromBinary(binary, type);
        }
    }
}