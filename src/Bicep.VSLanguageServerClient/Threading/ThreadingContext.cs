// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Bicep.VSLanguageServerClient.Threading
{
    /// <summary>
    /// Implements <see cref="IThreadingContext"/>, which provides an implementation of
    /// JoinableTaskFactory to Roslyn code.
    /// </summary>
    /// <remarks>
    /// <para>The JoinableTaskFactory is constructed from the
    /// JoinableTaskContext provided by the MEF container, if available. If no
    /// JoinableTaskContext is available, a new instance is constructed using the
    /// synchronization context of the current thread as the main thread.</para>
    /// </remarks>
    [Export(typeof(IThreadingContext))]
    internal sealed class ThreadingContext : IThreadingContext, IDisposable
    {
        private readonly CancellationTokenSource _disposalTokenSource = new();

        [ImportingConstructor]
        public ThreadingContext(JoinableTaskContext joinableTaskContext)
        {
            HasMainThread = joinableTaskContext.MainThread.IsAlive;
            JoinableTaskContext = joinableTaskContext;
            JoinableTaskFactory = joinableTaskContext.Factory;
            ShutdownBlockingTasks = new JoinableTaskCollection(JoinableTaskContext);
            ShutdownBlockingTaskFactory = JoinableTaskContext.CreateFactory(ShutdownBlockingTasks);
        }

        /// <inheritdoc/>
        public bool HasMainThread
        {
            get;
        }

        /// <inheritdoc/>
        public JoinableTaskContext JoinableTaskContext
        {
            get;
        }

        /// <inheritdoc/>
        public JoinableTaskFactory JoinableTaskFactory
        {
            get;
        }

        public JoinableTaskCollection ShutdownBlockingTasks { get; }

        private JoinableTaskFactory ShutdownBlockingTaskFactory { get; }

        public CancellationToken DisposalToken => _disposalTokenSource.Token;

        public JoinableTask RunWithShutdownBlockAsync(Func<CancellationToken, Task> func)
        {
            return ShutdownBlockingTaskFactory.RunAsync(() =>
            {
                DisposalToken.ThrowIfCancellationRequested();
                return func(DisposalToken);
            });
        }

        public void Dispose()
        {
            // https://github.com/Microsoft/vs-threading/blob/main/doc/cookbook_vs.md#how-to-write-a-fire-and-forget-method-responsibly
            _disposalTokenSource.Cancel();

            try
            {
                // Block Dispose until all async work has completed.
                JoinableTaskContext.Factory.Run(ShutdownBlockingTasks.JoinTillEmptyAsync);
            }
            catch (OperationCanceledException)
            {
                // this exception is expected because we signaled the cancellation token
            }
            catch (AggregateException ex)
            {
                // ignore AggregateException containing only OperationCanceledException
                ex.Handle(inner => inner is OperationCanceledException);
            }
        }
    }
}
