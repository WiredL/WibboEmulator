namespace WibboEmulator.WebSocketSharp.Server;

#region License
/*
 * WebSocketServiceManager.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2021 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using WibboEmulator.WebSocketSharp;

/// <summary>
/// Provides the management function for the WebSocket services.
/// </summary>
/// <remarks>
/// This class manages the WebSocket services provided by
/// the <see cref="WebSocketServer"/> or <see cref="HttpServer"/> class.
/// </remarks>
public class WebSocketServiceManager
{
    #region Private Fields

    private readonly Dictionary<string, WebSocketServiceHost> _hosts;
    private volatile bool _keepClean;
    private readonly Logger _log;
    private volatile ServerState _state;
    private readonly object _sync;
    private TimeSpan _waitTime;

    #endregion

    #region Internal Constructors

    internal WebSocketServiceManager(Logger log)
    {
        this._log = log;

        this._hosts = [];
        this._keepClean = true;
        this._state = ServerState.Ready;
        this._sync = ((ICollection)this._hosts).SyncRoot;
        this._waitTime = TimeSpan.FromSeconds(1);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the number of the WebSocket services.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> that represents the number of the services.
    /// </value>
    public int Count
    {
        get
        {
            lock (this._sync)
            {
                return this._hosts.Count;
            }
        }
    }

    /// <summary>
    /// Gets the service host instances for the WebSocket services.
    /// </summary>
    /// <value>
    ///   <para>
    ///   An <c>IEnumerable&lt;WebSocketServiceHost&gt;</c> instance.
    ///   </para>
    ///   <para>
    ///   It provides an enumerator which supports the iteration over
    ///   the collection of the service host instances.
    ///   </para>
    /// </value>
    public IEnumerable<WebSocketServiceHost> Hosts
    {
        get
        {
            lock (this._sync)
            {
                return [.. this._hosts.Values];
            }
        }
    }

    /// <summary>
    /// Gets the service host instance for a WebSocket service with
    /// the specified path.
    /// </summary>
    /// <value>
    ///   <para>
    ///   A <see cref="WebSocketServiceHost"/> instance or
    ///   <see langword="null"/> if not found.
    ///   </para>
    ///   <para>
    ///   The service host instance provides the function to access
    ///   the information in the service.
    ///   </para>
    /// </value>
    /// <param name="path">
    ///   <para>
    ///   A <see cref="string"/> that specifies an absolute path to
    ///   the service to find.
    ///   </para>
    ///   <para>
    ///   / is trimmed from the end of the string if present.
    ///   </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="path"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is not an absolute path.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> includes either or both
    ///   query and fragment components.
    ///   </para>
    /// </exception>
    public WebSocketServiceHost this[string path]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(path);

            if (path.Length == 0)
            {
                throw new ArgumentException("An empty string.", nameof(path));
            }

            if (path[0] != '/')
            {
                var msg = "It is not an absolute path.";

                throw new ArgumentException(msg, nameof(path));
            }

            if (path.IndexOfAny(['?', '#']) > -1)
            {
                var msg = "It includes either or both query and fragment components.";

                throw new ArgumentException(msg, nameof(path));
            }


            _ = this.InternalTryGetServiceHost(path, out var host);

            return host;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the inactive sessions in
    /// the WebSocket services are cleaned up periodically.
    /// </summary>
    /// <remarks>
    /// The set operation does nothing if the server has already started or
    /// it is shutting down.
    /// </remarks>
    /// <value>
    ///   <para>
    ///   <c>true</c> if the inactive sessions are cleaned up every 60
    ///   seconds; otherwise, <c>false</c>.
    ///   </para>
    ///   <para>
    ///   The default value is <c>true</c>.
    ///   </para>
    /// </value>
    public bool KeepClean
    {
        get => this._keepClean;

        set
        {
            lock (this._sync)
            {
                if (!this.CanSet())
                {
                    return;
                }

                foreach (var host in this._hosts.Values)
                {
                    host.KeepClean = value;
                }

                this._keepClean = value;
            }
        }
    }

    /// <summary>
    /// Gets the paths for the WebSocket services.
    /// </summary>
    /// <value>
    ///   <para>
    ///   An <c>IEnumerable&lt;string&gt;</c> instance.
    ///   </para>
    ///   <para>
    ///   It provides an enumerator which supports the iteration over
    ///   the collection of the paths.
    ///   </para>
    /// </value>
    public IEnumerable<string> Paths
    {
        get
        {
            lock (this._sync)
            {
                return [.. this._hosts.Keys];
            }
        }
    }

    /// <summary>
    /// Gets or sets the time to wait for the response to the WebSocket Ping
    /// or Close.
    /// </summary>
    /// <remarks>
    /// The set operation does nothing if the server has already started or
    /// it is shutting down.
    /// </remarks>
    /// <value>
    ///   <para>
    ///   A <see cref="TimeSpan"/> to wait for the response.
    ///   </para>
    ///   <para>
    ///   The default value is the same as 1 second.
    ///   </para>
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value specified for a set operation is zero or less.
    /// </exception>
    public TimeSpan WaitTime
    {
        get => this._waitTime;

        set
        {
            if (value <= TimeSpan.Zero)
            {
                var msg = "It is zero or less.";

                throw new ArgumentOutOfRangeException(nameof(value), msg);
            }

            lock (this._sync)
            {
                if (!this.CanSet())
                {
                    return;
                }

                foreach (var host in this._hosts.Values)
                {
                    host.WaitTime = value;
                }

                this._waitTime = value;
            }
        }
    }

    #endregion

    #region Private Methods

    private bool CanSet() => this._state is ServerState.Ready or ServerState.Stop;

    #endregion

    #region Internal Methods

    internal bool InternalTryGetServiceHost(
      string path, out WebSocketServiceHost host
    )
    {
        path = path.TrimSlashFromEnd();

        lock (this._sync)
        {
            return this._hosts.TryGetValue(path, out host);
        }
    }

    internal void Start()
    {
        lock (this._sync)
        {
            foreach (var host in this._hosts.Values)
            {
                host.Start();
            }

            this._state = ServerState.Start;
        }
    }

    internal void Stop(ushort code, string reason)
    {
        lock (this._sync)
        {
            this._state = ServerState.ShuttingDown;

            foreach (var host in this._hosts.Values)
            {
                host.Stop(code, reason);
            }

            this._state = ServerState.Stop;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a WebSocket service with the specified behavior, path,
    /// and delegate.
    /// </summary>
    /// <param name="path">
    ///   <para>
    ///   A <see cref="string"/> that specifies an absolute path to
    ///   the service to add.
    ///   </para>
    ///   <para>
    ///   / is trimmed from the end of the string if present.
    ///   </para>
    /// </param>
    /// <param name="initializer">
    ///   <para>
    ///   An <c>Action&lt;TBehavior&gt;</c> delegate or
    ///   <see langword="null"/> if not needed.
    ///   </para>
    ///   <para>
    ///   The delegate invokes the method called when initializing
    ///   a new session instance for the service.
    ///   </para>
    /// </param>
    /// <typeparam name="TBehavior">
    ///   <para>
    ///   The type of the behavior for the service.
    ///   </para>
    ///   <para>
    ///   It must inherit the <see cref="WebSocketBehavior"/> class.
    ///   </para>
    ///   <para>
    ///   And also, it must have a public parameterless constructor.
    ///   </para>
    /// </typeparam>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="path"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is not an absolute path.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> includes either or both
    ///   query and fragment components.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is already in use.
    ///   </para>
    /// </exception>
    public void AddService<TBehavior>(
      string path, Action<TBehavior> initializer
    )
      where TBehavior : WebSocketBehavior, new()
    {
        ArgumentNullException.ThrowIfNull(path);

        if (path.Length == 0)
        {
            throw new ArgumentException("An empty string.", nameof(path));
        }

        if (path[0] != '/')
        {
            var msg = "It is not an absolute path.";

            throw new ArgumentException(msg, nameof(path));
        }

        if (path.IndexOfAny(['?', '#']) > -1)
        {
            var msg = "It includes either or both query and fragment components.";

            throw new ArgumentException(msg, nameof(path));
        }

        path = path.TrimSlashFromEnd();

        lock (this._sync)
        {

            if (this._hosts.TryGetValue(path, out var host))
            {
                var msg = "It is already in use.";

                throw new ArgumentException(msg, nameof(path));
            }

            host = new WebSocketServiceHost<TBehavior>(path, initializer, this._log);

            if (!this._keepClean)
            {
                host.KeepClean = false;
            }

            if (this._waitTime != host.WaitTime)
            {
                host.WaitTime = this._waitTime;
            }

            if (this._state == ServerState.Start)
            {
                host.Start();
            }

            this._hosts.Add(path, host);
        }
    }

    /// <summary>
    /// Removes all WebSocket services managed by the manager.
    /// </summary>
    /// <remarks>
    /// A service is stopped with close status 1001 (going away)
    /// if it has already started.
    /// </remarks>
    public void Clear()
    {
        List<WebSocketServiceHost> hosts = null;

        lock (this._sync)
        {
            hosts = [.. this._hosts.Values];

            this._hosts.Clear();
        }

        foreach (var host in hosts)
        {
            if (host.State == ServerState.Start)
            {
                host.Stop(1001, string.Empty);
            }
        }
    }

    /// <summary>
    /// Removes a WebSocket service with the specified path.
    /// </summary>
    /// <remarks>
    /// The service is stopped with close status 1001 (going away)
    /// if it has already started.
    /// </remarks>
    /// <returns>
    /// <c>true</c> if the service is successfully found and removed;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <param name="path">
    ///   <para>
    ///   A <see cref="string"/> that specifies an absolute path to
    ///   the service to remove.
    ///   </para>
    ///   <para>
    ///   / is trimmed from the end of the string if present.
    ///   </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="path"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is not an absolute path.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> includes either or both
    ///   query and fragment components.
    ///   </para>
    /// </exception>
    public bool RemoveService(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (path.Length == 0)
        {
            throw new ArgumentException("An empty string.", nameof(path));
        }

        if (path[0] != '/')
        {
            var msg = "It is not an absolute path.";

            throw new ArgumentException(msg, nameof(path));
        }

        if (path.IndexOfAny(['?', '#']) > -1)
        {
            var msg = "It includes either or both query and fragment components.";

            throw new ArgumentException(msg, nameof(path));
        }

        path = path.TrimSlashFromEnd();
        WebSocketServiceHost host;

        lock (this._sync)
        {
            if (!this._hosts.TryGetValue(path, out host))
            {
                return false;
            }

            _ = this._hosts.Remove(path);
        }

        if (host.State == ServerState.Start)
        {
            host.Stop(1001, string.Empty);
        }

        return true;
    }

    /// <summary>
    /// Tries to get the service host instance for a WebSocket service with
    /// the specified path.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the service is successfully found; otherwise,
    /// <c>false</c>.
    /// </returns>
    /// <param name="path">
    ///   <para>
    ///   A <see cref="string"/> that specifies an absolute path to
    ///   the service to find.
    ///   </para>
    ///   <para>
    ///   / is trimmed from the end of the string if present.
    ///   </para>
    /// </param>
    /// <param name="host">
    ///   <para>
    ///   When this method returns, a <see cref="WebSocketServiceHost"/>
    ///   instance or <see langword="null"/> if not found.
    ///   </para>
    ///   <para>
    ///   The service host instance provides the function to access
    ///   the information in the service.
    ///   </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="path"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is not an absolute path.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> includes either or both
    ///   query and fragment components.
    ///   </para>
    /// </exception>
    public bool TryGetServiceHost(string path, out WebSocketServiceHost host)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (path.Length == 0)
        {
            throw new ArgumentException("An empty string.", nameof(path));
        }

        if (path[0] != '/')
        {
            var msg = "It is not an absolute path.";

            throw new ArgumentException(msg, nameof(path));
        }

        if (path.IndexOfAny(['?', '#']) > -1)
        {
            var msg = "It includes either or both query and fragment components.";

            throw new ArgumentException(msg, nameof(path));
        }

        return this.InternalTryGetServiceHost(path, out host);
    }

    #endregion
}
