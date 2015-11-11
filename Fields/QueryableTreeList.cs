namespace Spark.Sitecore.Client.Fields
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;

	using global::Sitecore.Data.Items;
	using global::Sitecore.Shell.Applications.ContentEditor;

	/// <summary>
	/// A Sitecore Client Treelist where the Source field can support Sitecore's Query syntax
	/// to set the root of the tree that appears in the control.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Please see Sitecore's scrapbook entry on the topic:
	/// http://sdn.sitecore.net/Scrapbook/Custom%20TreeList%20that%20supports%20query.aspx
	/// </para>
	/// <list type="number">
	/// <listheader>
	/// Installation Instructions: 
	/// </listheader>
	/// <item><description>
	/// Duplicate item
	/// /sitecore/system/Field types/tree list
	/// to :
	/// /sitecore/system/Field types/queryable treelist 
	/// </description></item>
	/// <item><description>
	/// Empty the field values and fill these two:
	/// Assembly: Spark.Sitecore
	/// Class: Spark.Sitecore.Client.Fields.QueryableTreeList
	/// </description></item>
	/// <item><description>Add the new “queryable treelist” field to the Data Template that needs it.</description></item>
	/// <item><description>
	/// Fill the Source with the Xpath to set the root object. 
	/// Note: "query:" must prefix the XPath.
	/// </description></item>
	/// </list>
	/// </remarks>
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "StyleCop doesn't like URLs.")]
	public class QueryableTreeList : TreeList
	{
		#region Properties
		/// <summary>
		/// Gets or sets the location and parameters of the root Item of the Tree.
		/// </summary>
		/// <remarks>
		/// We're overriding this Property to support Sitecore Query syntax to
		/// dynamically look for the tree root based upon context.
		/// Queries here support the Sitecore XPath syntax and are executed from
		/// the context of the Item being edited.
		/// </remarks>
		public new string Source
		{
			get
			{
				return base.Source;
			}

			set
			{
				if (!value.ToLower(CultureInfo.InvariantCulture).Contains("query:"))
				{
					base.Source = value;
				}
				else
				{
					/* Treelist supports a number of Querystring-based settings, 
					 * we need to separate the path from the Querystring so we can get the Item
					 * referenced by the query: statement.
					 */

					string path = ExtractPath(value);
					string parameters = ExtractParameters(value);

					// FIX
                    			// old: Item sourceTarget = this.GetItem().Axes.GetItem(path);
            				Item sourceTarget = Sitecore.Context.ContentDatabase.Items[ItemID].Axes.SelectSingleItem(path);

					if (sourceTarget != null)
					{
						base.Source = "DataSource=" + sourceTarget.Paths.FullPath + parameters;
					}
					else
					{
						base.Source = value; // let the base class deal with it.
					}
				}
			}
		}
		#endregion

		/// <summary>
		/// Gets the XPath statement to find the Source item.
		/// </summary>
		/// <param name="query">The unparsed string from the Source field of the Template Field Item.</param>
		/// <returns>The XPath statement segment of the string.</returns>
		private static string ExtractPath(string query)
		{
			string output = string.Empty;

			if (query.ToLowerInvariant().Contains("datasource="))
			{
				query = query.Substring(query.ToLowerInvariant().IndexOf("datasource=", StringComparison.OrdinalIgnoreCase) + "datasource=".Length);
				int ampersandIndex = query.IndexOf("&", StringComparison.OrdinalIgnoreCase);
				if (ampersandIndex != -1)
				{
					query = query.Substring(0, ampersandIndex);
				}
			}

			if (query.StartsWith("query:", StringComparison.OrdinalIgnoreCase))
			{
				output = query.Substring("query:".Length);
			}

			if (output.Contains("&"))
			{
				output = output.Substring(0, output.IndexOf("&", StringComparison.OrdinalIgnoreCase));

				// a/b/c&param=d
				// 0123456789012 - len   = 13
				//      5        - index =  5
				// 01234         - len   =  5
				// a/b/c         - output
			}

			return output;
		}

		/// <summary>
		/// Isolates the ampersand delimited parameters portion of the Source field.
		/// </summary>
		/// <param name="query">The unparsed string from the Source field of the Template Field Item.</param>
		/// <returns>The ampersand delmited parameters segment of the string.</returns>
		private static string ExtractParameters(string query)
		{
			var output = string.Empty;

			if (query.Contains("&"))
			{
				output = query.Substring(query.IndexOf("&", StringComparison.OrdinalIgnoreCase), query.Length - query.IndexOf("&", StringComparison.OrdinalIgnoreCase));

				// b/c&param=d
				// 01234567890 - len   = 11
				//    3        - index =  3
				//    12345678 - len   =  8
				// &param=d    - output
			}

			return output;
		}
	}
}
