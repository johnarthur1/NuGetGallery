﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WebBackgrounder;

namespace NuGetGallery.Infrastructure.Jobs
{
    internal class NuGetJobCoordinator : IJobCoordinator
    {
        public void Dispose()
        {
        }

        public Task GetWork(IJob job)
        {
            Task task;
            try
            {
                task = job.Execute();
            }
            catch (Exception exception)
            {
                Exception e = exception;
                task = new Task(delegate
                {
                    throw e;
                });
            }

            var continuation = task.ContinueWith(t =>
            {
                if (task.IsCanceled)
                {
                    Debug.WriteLine("IsCanceled: " + job.GetType());
                }
                else if (task.IsFaulted)
                {
                    Debug.WriteLine("IsFaulted: " + job.GetType());
                }
                else if (task.IsCompleted)
                {
                    Debug.WriteLine("IsCompleted: " + job.GetType());
                }
            }, TaskContinuationOptions.ExecuteSynchronously);

            var wrapper = new Task(() =>
            {
                task.Start();
                continuation.Wait();
            });

            return wrapper;
        }
    }
}