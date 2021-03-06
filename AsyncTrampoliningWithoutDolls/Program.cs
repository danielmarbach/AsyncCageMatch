﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace AsyncTrampolining
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

    public static class Trampoline
    {
        public static Func<T1, T2, Task> MakeTrampoline<T1, T2>(Func<T1, T2, Task<Bounce<T1, T2>>> function)
        {
            Func<T1, T2, Task> trampolined = async (T1 arg1, T2 arg2) =>
            {
                T1 currentArg1 = arg1;
                T2 currentArg2 = arg2;

                while (true)
                {
                    Bounce<T1, T2> result = await function(currentArg1, currentArg2);

                    if (result.HasResult)
                    {
                        return;
                    }

                    currentArg1 = result.Param1;
                    currentArg2 = result.Param2;
                }
            };

            return trampolined;
        }


        public static Bounce<T1, T2> Recurse<T1, T2>(T1 arg1, T2 arg2)
        {
            return new Bounce<T1, T2>(arg1, arg2);
        }

        public static Bounce<T1, T2> ReturnResult<T1, T2>()
        {
            return new Bounce<T1, T2>("Foo");
        }

    }


    public struct Bounce<T1, T2>
    {
        public T1 Param1 { get; private set; }
        public T2 Param2 { get; private set; }

        public bool HasResult { get; private set; }
        public bool Recurse { get; private set; }

        public Bounce(T1 param1, T2 param2) : this()
        {
            Param1 = param1;
            Param2 = param2;
            HasResult = false;

            Recurse = true;

        }

        public Bounce(string foo)
        {
            HasResult = true;

            Recurse = false;

            Param1 = default(T1);
            Param2 = default(T2);
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
            return InvokeNext(context);
        }

        Task InvokeNext(BehaviorContext context)
        {
            int currentIndex = 0;
            var function = Trampoline.MakeTrampoline(async (BehaviorContext ctx, object foo) =>
            {
                if (currentIndex == behaviors.Count)
                {
                    return Trampoline.ReturnResult<BehaviorContext, object>();
                }

                var behavior = behaviors[currentIndex];
                currentIndex += 1;
                Bounce<BehaviorContext, object> result = Trampoline.ReturnResult<BehaviorContext, object>();

                try
                {
                    await behavior.Invoke(ctx);
                    result = Trampoline.Recurse(ctx, default(object));
                }
                catch (Exception e)
                {
                    // capture e on the context
                    result = Trampoline.ReturnResult<BehaviorContext, object>();
                    throw;
                }

                return result;
            });

            return function(context, null);
        }
    }

    public delegate FuncRec<T, R> FuncRec<T, R>(T t);

    public delegate Task Invoke(BehaviorContext context);

    public delegate Task Next(BehaviorContext context);

    public delegate Func<Invoke> Trampolin(Func<BehaviorContext, Invoke> invoke);

    public interface IBehavior
    {
        Task Invoke(BehaviorContext context);
    }
    public class DelayBehavior1 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

        }
    }

    public class DelayBehavior2 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

        }
    }

    public class DelayBehavior3 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

        }
    }

    public class DelayBehavior4 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

        }
    }

    public class DelayBehavior5 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

        }
    }

    public class DelayBehavior6 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DelayBehavior7 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DelayBehavior8 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DelayInUsingBehavior1 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await Task.Delay(10).ConfigureAwait(false);

                scope.Complete();
            }
        }
    }

    public class DelayInUsingBehavior2 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await Task.Delay(10).ConfigureAwait(false);

                scope.Complete();
            }
        }
    }

    public class DelayTwiceBehavior1 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DelayTwiceBehavior2 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DelayTwiceBehavior3 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DelayTwiceBehavior4 : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class ThrowExceptionBehavior : IBehavior
    {
        public async Task Invoke(BehaviorContext context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            throw new InvalidOperationException(nameof(ThrowExceptionBehavior));
        }
    }

    public class PassThroughBehavior1 : IBehavior
    {
        public Task Invoke(BehaviorContext context)
        {
            return Task.FromResult(0);
        }
    }

    public class PassThroughBehavior2 : IBehavior
    {
        public Task Invoke(BehaviorContext context)
        {
            return Task.FromResult(0);
        }
    }

    public class BehaviorContext
    {
    }
}
