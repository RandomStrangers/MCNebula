﻿/*
    Copyright 2015 MCGalaxy
        
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Collections.Generic;
using System.Threading;

namespace MCNebula
{ 
    /// <summary> Asynchronously performs work on a background thread </summary>
    public abstract class AsyncWorker<T> 
    {
        AutoResetEvent handle = new AutoResetEvent(false);
        volatile bool terminating;
        
        protected Queue<T> queue = new Queue<T>();
        protected readonly object queueLock = new object();
        
        protected abstract void HandleNext();
        /// <summary> Name to assign the worker thread </summary>
        protected abstract string ThreadName { get; }
        
        void SendLoop() {
            for (;;) {
                if (terminating) break;
                
                try {
                    HandleNext();
                } catch (Exception ex) {
                    Logger.LogError(ex);
                }
            }
            
            // cleanup state
            try {
                lock (queueLock) queue.Clear();
                handle.Close();
            } catch {
            }
        }      
        
        void WakeupWorker() {
            try {
                handle.Set();
            } catch (ObjectDisposedException) {
                // for very rare case where handle's already been destroyed
            }
        }
        
        protected void WaitForWork() { handle.WaitOne(); }
        
        
        /// <summary> Starts the background worker thread </summary>
        public void RunAsync() {
            Thread worker;
            Server.StartThread(out worker, ThreadName, SendLoop);
        }
        
        public void StopAsync() {
            terminating = true;
            WakeupWorker();
        }
        
        /// <summary> Enqueues work to be performed asynchronously </summary>
        public void QueueAsync(T msg) {
            lock (queueLock) queue.Enqueue(msg);
            WakeupWorker();
        }
    }
}
