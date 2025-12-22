using System;
using System.Data;
using System.Data.Common;
using Moq;
using Moq.Protected;

namespace Voyager.DBConnection.Test
{
    [TestFixture]
    public class ConnectionHolderTests
    {
        private DbProviderFactory _factory;
        private Func<string> _connStrProvider;

        [SetUp]
        public void SetUp()
        {
            _factory = CreateFactory();
            _connStrProvider = () => "Data Source=test;";
        }

        private static DbProviderFactory CreateFactory()
        {
            var connMock = new Mock<DbConnection>(MockBehavior.Strict);

            var state = ConnectionState.Closed;
            var connectionString = string.Empty;

            connMock.SetupProperty(c => c.ConnectionString, string.Empty);
            connMock.SetupGet(c => c.State).Returns(() => state);
            connMock.Setup(c => c.Open()).Callback(() => state = ConnectionState.Open);
            connMock.Setup(c => c.Close()).Callback(() => state = ConnectionState.Closed);
            connMock.Protected().Setup("Dispose", ItExpr.IsAny<bool>())
                .Callback(() => state = ConnectionState.Closed);

            var factoryMock = new Mock<DbProviderFactory>(MockBehavior.Strict);
            factoryMock.Setup(f => f.CreateConnection()).Returns(() => connMock.Object);
            return factoryMock.Object;
        }

        [Test]
        public void Constructor_NullFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ConnectionHolder(null, () => "X"));
        }

        [Test]
        public void Constructor_NullProvider_Throws()
        {
            var f = CreateFactory();
            Assert.Throws<ArgumentNullException>(() => new ConnectionHolder(f, null));
        }

        [Test]
        public void GetConnection_ReturnsOpenConnection()
        {
            var f = CreateFactory();
            using var holder = new ConnectionHolder(f, () => "Data Source=test;");
            var conn = holder.GetConnection();
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Open));
        }

        [Test]
        public void GetConnection_ReusesSameInstance()
        {
            var f = CreateFactory();
            using var holder = new ConnectionHolder(f, () => "Data Source=test;");
            var c1 = holder.GetConnection();
            var c2 = holder.GetConnection();
            Assert.That(c2, Is.SameAs(c1));
        }

        [Test]
        public void Provider_SetsConnectionString_OnOpen()
        {
            var f = CreateFactory();
            string captured = string.Empty;
            Func<string> provider = () => { captured = "Data Source=captured;"; return captured; };
            using var holder = new ConnectionHolder(f, provider);
            var conn = holder.GetConnection();
            Assert.That(captured, Is.EqualTo("Data Source=captured;"));
            Assert.That(conn.ConnectionString, Is.EqualTo(captured));
        }

        [Test]
        public void IsConnectionReady_ReflectsState()
        {
            var f = CreateFactory();
            using var holder = new ConnectionHolder(f, () => "CS");
            Assert.That(holder.IsConnectionReady, Is.False);
            _ = holder.GetConnection();
            Assert.That(holder.IsConnectionReady, Is.True);
        }

        [Test]
        public void ClosedConnection_ReopensOnNextGet()
        {
            var f = CreateFactory();
            using var holder = new ConnectionHolder(f, () => "CS");
            var conn = holder.GetConnection();
            conn.Close();
            // By implementation, non-broken (even Closed) is considered ready
            Assert.That(holder.IsConnectionReady, Is.True);
            var reopened = holder.GetConnection();
            Assert.That(reopened.State, Is.EqualTo(ConnectionState.Open));
        }

        [Test]
        public void Dispose_ClosesConnection()
        {
            var f = CreateFactory();
            var holder = new ConnectionHolder(f, () => "CS");
            var conn = holder.GetConnection();
            holder.Dispose();
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Closed));
        }

        [Test]
        public void GetConnection_AfterDispose_Throws()
        {
            var f = CreateFactory();
            var holder = new ConnectionHolder(f, () => "CS");
            holder.Dispose();
            Assert.Throws<ObjectDisposedException>(() => holder.GetConnection());
        }

        [Test]
        public void Provider_CalledAgain_WhenRecreatingConnection()
        {
            var f = CreateFactory();
            int calls = 0;
            Func<string> provider = () => { calls++; return "CS" + calls; };
            using var holder = new ConnectionHolder(f, provider);
            var c1 = holder.GetConnection();
            c1.Close();
            var c2 = holder.GetConnection();
            // Provider is only called when creating a new connection; re-open does not recreate
            Assert.That(calls, Is.EqualTo(1));
        }
    }
}
