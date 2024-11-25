﻿using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace NServer.Core.Interfaces.Database
{
    internal interface IDatabaseConnection
    {
        void OpenConnection(CancellationToken cancellationToken = default);
        Task<int> ExecuteNonQueryAsync(string query, params object[] parameters);
        Task<T> ExecuteScalarAsync<T>(string query, params object[] parameters);
    }
}