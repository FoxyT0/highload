namespace SpaceBattle.Lib.Test;

using System.Collections.Concurrent;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;

public class MyThreadUnitTests
{
    public class SomeCol
    {
        public MyThread? tr;
        public IReciever? irc;
        public ISender? isd;
        public IReciever? orc;
        public ISender? osd;


        public SomeCol() { }
    }
    public class SetThreadStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            MyThread q = (MyThread)args[1];
            var allt = IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads");
            Action act = () =>
            {
                allt[id].tr = q;
            };
            return IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Adapters.CommandAdapter", act);
        }
    }

    public class SetInnerRecieverStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            BlockingCollection<SpaceBattle.Lib.ICommand> q = (BlockingCollection<SpaceBattle.Lib.ICommand>)args[1];
            var allt = IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads");
            Action act = () =>
            {
                allt[id].irc = new RecieverAdapter(q);
            };
            return new CommandAdapter(act);
        }
    }

    public class SetInnerSenderStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            BlockingCollection<SpaceBattle.Lib.ICommand> q = (BlockingCollection<SpaceBattle.Lib.ICommand>)args[1];
            var allt = IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads");
            Action act = () =>
            {
                allt[id].isd = new SenderAdapter(q);
            };
            return new CommandAdapter(act);
        }
    }

	public class SetOuterRecieverStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            BlockingCollection<SpaceBattle.Lib.ICommand> q = (BlockingCollection<SpaceBattle.Lib.ICommand>)args[1];
            var allt = IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads");
            Action act = () =>
            {
                allt[id].orc = new RecieverAdapter(q);
            };
            return new CommandAdapter(act);
        }
    }

    public class SetOuterSenderStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            BlockingCollection<SpaceBattle.Lib.ICommand> q = (BlockingCollection<SpaceBattle.Lib.ICommand>)args[1];
            var allt = IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads");
            Action act = () =>
            {
                allt[id].osd = new SenderAdapter(q);
            };
            return new CommandAdapter(act);
        }
    }


    public class GetThreadStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            return IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads")[id].tr!;
        }
    }

    public class GetInnerRecieverStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            return IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads")[id].irc!;
        }
    }

    public class GetInnerSenderStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            return IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads")[id].isd!;
        }
    }

	public class GetOuterRecieverStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            return IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads")[id].orc!;
        }
    }

    public class GetOuterSenderStrategy
    {
        public object run_strategy(params object[] args)
        {
            string id = (string)args[0];
            return IoC.Resolve<Dictionary<string, SomeCol>>("Game.Threads.AllThreads")[id].osd!;
        }
    }

    public class CommandAdapter : SpaceBattle.Lib.ICommand
    {
        Action act;
        public CommandAdapter(Action act)
        {
            this.act = act;
        }
        public void Execute()
        {
            act();
        }
    }

    public class CommandAdapterStrategy : IStrategy
    {
        public object run_strategy(params object[] args)
        {
            Action act = (Action)args[0];
            SpaceBattle.Lib.ICommand ca = new CommandAdapter(act);
            return ca;
        }
    }

    public class RegistrationCommand : SpaceBattle.Lib.ICommand
    {
        Dictionary<string, SomeCol> regt;
        public RegistrationCommand(Dictionary<string, SomeCol> regt)
        {
            this.regt = regt;
        }
        public void Execute()
        {
            var ic = IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"));
            IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", ic).Execute();

            var hscmd = new HardStopThreadStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Commands.HardStop", (object[] args) => hscmd.run_strategy(args)).Execute();

            var sscmd = new SoftStopThreadStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Commands.SoftStop", (object[] args) => sscmd.run_strategy(args)).Execute();

            var casts = new CreateAndStartThreadStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Commands.CreateThread", (object[] args) => casts.run_strategy(args)).Execute();

            var scmd = new SendCommandStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Commands.SendCommand", (object[] args) => scmd.run_strategy(args)).Execute();

            var st = new Mock<IStrategy>();
            st.Setup(o => o.run_strategy()).Returns(regt);
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.AllThreads", (object[] args) => st.Object.run_strategy(args)).Execute();

            var t1 = new SetThreadStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.SetThread", (object[] args) => t1.run_strategy(args)).Execute();

            var t2 = new SetInnerRecieverStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.SetInnerReciever", (object[] args) => t2.run_strategy(args)).Execute();

            var t3 = new SetInnerSenderStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.SetInnerSender", (object[] args) => t3.run_strategy(args)).Execute();

			var t21 = new SetOuterRecieverStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.SetOuterReciever", (object[] args) => t2.run_strategy(args)).Execute();

            var t31 = new SetOuterSenderStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.SetOuterSender", (object[] args) => t3.run_strategy(args)).Execute();


            var t4 = new GetThreadStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.GetThread", (object[] args) => t4.run_strategy(args)).Execute();

            var t5 = new GetInnerRecieverStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.GetInnerReciever", (object[] args) => t5.run_strategy(args)).Execute();

            var t6 = new GetInnerSenderStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.GetInnerSender", (object[] args) => t6.run_strategy(args)).Execute();

			var t51 = new GetOuterRecieverStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.GetOuterReciever", (object[] args) => t5.run_strategy(args)).Execute();

            var t61 = new GetOuterSenderStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Threads.GetOuterSender", (object[] args) => t6.run_strategy(args)).Execute();

            var cmdadapt = new CommandAdapterStrategy();
            IoC.Resolve<ICommand>("IoC.Register", "Game.Adapters.CommandAdapter", (object[] args) => cmdadapt.run_strategy(args)).Execute();

            var fakeex = new Mock<IStrategy>();
            fakeex.Setup(o => o.run_strategy()).Returns(new Exception());
            IoC.Resolve<ICommand>("IoC.Register", "Game.Exceptions.WrongThread", (object[] args) => fakeex.Object.run_strategy(args)).Execute();
        }
    }

    Dictionary<string, SomeCol> regt;

    public MyThreadUnitTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        regt = new Dictionary<string, SomeCol>();
        var somecol = new SomeCol();
        regt.Add("1", somecol);
        regt.Add("2", somecol);

        new RegistrationCommand(regt).Execute();
    }

    [Fact]
    public void CreateThreadTests()
    {
        var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1");
        var waiter = new AutoResetEvent(false);
        var smfr = new Mock<SpaceBattle.Lib.ICommand>();
        smfr.Setup(obj => obj.Execute()).Callback(() => waiter.Set()).Verifiable();

        nt.Execute();
        IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", smfr.Object).Execute();

        waiter.WaitOne();
        smfr.Verify();
    }

    [Fact]
    public void CreateThreadActTests()
    {
        var waiter = new AutoResetEvent(false);
        var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1", () => { waiter.Set(); });

        nt.Execute();

        waiter.WaitOne();
    }
    [Fact]
    public void HardStopCommandTests()
    {
        var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1");
        var waiter = new AutoResetEvent(false);

        nt.Execute();
        IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", new RegistrationCommand(regt)).Execute();
        var hscmd = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.HardStop", "1", () => { waiter.Set(); });
        hscmd.Execute();

        waiter.WaitOne();
    }

    [Fact]
    public void SoftStopCommandTests()
    {
        var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1");
        var waiter = new AutoResetEvent(false);

        nt.Execute();
        IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", new RegistrationCommand(regt)).Execute();
        var sscmd = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SoftStop", "1", () => { waiter.Set(); });
        sscmd.Execute();

        waiter.WaitOne();
    }

	[Fact]
	public void HardStopQueueCheck()
	{
		var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1");
		var waiter = new AutoResetEvent(false);
		var fakecmd = new Mock<SpaceBattle.Lib.ICommand>();
		
		nt.Execute();
		IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", new RegistrationCommand(regt)).Execute();
        var hscmd = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.HardStop", "1", () => { waiter.Set(); });
        hscmd.Execute();
		IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", fakecmd.Object).Execute();

		waiter.WaitOne();
		var rc = IoC.Resolve<IReciever>("Game.Threads.GetInnerReciever", "1");
		Assert.False(rc.isEmpty());
	}

	[Fact]
	public void SoftStopQueueCheck()
	{
		var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1");
		var waiter = new AutoResetEvent(false);
		var fakecmd = new Mock<SpaceBattle.Lib.ICommand>();
		fakecmd.Setup(obj => obj.Execute()).Verifiable();

		nt.Execute();
		IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", new RegistrationCommand(regt)).Execute();
        var sscmd = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SoftStop", "1", () => { waiter.Set(); });
        sscmd.Execute();
		IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", fakecmd.Object).Execute();

		waiter.WaitOne();
		fakecmd.Verify();
		var rc = IoC.Resolve<IReciever>("Game.Threads.GetInnerReciever", "1");
		Assert.True(rc.isEmpty());
	}

    [Fact]
    public void HardExceptionCommand()
    {
        var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1");

        nt.Execute();
        var scmd = new HardStopThreadCommand("2");
        IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", scmd);

        Assert.Throws<Exception>(scmd.Execute);
    }

    [Fact]
    public void SoftExceptionCommand()
    {
        var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1");

        nt.Execute();
        var scmd = new SoftStopThreadCommand("2", () => { });
        IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", scmd);

        Assert.Throws<Exception>(scmd.Execute);
    }

    [Fact]
    public void HardWithoutLambda()
    {
        var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1");
        nt.Execute();
        IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", new RegistrationCommand(regt)).Execute();
        var sscmd = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.HardStop", "1");
        sscmd.Execute();
    }
    [Fact]
    public void SoftWithoutLambda()
    {
        var nt = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.CreateThread", "1");
        nt.Execute();
        IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SendCommand", "1", new RegistrationCommand(regt)).Execute();
        var sscmd = IoC.Resolve<SpaceBattle.Lib.ICommand>("Game.Commands.SoftStop", "1");
        sscmd.Execute();
    }
}

