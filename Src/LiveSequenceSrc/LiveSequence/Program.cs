using System;
using System.Windows.Forms;
using LiveSequence.Common;

namespace LiveSequence
{
    internal class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Run(args);
        }

        private static void Run(string[] args)
        {
            var controller = new MainFormController();
            bool consoleOnly = false;

            if (args.Length > 0)
            {
                CommandLineArguments cla = CommandLineArguments.ParseArguments(args);
                if (cla != null)
                {
                    RunConsole(controller, cla);
                }

                consoleOnly = CommandLineArguments.ConsoleOnly;
            }

            if (!consoleOnly)
            {
                RunGUI(controller);
            }
        }

        private static void RunGUI(MainFormController controller)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(controller));
        }

        private static void RunConsole(MainFormController controller, CommandLineArguments args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ConsoleView(controller, args));
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating)
            {
                Logger.Current.Error("Unhandled exception", e.ExceptionObject as Exception);
            }
        }
    }
}