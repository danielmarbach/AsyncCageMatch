using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Transactions;

namespace AsyncMagic
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DoIt().GetAwaiter().GetResult();
            }
            catch
            {
            }

            var stopWatch = new Stopwatch();
            Console.WriteLine(GC.GetTotalMemory(false));
            stopWatch.Start();
            try
            {
                DoIt().GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            stopWatch.Stop();
            Console.WriteLine(GC.GetTotalMemory(false));
            Console.WriteLine();
            Console.WriteLine(stopWatch.Elapsed);
            Console.ReadLine();
        }

        private static Task DoIt()
        {
            var behaviors = new List<IBehavior>
            {
                new DelayBehavior1(),
                new DelayBehavior2(),
                new DelayBehavior3(),
                new DelayBehavior4(),
                new DelayInUsingBehavior1(),
                new PassThroughBehavior1(),
                new DelayTwiceBehavior1(),
                new DelayTwiceBehavior2(),
                new DelayTwiceBehavior3(),
                new DelayTwiceBehavior4(),
                new DelayInUsingBehavior2(),
                new PassThroughBehavior2(),
                new DelayBehavior5(),
                new DelayBehavior6(),
                new DelayBehavior7(),
                new DelayBehavior8(),
                new ThrowExceptionBehavior()
            };

            var context = new BehaviorContext();
            var chain = new BehaviorChain(behaviors);
            return chain.Invoke(context);
        }
    }

    public class BehaviorChain
    {
        private readonly List<IBehavior> behaviors;

        public BehaviorChain(IEnumerable<IBehavior> behaviors)
        {
            this.behaviors = behaviors.ToList();
        }

        public async Task Invoke(BehaviorContext context)
        {
            var continuations = new Stack<BehaviorContinuation>();
            ExceptionDispatchInfo dispatchInfo = null;
            foreach (var behavior in behaviors)
            {
                var continuation = BehaviorContinuation.Empty;
                try
                {
                    continuation = await behavior.Invoke(context).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    dispatchInfo = ExceptionDispatchInfo.Capture(e);
                    break;
                }
                finally
                {
                    continuations.Push(continuation);
                }
            }

            foreach (var continuation in continuations)
            {
                if (dispatchInfo == null)
                {
                    await continuation.After().ConfigureAwait(false);
                }
                else
                {
                    try
                    {
                        if (continuation.Catch != null)
                        {
                            await continuation.Catch(dispatchInfo).ConfigureAwait(false);
                            dispatchInfo = null;
                        }
                    }
                    catch (Exception e)
                    {
                        dispatchInfo = ExceptionDispatchInfo.Capture(e);
                    }
                }

                await continuation.Finally().ConfigureAwait(false);
            }

            dispatchInfo?.Throw();
        }
    }

    public class BehaviorContinuation
    {
        public Func<Task> After { get; set; } = () => Task.CompletedTask;
        public Func<Task> Finally { get; set; } = () => Task.CompletedTask;

        public Func<ExceptionDispatchInfo, Task> Catch { get; set; }

        public static BehaviorContinuation Empty = new BehaviorContinuation();

        public static Task<BehaviorContinuation> Completed = Empty;

        public static implicit operator Task<BehaviorContinuation>(BehaviorContinuation continuation)
        {
            return Task.FromResult(continuation);
        }
    }

    public interface IBehavior
    {
        Task<BehaviorContinuation> Invoke(BehaviorContext context);
    }

    public class DelayBehavior1 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return BehaviorContinuation.Empty;
        }
    }

    public class DelayBehavior2 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return BehaviorContinuation.Empty;
        }
    }

    public class DelayBehavior3 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return BehaviorContinuation.Empty;
        }
    }

    public class DelayBehavior4 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return BehaviorContinuation.Empty;
        }
    }

    public class DelayBehavior5 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return BehaviorContinuation.Empty;
        }
    }

    public class DelayBehavior6 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return BehaviorContinuation.Empty;
        }
    }

    public class DelayBehavior7 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return BehaviorContinuation.Empty;
        }
    }

    public class DelayBehavior8 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return BehaviorContinuation.Empty;
        }
    }

    public class DelayInUsingBehavior1 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            await Task.Delay(10).ConfigureAwait(false);

            return new BehaviorContinuation
            {
                After = () =>
                {
                    scope.Complete();
                    return Task.CompletedTask;
                },
                Finally = () =>
                {
                    scope.Dispose();
                    return Task.CompletedTask;
                },
            };
        }
    }

    public class DelayInUsingBehavior2 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            await Task.Delay(10).ConfigureAwait(false);

            return new BehaviorContinuation
            {
                After = () =>
                {
                    scope.Complete();
                    return Task.CompletedTask;
                },
                Finally = () =>
                {
                    scope.Dispose();
                    return Task.CompletedTask;
                },
            };
        }
    }

    public class DelayTwiceBehavior1 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return new BehaviorContinuation()
            {
                After = () => Task.Delay(10)
            };
        }
    }

    public class DelayTwiceBehavior2 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return new BehaviorContinuation()
            {
                After = () => Task.Delay(10)
            };
        }
    }

    public class DelayTwiceBehavior3 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return new BehaviorContinuation()
            {
                After = () => Task.Delay(10)
            };
        }
    }

    public class DelayTwiceBehavior4 : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            return new BehaviorContinuation()
            {
                After = () => Task.Delay(10)
            };
        }
    }

    public class ThrowExceptionBehavior : IBehavior
    {
        public async Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            throw new InvalidOperationException(nameof(ThrowExceptionBehavior));
        }
    }

    public class PassThroughBehavior1 : IBehavior
    {
        public Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            return BehaviorContinuation.Completed;
        }
    }

    public class PassThroughBehavior2 : IBehavior
    {
        public Task<BehaviorContinuation> Invoke(BehaviorContext context)
        {
            return BehaviorContinuation.Completed;
        }
    }

    public class BehaviorContext
    {
    }
}
