﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace XamlingCore.Portable.Util.Lock
{
    //Thanks to Scott Hanselman and Stephen Toubs
    //http://www.hanselman.com/blog/ComparingTwoTechniquesInNETAsynchronousCoordinationPrimitives.aspx

    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> m_releaser;

        public AsyncLock()
        {
            m_releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                        m_releaser :
                        wait.ContinueWith((_, state) => (IDisposable)state,
                            m_releaser.Result, CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;
            internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }
            public void Dispose() { m_toRelease.m_semaphore.Release(); }
        }
    }

    public static class NamedLock
    {
        private static readonly Dictionary<string, AsyncLock> Locks = new Dictionary<string, AsyncLock>();

        private static readonly AsyncLock Locker = new AsyncLock();

        static SemaphoreSlim msr = new SemaphoreSlim(1);

        public static AsyncLock Get(string name)
        {
            if (Locks.ContainsKey(name))
            {
                return Locks[name];
            }

            msr.Wait();


            if (Locks.ContainsKey(name))
            {
                return Locks[name];
            }

            var newLock = new AsyncLock();
            Locks.Add(name, newLock);

            msr.Release();

            return newLock;
        }

        public static async Task Clear()
        {
            msr.Wait();
            Locks.Clear();
            msr.Release();
        }
    }
}
