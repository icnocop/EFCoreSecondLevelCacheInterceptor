using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

public partial class SecondLevelCacheInterceptorBasicTests
{
    private class UtcDateTimeConvertingDbCommandInterceptor : DbCommandInterceptor
    {
        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            if (result != null && !(result is UtcDateTimeConvertingDbDataReader))
            {
                return new UtcDateTimeConvertingDbDataReader(result);
            }

            return base.ReaderExecuted(command, eventData, result);
        }

        private class UtcDateTimeConvertingDbDataReader : DelegatingDbDataReader
        {
            public UtcDateTimeConvertingDbDataReader(DbDataReader source)
                : base(source)
            {
            }

            public override DateTime GetDateTime(int ordinal)
            {
                return DateTime.SpecifyKind(base.GetDateTime(ordinal), DateTimeKind.Utc);
            }
        }

        internal abstract class DelegatingDbDataReader : DbDataReader
        {
            private readonly DbDataReader source;

            public DelegatingDbDataReader(DbDataReader source)
            {
                this.source = source;
            }

            public override int Depth
            {
                get { return this.source.Depth; }
            }

            public override int FieldCount
            {
                get { return this.source.FieldCount; }
            }

            public override bool HasRows
            {
                get { return this.source.HasRows; }
            }

            public override bool IsClosed
            {
                get { return this.source.IsClosed; }
            }

            public override int RecordsAffected
            {
                get { return this.source.RecordsAffected; }
            }

            public override int VisibleFieldCount
            {
                get { return this.source.VisibleFieldCount; }
            }

            public override object this[string name]
            {
                get { return this.source[name]; }
            }

            public override object this[int ordinal]
            {
                get { return this.source[ordinal]; }
            }

            public override bool GetBoolean(int ordinal)
            {
                return this.source.GetBoolean(ordinal);
            }

            public override byte GetByte(int ordinal)
            {
                return this.source.GetByte(ordinal);
            }

            public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
            {
                return this.source.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
            }

            public override char GetChar(int ordinal)
            {
                return this.source.GetChar(ordinal);
            }

            public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
            {
                return this.source.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
            }

            public override string GetDataTypeName(int ordinal)
            {
                return this.source.GetDataTypeName(ordinal);
            }

            public override DateTime GetDateTime(int ordinal)
            {
                return this.source.GetDateTime(ordinal);
            }

            public override decimal GetDecimal(int ordinal)
            {
                return this.source.GetDecimal(ordinal);
            }

            public override double GetDouble(int ordinal)
            {
                return this.source.GetDouble(ordinal);
            }

            public override IEnumerator GetEnumerator()
            {
                return this.source.GetEnumerator();
            }

            public override Type GetFieldType(int ordinal)
            {
                return this.source.GetFieldType(ordinal);
            }

            public override float GetFloat(int ordinal)
            {
                return this.source.GetFloat(ordinal);
            }

            public override Guid GetGuid(int ordinal)
            {
                return this.source.GetGuid(ordinal);
            }

            public override short GetInt16(int ordinal)
            {
                return this.source.GetInt16(ordinal);
            }

            public override int GetInt32(int ordinal)
            {
                return this.source.GetInt32(ordinal);
            }

            public override long GetInt64(int ordinal)
            {
                return this.source.GetInt64(ordinal);
            }

            public override string GetName(int ordinal)
            {
                return this.source.GetName(ordinal);
            }

            public override int GetOrdinal(string name)
            {
                return this.source.GetOrdinal(name);
            }

            public override string GetString(int ordinal)
            {
                return this.source.GetString(ordinal);
            }

            public override object GetValue(int ordinal)
            {
                return this.source.GetValue(ordinal);
            }

            public override int GetValues(object[] values)
            {
                return this.source.GetValues(values);
            }

            public override bool IsDBNull(int ordinal)
            {
                return this.source.IsDBNull(ordinal);
            }

            public override bool NextResult()
            {
                return this.source.NextResult();
            }

            public override bool Read()
            {
                return this.source.Read();
            }

            public override void Close()
            {
                this.source.Close();
            }

            public override T GetFieldValue<T>(int ordinal)
            {
                return this.source.GetFieldValue<T>(ordinal);
            }

            public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
            {
                return this.source.GetFieldValueAsync<T>(ordinal, cancellationToken);
            }

            public override Type GetProviderSpecificFieldType(int ordinal)
            {
                return this.source.GetProviderSpecificFieldType(ordinal);
            }

            public override object GetProviderSpecificValue(int ordinal)
            {
                return this.source.GetProviderSpecificValue(ordinal);
            }

            public override int GetProviderSpecificValues(object[] values)
            {
                return this.source.GetProviderSpecificValues(values);
            }

            public override DataTable GetSchemaTable()
            {
                return this.source.GetSchemaTable();
            }

            public override Stream GetStream(int ordinal)
            {
                return this.source.GetStream(ordinal);
            }

            public override TextReader GetTextReader(int ordinal)
            {
                return this.source.GetTextReader(ordinal);
            }

            public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
            {
                return this.source.IsDBNullAsync(ordinal, cancellationToken);
            }

            public override Task<bool> ReadAsync(CancellationToken cancellationToken)
            {
                return this.source.ReadAsync(cancellationToken);
            }

            protected override DbDataReader GetDbDataReader(int ordinal)
            {
                return this.source.GetData(ordinal);
            }
        }
    }
}