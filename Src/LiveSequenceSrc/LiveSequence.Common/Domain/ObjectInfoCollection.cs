#define CODE_ANALYSIS
namespace LiveSequence.Common.Domain
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Diagnostics.CodeAnalysis;
  using System.Linq;

  /// <summary>
  /// Collection for the ObjectInfo objects that make up the sequence.
  /// </summary>
  internal class ObjectInfoCollection : ObservableCollection<ObjectInfo>, INotifyPropertyChanged
  {
    /// <summary>Contains a reference to the current ObjectInfo object.</summary>
    private ObjectInfo current;

    /// <summary>Contains a boolean that indicates whether or not the collection is dirty.</summary>
    private bool dirty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceCollection"/> class.
    /// </summary>
    internal ObjectInfoCollection()
    {
    }

    /// <summary>
    /// An ObjectInfo or message was added, removed or modified in the list. This is used
    /// instead of CollectionChanged since CollectionChanged can be raised before the 
    /// relationships are setup (the ObjectInfo was added to the list, but its Messages
    /// collections have not been established). This means the subscriber 
    /// (the diagram control) will update before all of the information is available and 
    /// relationships will not be displayed.
    /// The ContentChanged event addresses this problem and allows the flexibility to
    /// raise the event after *all* information has been added to the list, and *all* of
    /// the messages have been established. 
    /// Objects that add or remove object information from the list, or add or remove messages
    /// should call OnContentChanged when they want to notify subscribers that all
    /// changes have been made.
    /// </summary>
    public event EventHandler<ContentChangedEventArgs> ContentChanged;

    /// <summary>
    /// Occurs when the current reference is changed.
    /// </summary>
    public event EventHandler CurrentChanged;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    protected override event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Gets or sets the current.
    /// </summary>
    /// <value>The current.</value>
    public ObjectInfo Current
    {
      get
      {
        return this.current;
      }

      set
      {
        if (this.current != value)
        {
          this.current = value;
          this.OnPropertyChanged("Current");
          this.OnCurrentChanged();
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is dirty.
    /// </summary>
    /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The property is currently not directly called, but a property without the getter will raise different FxCop messages.")]
    public bool IsDirty
    {
      get
      {
        return this.dirty;
      }

      set
      {
        this.dirty = value;
      }
    }

    /// <summary>
    /// Invokes the ContentChanged event.
    /// </summary>
    /// <remarks>
    /// The details of a ObjectInfo changed.
    /// </remarks>
    public void OnContentChanged()
    {
      this.dirty = true;
      if (this.ContentChanged != null)
      {
        this.ContentChanged(this, new ContentChangedEventArgs(null));
      }
    }

    /// <summary>
    /// Invokes the ContentChanged event and passes the new ObjectInfo object in the EventArgs.
    /// </summary>
    /// <param name="newObjectInfo">The new object info.</param>
    /// <remarks>
    /// The details of a ObjectInfo changed, and a new ObjectInfo was added to the collection.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This OnContentChanged method is made ready for possible future use to travel the stack through the control in stead of through the tree.")]
    public void OnContentChanged(ObjectInfo newObjectInfo)
    {
      this.dirty = true;
      if (this.ContentChanged != null)
      {
        this.ContentChanged(this, new ContentChangedEventArgs(newObjectInfo));
      }
    }

    /// <summary>
    /// Finds the specified key.
    /// </summary>
    /// <param name="key">The key value.</param>
    /// <returns>The ObjectInfo object which matches the given key; null when not found.</returns>
    public ObjectInfo Find(string key)
    {
      var query = from o in this.Items
                  where string.Compare(o.Key, key, StringComparison.Ordinal) == 0
                  select o;

      List<ObjectInfo> result = query.ToList<ObjectInfo>();
      return result.Count > 0 ? result[0] : null;
    }

    /// <summary> 
    /// The primary ObjectInfo changed in the list.
    /// </summary>
    protected void OnCurrentChanged()
    {
      if (this.CurrentChanged != null)
      {
        this.CurrentChanged(this, EventArgs.Empty);
      }
    }

    #region INotifyPropertyChanged Members

    /// <summary>
    /// Invokes the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
      if (this.PropertyChanged != null)
      {
        this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    #endregion
  }
}
