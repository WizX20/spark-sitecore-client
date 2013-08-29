namespace Spark.Sitecore.Client.Fields
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Web.UI.WebControls;

	using global::Sitecore;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.Globalization;
	using global::Sitecore.Shell.Applications.ContentEditor;
	using global::Sitecore.Web.UI.HtmlControls;
	using global::Sitecore.Web.UI.Sheer;

	/// <summary>
	/// Overrides the standard Sitecore Droptree field type. 
	/// Allows you to specify the tree item templates which are valid for display and for selection.
	/// Same rules as Treelist.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <listheader>
	/// Available Parameters: 
	/// </listheader>
	/// <item><description>DataSource,</description></item>
	/// <item><description>ExcludeTemplatesForDisplay,</description></item> 
	/// <item><description>ExcludeTemplatesForSelection,</description></item> 
	/// <item><description>IncludeTemplatesForDisplay,</description></item> 
	/// <item><description>IncludeTemplatesForSelection</description></item>
	/// </list>
	/// <list type="number">
	/// <listheader>
	/// Installation Instructions: 
	/// </listheader>
	/// <item><description>
	/// Duplicate item
	/// /sitecore/system/Field types/droptree
	/// to :
	/// /sitecore/system/Field types/filtered droptree
	/// </description></item>
	/// <item><description>
	/// Empty the field values and fill these two:
	/// Assembly: Spark.Sitecore.Client
	/// Class: Spark.Sitecore.Client.Fields.FilteredTree
	/// </description></item>
	/// <item><description>Add the new “filtered droptree” field to the Data Template that needs it.</description></item>
	/// <item><description>Fill the Source with the appropriate parameters.</description></item>
	/// </list>
	/// </remarks>
	public class FilteredTree : Tree
	{
		/// <summary>
		/// Specifies the delimiter.
		/// </summary>
		private static readonly char[] TemplateNameDelimiters = new[] { ',' };

		#region Properties
		/// <summary>
		/// Internal instance for the Field's Source parameter.
		/// </summary>
		private string source;

		/// <summary>
		/// Gets or sets the root node of the tree to be displayed in the control, along with any filtering parameters.
		/// </summary>
		public new string Source
		{
			get
			{
				return this.source;
			}

			set
			{
				this.source = value;
				if (value.IndexOf("datasource=", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					this.ExcludeTemplatesForSelection = StringUtil.ExtractParameter("ExcludeTemplatesForSelection", value).Trim();
					this.IncludeTemplatesForSelection = StringUtil.ExtractParameter("IncludeTemplatesForSelection", value).Trim();
					this.IncludeTemplatesForDisplay = StringUtil.ExtractParameter("IncludeTemplatesForDisplay", value).Trim();
					this.ExcludeTemplatesForDisplay = StringUtil.ExtractParameter("ExcludeTemplatesForDisplay", value).Trim();
					base.Source = StringUtil.ExtractParameter("DataSource", value).Trim();
				}
				else
				{
					base.Source = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the templates used to exclude Items displayed in the tree.
		/// </summary>
		public string ExcludeTemplatesForDisplay
		{
			get
			{
				return this.GetViewStateString("ExcludeTemplatesForDisplay");
			}

			set
			{
				Assert.ArgumentNotNull(value, "value");
				this.SetViewStateString("ExcludeTemplatesForDisplay", value);
			}
		}

		/// <summary>
		/// Gets or sets the templates used to exclude Items from user selection.
		/// </summary>
		public string ExcludeTemplatesForSelection
		{
			get
			{
				return this.GetViewStateString("ExcludeTemplatesForSelection");
			}

			set
			{
				Assert.ArgumentNotNull(value, "value");
				this.SetViewStateString("ExcludeTemplatesForSelection", value);
			}
		}

		/// <summary>
		/// Gets or sets the templates used to explicitly include Items displayed in the tree.
		/// </summary>
		public string IncludeTemplatesForDisplay
		{
			get
			{
				return this.GetViewStateString("IncludeTemplatesForDisplay");
			}

			set
			{
				Assert.ArgumentNotNull(value, "value");
				this.SetViewStateString("IncludeTemplatesForDisplay", value);
			}
		}

		/// <summary>
		/// Gets or sets the templates used to explicitly include Items for user selection.
		/// </summary>
		public string IncludeTemplatesForSelection
		{
			get
			{
				return this.GetViewStateString("IncludeTemplatesForSelection");
			}

			set
			{
				Assert.ArgumentNotNull(value, "value");
				this.SetViewStateString("IncludeTemplatesForSelection", value);
			}
		}
		#endregion

		/// <summary>
		/// Overridden in order to use FilterableDataTreeView
		/// Essentially reverse engineered from the stock control except for the
		/// designated line.
		/// </summary>
		protected override void DropDown()
		{
			if (!string.IsNullOrEmpty(this.Value))
			{
				var dataContext = global::Sitecore.Context.ClientPage.FindSubControl(this.DataContext) as DataContext;
				Assert.IsNotNull(
					dataContext,
					typeof(DataContext),
					"Datacontext \"{0}\" not found.",
					new object[] { this.DataContext });

				// ReSharper disable PossibleNullReferenceException
				dataContext.Folder = this.Value;
				// ReSharper restore PossibleNullReferenceException
			}

			System.Web.UI.Control hiddenHolder = UIUtil.GetHiddenHolder(this);
			DataTreeNode dataTreeNode = null;
			var scrollbox = new Scrollbox();
			global::Sitecore.Context.ClientPage.AddControl(hiddenHolder, scrollbox);
			scrollbox.Width = 300;
			scrollbox.Height = 400;
			scrollbox.Padding = "0";
			scrollbox.Border = "1px solid black";

			var dataTreeview = this.GetDataTreeview(); // this one line is the change

			dataTreeview.Class = "scTreeview scPopupTree";
			dataTreeview.DataContext = this.DataContext;
			dataTreeview.ID = this.ID + "_treeview";
			dataTreeview.AllowDragging = false;
			if (this.AllowNone)
			{
				dataTreeNode = new DataTreeNode();
				global::Sitecore.Context.ClientPage.AddControl(dataTreeview, dataTreeNode);
				dataTreeNode.ID = this.ID + "_none";
				dataTreeNode.Header = Translate.Text("[none]");
				dataTreeNode.Expandable = false;
				dataTreeNode.Expanded = false;
				dataTreeNode.Value = "none";
				dataTreeNode.Icon = "Applications/16x16/forbidden.png";
			}

			global::Sitecore.Context.ClientPage.AddControl(scrollbox, dataTreeview);
			dataTreeview.Width = new Unit(100.0, UnitType.Percentage);
			dataTreeview.Click = this.ID + ".Select";
			dataTreeview.DataContext = this.DataContext;
			if (string.IsNullOrEmpty(this.Value) && dataTreeNode != null)
			{
				dataTreeview.ClearSelection();
				dataTreeNode.Selected = true;
			}

			SheerResponse.ShowPopup(this.ID, "below-right", scrollbox);
		}

		/// <summary>
		/// Sets up the FilterableTreeView instead of the stock TreeView.
		/// </summary>
		/// <returns>A filterable tree view.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Mirroring Sitecore lexicon.")]
		protected virtual DataTreeview GetDataTreeview()
		{
			var treeview = new FilterableDataTreeview
				{
					ExcludeTemplatesForDisplay = GetTemplateNames(this.ExcludeTemplatesForDisplay),
					ExcludeTemplatesForSelection = GetTemplateNames(this.ExcludeTemplatesForSelection),
					IncludeTemplatesForDisplay = GetTemplateNames(this.IncludeTemplatesForDisplay),
					IncludeTemplatesForSelection = GetTemplateNames(this.IncludeTemplatesForSelection)
				};

			return treeview;
		}

		/// <summary>
		/// Returns the appropriate templates as an array.
		/// </summary>
		/// <param name="templates">The string to parse.</param>
		/// <returns>An array of templates or null.</returns>
		private static string[] GetTemplateNames(string templates)
		{
			if (string.IsNullOrEmpty(templates))
			{
				return null;
			}

			return templates.Split(TemplateNameDelimiters, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
