using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AsyncPartialDolls
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

            //Parallel.For(0, 100, new ParallelOptions { MaxDegreeOfParallelism = 5 }, i =>
            //{
            //    try
            //    {
            //        DoIt().GetAwaiter().GetResult();
            //    }
            //    catch
            //    {
            //    }

            //});
            stopWatch.Stop();
            Console.WriteLine(GC.GetTotalMemory(false));
            Console.WriteLine();
            Console.WriteLine(stopWatch.Elapsed);
            Console.ReadLine();
        }

        private static Task DoIt()
        {
            var behaviors = new List<IBehavior<Parent>>
            {
                new DoSomethingBeforeNextForParent(),
                new DoSomethingAfterNextForParent(),
                new SurroundNextForParent(),
                new DoSomethingBeforeNextForParent(),
                new DoSomethingAfterNextForParent(),
                new DoSomethingBeforeNextForParent(),
                new DoSomethingAfterNextForParent(),
                new DoSomethingBeforeNextForParent(),
                new DoSomethingAfterNextForParent(),
                new DoSomethingBeforeNextForParent(),
                new DoSomethingAfterNextForParent(),
                new DoSomethingBeforeNextForParent(),
                new DoSomethingAfterNextForParent(),
                new DoSomethingBeforeNextForParent(),
                new DoSomethingAfterNextForParent(),
                new DoSomethingBeforeNextForParent(),
                new DoSomethingAfterNextForParent(),
                new ThrowBehavior()
            };

            var context = new Parent();
            var chain = new BehaviorChain(behaviors);
            return chain.Invoke(context);
        }
    }

    public class BehaviorChain
    {
        private readonly List<IBehavior<Parent>> behaviors;

        public BehaviorChain(IEnumerable<IBehavior<Parent>> behaviors)
        {
            this.behaviors = behaviors.ToList();
        }

        public Task Invoke(Parent context)
        {
            return InvokeNext(context, 0);
        }

        Task InvokeNext(Parent context, int currentIndex)
        {
            if (currentIndex == behaviors.Count)
            {
                return Task.CompletedTask;
            }

            var behavior = behaviors[currentIndex];

            return behavior.Invoke(context, newContext => InvokeNext(newContext, currentIndex + 1));
        }
    }

    public interface IBehavior { }
 

    public interface IBeforeBehavior<in TContext> : IBehavior
        where TContext : Parent
    {
        Task Invoke(TContext context);
    }

    public interface IAfterBehavior<in TContext> : IBehavior
        where TContext : Parent
    {
        Task Invoke(TContext context);
    }

    public interface ISurroundBehavior<TContext> : IBehavior
        where TContext : Parent
    {
        Task Invoke(TContext context, Func<TContext, Task> next);
    }

    public interface IBehavior<TContext>
        where TContext : Parent
    {
        Task Invoke(TContext context, Func<TContext, Task> next);
    }

    public abstract class Behavior : IBehavior<Parent>
    {

        async Task IBehavior<Parent>.Invoke(Parent context, Func<Parent, Task> next)
        {
            var interfaces = GetType().GetInterfaces();
            if (interfaces.Any(t => t.Name.StartsWith("IAfterBehavior")))
            {
                await next(context).ConfigureAwait(false);
                var behaviorInterface = this.GetType().GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAfterBehavior<>));
                var contextType = behaviorInterface.GetGenericArguments()[0];
                var methodInfo = behaviorInterface.GetMethods().Single(m => m.Name == "Invoke" && m.GetGenericArguments()[0] == contextType);
                var target = Expression.Parameter(typeof(object));
                var contextParam = Expression.Parameter(typeof(object));

                var castTarget = Expression.Convert(target, behaviorInterface);

                var methodParameters = methodInfo.GetParameters();
                var contextCastParam = Expression.Convert(contextParam, contextType);

                Expression body = Expression.Call(castTarget, methodInfo, contextCastParam);

                var execute = Expression.Lambda<Func<object, object, Task>>(body, target, contextParam).Compile();
                await execute(this, context).ConfigureAwait(false);
                return;
            }

            if (interfaces.Any(t => t.Name.StartsWith("IBeforeBehavior")))
            {
                var behaviorInterface = this.GetType().GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IBeforeBehavior<>));
                var contextType = behaviorInterface.GetGenericArguments()[0];
                var methodInfo = behaviorInterface.GetMethods().Single(m => m.Name == "Invoke" && m.GetParameters()[0].ParameterType == contextType);
                var target = Expression.Parameter(typeof(object));
                var contextParam = Expression.Parameter(typeof(object));

                var castTarget = Expression.Convert(target, behaviorInterface);

                var methodParameters = methodInfo.GetParameters();
                var contextCastParam = Expression.Convert(contextParam, contextType);

                Expression body = Expression.Call(castTarget, methodInfo, contextCastParam);

                var execute = Expression.Lambda<Func<object, object, Task>>(body, target, contextParam).Compile();
                await execute(this, context).ConfigureAwait(false);
                await next(context).ConfigureAwait(false);
                return;
            }

            var surround = this as ISurroundBehavior<Parent>;
            if (surround != null)
            {
                await surround.Invoke(context, next).ConfigureAwait(false);
                return;
            }
        }
    }

    public class ThrowBehavior : Behavior, IBeforeBehavior<Parent>
    {
        public async Task Invoke(Parent context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            throw new InvalidOperationException();
        }
    }

    public class DoSomethingBeforeNextForParent : Behavior, IBeforeBehavior<Parent>
    {
        public async Task Invoke(Parent context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            // await next(context).ConfigureAwait(false);
        }
    }

    public class DoSomethingAfterNextForParent : Behavior, IAfterBehavior<Parent>
    {
        public async Task Invoke(Parent context)
        {
            // await next(context).ConfigureAwait(false);

            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class SurroundNextForParent : Behavior, ISurroundBehavior<Parent>
    {
        public async Task Invoke(Parent context, Func<Parent, Task> next)
        {
            await Task.Delay(10).ConfigureAwait(false);
            await next(context).ConfigureAwait(false);
            await Task.Delay(10).ConfigureAwait(false);
        }
    }

    public class DoSomethingBeforeNextForChild : Behavior, IBeforeBehavior<Child>
    {
        public async Task Invoke(Child context)
        {
            await Task.Delay(10).ConfigureAwait(false);

            // await next(context).ConfigureAwait(false);
        }
    }

    //public class DoSomethingAfterNextForChild : IBehavior<Child>
    //{
    //    public async Task Invoke(Child context)
    //    {
    //        // await next(context).ConfigureAwait(false);

    //        await Task.Delay(10).ConfigureAwait(false);
    //    }
    //}

    //public class SurroundNextForChild : IBehavior<Child>
    //{
    //    public async Task Invoke(Child context)
    //    {
    //        await Task.Delay(10).ConfigureAwait(false);
    //        // await next(context).ConfigureAwait(false);
    //        await Task.Delay(10).ConfigureAwait(false);
    //    }
    //}

    //public class DoSomethingBeforeNextForGrandChild : IBehavior<GrandChild>
    //{
    //    public async Task Invoke(GrandChild context)
    //    {
    //        await Task.Delay(10).ConfigureAwait(false);

    //        // await next(context).ConfigureAwait(false);
    //    }
    //}

    //public class DoSomethingAfterNextForGrandChild : IBehavior<GrandChild>
    //{
    //    public async Task Invoke(GrandChild context)
    //    {
    //        // await next(context).ConfigureAwait(false);

    //        await Task.Delay(10).ConfigureAwait(false);
    //    }
    //}

    //public class SurroundNextForGrandhild : IBehavior<GrandChild>
    //{
    //    public async Task Invoke(GrandChild context)
    //    {
    //        await Task.Delay(10).ConfigureAwait(false);
    //        // await next(context).ConfigureAwait(false);
    //        await Task.Delay(10).ConfigureAwait(false);
    //    }
    //}

    public class Parent
    {
    }

    public class Child : Parent
    {

    }

    public class GrandChild : Child { }
}
