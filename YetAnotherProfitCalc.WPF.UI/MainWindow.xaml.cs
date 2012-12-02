using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YetAnotherProfitCalc.WPF.UI
{
    // http://stackoverflow.com/questions/4225867/how-can-i-turn-binding-errors-into-runtime-exceptions
    public class BindingErrorListener : TraceListener
    {
        private Action<string> logAction;
        public static void Listen(Action<string> logAction)
        {
            PresentationTraceSources.DataBindingSource.Listeners
                .Add(new BindingErrorListener() { logAction = logAction, Filter = new BindingErrorFilter() });
        }
        public override void Write(string message) { }
        public override void WriteLine(string message)
        {
            logAction(message);
        }
    }

    public class BindingErrorFilter : TraceFilter
    {
        override public bool ShouldTrace(TraceEventCache cache, string source,
        TraceEventType eventType, int id, string formatOrMessage,
        object[] args, object data, object[] dataArray)
        {
            return (eventType & TraceEventType.Error) != 0 ||
                (eventType & TraceEventType.Critical) != 0;
        }
    }

    public class BindingException : Exception
    {
        public BindingException() : base() { }
        public BindingException(string message) : base(message) { }
        public BindingException(string message, Exception inner) : base(message, inner) { }
    }

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
            BindingErrorListener.Listen(m => { throw new BindingException(m); });
			InitializeComponent();
		}
	}
}
