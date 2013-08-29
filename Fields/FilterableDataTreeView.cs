namespace Spark.Sitecore.Client.Fields
{
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	using global::Sitecore.Data.Items;
	using global::Sitecore.Web.UI.HtmlControls;
	using global::Sitecore.Web.UI.Sheer;

	/// <summary>
	/// Overrides DataTreeView to provide filtering capabilities.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Treeview comes from Sitecore's lexicon.")]
	public class FilterableDataTreeview : DataTreeview
	{
		#region Properties
		/// <summary>
		/// Gets or sets the array of template names to exclude from display.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Mirroring Sitecore architecture.")]
		public string[] ExcludeTemplatesForDisplay
		{
			get { return this.GetViewStateProperty("ExcludeTemplatesForDisplay", null) as string[]; }
			set { this.SetViewStateProperty("ExcludeTemplatesForDisplay", value, null); }
		}

		/// <summary>
		/// Gets or sets the array of templates to exclude from selection.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Mirroring Sitecore architecture.")]
		public string[] ExcludeTemplatesForSelection
		{
			get { return this.GetViewStateProperty("ExcludeTemplatesForSelection", null) as string[]; }
			set { this.SetViewStateProperty("ExcludeTemplatesForSelection", value, null); }
		}

		/// <summary>
		/// Gets or sets the array of templates to display.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Mirroring Sitecore architecture.")]
		public string[] IncludeTemplatesForDisplay
		{
			get { return this.GetViewStateProperty("IncludeTemplatesForDisplay", null) as string[]; }
			set { this.SetViewStateProperty("IncludeTemplatesForDisplay", value, null); }
		}

		/// <summary>
		/// Gets or sets the array of templates to include in selection options.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Mirroring Sitecore architecture.")]
		public string[] IncludeTemplatesForSelection
		{
			get { return this.GetViewStateProperty("IncludeTemplatesForSelection", null) as string[]; }
			set { this.SetViewStateProperty("IncludeTemplatesForSelection", value, null); }
		}
		#endregion

		#region Methods
		/// <summary>
		/// Contact behavior for Sitecore Client field controls.
		/// </summary>
		/// <param name="message">The message.</param>
		public override void HandleMessage(Message message)
		{
			/* Ignore keydown events in general.
			 * Standard DropTree doesn't trigger behavior from these
			 * but clicking a disabled node will focus it and make it possible to trigger base class keydown event behavior (with unexpected results).
			 */
			if (message.Name == "event:keydown")
			{
				global::Sitecore.Context.ClientPage.ClientResponse.ClosePopups(false);
				message.CancelDispatch = true;
				return;
			}

			base.HandleMessage(message);
		}

		/// <summary>
		/// Overridden base class behavior to account for filtering.
		/// </summary>
		/// <param name="dataContext">The data context.</param>
		/// <param name="control">The control.</param>
		/// <param name="root">The root Item.</param>
		/// <param name="folder">The Folder Item.</param>
		/// <param name="selectedIDs">The selected IDs.</param>
		protected override void Populate(DataContext dataContext, System.Web.UI.Control control, Item root, Item folder, string selectedIDs)
		{
			if (this.ExcludeTemplatesForDisplay == null && this.IncludeTemplatesForDisplay == null)
			{
				// nothing need to filter
				base.Populate(dataContext, control, root, folder, selectedIDs);
				return;
			}

			// prevent items of templates excluded for display from being added to the tree
			if (this.IncludeTemplatesForDisplay != null && this.IncludeTemplatesForDisplay.Contains(root.TemplateName) == false)
			{
				return;
			}

			if (this.ExcludeTemplatesForDisplay != null && this.ExcludeTemplatesForDisplay.Contains(root.TemplateName))
			{
				return;
			}

			base.Populate(dataContext, control, root, folder, selectedIDs);

			// ensure that expandable nodes have at least one child appropriate for display
			var nodesToCheck = control.Controls.OfType<DataTreeNode>().Where(p => p.Visible && p.Expandable);
			foreach (var node in nodesToCheck)
			{
				if (string.IsNullOrEmpty(node.ItemID))
				{
					continue;
				}

				var item = dataContext.GetItem(node.ItemID);

				if (item == null)
				{
					continue;
				}

				var shouldExpand = false;
				var childItems = item.Children;
				foreach (Item childItem in childItems)
				{
					if (this.IncludeTemplatesForDisplay != null && this.IncludeTemplatesForDisplay.Contains(childItem.TemplateName))
					{
						shouldExpand = true;
						break;
					}

					if (this.ExcludeTemplatesForDisplay != null && !this.ExcludeTemplatesForDisplay.Contains(childItem.TemplateName))
					{
						shouldExpand = true;
						break;
					}
				}

				if (shouldExpand == false)
				{
					node.Expandable = false;
					node.Expanded = false;
				}
			}
		}

		/// <summary>
		/// Contract behavior for Sitecore Client tree control.
		/// </summary>
		/// <param name="item">The item of the tree node.</param>
		/// <param name="parent">The item's parent.</param>
		/// <returns>The Item's node.</returns>
		protected override TreeNode GetTreeNode(Item item, System.Web.UI.Control parent)
		{
			var node = base.GetTreeNode(item, parent);

			// disable or hide nodes as appropriate
			if (this.ExcludeTemplatesForSelection != null && this.ExcludeTemplatesForSelection.Contains(item.TemplateName))
			{
				node.Enabled = false;
			}

			if (this.ExcludeTemplatesForDisplay != null && this.ExcludeTemplatesForDisplay.Contains(item.TemplateName))
			{
				node.Visible = false;
			}

			if (this.IncludeTemplatesForSelection != null && this.IncludeTemplatesForSelection.Contains(item.TemplateName) == false)
			{
				node.Enabled = false;
			}

			if (this.IncludeTemplatesForDisplay != null && this.IncludeTemplatesForDisplay.Contains(item.TemplateName) == false)
			{
				node.Visible = false;
			}

			return node;
		}

		/// <summary>
		/// Contract behavior for Sitecore Client field controls.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="node">The clicked node.</param>
		protected override void NodeClicked(Message message, TreeNode node)
		{
			// ignore clicks on disabled nodes
			var datanode = node as DataTreeNode;
			if (datanode != null && datanode.Enabled == false)
			{
				global::Sitecore.Context.ClientPage.ClientResponse.ClosePopups(false);
				message.CancelDispatch = true;
				return;
			}

			base.NodeClicked(message, node);
		}
		#endregion
	}
}
