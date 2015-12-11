using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AsyncOnly
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DoIt().GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            Console.WriteLine();

            var stopWatch = new Stopwatch();
            Console.WriteLine(GC.GetTotalMemory(false));
            Console.WriteLine();
            stopWatch.Start();

            Parallel.For(0, 100, new ParallelOptions { MaxDegreeOfParallelism = 5 }, i =>
              {
                  try
                  {
                      DoIt().GetAwaiter().GetResult();
                  }
                  catch
                  {
                  }

              });
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

        public Task Invoke(BehaviorContext context)
        {
            return InvokeNext(context, 0);
        }

        Task InvokeNext(BehaviorContext context, int currentIndex)
        {
            if (currentIndex == behaviors.Count)
            {
                return Task.CompletedTask;
            }

            var behavior = behaviors[currentIndex];

            return behavior.Invoke(context, newContext => InvokeNext(newContext, currentIndex + 1));
        }
    }

    public interface IBehavior
    {
        Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next);
    }

    public class DelayBehavior1 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);
        }
    }

    public class DelayBehavior2 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);
        }
    }

    public class DelayBehavior3 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context);
        }
    }

    public class DelayBehavior4 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);
        }
    }

    public class DelayBehavior5 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);
        }
    }

    public class DelayBehavior6 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);
        }
    }

    public class DelayBehavior7 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);
        }
    }

    public class DelayBehavior8 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);
        }
    }

    public class DelayInUsingBehavior1 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await Task.Delay(10).ConfigureAwait(false);

                await next(context).ConfigureAwait(false);

                scope.Complete();
            }
        }
    }

    public class DelayInUsingBehavior2 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await Task.Delay(10).ConfigureAwait(false);

                await next(context).ConfigureAwait(false);

                scope.Complete();
            }
        }
    }

    public class DelayTwiceBehavior1 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DelayTwiceBehavior2 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DelayTwiceBehavior3 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DelayTwiceBehavior4 : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class ThrowExceptionBehavior : IBehavior
    {
        public async Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);

            throw new InvalidOperationException(nameof(ThrowExceptionBehavior));
        }
    }

    public class PassThroughBehavior1 : IBehavior
    {
        public Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            return next(context);
        }
    }

    public class PassThroughBehavior2 : IBehavior
    {
        public Task Invoke(BehaviorContext context, Func<BehaviorContext, Task> next)
        {
            return next(context);
        }
    }

    public class BehaviorContext
    {
    }
}
