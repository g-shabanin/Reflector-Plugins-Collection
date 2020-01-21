using System;
using System.Reflection;
using System.Windows.Forms;
using System.Workflow.Activities.Rules;
using System.Workflow.Activities.Rules.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Xml;
using Reflector;
using Reflector.CodeModel;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Reflector.RuleSetEditor.Properties;

namespace Reflector.RuleSetEditor
{
    /// <summary>
    /// Enables type-centric access to the Windows Workflow Rule Set Editor.
    /// </summary>
    class RuleSetEditorPackage : IPackage
    {
        private ICommandBar assemblyMenu = null;
        private ICommandBar toolsMenu = null;
        private ICommandBarButton newRuleSetButton = null;
        private ICommandBarButton editRuleSetButton = null;
        private ICommandBarSeparator separator = null;
        private IAssemblyBrowser assemblyBrowser = null;
        private IWindowManager windowManager = null;
        private Type selectedType = null;

        /// <summary>
        /// Loads the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public void Load(IServiceProvider serviceProvider)
        {
            // Get the command bar manager
            ICommandBarManager cbm = (ICommandBarManager)serviceProvider.GetService(typeof(ICommandBarManager));

            // We'll only add to the TypeDeclaration context menu and Tools menu
            assemblyMenu = cbm.CommandBars["Browser.TypeDeclaration"];

            toolsMenu = cbm.CommandBars["Tools"];

            // Add the "New" button
            newRuleSetButton = assemblyMenu.Items.AddButton("&New Rule Set", new EventHandler(newRuleSetButton_Click));

            // Add separator
            separator = toolsMenu.Items.AddSeparator();

            // Add the "Edit" button
            editRuleSetButton = toolsMenu.Items.AddButton("&Edit Rule Set", new EventHandler(editRuleSetButton_Click));

            // Hook the assembly browser
            assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));

            // Get a reference to the host window
            windowManager = (IWindowManager)serviceProvider.GetService(typeof(IWindowManager));
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public void Unload()
        {
            // Remove both "New", "Edit", and separator menu items
            assemblyMenu.Items.Remove(newRuleSetButton);
            toolsMenu.Items.Remove(editRuleSetButton);
            toolsMenu.Items.Remove(separator);
        }

        /// <summary>
        /// Gets the type currently selected.
        /// </summary>
        /// <returns>The <see cref="T:System.Type"/> currently selected.</returns>
        private Type GetSelectedType()
        {
            // Check if the selected item is of type ITypeDeclaration
            ITypeDeclaration type = assemblyBrowser.ActiveItem as ITypeDeclaration;
            Type result = null;

            // If user is on an ITypeDeclaration
            if (type != null)
            {
                // Cast the owner as both a type declaration and a module
                ITypeDeclaration type2 = type.Owner as ITypeDeclaration;
                IModule module = type.Owner as IModule;

                // If the owner is a type, and not a module
                while ((type2 != null) && (module == null))
                {
                    // We have a nested type, so cast again to find the owning module
                    module = type2.Owner as IModule;
                    type2 = type2.Owner as ITypeDeclaration;
                }

                // We should have the owning module now
                if (module != null)
                {
                    // Load the owner assembly
                    Assembly asm = Assembly.LoadFile(Environment.ExpandEnvironmentVariables(module.Location));

                    // Grab the module's reference assemblies
                    foreach (IAssemblyReference reference in module.AssemblyReferences)
                    {
                        // Auto-resolve assemblies, or prompt the user as needed
                        IAssembly resolvedAssembly = reference.Resolve();

                        // If the assembly was resolved
                        if (resolvedAssembly != null)
                        {
                            // Load the referenced assembly
                            Assembly assembly = Assembly.LoadFile(Environment.ExpandEnvironmentVariables(resolvedAssembly.Location));

                            // Create an assembly name to load into the AppDomain
                            AssemblyName assemblyName = new AssemblyName(assembly.FullName);

                            // Set the file path for successful AppDomain loading
                            assemblyName.CodeBase = assembly.CodeBase;

                            // Now load the assembly into the AppDomain
                            // Note: This enables the DynamicTypeProvider to
                            // find (and load) a type to be used with the
                            // RuleSetDialog
                            AppDomain.CurrentDomain.Load(assemblyName);
                        }
                    }

                    // Get the .NET type currently selected
                    result = asm.GetType(type.Namespace + "." + type.Name);
                }
            }

            return result;
        }

        /// <summary>
        /// Handles the Click event of the newRuleSetButton button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void newRuleSetButton_Click(object sender, EventArgs args)
        {
            selectedType = GetSelectedType();

            if (selectedType != null)
            {
                // Open rule set editor for the specified type, using our type provider
                // for type resolution, and no starting point for a rule set.
                RuleSetDialog rsd = new RuleSetDialog(selectedType, new DynamicTypeProvider(), null);

                // Hook to prompt the user to save the rule set (or discard editing)
                ((Button)(rsd.AcceptButton)).Click += new EventHandler(okButton_Click);
                ((Button)(rsd.CancelButton)).Click += new EventHandler(cancelButton_Click);

                // New mode
                rsd.Tag = string.Empty;

                // UI Help: Show the type name while editing
                rsd.Text += " - " + selectedType.AssemblyQualifiedName;

                // Show the dialog
                rsd.Show((IWin32Window)windowManager);

                rsd.BringToFront();
            }
        }

        /// <summary>
        /// Handles the Click event of the cancelButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void cancelButton_Click(object sender, EventArgs e)
        {
            Button cancelButton = sender as Button;

            if (cancelButton != null)
            {
                RuleSetDialog rsd = cancelButton.FindForm() as RuleSetDialog;

                if (rsd != null)
                {
                    rsd.Close();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the okButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void okButton_Click(object sender, EventArgs e)
        {
            Button okButton = sender as Button;

            if (okButton != null)
            {
                RuleSetDialog rsd = okButton.FindForm() as RuleSetDialog;

                if (rsd != null)
                {
                    string fileName = string.Empty;
                    string tag = rsd.Tag.ToString();

                    if ((tag.Length > 0) && (File.Exists(tag)))
                    {
                        fileName = tag;
                    }
                    else
                    {
                        SaveRulesFile(ref fileName);
                    }

                    // If a file is specified
                    if (fileName.Length > 0)
                    {
                        // Now save the file
                        using (XmlWriter rulesWriter = XmlWriter.Create(fileName))
                        {
                            // Record the type for editing
                            rulesWriter.WriteComment(selectedType.AssemblyQualifiedName);
                            rulesWriter.WriteComment(selectedType.FullName);
                            rulesWriter.WriteComment(selectedType.Assembly.CodeBase);

                            // Serialize the rule set
                            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                            serializer.Serialize(rulesWriter, rsd.RuleSet);
                        }

                        rsd.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the editRuleSetButton button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void editRuleSetButton_Click(object sender, EventArgs args)
        {
            string fileName = string.Empty;

            // Prompt the user to open an existing file
            if (OpenRulesFile(ref fileName) == DialogResult.OK)
            {
                RuleSetEntity entity = GetRuleSetEntity(fileName);

                if (entity.Type == null)
                {
                    MessageBox.Show((IWin32Window)windowManager, string.Format(Resources.RuleSetTypeNotFoundMessage, entity.AssemblyQualifiedName, entity.FullName, entity.CodeBase), Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Local reference to save later
                    selectedType = entity.Type;

                    // Load the rule set editor for the specified type, use types from the current
                    // AppDomain, and pre-load deserialized rule set.
                    RuleSetDialog rsd = new RuleSetDialog(entity.Type, new DynamicTypeProvider(), entity.RuleSet);

                    // Hook to prompt the user to save the rule set (or discard editing)
                    ((Button)(rsd.AcceptButton)).Click += new EventHandler(okButton_Click);
                    ((Button)(rsd.CancelButton)).Click += new EventHandler(cancelButton_Click);

                    // Edit mode
                    rsd.Tag = fileName;

                    // UI Help: Show the type name while editing
                    rsd.Text += " - " + entity.AssemblyQualifiedName;

                    // Show the dialog
                    rsd.Show((IWin32Window)windowManager);
                }
            }
        }

        private RuleSetEntity GetRuleSetEntity(string fileName)
        {
            RuleSetEntity result = new RuleSetEntity();

            // Read the file selected by the user
            using (XmlTextReader rulesReader = new XmlTextReader(fileName))
            {
                InitializeAssemblyInformation(fileName, result);

                result.Type = GetType(result);

                if (result.Type != null)
                {
                    // Deserialize XML rule set
                    WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                    result.RuleSet = (RuleSet)serializer.Deserialize(rulesReader);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the type from the specified RuleSetEntity.
        /// </summary>
        /// <param name="entity">The <seealso cref="RuleSetEntity"/>.</param>
        /// <returns>The type defined by the <seealso cref="RuleSetEntity"/>; otherwise, null if it could not be resolved.</returns>
        private Type GetType(RuleSetEntity entity)
        {
            // Attempt to load the fully qualified type
            Type result = Type.GetType(entity.AssemblyQualifiedName);

            // If it was not found, but we have the type full name and code base location
            if ((result == null) && (!string.IsNullOrEmpty(entity.FullName)) && (!string.IsNullOrEmpty(entity.CodeBase)))
            {
                AssemblyName an = new AssemblyName();

                // Define the code base (file) location
                an.CodeBase = entity.CodeBase;

                // Load the assembly
                Assembly asm = Assembly.Load(an);

                // Cherry pick the type
                result = asm.GetType(entity.FullName);
            }

            return result;
        }

        /// <summary>
        /// Initializes the assembly information within the RuleSetEntity.
        /// </summary>
        /// <param name="fileName">Name of the ruleset file.</param>
        /// <param name="entity">The <seealso cref="RuleSetEntity"/> to initialize.</param>
        private void InitializeAssemblyInformation(string fileName, RuleSetEntity entity)
        {
            XDocument doc = XDocument.Load(fileName);
            XComment aqn = doc.FirstNode as XComment;
            XComment fn = (aqn != null) ? aqn.NextNode as XComment : null;
            XComment cb = (fn != null) ? fn.NextNode as XComment : null;

            // If the first node was a comment
            if (aqn != null)
            {
                // Assume this comment was the qualified assembly name
                entity.AssemblyQualifiedName = aqn.Value;
            }

            // If the second node was a comment
            if (fn != null)
            {
                // Assume this comment was the full type name
                entity.FullName = fn.Value;
            }

            // If the third node was a comment
            if (cb != null)
            {
                // Assume this comment was the code base (file) location
                entity.CodeBase = cb.Value;
            }
        }

        /// <summary>
        /// Prompts the user to open a *.rules file.
        /// </summary>
        /// <param name="fileName">Name of the file the user selected.</param>
        /// <returns></returns>
        private DialogResult OpenRulesFile(ref string fileName)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult result;

            // Explicitly set desired dialog functionality.
            ofd.AddExtension = true;
            ofd.AutoUpgradeEnabled = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.DefaultExt = ".rules";
            ofd.DereferenceLinks = true;
            ofd.Filter = "Rules (*.rules)|*.rules|All Files (*.*)|*.*";
            ofd.FilterIndex = 0;
            ofd.Multiselect = false;
            ofd.ReadOnlyChecked = false;
            ofd.ShowReadOnly = false;
            ofd.SupportMultiDottedExtensions = true;
            ofd.Title = "Open File...";
            ofd.ValidateNames = true;

            result = ofd.ShowDialog();

            fileName = ofd.FileName;

            return result;
        }

        /// <summary>
        /// Prompts the user to save the *.rules file.
        /// </summary>
        /// <param name="fileName">Name of the file the user selected.</param>
        /// <returns></returns>
        private DialogResult SaveRulesFile(ref string fileName)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            DialogResult result;

            // Explicitly set desired dialog functionality.
            sfd.AddExtension = true;
            sfd.AutoUpgradeEnabled = true;
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;
            sfd.CreatePrompt = false;
            sfd.DefaultExt = ".rules";
            sfd.Filter = "Rules (*.rules)|*.rules|All Files (*.*)|*.*";
            sfd.FilterIndex = 0;
            sfd.OverwritePrompt = true;
            sfd.Title = "Save As...";
            sfd.ValidateNames = true;

            result = sfd.ShowDialog();

            fileName = sfd.FileName;

            return result;
        }
    }
}
