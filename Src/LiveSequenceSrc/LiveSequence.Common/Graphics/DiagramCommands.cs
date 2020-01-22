using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LiveSequence.Common.Context;

namespace LiveSequence.Common.Graphics
{
  public static class DiagramCommands
  {
    private static Dispatcher dispatcher;

    private static RoutedUICommand filterActorCommand = new RoutedUICommand("Filter out actor", "filterOutActor", typeof(DiagramCommands));

    private static RoutedUICommand filterCallsCommand = new RoutedUICommand("Filter out downstream calls", "filterOutDownStream", typeof(DiagramCommands));

    static DiagramCommands()
    {
      dispatcher = Dispatcher.CurrentDispatcher;

      CommandManager.RegisterClassCommandBinding(typeof(MenuItem), new CommandBinding(DiagramCommands.FilterOutActor, FilterOutActorHandler, CanFilterOutActorHandler));
      CommandManager.RegisterClassCommandBinding(typeof(MenuItem), new CommandBinding(DiagramCommands.FilterOutCalls, FilterOutCallsHandler, CanFilterOutCallsHandler));
    }

    public static RoutedUICommand FilterOutActor
    {
      get
      {
        return filterActorCommand;
      }
    }

    public static RoutedUICommand FilterOutCalls
    {
      get
      {
        return filterCallsCommand;
      }
    }

    private static void CanFilterOutActorHandler(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private static void CanFilterOutCallsHandler(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;
      e.Handled = true;
    }

    private static void FilterOutActorHandler(object sender, ExecutedRoutedEventArgs e)
    {
      DiagramSequenceNode node = e.Parameter as DiagramSequenceNode;
      if (node != null)
      {
        if (node.ObjectInfo.Key == DiagramContext.DiagramObjects[0].Key)
        {
          // root may not be eliminated...
          Console.WriteLine("attempt to remove root...");
          return;
        }

        DiagramContext.FilterOut(node.ObjectInfo);
        RefreshDiagram();
      }
    }

    private static void FilterOutCallsHandler(object sender, ExecutedRoutedEventArgs e)
    {
      DiagramSequenceNode node = e.Parameter as DiagramSequenceNode;
      if (node != null)
      {
        if (node.ObjectInfo.Key == DiagramContext.DiagramObjects[0].Key)
        {
          // root may not be eliminated...
          Console.WriteLine("attempt to remove root...");
          return;
        }

        DiagramContext.FilterOut(node.ObjectInfo, false);
        RefreshDiagram();
      }
    }

    private static void RefreshDiagram()
    {
      DiagramContext.DiagramObjects.OnContentChanged();

      DiagramContext.DiagramObjects.IsDirty = false;
    }
  }
}
